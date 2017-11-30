using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using TOIClasses;
using static TOIFeedServer.Extensions;

namespace TOIFeedServer.Managers
{
    public class ToiManager
    {
        private readonly Database _db;

        public ToiManager(Database db)
        {
            _db = db;
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetToiByTagIds(IEnumerable<string> ids)
        {
            var res = await _db.Tois.Find(t => t.Tags.Any(i => ids.Contains(i)));
            return res;
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetToiByGpsLocation(GpsLocation gpsLocations)
        {
            var tags = await _db.Tags.Find(t => t.Type == TagType.Gps && t.WithinRange(gpsLocations));
            if (tags.Status == DatabaseStatusCode.NoElement)
            {
                return new DbResult<IEnumerable<ToiModel>>(null, DatabaseStatusCode.NoElement);
            }

            var toi = await GetToiByTagIds(tags.Result.Select(t => t.Id));
            return toi;
        }

        private static bool TryParseInformationType(string informationType, out ToiInformationType type)
        {
            switch (informationType)
            {
                case "Website":
                    type = ToiInformationType.Website;
                    return true;
                case "Video":
                    type = ToiInformationType.Video;
                    return true;
                case "Image":
                    type = ToiInformationType.Image;
                    return true;
                case "Audio":
                    type = ToiInformationType.Audio;
                    return true;
                case "Text":
                    type = ToiInformationType.Text;
                    return true;
                default:
                    type = ToiInformationType.Website;
                    return false;
            }
        }

        private ToiModel ValidateToiForm(IFormCollection form, bool update, out string error)
        {
            var nonEmpty = new List<string> { "title", "url", "type", "image" };
            var canBeEmpty = new List<string> { "contexts", "tags", "description" };

            if (update && (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0])))
            {
                error = "Please supply an id";
                return null;
            }

            var missing = canBeEmpty.Where(field => !form.ContainsKey(field));
            if (missing.Any())
            {
                error = "Missing values for: " + String.Join(", ", missing);
                return null;
            }

            missing = nonEmpty.Where(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]));
            if (missing.Any())
            {
                error = "Missing values for: " + String.Join(", ", missing);
                return null;
            }

            if (!TryParseInformationType(form["type"][0], out var type))
            {
                error = "Invalid information type: " + form["type"][0];
                return null;
            }

            var contextIds = SplitIds(form["contexts"][0]).ToList();
            var tagIds = SplitIds(form["tags"][0]).ToList();
            

            var tm = new ToiModel
            {
                Id = update ? form["id"][0] : Guid.NewGuid().ToString("N"),
                Description = form["description"][0],
                Title = form["title"][0],
                Url = form["url"][0],
                Image = form["image"][0],
                Contexts = contextIds,
                Tags = tagIds,
                InformationType = type
            };
            error = string.Empty;
            return tm;
        }

        public async Task<UserActionResponse<ToiModel>> CreateToi(IFormCollection form)
        {
            var toi = ValidateToiForm(form, false, out var error);
            if (toi == null)
                return new UserActionResponse<ToiModel>(error, null);
            if (await _db.Tois.Insert(toi) != DatabaseStatusCode.Created)
                return new UserActionResponse<ToiModel>("Could not create the ToI", null);
            return  new UserActionResponse<ToiModel>("The ToI was created", toi);
        }

        public async Task<UserActionResponse<ToiModel>> UpdateToi(IFormCollection form)
        {
            var toi = ValidateToiForm(form, true, out var error);
            if (toi == null)
                return new UserActionResponse<ToiModel>(error, null);
            if (await _db.Tois.Update(toi.Id, toi) != DatabaseStatusCode.Updated)
                return new UserActionResponse<ToiModel>("Could not update the ToI", null);
            return new UserActionResponse<ToiModel>("The ToI was updated", toi);
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByContext(string context)
        {
            DbResult<IEnumerable<ToiModel>> result;
            if (context == "")
            {
                result = await _db.Tois.GetAll();
            }
            else
            {
                var ids = context.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet();
                result = await _db.Tois.Find(t => t.Contexts.Any(ids.Contains));
            }
            return result;
        }
        public async Task<UserActionResponse<ContextModel>> CreateContext(IFormCollection form)
        {
            var context = ValidateContextForm(form, false, out var error);
            if (context == null)
                return new UserActionResponse<ContextModel>(error, null);
            if(await _db.Contexts.Insert(context) != DatabaseStatusCode.Created)
                return new UserActionResponse<ContextModel>("Could not create the context", null);
            return new UserActionResponse<ContextModel>("The context was created", context);
        }


        public async Task<UserActionResponse<ContextModel>> UpdateContext(IFormCollection form)
        {
            var context = ValidateContextForm(form, true, out var error);
            if (context == null)
                return new UserActionResponse<ContextModel>(error, null);
            if (await _db.Contexts.Update(context.Id, context) != DatabaseStatusCode.Updated)
                return new UserActionResponse<ContextModel>("Could not update the context", null);
            return new UserActionResponse<ContextModel>("The context was updated", context);
        }
        private ContextModel ValidateContextForm(IFormCollection form, bool update, out string error)
        {
            var nonEmpty = new List<string> { "title", "description" };

            var missing = nonEmpty.Where(field => !form.ContainsKey(field));
            if (missing.Any())
            {
                error = "Missing values for: " + String.Join(", ", missing);
                return null;
            }

            if (update && (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0])))
            {
                error = "Please supply an id for the context you wish to update.";
                return null;
            }

            var ctx = new ContextModel
            {
                Id = update ? form["id"][0] : Guid.NewGuid().ToString("N"),
                Description = form["description"][0],
                Title = form["title"][0],
            };
            error = string.Empty;
            return ctx;
        }

        public async Task<bool> DeleteContext(IFormCollection form)
        {
            if (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0]))
                return false;
            return await _db.Contexts.Delete(form["id"][0]) == DatabaseStatusCode.Deleted;
        }
    }
}
