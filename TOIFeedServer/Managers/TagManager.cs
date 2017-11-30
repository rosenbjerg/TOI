﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TOIClasses;

namespace TOIFeedServer.Managers
{
    public class TagManager
    {
        private readonly Database _db;

        public TagManager(Database db)
        {
            _db = db;
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTags(HashSet<string> ids = null)
        {
            if (ids == null)
                return await _db.Tags.GetAll();
            return await _db.Tags.Find(t => ids.Contains(t.Id));
        }

        
        private static readonly char[] TrimChars = new char[]
        {
            ':', ' ', '-', ',', '.'
        };
        private static TagModel ValidateTagForm(IFormCollection form, bool update)
        {
            var fields = new List<string> { "title", "longitude", "latitude", "radius", "type", "id" };
            if (fields.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]))) 
                return null;

            if (!int.TryParse(form["radius"][0], out var radius) || 
                radius < 1 ||
                !double.TryParse(form["longitude"][0].Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var longitude) ||
                !double.TryParse(form["latitude"][0].Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var latitude) ||
                    !TryParseTagType(form["type"][0], out var type))
                return null;
            
            return new TagModel
            {
                Title = form["title"][0],
                Id = string.Join("", form["id"][0].Split(TrimChars, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant(),
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

        public async Task<TagModel> CreateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form, false);
            if (tag == null) return null;
            return await _db.Tags.Insert(tag) != DatabaseStatusCode.Created ? null : tag;
        }

        public async Task<TagModel> UpdateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form, true);
            if (tag == null) return null;
            return await _db.Tags.Update(tag.Id, tag) != DatabaseStatusCode.Updated ? null : tag;
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
    }
}