using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public static class StorageService
    {
        public const  string DataDir   = "data";
        public const  string UsersFile = @"data\users.csv";
        public static string TxFile    => $@"data\tx_{Session.CurrentUser.Username}.csv";
        public static string BackupDir => @"data\backups";

        public static void Init()
        {
            Directory.CreateDirectory(DataDir);
            Directory.CreateDirectory(BackupDir);
        }

        // ── Users ─────────────────────────────────────────────
        public static List<User> LoadUsers()
        {
            Init();
            var list = new List<User>();
            if (!File.Exists(UsersFile)) return list;
            foreach (var line in File.ReadAllLines(UsersFile))
            {
                var p = line.Split('|');
                if (p.Length < 7) continue;
                double budget = p.Length > 7 && double.TryParse(p[7], out double b) ? b : 5000.0;
                list.Add(new User
                {
                    Id       = int.Parse(p[0]),
                    Username = p[1],
                    Password = p[2],
                    Email    = p[3],
                    Balance  = double.Parse(p[4]),
                    Theme    = Enum.TryParse(p[5], out AppTheme t) ? t : AppTheme.Dark,
                    Currency = Enum.TryParse(p[6], out Currency c) ? c : Currency.USD,
                    Budget   = budget
                });
            }
            return list;
        }

        public static void SaveUsers(List<User> users)
        {
            Init();
            File.WriteAllLines(UsersFile,
                users.Select(u =>
                    $"{u.Id}|{u.Username}|{u.Password}|{u.Email}|{u.Balance:F2}|{u.Theme}|{u.Currency}|{u.Budget:F2}"));
        }

        // ── Transactions ──────────────────────────────────────
        public static List<Transaction> LoadTransactions(bool includeDeleted = false)
        {
            Init();
            var list = new List<Transaction>();
            if (!File.Exists(TxFile)) return list;
            foreach (var line in File.ReadAllLines(TxFile))
            {
                var p = line.Split('|');
                if (p.Length < 8) continue;
                bool deleted = p[7] == "1";
                if (deleted && !includeDeleted) continue;
                list.Add(new Transaction
                {
                    Id          = int.Parse(p[0]),
                    Type        = (TxType)int.Parse(p[1]),
                    Category    = (TxCategory)int.Parse(p[2]),
                    Amount      = double.Parse(p[3]),
                    Description = p[4],
                    Date        = DateTime.Parse(p[5]),
                    Note        = p.Length > 6 ? p[6] : "",
                    Deleted     = deleted
                });
            }
            return list;
        }

        public static void SaveTransactions(List<Transaction> list)
        {
            Init();
            File.WriteAllLines(TxFile,
                list.Select(t =>
                    $"{t.Id}|{(int)t.Type}|{(int)t.Category}|{t.Amount:F2}|{t.Description}|{t.Date:yyyy-MM-dd}|{t.Note}|{(t.Deleted?1:0)}"));
        }

        public static void Backup()
        {
            Init();
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (File.Exists(UsersFile))
                File.Copy(UsersFile, Path.Combine(BackupDir, $"users_{stamp}.csv"), true);
            if (File.Exists(TxFile))
                File.Copy(TxFile, Path.Combine(BackupDir, $"tx_{Session.CurrentUser.Username}_{stamp}.csv"), true);
        }

        // ── CSV Export ────────────────────────────────────────
        public static void ExportCsv(string path, List<Transaction> txs, string sym)
        {
            var lines = new List<string> { "ID,Date,Type,Category,Description,Amount,Note" };
            foreach (var t in txs)
                lines.Add($"{t.Id},{t.DateLabel},{t.TypeLabel},{t.CatLabel},\"{t.Description}\",{t.AmountLabel(sym)},{t.Note}");
            File.WriteAllLines(path, lines);
        }
    }
}
