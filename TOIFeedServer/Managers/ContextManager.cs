using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TOIClasses;

namespace TOIFeedServer.Managers
{
    class ContextManager
    {
        public Database _db { get; private set; }

        public ContextManager(Database db)
        {
            _db = db;
        }

        public async Task<UserActionResponse<ContextModel>> CreateContext(IFormCollection form)
        {
            var context = ValidateContextForm(form, false, out var error);
            if (context == null)
                return new UserActionResponse<ContextModel>(error, null);
            if (await _db.Contexts.Insert(context) != DatabaseStatusCode.Created)
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
