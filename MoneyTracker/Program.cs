using System;
using System.Windows.Forms;
using MoneyTracker.Services;

namespace MoneyTracker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            StorageService.Init();
            Application.Run(new Forms.LoginForm());
        }
    }
}
