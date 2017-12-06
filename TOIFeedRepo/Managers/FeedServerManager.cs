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
using FormValidator = FormValidator.FormValidator;

namespace TOIFeedRepo.Managers
{
    internal class FeedServerManager
    {
        private global::FormValidator.FormValidator _registerFormValidator { get; }
        private FeedRepoDatabase _db { get; }

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
                Id = Guid.Empty.ToString("N"),
                Title = form["title"],
                BaseUrl = form["baseUrl"],
                IsActive = false,
                LocationCenter = new GpsLocation
                {
                    Longitude = double.Parse(form["longitude"]),
                    Latitude = double.Parse(form["longitude"])
                },
                Radius = double.Parse(form["radius"])
            };
            if (form.ContainsKey("description"))
                feed.Description = form["description"];

            var dbResult = _db.Feeds.Insert(feed);
            return dbResult.Result == DatabaseStatusCode.Ok
                ? new UserActionResponse<Feed>("Your feed has been created.", feed) 
                : new UserActionResponse<Feed>("The feed could not be inserted in the database.", null);
        }
    }
}
