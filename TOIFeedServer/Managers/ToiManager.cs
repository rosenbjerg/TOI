using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using TOIFeedServer.Database;
using TOIFeedServer.Models;
using static TOIFeedServer.Extensions;

namespace TOIFeedServer.Managers
{
    public class ToiManager
    {
        private readonly DatabaseService _dbService;

        public ToiManager(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        private ToiModel ValidateToiForm(IFormCollection form)
        {
            var fields = new List<string> { "contexts", "tags", "title", "url", "description" };

            if (fields.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0])))
                return null;

            var contextIds = SplitIds(form["contexts"][0]).ToList();
            var tagIds = SplitIds(form["tags"][0]).ToList();

            var tm = new ToiModel
            {
                Id = Guid.NewGuid().ToString("N"),
                Description = form["description"],
                Title = form["title"],
                Url = form["url"],
                Image = form.ContainsKey("image") ? form["image"] : StringValues.Empty,
                Contexts = contextIds,
                Tags = tagIds
            };

            return tm;
        }

        public async Task<string> CreateToi(IFormCollection form)
        {
            var toi = ValidateToiForm(form);
            if (toi == null) return "-1";
            return await _dbService.InsertToiModel(toi) == DatabaseStatusCode.Created ? toi.Id : "-1";
        }

        public async Task<bool> UpdateToi(IFormCollection form)
        {
            var toi = ValidateToiForm(form);
            if (toi == null) return false;
            return await _dbService.UpdateToiModel(toi) == DatabaseStatusCode.Updated;
        }
        
        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByContext(string context)
        {
            DbResult<IEnumerable<ToiModel>> result;
            if (context == "")
            {
                result = await _dbService.GetAllToiModels();
            }
            else
            {
                var ids = context.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet();
                result = await _dbService.GetToisByContext(ids);
            }
            return result;
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetToiByTagIds(IEnumerable<string> ids)
        {
            var res = await _dbService.GetToisByTagIds(ids);
            return res;
        }
    }
}