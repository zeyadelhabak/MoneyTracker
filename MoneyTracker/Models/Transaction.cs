using System;

namespace MoneyTracker.Models
{
    public class Transaction
    {
        public int        Id          { get; set; }
        public TxType     Type        { get; set; }
        public TxCategory Category    { get; set; }
        public double     Amount      { get; set; }
        public string     Description { get; set; } = "";
        public DateTime   Date        { get; set; } = DateTime.Today;
        public bool       Deleted     { get; set; }
        public string     Note        { get; set; } = "";

        public string TypeLabel  => Type == TxType.Income ? "Income" : "Expense";
        public string CatLabel   => Category.ToString();
        public string DateLabel  => Date.ToString("yyyy-MM-dd");

        public string AmountLabel(string sym)
            => Type == TxType.Income ? $"+{sym}{Amount:F2}" : $"-{sym}{Amount:F2}";
    }
}
