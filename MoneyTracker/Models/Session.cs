namespace MoneyTracker.Models
{
    public static class Session
    {
        public static bool IsLoggedIn  { get; private set; }
        public static User CurrentUser { get; private set; } = new User();
        public static void Login(User u)  { CurrentUser = u; IsLoggedIn = true; }
        public static void Logout()       { CurrentUser = new User(); IsLoggedIn = false; }
        public static void UpdateBalance(double d) => CurrentUser.Balance += d;
    }
}
