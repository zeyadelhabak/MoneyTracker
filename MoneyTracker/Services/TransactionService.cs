using System;
using System.Collections.Generic;
using System.Linq;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public static class TransactionService
    {
        public static List<Transaction> GetAll() =>
            StorageService.LoadTransactions(false).OrderByDescending(t => t.Date).ToList();

        public static void Add(Transaction tx)
        {
            var all = StorageService.LoadTransactions(true);
            tx.Id   = all.Count > 0 ? all.Max(t => t.Id) + 1 : 1;
            all.Add(tx);
            StorageService.SaveTransactions(all);
            Session.UpdateBalance(tx.Type == TxType.Income ? tx.Amount : -tx.Amount);
            AuthService.SaveCurrentUser();
        }

        public static void Edit(Transaction updated)
        {
            var all = StorageService.LoadTransactions(true);
            var old = all.FirstOrDefault(t => t.Id == updated.Id);
            if (old == null) return;
            Session.UpdateBalance((old.Type == TxType.Income ? -old.Amount : old.Amount) +
                                  (updated.Type == TxType.Income ? updated.Amount : -updated.Amount));
            all[all.IndexOf(old)] = updated;
            StorageService.SaveTransactions(all);
            AuthService.SaveCurrentUser();
        }

        public static void Delete(int id)
        {
            var all = StorageService.LoadTransactions(true);
            var tx  = all.FirstOrDefault(t => t.Id == id);
            if (tx == null) return;
            tx.Deleted = true;
            StorageService.SaveTransactions(all);
            Session.UpdateBalance(tx.Type == TxType.Income ? -tx.Amount : tx.Amount);
            AuthService.SaveCurrentUser();
        }

        public static List<Transaction> Search(string kw = "",
            TxType? type = null, TxCategory? cat = null,
            DateTime? from = null, DateTime? to = null)
        {
            return GetAll().Where(t =>
                (string.IsNullOrEmpty(kw) ||
                 t.Description.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 t.Note.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (type == null || t.Type == type) &&
                (cat  == null || t.Category == cat) &&
                (from == null || t.Date >= from) &&
                (to   == null || t.Date <= to)
            ).ToList();
        }
    }
}
