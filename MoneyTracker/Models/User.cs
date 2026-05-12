namespace MoneyTracker.Models
{
    public class User
    {
        public int      Id       { get; set; }
        public string   Username { get; set; } = "";
        public string   Password { get; set; } = "";
        public string   Email    { get; set; } = "";
        public double   Balance  { get; set; }
        public double   Budget   { get; set; } = 5000.0;
        public AppTheme Theme    { get; set; } = AppTheme.Dark;
        public Currency Currency { get; set; } = Currency.USD;

        public string CurrencySymbol
        {
            get
            {
                if (Currency == Currency.EUR) return "€";
                if (Currency == Currency.GBP) return "£";
                if (Currency == Currency.EGP) return "E£";
                if (Currency == Currency.SAR) return "SR";
                return "$";
            }
        }
    }
}
