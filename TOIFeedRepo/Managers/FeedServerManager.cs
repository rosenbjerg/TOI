using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TOIClasses;
using TOIFeedRepo.Database;
using TOIFeedServer;
using TOIFeedServer.Managers;
using FormValidator;
using Microsoft.AspNetCore.Http;

namespace TOIFeedRepo.Managers
{
    internal class FeedServerManager
    {
        private readonly FormValidator.FormValidator _registerFormValidator;
        private readonly FormValidator.FormValidator _updateFormValidator;
        private readonly FeedRepoDatabase _db;
        private readonly ApiKeyGenerator _keygen;


        public FeedServerManager(FeedRepoDatabase db)
        {
            _db = db;
            _registerFormValidator = FormValidatorBuilder
                .New()
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
                .RequiresString("active", val => val == "true" || val == "false")
                .RequiresRational("latitude", -85.05115, 85)
                .RequiresRational("longitude", -180, 180)
                .RequiresRational("radius", 0, double.MaxValue)
                .RequiresString("contactEmail")
                .Build();
            _keygen = new ApiKeyGenerator(db);
        }

        public async Task<IEnumerable<Feed>> AllActiveFeeds()
        {
            var feedResults = await _db.Feeds.Find(f => f.IsActive);
            return feedResults.Status == DatabaseStatusCode.Ok ? feedResults.Result : null;
        }

        public async Task<IEnumerable<Feed>> FeedsFromLocation(GpsLocation gpsLoc)
        {
            var feedResults = await _db.Feeds.Find(f => f.IsActive && f.WithinRange(gpsLoc));
            return feedResults.Status == DatabaseStatusCode.Ok ? feedResults.Result : null;
        }

        public async Task<UserActionResponse<Feed>> RegisterFeed(IFormCollection form)
        {
            if (!_registerFormValidator.Validate(form))
            {
                return new UserActionResponse<Feed>("One of the required fields are missing", null);
            }

            //TODO generate API key!
            var feed = new Feed
            {
                Id = _keygen.GenerateNew(),
                Title = form["title"][0],
                BaseUrl = form["baseUrl"][0],
                IsActive = false,
                LocationCenter = new GpsLocation
                {
                    Longitude = double.Parse(form["longitude"][0]),
                    Latitude = double.Parse(form["longitude"][0])
                },
                Radius = double.Parse(form["radius"][0])
            };
            if (form.ContainsKey("description"))
                feed.Description = form["description"][0];

            var dbResult = _db.Feeds.Insert(feed);
            return dbResult.Result == DatabaseStatusCode.Ok
                ? new UserActionResponse<Feed>("Your feed has been created.", feed) 
                : new UserActionResponse<Feed>("The feed could not be inserted in the database.", null);
        }

        public async Task<UserActionResponse<Feed>> UpdateFeed(IFormCollection form)
        {
            if (!_updateFormValidator.Validate(form))
            {
                return new UserActionResponse<Feed>("One of the required fields are missing", null);
            }
            var feed = new Feed
            {
                Id = form["id"][0],
                Title = form["title"][0],
                BaseUrl = form["baseUrl"][0],
                IsActive = false,
                LocationCenter = new GpsLocation
                {
                    Longitude = double.Parse(form["longitude"][0]),
                    Latitude = double.Parse(form["longitude"][0])
                },
                Radius = double.Parse(form["radius"])
            };
            if (form.ContainsKey("description"))
                feed.Description = form["description"];
        }
    }
}
