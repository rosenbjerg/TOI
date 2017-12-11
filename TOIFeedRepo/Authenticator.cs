using System;
using System.Threading.Tasks;
using FormValidator;
using Microsoft.AspNetCore.Http;
using TOIClasses;
using TOIFeedRepo.Database;
using TOIFeedServer;

namespace TOIFeedRepo
{
    internal class Authenticator
    {
        private readonly ApiKeyGenerator _keygen;
        private readonly FormValidator.FormValidator _requestFormValidator;
        private readonly FeedRepoDatabase _db;

        public Authenticator(FeedRepoDatabase db)
        {
            _db = db;
            _keygen = new ApiKeyGenerator(db);
            _requestFormValidator = FormValidatorBuilder.New()
                .RequiresString("email")
                .RequiresString("type")
                .RequiresString("name")
                .RequiresString("street")
                .RequiresString("zip", 4)
                .RequiresString("city")
                .RequiresString("country")
                .Build();

        }

        public async Task<string> RequestApiKey(IFormCollection form)
        {
            if (!_requestFormValidator.Validate(form))
                return null;
            var customer = new FeedOwner { 
                Id = Guid.NewGuid().ToString("N"),
                Email = form["email"][0],
                Type = form["type"][0],
                Name = form["name"][0],
                Street = form["street"][0],
                Zip = form["zip"],
                City = form["city"],
                Country = form["country"]
            };
            var custInsertion = await _db.Customers.Insert(customer);
            if (custInsertion != DatabaseStatusCode.Ok)
                return null;
            var key = _keygen.GenerateNew();
            await _db.Feeds.Insert(new Feed()
            {
                Id = key,
                Owner = customer 
            });

            return key;
        }
    }
}