using System;
using System.Collections.Generic;
using System.Linq;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class MonthlySummary
    {
        public string Label   { get; set; } = "";
        public double Income  { get; set; }
        public double Expense { get; set; }
        public double Net     => Income - Expense;
        public double Savings => Net > 0 ? Net : 0;
        public int    Count   { get; set; }
    }

    public static class AnalyticsService
    {
        private static List<Transaction> All() => TransactionService.GetAll();

        public static double TotalIncome()
            => All().Where(t => t.Type == TxType.Income).Sum(t => t.Amount);
        public static double TotalExpenses()
            => All().Where(t => t.Type == TxType.Expense).Sum(t => t.Amount);

        public static double MonthIncome(DateTime m)
            => All().Where(t => t.Type == TxType.Income  && t.Date.Year==m.Year && t.Date.Month==m.Month).Sum(t=>t.Amount);
        public static double MonthExpenses(DateTime m)
            => All().Where(t => t.Type == TxType.Expense && t.Date.Year==m.Year && t.Date.Month==m.Month).Sum(t=>t.Amount);
        public static double WeekExpenses()
            => All().Where(t => t.Type == TxType.Expense && t.Date >= DateTime.Today.AddDays(-7)).Sum(t=>t.Amount);

        public static double SavingsRate()
        {
            double inc = TotalIncome();
            return inc == 0 ? 0 : Math.Max(0, (inc - TotalExpenses()) / inc * 100);
        }

        public static double BudgetUsedPercent(DateTime m)
        {
            double budget = Session.CurrentUser.Budget;
            if (budget <= 0) return 0;
            return Math.Min(100, MonthExpenses(m) / budget * 100);
        }

        public static Dictionary<string, double> ByCategory(TxType type = TxType.Expense)
        {
            var d = new Dictionary<string, double>();
            foreach (var t in All().Where(t => t.Type == type))
            {
                if (!d.ContainsKey(t.CatLabel)) d[t.CatLabel] = 0;
                d[t.CatLabel] += t.Amount;
            }
            return d.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        public static List<MonthlySummary> GetMonthlySummaries(int months = 6)
        {
            var result = new List<MonthlySummary>();
            var all    = All();
            var now    = DateTime.Today;
            for (int i = months - 1; i >= 0; i--)
            {
                var m   = now.AddMonths(-i);
                var inc = all.Where(t => t.Type==TxType.Income  && t.Date.Year==m.Year && t.Date.Month==m.Month).Sum(t=>t.Amount);
                var exp = all.Where(t => t.Type==TxType.Expense && t.Date.Year==m.Year && t.Date.Month==m.Month).Sum(t=>t.Amount);
                var cnt = all.Count(t => t.Date.Year==m.Year && t.Date.Month==m.Month);
                result.Add(new MonthlySummary { Label=m.ToString("MMM yy"), Income=inc, Expense=exp, Count=cnt });
            }
            return result;
        }

        public static int CountThisMonth()
        {
            var now = DateTime.Today;
            return All().Count(t => t.Date.Year==now.Year && t.Date.Month==now.Month);
        }
    }
}
