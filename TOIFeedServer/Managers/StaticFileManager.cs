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

        private static StaticFile ValidateStaticFileForm(IFormCollection form, out string error, string number = "", bool update = false)
        {
            var fields = new [] {$"title{number}"};

            var missing = fields.Where(f => !form.ContainsKey(f) || string.IsNullOrEmpty(form[f][0]));
            if (missing.Any())
            {
                error = "Missing values for: " + string.Join(", ", missing);
                return null;
            }

            if (update && (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0])))
            {
                error = "No id could be found for the form";
                return null;
            }
            if (update && (!form.ContainsKey("filetype") || string.IsNullOrEmpty(form["filetype"][0])))
            {
                error = "Please supply a filetype";
                return null;
            }

            var sf = new StaticFile
            {
                Title = form[$"title{number}"][0],
                Id = update ? form["id"][0] : Guid.NewGuid().ToString("N"),
            };
            if (form.ContainsKey($"description{number}"))
                sf.Description = form[$"description{number}"][0];

            error = string.Empty;
            return sf;
        }

        public async Task<UserActionResponse<StaticFile>> UpdateStaticFile(IFormCollection form)
        {
            var sf = ValidateStaticFileForm(form, out var error, update: true);
            if(sf == null)
                return new UserActionResponse<StaticFile>(error, null);
            sf.Filetype = form["filetype"][0];
            var update = await _db.Files.Update(sf.Id, sf);
            return update != DatabaseStatusCode.Updated 
                ? new UserActionResponse<StaticFile>("Could not update the file", null) 
                : new UserActionResponse<StaticFile>("Your file was updated", sf);
        }

        public async Task<UserActionResponse<List<StaticFile>>> UploadFiles(IFormCollection form)
        {
            if(form.Files.Count == 0)
                return new UserActionResponse<List<StaticFile>>("No files have been selected", null);
            var succeededUploads = new List<StaticFile>();
            foreach (var formFile in form.Files)
            {
                var no = formFile.Name.Substring(4);

                var staticFile = ValidateStaticFileForm(form, out var error, no);
                if (staticFile == null)
                    continue;
                staticFile.Filetype = Path.GetExtension(formFile.FileName).TrimStart('.').ToLowerInvariant();

                var filepath = Path.Combine(UploadDir, staticFile.GetFilename());
                try
                {
                    using (var upload = File.Create(filepath))
                    {
                        await formFile.CopyToAsync(upload);
                    }
                    succeededUploads.Add(staticFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if(File.Exists(filepath))
                        File.Delete(filepath);
                }

            }
            //Insert the form handles
            await _db.Files.Insert(succeededUploads.ToArray());

            return new UserActionResponse<List<StaticFile>>($"{succeededUploads.Count} / {form.Files.Count} files uploaded successfully", succeededUploads);
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
