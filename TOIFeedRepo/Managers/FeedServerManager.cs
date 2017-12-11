using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TOIClasses;
using TOIFeedRepo.Database;
using TOIFeedServer;
using TOIFeedServer.Managers;
using FormValidator;
using Microsoft.AspNetCore.Http;
using Validator = FormValidator.FormValidator;

namespace TOIFeedRepo.Managers
{
    internal class FeedServerManager
    {
        private readonly Validator _registerFormValidator;
        private readonly Validator _updateFormValidator;
        private readonly FeedRepoDatabase _db;
        private readonly ApiKeyGenerator _keygen;
        private readonly Validator _activationFormValidator;
        private readonly Validator _positionFormValidtor;


        public FeedServerManager(FeedRepoDatabase db)
        {
            _db = db;
            _registerFormValidator = FormValidatorBuilder
                .New()
                .RequiresString("apiKey")
                .RequiresString("title")
                .RequiresString("baseUrl")
                .RequiresRational("latitude", -85.05115, 85)
                .RequiresRational("longitude", -180, 180)
                .RequiresRational("radius", 0, double.MaxValue)
                .RequiresString("contactEmail")
                .Build();
            _updateFormValidator = FormValidatorBuilder
                .New()
                .RequiresString("id")
                .RequiresString("title")
                .RequiresString("baseUrl")
                .RequiresString("apiKey")
                .Build();
            _activationFormValidator = FormValidatorBuilder
                .New()
                .RequiresString("apiKey")
                .RequiresString("active")
                .Build();
            _positionFormValidtor = FormValidatorBuilder
                .New()
                .RequiresString("apiKey")
                .RequiresRational("latitude", -85.05115, 85)
                .RequiresRational("longitude", -180, 180)
                .RequiresInteger("radius", 0, int.MaxValue)
                .Build();
            _keygen = new ApiKeyGenerator(db);
        }

        public async Task<IEnumerable<Feed>> AllActiveFeeds()
        {
            var feedResults = await _db.Feeds.Find(f => f.IsActive);
            return feedResults.Status == DatabaseStatusCode.Ok ? feedResults.Result : null;
        }

        public async Task<IEnumerable<Feed>> FeedsFromLocation(LocationModel gpsLoc)
        {
            var feedResults = await _db.Feeds.Find(f => f.IsActive);
            var withinRange = feedResults.Result.Where(f => f.WithinRange(gpsLoc));
            return withinRange;
        }

        public async Task<Feed> GetFeedServer(string apiKey)
        {
            var feed = await _db.Feeds.FindOne(f => f.Id == apiKey);
            return feed.Result;
        }

        public async Task<UserActionResponse<Feed>> RegisterFeed(IFormCollection form)
        {
            if (!_registerFormValidator.Validate(form))
            {
                return new UserActionResponse<Feed>("One of the required fields are missing", null);
            }
            
            var feed = new Feed
            {
                Id = form["apiKey"][0],
                Title = form["title"][0],
                BaseUrl = form["baseUrl"][0],
                IsActive = false,
                Longitude = double.Parse(form["longitude"][0]),
                Latitude = double.Parse(form["longitude"][0]),
                Radius = int.Parse(form["radius"][0])
            };
            if (form.ContainsKey("description"))
                feed.Description = form["description"][0];

            var dbResult = await _db.Feeds.Update(feed.Id, feed);
            return dbResult == DatabaseStatusCode.Ok
                ? new UserActionResponse<Feed>("Your feed has been created.", feed) 
                : new UserActionResponse<Feed>("The feed could not be inserted in the database.", null);
        }

        public async Task<UserActionResponse<Feed>> UpdateFeed(IFormCollection form)
        {
            if (!_updateFormValidator.Validate(form))
            {
                return new UserActionResponse<Feed>("One of the required fields are missing", null);
            }
            var apiKey = form["apiKey"][0];
            var feedExists = await _db.Feeds.FindOne(f => f.Id == apiKey);
            if (feedExists.Status != DatabaseStatusCode.Ok)
            {
                return new UserActionResponse<Feed>("Invalid ApiKey", null);
            }
            var oldFeed = feedExists.Result;

            oldFeed.Title = form["title"][0];
            oldFeed.BaseUrl = form["baseUrl"][0];
            if (form.ContainsKey("description"))
                oldFeed.Description = form["description"];

            var updated = await _db.Feeds.Update(oldFeed.Id, oldFeed);
            return updated == DatabaseStatusCode.Updated 
                ? new UserActionResponse<Feed>("Your feed was updated", oldFeed) 
                : new UserActionResponse<Feed>("Could not update feed", null);
        }

        public async Task<UserActionResponse<Feed>> UpdateActivation(IFormCollection form)
        {
            if (!_activationFormValidator.Validate(form))
            {
                return new UserActionResponse<Feed>("Some fields were missing", null);
            }
            var apiKey = form["apiKey"][0];
            var feedExists = await _db.Feeds.FindOne(f => f.Id == apiKey);
            if (feedExists.Status != DatabaseStatusCode.Ok)
            {
                return new UserActionResponse<Feed>("Invalid ApiKey", null);
            }
            var oldFeed = feedExists.Result;

            oldFeed.IsActive = bool.Parse(form["active"][0]);

            var updated = await _db.Feeds.Update(oldFeed.Id, oldFeed);
            return updated == DatabaseStatusCode.Updated
                ? new UserActionResponse<Feed>("Your feed was updated", oldFeed)
                : new UserActionResponse<Feed>("Could not update feed", null);
        }

        public async Task<UserActionResponse<Feed>> UpdatePosition(IFormCollection form)
        {
            if (!_positionFormValidtor.Validate(form))
            {
                return new UserActionResponse<Feed>("Some fields were missing", null);
            }
            var apiKey = form["apiKey"][0];
            var feedExists = await _db.Feeds.FindOne(f => f.Id == apiKey);
            if (feedExists.Status != DatabaseStatusCode.Ok)
            {
                return new UserActionResponse<Feed>("Invalid ApiKey", null);
            }
            var oldFeed = feedExists.Result;

            oldFeed.Latitude = double.Parse(form["latitude"][0]);
            oldFeed.Longitude = double.Parse(form["longitude"][0]);
            oldFeed.Radius = int.Parse(form["radius"][0]);

            var updated = await _db.Feeds.Update(oldFeed.Id, oldFeed);
            return updated == DatabaseStatusCode.Updated
                ? new UserActionResponse<Feed>("Your feed was updated", oldFeed)
                : new UserActionResponse<Feed>("Could not update feed", null);
        }
    }
}
