using TOIClasses;

namespace TOIFeedServer.Managers
{
    public class User : ModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        
    }
    
    public class UserManager
    {
        private Database _db;

        public UserManager(Database db)
        {
            _db = db;
        }

        public User CreateUser(string username, string password, string email)
        {
            if (_db.Users.FindOne(u => u.Username == username || u.Email == email) != null)
                return null;
            var user = new User
            {
                Username = username,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };
            return _db.Users.Insert(user) != null ? user : null;
        }
    }
}