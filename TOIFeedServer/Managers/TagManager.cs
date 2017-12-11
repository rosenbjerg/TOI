using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using TOIClasses;

namespace TOIFeedServer.Managers
{
    public class UserActionResponse<T>
    {
        public string Message { get; }
        public T Result { get; }

        public UserActionResponse(string message, T result)
        {
            Message = message;
            Result = result;
        }
    }
    public class TagManager
    {
        private readonly Database _db;

        public TagManager(Database db)
        {
            _db = db;
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTags(IQueryCollection query)
        {
            HashSet<string> ids;
            if (query.ContainsKey("ids"))
            {
                ids = Extensions.SplitIds(query["ids"][0]).ToHashSet();
                return await _db.Tags.Find(t => ids.Contains(t.Id));
            }

            if (query.ContainsKey("contexts"))
            {
                var contexts = Extensions.SplitIds(query["contexts"][0]).ToHashSet();
                var toisSearch = await _db.Tois.Find(toi => toi.Contexts.Any(c => contexts.Contains(c)));
                if (toisSearch.Status == DatabaseStatusCode.NoElement)
                    return new DbResult<IEnumerable<TagModel>>(null, DatabaseStatusCode.NoElement);
                var tagIds = toisSearch.Result
                    .SelectMany(toi => toi.Tags)
                    .ToHashSet();
                return await _db.Tags.Find(tag => tagIds.Contains(tag.Id));
            }

            try
            {
                return await _db.Tags.GetAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new DbResult<IEnumerable<TagModel>>(null, DatabaseStatusCode.NoElement);
            }
        }

        
        private static readonly char[] TrimChars = 
        {
            ':', ' ', '-', ',', '.'
        };
        private static TagModel ValidateTagForm(IFormCollection form, out string error, bool update = false)
        {
            var fields = new List<string> { "title", "longitude", "latitude", "radius", "type"};
            var missing = fields.Where(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]));
            if (missing.Any())
            {
                error = "Missing values for: " + string.Join(", ", missing);
                return null;
            }

            if (!int.TryParse(form["radius"][0], out var radius) ||
                radius < 1)
            {
                error = "Invalid radius";
                return null;
            }
            if (!double.TryParse(form["longitude"][0].Replace(",", "."), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var longitude) || longitude < -180 || longitude > 180)
            {
                error = "Invalid longitude";
                return null;
            }
            if(!double.TryParse(form["latitude"][0].Replace(",", "."), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var latitude) || latitude < -85.05115 || latitude > 85)
            {
                error = "Invalid latitude";
                return null;
            }
            if (!TryParseTagType(form["type"][0], out var type))
            {
                error = "Invalid tag type";
                return null;
            }
            
            if (type != TagType.Gps && (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0])))
            {
                error = "Please supply a valid hardware id";
                return null;
            }

            string id;
            if (type == TagType.Gps && !update)
            {
                id = Guid.NewGuid().ToString("N");
            }
            else
            {
                id = form["id"];
            }

            error = string.Empty;
            return new TagModel
            {
                Title = form["title"][0],
                Id = id,
                Radius = radius,
                Longitude = longitude,
                Latitude = latitude,
                Type = type
            };
        }

        private static bool TryParseTagType(string input, out TagType type)
        {
            switch (input)
            {
                case "Wifi":
                    type = TagType.Wifi;
                    return true;
                case "Bluetooth":
                    type = TagType.Bluetooth;
                    return true;
                case "Gps":
                    type = TagType.Gps;
                    return true;
                case "Nfc":
                    type = TagType.Nfc;
                    return true;
                default:
                    type = default(TagType);
                    return false;
            }
        }

        public async Task<UserActionResponse<TagModel>> CreateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form, out var error);
            if (tag == null)
                return new UserActionResponse<TagModel>(error, null);

            if(await _db.Tags.Insert(tag) == DatabaseStatusCode.AlreadyContainsElement)
                return new UserActionResponse<TagModel>("Another tag with that id already exists", null);
            return new UserActionResponse<TagModel>("Tag created succesfully.", tag);
        }

        public async Task<UserActionResponse<TagModel>> UpdateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form, out var error, true);
            if (tag == null)
                return new UserActionResponse<TagModel>(error, null);
            if (await _db.Tags.Update(tag.Id, tag) != DatabaseStatusCode.Updated)
                return new UserActionResponse<TagModel>("Whoops! Could not update the tag", null);
            return new UserActionResponse<TagModel>("Tag updated succesfully.", tag);
        }

        public async Task<TagModel> GetTag(IQueryCollection queries)
        {
            try
            {
                //trim query så den er ROBUST!    
                var id = queries["id"][0];
                var dbRes = await _db.Tags.FindOne(id);
                return dbRes.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                return null;
            }
        }

        public async Task<bool> DeleteTag(IFormCollection form)
        {
            if (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0]))
                return false;
            var id = form["id"][0];

            var toisToUpdate = await _db.Tois.Find(t => t.Tags.Contains(id));
            if (toisToUpdate.Status == DatabaseStatusCode.NoElement)
                return await _db.Tags.Delete(id) == DatabaseStatusCode.Deleted;
            var toiList = toisToUpdate.Result.ToList();

            //Delete the tag from all the ToIs that are associated with it
            foreach (var toi in toiList)
            {
                toi.Tags.Remove(id);
                await _db.Tois.Update(toi.Id, toi);
            }

            return await _db.Tags.Delete(id) == DatabaseStatusCode.Deleted;
        }
    }
}