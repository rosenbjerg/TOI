using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServiceStack;
using TOIClasses;

namespace TOIFeedServer.Managers
{
    class StaticFileManager
    {
        private Database _db;

        public StaticFileManager(Database db)
        {
            _db = db;
        }

        public async Task<IEnumerable<StaticFile>> AllStaticFiles()
        {
            return (await _db.Files.GetAll()).Result;
        }

        public async Task<DbResult<StaticFile>> GetStaticFile(string id)
        {
            return  await _db.Files.FindOne(f => f.Id == id);
        }

        private StaticFile ValidateStaticFileForm(IFormCollection form, out string error, int? number = null)
        {
            var fileNumber = (number != null) ? number.ToString() : "";
            var fields = new [] {$"title{fileNumber}", $"description{fileNumber}"};

            var missing = fields.Where(f => !form.ContainsKey(f) || string.IsNullOrEmpty(form[f][0]));
            if (missing.Any())
            {
                error = "Missing values for: " + String.Join(", ", missing);
                return null;
            }

            var sf = new StaticFile
            {
                Title = form["title"][0],
                Id = Guid.NewGuid().ToString("N"),
                Description = form["description"][0]
            };
            error = String.Empty;
            return sf;
        }

        public async Task<UserActionResponse<StaticFile>> UpdateStaticFile(IFormCollection form)
        {
            if(!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0]))
                return new UserActionResponse<StaticFile>("No id could be found for the form", null);
            var sf = ValidateStaticFileForm(form, out var error);
            if(sf == null)
                return new UserActionResponse<StaticFile>(error, null);
            sf.Filetype = Path.GetExtension(form.Files[0].FileName);

            try
            {
                var filePath = Path.Combine(".", "uploads", $"{sf.Id}.{sf.Filetype}");
                File.Delete(filePath);
                using (var fileReplace = File.Create(filePath))
                {
                    await form.Files[0].CopyToAsync(fileReplace);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
                return new UserActionResponse<StaticFile>("Could not store the file", null);
            }

            if (await _db.Files.Update(sf.Id, sf) != DatabaseStatusCode.Updated)
                return new UserActionResponse<StaticFile>("The file has been stored, but not the details about it", null);
            return new UserActionResponse<StaticFile>("Your file was updated", sf);
        }

        public async Task<UserActionResponse<bool>> UploadFile(IFormCollection form)
        {
            if(form.Files.Count == 0)
                return new UserActionResponse<bool>("No files have been selected", false);
            var sfs = new List<StaticFile>();

            //Iterate through the files and check that the supplied information about them is valid
            for (var i = 0; i < form.Files.Count; i++)
            {
                var sf = ValidateStaticFileForm(form, out var error, i);
                if (sf == null)
                    return new UserActionResponse<bool>(error, false);
                sf.Filetype = Path.GetExtension(form.Files[i].FileName);
                sfs.Add(sf);
            }
            //Insert the form handles
            await _db.Files.Insert(sfs.ToArray());

            //Write all the files to the uploads folder
            var writeError = string.Empty;
            for (var i = 0; i < form.Files.Count; i++)
            {
                try
                {
                    using (var upload = File.Create(Path.Combine(".", "uploads", $"{sfs[i].Id}.{sfs[i].Filetype}")))
                    {
                        await form.Files[i].CopyToAsync(upload);
                    }
                }
                catch (IOException e)
                {
                    writeError = "One or more form uploads failed";
                    Console.WriteLine(e.Message);
                }
            }

            return new UserActionResponse<bool>(writeError, string.IsNullOrEmpty(writeError));
        }

        public async Task<bool> DeleteStaticFile(IFormCollection form)
        {
            if (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0]))
                return false;
            var id = form["id"][0];

            var fileRes = await _db.Files.FindOne(f => f.Id == id);
            if (fileRes.Status == DatabaseStatusCode.NoElement)
                return false;
            var file = fileRes.Result;
            File.Delete(Path.Combine(".", "uploads", $"{file.Id}.{file.Filetype}"));
            if (await _db.Files.Delete(id) != DatabaseStatusCode.Deleted)
                return false;

            return true;
        }
    }
}
