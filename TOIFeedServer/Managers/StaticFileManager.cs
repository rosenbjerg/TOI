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
        public const string UploadDir = "./public/uploads";

        public StaticFileManager(Database db)
        {
            _db = db;
            if (Directory.Exists(UploadDir)) return;
            Console.WriteLine("Creating uploads folder.");
            Directory.CreateDirectory(UploadDir);
        }

        public async Task<IEnumerable<StaticFile>> AllStaticFiles()
        {
            return (await _db.Files.GetAll()).Result;
        }

        public async Task<DbResult<StaticFile>> GetStaticFile(string id)
        {
            return  await _db.Files.FindOne(f => f.Id == id);
        }

        private static StaticFile ValidateStaticFileForm(IFormCollection form, out string error, int? number = null)
        {
            var fileNumber = (number != null) ? number.ToString() : "";
            var fields = new [] {$"title{fileNumber}", $"description{fileNumber}", $"file{fileNumber}"};

            var missing = fields.Where(f => !form.ContainsKey(f) || string.IsNullOrEmpty(form[f][0]));
            if (missing.Any())
            {
                error = "Missing values for: " + string.Join(", ", missing);
                return null;
            }

            var sf = new StaticFile
            {
                Title = form["title"][0],
                Id = Guid.NewGuid().ToString("N"),
                Description = form["description"][0]
            };
            error = string.Empty;
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

            return await _db.Files.Update(sf.Id, sf) != DatabaseStatusCode.Updated 
                ? new UserActionResponse<StaticFile>("The file has been stored, but not the details about it", null) 
                : new UserActionResponse<StaticFile>("Your file was updated", sf);
        }

        public async Task<UserActionResponse<IEnumerable<StaticFile>>> UploadFiles(IFormCollection form)
        {
            if(form.Files.Count == 0)
                return new UserActionResponse<IEnumerable<StaticFile>>("No files have been selected", null);
            var sfs = new List<StaticFile>();

            //Iterate through the files and check that the supplied information about them is valid
            for (var i = 0; i < form.Files.Count; i++)
            {
                var sf = ValidateStaticFileForm(form, out var error, i);
                if (sf == null)
                    continue;
                sf.Filetype = Path.GetExtension(form.Files[i].FileName);
                
                try
                {
                    using (var upload = File.Create(Path.Combine(UploadDir, $"{sf.Id}.{sf.Filetype}")))
                    {
                        await form.Files[i].CopyToAsync(upload);
                    }
                    sfs.Add(sf);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            //Insert the form handles
            await _db.Files.Insert(sfs.ToArray());

            return new UserActionResponse<IEnumerable<StaticFile>>($"{sfs.Count} / {form.Files.Count} files uploaded successfully", sfs);
        }

        public async Task<UserActionResponse<bool>> DeleteStaticFile(IFormCollection form)
        {
            if (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0]))
                return new UserActionResponse<bool>("Could not find an id", false);
            var id = form["id"][0];

            var fileRes = await _db.Files.FindOne(f => f.Id == id);
            if (fileRes.Status == DatabaseStatusCode.NoElement)
                return new UserActionResponse<bool>("There is no file with that id", false);
            var file = fileRes.Result;
            File.Delete(Path.Combine(UploadDir, file.GetFilename()));
            return await _db.Files.Delete(id) == DatabaseStatusCode.Deleted 
                ? new UserActionResponse<bool>("The item was deleted", true)
                : new UserActionResponse<bool>("The file was partially deleted", false);
        }
    }
}
