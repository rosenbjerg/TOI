using System;
using System.Security.Cryptography;
using System.Text;
using TOIFeedRepo.Database;

namespace TOIFeedRepo
{
    class ApiKeyGenerator
    {
        private FeedRepoDatabase _db;
        private readonly RandomNumberGenerator _cryptoGen = RandomNumberGenerator.Create();
        private readonly Random _random = new Random();
        
        public ApiKeyGenerator(FeedRepoDatabase db)
        {
            _db = db;
        }

        public string GenerateNew()
        {
            var data = new byte[32];
            _cryptoGen.GetBytes(data);
            var b64 = Convert.ToBase64String(data);
            var idSb = new StringBuilder(b64, 46);
            idSb.Replace('+', (char)_random.Next(97, 122));
            idSb.Replace('=', (char)_random.Next(97, 122));
            idSb.Replace('/', (char)_random.Next(97, 122));
            var id = idSb.ToString();
            return _db.Feeds.FindOne(id) == null 
                ? id 
                : GenerateNew();
        }
    }
}