using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using TOIClasses;
using TOIFeedServer.Database;
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

        private ToiModel ValidateToiForm(IFormCollection form, bool update)
        {
            var nonEmpty = new List<string> { "title", "url", "type", "image" };
            var canBeEmpty = new List<string> { "contexts", "tags", "description" };

            if (update && (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0])))
                return null;
            
            if (canBeEmpty.Any(field => !form.ContainsKey(field)))
                return null;
            
            if (nonEmpty.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0])))
                return null;

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
                Tags = tagIds
            };

            return tm;
        }

        public async Task<ToiModel> CreateToi(IFormCollection form)
        {
            var toi = ValidateToiForm(form, false);
            if (toi == null) return null;
            return await _dbService.InsertToiModel(toi) == DatabaseStatusCode.Created ? toi : null;
        }

        public async Task<ToiModel> UpdateToi(IFormCollection form)
        {
            var toi = ValidateToiForm(form, true);
            if (toi == null) return null;
            return await _dbService.UpdateToiModel(toi) == DatabaseStatusCode.Updated ? toi : null;
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
        public async Task<ContextModel> CreateContext(IFormCollection form)
        {
            var context = ValidateContextForm(form, false);
            if (context == null) return null;
            return await _dbService.InsertContext(context) == DatabaseStatusCode.Created ? context : null;
        }


        public async Task<ContextModel> UpdateContext(IFormCollection form)
        {
            var context = ValidateContextForm(form, true);
            if (context == null) return null;
            return await _dbService.UpdateContext(context) == DatabaseStatusCode.Updated ? context : null;
        }
        private ContextModel ValidateContextForm(IFormCollection form, bool update)
        {
            var nonEmpty = new List<string> { "title", "description" };

            if (nonEmpty.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0])))
                return null;
            if (update && (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0])))
                return null;
            
            var ctx = new ContextModel
            {
                Id = update ? form["id"][0] : Guid.NewGuid().ToString("N"),
                Description = form["description"][0],
                Title = form["title"][0],
            };

            return ctx;
        }

        public async Task<bool> DeleteContext(IFormCollection form)
        {
            if (!form.ContainsKey("id") || string.IsNullOrEmpty(form["id"][0]))
                return false;
            return await _dbService.DeleteContext(form["id"][0]) == DatabaseStatusCode.Deleted;
        }
    }
}