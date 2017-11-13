using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
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

        private async Task<ToiModel> ValidateToiForm(IFormCollection form)
        {
            var fields = new List<string> { "contexts", "tags", "title", "url", "description" };

            if (fields.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0])))
                return null;

            var contextIds = ParseGuids(form["contexts"][0]).ToHashSet();
            var tagIds = ParseGuids(form["tags"][0]).ToHashSet();
        
            var contexts = await _dbService.GetContextsFromId(contextIds);
            var tags = await _dbService.GetTagsFromId(tagIds);
            if (contexts.Status != DatabaseStatusCode.Ok || tags.Status != DatabaseStatusCode.Ok)
                return null;
            
            var tm = new ToiModel
            {
                Id = Guid.NewGuid(),
                Description = form["description"],
                Title = form["title"],
                Url = form["url"],
                Image = form.ContainsKey("image") ? form["image"] : StringValues.Empty
            };
            tm.ContextModels = contexts.Result.Select(c => new ToiContextModel(tm, c)).ToList();
            tm.TagModels = tags.Result.Select(t => new ToiTagModel(tm, t)).ToList();

            return tm;
        }

        public async Task<Guid> CreateToi(IFormCollection form)
        {
            var toi = await ValidateToiForm(form);
            if (toi == null) return Guid.Empty;
            return await _dbService.InsertToiModel(toi) == DatabaseStatusCode.Created ? toi.Id : Guid.Empty;
        }

        public async Task<bool> UpdateToi(IFormCollection form)
        {
            var toi = await ValidateToiForm(form);
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
                    .Select(GuidParse)
                    .ToHashSet();
                result = await _dbService.GetToisByContext(ids);
            }
            return result;
        }
    }
}