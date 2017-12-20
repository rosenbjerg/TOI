using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TOIClasses;
using Rosenbjerg.SessionManager;

namespace TOIFeedServer.Managers
{
    public class User : ModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        
    }
    
    public class UserManager
    {
        private Database _db;
        private SessionManager<string> _sessionManager;

        public UserManager(Database db)
        {
            _db = db;
            _sessionManager = new SessionManager<string>(TimeSpan.FromDays(7), secure: false);
        }

        public async Task<UserActionResponse<User>> CreateUser(IFormCollection form)
        {
            var fields = new[] {"username", "password", "email"};
            var missing = fields.Where(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]));
            if (missing.Any())
            {
                return new UserActionResponse<User>("Missing values for: " + string.Join(", ", missing), null);
            }
            string username = form["username"][0], password = form["password"][0], email = form["email"][0];

            if (await _db.Users.FindOne(u => u.Username == username || u.Email == email) != null)
                return new UserActionResponse<User>("A user with the given credentials already exists", null);
            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Username = username,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };
            return await _db.Users.Insert(user) == DatabaseStatusCode.Created 
                ? new UserActionResponse<User>("Your account has been created", user) 
                : new UserActionResponse<User>("Could not create your account", user);
        }

        public async Task<UserActionResponse<bool>> Login(string username, string password)
        {
            var user = await _db.Users.FindOne(u => u.Username == username);
            if (user.Status == DatabaseStatusCode.NoElement)
                return new UserActionResponse<bool>("Wrong username or password", false);

            return !BCrypt.Net.BCrypt.Verify(password, user.Result.Password) 
                ? new UserActionResponse<bool>("Wrong username or password", false) 
                : new UserActionResponse<bool>(_sessionManager.OpenSession(username), true);
        }

        public bool VerifyToken(string token)
        {
            return _sessionManager.TryAuthenticateToken(token, out var username);
        }
    }
}