using System.Drawing;
using MoneyTracker.Models;

namespace MoneyTracker.UI
{
    public static class Theme
    {
        public static AppTheme Mode { get; private set; } = AppTheme.Dark;
        public static void Apply(AppTheme m) => Mode = m;

        public static Color BgBase    => Mode==AppTheme.Dark ? Color.FromArgb(8,12,24)   : Color.FromArgb(244,246,252);
        public static Color BgSurface => Mode==AppTheme.Dark ? Color.FromArgb(14,20,38)  : Color.FromArgb(255,255,255);
        public static Color BgCard    => Mode==AppTheme.Dark ? Color.FromArgb(18,26,48)  : Color.FromArgb(238,242,255);
        public static Color BgHeader  => Mode==AppTheme.Dark ? Color.FromArgb(11,16,32)  : Color.FromArgb(226,232,250);
        public static Color BgSidebar => Mode==AppTheme.Dark ? Color.FromArgb(9,13,26)   : Color.FromArgb(18,28,72);
        public static Color BgInput   => Mode==AppTheme.Dark ? Color.FromArgb(22,32,58)  : Color.FromArgb(248,250,255);
        public static Color TxtPrimary=> Mode==AppTheme.Dark ? Color.FromArgb(218,226,244): Color.FromArgb(16,22,48);
        public static Color TxtMuted  => Mode==AppTheme.Dark ? Color.FromArgb(100,118,158): Color.FromArgb(94,106,142);
        public static Color TxtHint   => Mode==AppTheme.Dark ? Color.FromArgb(60,80,120)  : Color.FromArgb(160,170,200);
        public static Color Border    => Mode==AppTheme.Dark ? Color.FromArgb(28,38,66)   : Color.FromArgb(210,218,238);

        public static readonly Color Accent     = Color.FromArgb(0,210,120);
        public static readonly Color AccentDark = Color.FromArgb(0,165,88);
        public static readonly Color SidebarSel = Color.FromArgb(0,155,90);
        public static readonly Color Income     = Color.FromArgb(0,195,108);
        public static readonly Color Expense    = Color.FromArgb(220,68,68);
        public static readonly Color Warning    = Color.FromArgb(255,175,42);
        public static readonly Color Info       = Color.FromArgb(58,140,230);
        public static readonly Color Success    = Color.FromArgb(0,198,112);
        public static readonly Color Danger     = Color.FromArgb(210,55,55);

        public static readonly Color[] Chart = {
            Color.FromArgb(0,195,108), Color.FromArgb(220,68,68),
            Color.FromArgb(58,140,230),Color.FromArgb(255,175,42),
            Color.FromArgb(168,88,200),Color.FromArgb(80,198,210),
            Color.FromArgb(255,118,78),Color.FromArgb(132,198,78)
        };

        public static readonly Font H1    = new Font("Segoe UI",22f,FontStyle.Bold);
        public static readonly Font H2    = new Font("Segoe UI",14f,FontStyle.Bold);
        public static readonly Font H3    = new Font("Segoe UI",10.5f,FontStyle.Bold);
        public static readonly Font Bold  = new Font("Segoe UI",9.5f,FontStyle.Bold);
        public static readonly Font Body  = new Font("Segoe UI",9.5f);
        public static readonly Font Small = new Font("Segoe UI",8.5f);
        public static readonly Font Cap   = new Font("Segoe UI",8.0f);
        public static readonly Font Mono  = new Font("Consolas",9.5f);

        public const int RCard = 8;
        public const int RBtn  = 7;
    }
}
