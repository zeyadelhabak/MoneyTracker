namespace MoneyTracker.Models
{
    public enum TxType { Income = 1, Expense = 2 }
    public enum TxCategory
    {
        Food=1, Transport=2, Shopping=3, Bills=4,
        Entertainment=5, Salary=6, Investment=7, Other=8
    }
    public enum AppTheme { Dark, Light }
    public enum Currency { USD, EUR, GBP, EGP, SAR }
}
