using System.Linq;
using System.Text.RegularExpressions;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public enum AuthResult { Ok, UserExists, NotFound, BadPassword, Full, Invalid }

    public static class AuthService
    {
        public static bool ValidateUsername(string u) =>
            u != null && u.Length >= 3 && u.Length <= 31 &&
            Regex.IsMatch(u, @"^[a-zA-Z0-9_]+$");

        public static bool ValidatePassword(string p) => p != null && p.Length >= 6;

        public static AuthResult Register(string username, string password, string email)
        {
            if (!ValidateUsername(username) || !ValidatePassword(password))
                return AuthResult.Invalid;
            var users = StorageService.LoadUsers();
            if (users.Count >= 200)                     return AuthResult.Full;
            if (users.Any(u => u.Username == username)) return AuthResult.UserExists;
            users.Add(new User { Id=users.Count+1, Username=username, Password=password, Email=email });
            StorageService.SaveUsers(users);
            return AuthResult.Ok;
        }

        public static AuthResult Login(string username, string password)
        {
            var u = StorageService.LoadUsers().FirstOrDefault(x => x.Username == username);
            if (u == null)              return AuthResult.NotFound;
            if (u.Password != password) return AuthResult.BadPassword;
            Session.Login(u);
            return AuthResult.Ok;
        }

        public static void SaveCurrentUser()
        {
            var users = StorageService.LoadUsers();
            int idx   = users.FindIndex(u => u.Id == Session.CurrentUser.Id);
            if (idx >= 0) { users[idx] = Session.CurrentUser; StorageService.SaveUsers(users); }
        }
    }
}
