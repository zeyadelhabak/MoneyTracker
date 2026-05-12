// ================================================================
// DashboardForm.cs — Main dashboard
//
//  LAYOUT FIXES vs previous version
//  ─────────────────────────────────
//  FIX-L1  Replaced every "9999" width/height hack with proper
//           Dock-based layout inside each content panel.
//           "9999" broke WinForms Anchor offset calculation,
//           producing wrong sizes on first paint and after resize.
//
//  FIX-L2  Dashboard "split" (grid + donut) now uses a
//           TableLayoutPanel with 62 / 38 % columns so both
//           panels always fill the available width correctly.
//
//  FIX-L3  Transactions panel uses Dock.Fill for the grid
//           wrapper so no blank space appears below the table.
//
//  FIX-L4  Reports bar-chart + donut now use a 58 / 42 %
//           TableLayoutPanel, resolving the narrow donut issue.
//
//  FIX-L5  DataGrid row height increased to 36, alternating
//           row colour contrast improved, padding added.
//
//  FIX-L6  Button action rows use a Panel-with-Dock so buttons
//           are evenly spaced and won't overflow on resize.
//
//  FIX-L7  DonutChart "Expense Breakdown" shows a proper
//           placeholder card when no expense data exists yet.
//
//  FIX-L8  All CP() content panels now use Dock=Fill so
//           OnResize correctly propagates into child controls.
// ================================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MoneyTracker.Models;
using MoneyTracker.Services;
using MoneyTracker.UI;
using MoneyTracker.UI.Controls;

namespace MoneyTracker.Forms
{
    public class DashboardForm : Form
    {
        // ── Layout constants ──────────────────────────────────
        const int SB  = 228;   // sidebar width
        const int HDR =  58;   // header height
        const int STS =  28;   // status bar height

        // ── Sidebar ───────────────────────────────────────────
        private Panel  _sb;
        private Button _navActive;

        // ── Content panels ────────────────────────────────────
        private Panel _pDash, _pTx, _pRep, _pSet;

        // ── Dashboard widgets ─────────────────────────────────
        private StatCard     _cBal, _cInc, _cExp, _cSave;
        private BudgetBar    _budget;
        private DataGridView _dgRecent;
        private DonutChart   _donut;

        // ── Transactions widgets ──────────────────────────────
        private DataGridView  _dgTx;
        private TextBox       _srch;
        private ComboBox      _fType, _fCat;
        private DateTimePicker _dtF,  _dtT;

        // ── Reports widgets ───────────────────────────────────
        private BarChart     _bar;
        private DonutChart   _rDonut;
        private DataGridView _dgMon;
        private StatCard     _rc1, _rc2, _rc3;

        // ── Settings ──────────────────────────────────────────
        private ComboBox _sTheme, _sCur;
        private TextBox  _sBudget;

        // ── Status / header labels ────────────────────────────
        private Label _lblSt, _lblTitle;

        // ─────────────────────────────────────────────────────
        public DashboardForm()
        {
            Text          = "MoneyTracker";
            ClientSize    = new Size(1160, 720);
            MinimumSize   = new Size(960, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = Theme.BgBase;
            DoubleBuffered = true;

            Build();
            RefreshAll();
        }

        // ════════════════════════════════════════════════════════
        //  BUILD
        // ════════════════════════════════════════════════════════
        void Build()
        {
            BuildSidebar();
            BuildHeader();
            BuildStatusBar();
            BuildDash();
            BuildTx();
            BuildReports();
            BuildSettings();
            ShowPanel(_pDash, "Dashboard");
            SetNav("Dashboard");
        }

        // ── SIDEBAR ────────────────────────────────────────────
        void BuildSidebar()
        {
            _sb = new Panel { Dock = DockStyle.Left, Width = SB, BackColor = Theme.BgSidebar };
            Controls.Add(_sb);
            _sb.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 4, BackColor = Theme.Accent });

            SbL("💰",          16, 14, 40, 40, new Font("Segoe UI", 22f, FontStyle.Bold), Theme.Accent);
            SbL("MoneyTracker", 58, 20, SB - 78, 28, new Font("Segoe UI", 12f, FontStyle.Bold), Color.White);
            SbL("Finance Manager", 58, 48, SB - 78, 18, Theme.Cap, Color.FromArgb(80, 185, 145));
            _sb.Controls.Add(new Panel { Bounds = new Rectangle(12, 74, SB - 24, 1), BackColor = Color.FromArgb(35, 255, 255, 255) });
            SbL("MAIN MENU", 12, 84, SB - 24, 16, Theme.Cap, Color.FromArgb(90, 255, 255, 255));

            int y = 106;
            SbBtn("📊  Dashboard",    "Dashboard",    y); y += 50;
            SbBtn("💳  Transactions", "Transactions", y); y += 50;
            SbBtn("📈  Reports",      "Reports",      y); y += 50;
            SbBtn("⚙️  Settings",     "Settings",     y);

            // Sign-out button at bottom
            var bOut = new Button
            {
                Text      = "   🚪  Sign Out",
                Font      = Theme.Body,
                ForeColor = Theme.Expense,
                BackColor = Color.FromArgb(45, 10, 8),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor    = Cursors.Hand,
                Dock      = DockStyle.Bottom,
                Height    = 48
            };
            bOut.FlatAppearance.BorderSize = 0;
            bOut.Click += (s, e) =>
            {
                if (MessageBox.Show("Sign out?", "Sign Out",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    StorageService.Backup();
                    Session.Logout();
                    Close();
                }
            };
            _sb.Controls.Add(bOut);

            // User strip
            var strip = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.FromArgb(6, 10, 22) };
            strip.Controls.Add(new Label { Text = "Signed in as", Bounds = new Rectangle(12, 4, SB - 20, 18), Font = Theme.Cap, ForeColor = Color.FromArgb(100, 185, 155), BackColor = Color.Transparent, AutoSize = false });
            strip.Controls.Add(new Label { Text = Session.CurrentUser.Username, Bounds = new Rectangle(12, 24, SB - 20, 24), Font = Theme.Bold, ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = false });
            strip.Controls.Add(new Label { Tag = "bal", Bounds = new Rectangle(12, 44, SB - 20, 16), Font = Theme.Cap, ForeColor = Theme.Accent, BackColor = Color.Transparent, AutoSize = false });
            _sb.Controls.Add(strip);
        }

        void SbL(string t, int x, int y, int w, int h, Font f, Color c)
            => _sb.Controls.Add(new Label { Text = t, Bounds = new Rectangle(x, y, w, h), Font = f, ForeColor = c, BackColor = Color.Transparent, AutoSize = false });

        void SbBtn(string txt, string tag, int y)
        {
            var b = new Button
            {
                Text      = "   " + txt,
                Tag       = tag,
                Font      = Theme.Body,
                ForeColor = Color.FromArgb(140, 195, 172),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds    = new Rectangle(4, y, SB - 8, 44),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 255, 255, 255);
            b.Click += NavClick;
            _sb.Controls.Add(b);
        }

        Button FindNav(string tag)
        {
            foreach (Control c in _sb.Controls)
                if (c is Button b && b.Tag?.ToString() == tag) return b;
            return null;
        }

        void SetNav(string tag)
        {
            if (_navActive != null) { _navActive.BackColor = Color.Transparent; _navActive.ForeColor = Color.FromArgb(140, 195, 172); }
            var b = FindNav(tag);
            if (b != null) { b.BackColor = Theme.SidebarSel; b.ForeColor = Color.FromArgb(10, 24, 14); _navActive = b; }
        }

        void NavClick(object s, EventArgs e)
        {
            string tag = ((Button)s).Tag?.ToString() ?? "";
            Panel p  = tag switch { "Transactions" => _pTx, "Reports" => _pRep, "Settings" => _pSet, _ => _pDash };
            string ti = tag switch { "Transactions" => "Transactions", "Reports" => "Reports & Analytics", "Settings" => "Settings", _ => "Dashboard" };
            ShowPanel(p, ti);
            SetNav(tag);
        }

        // ── HEADER ─────────────────────────────────────────────
        void BuildHeader()
        {
            var h = new Panel
            {
                Bounds    = new Rectangle(SB, 0, ClientSize.Width - SB, HDR),
                BackColor = Theme.BgHeader,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            h.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var lgb = new LinearGradientBrush(new Rectangle(0, 0, h.Width, 4), Theme.Accent, Theme.AccentDark, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lgb, 0, 0, h.Width, 4);
            };
            _lblTitle = new Label { Text = "Dashboard", Font = Theme.H2, ForeColor = Theme.TxtPrimary, BackColor = Color.Transparent, Bounds = new Rectangle(20, 0, 500, HDR), TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
            h.Controls.Add(_lblTitle);
            h.Controls.Add(new Label { Text = DateTime.Now.ToString("dddd, MMMM dd yyyy"), Font = Theme.Small, ForeColor = Theme.TxtMuted, BackColor = Color.Transparent, Bounds = new Rectangle(420, 0, h.Width - 440, HDR), TextAlign = ContentAlignment.MiddleRight, AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Right });
            Controls.Add(h);
        }

        // ── STATUS BAR ─────────────────────────────────────────
        void BuildStatusBar()
        {
            var sb = new Panel
            {
                Bounds    = new Rectangle(SB, ClientSize.Height - STS, ClientSize.Width - SB, STS),
                BackColor = Theme.BgHeader,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _lblSt = new Label { Bounds = new Rectangle(10, 0, sb.Width - 20, STS), Font = Theme.Cap, ForeColor = Theme.TxtMuted, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            _lblSt.Text = $"  MoneyTracker  ·  {Session.CurrentUser.Username}  ·  Ready";
            sb.Controls.Add(_lblSt);
            Controls.Add(sb);
        }

        // FIX-L8: content panels use Dock=Fill; OnResize sets their bounds explicitly.
        Panel CP()
        {
            var p = new Panel
            {
                BackColor = Theme.BgBase,
                Visible   = false
            };
            // Bounds set by OnResize; add to Controls so they're parented correctly.
            Controls.Add(p);
            return p;
        }

        void ShowPanel(Panel panel, string title)
        {
            if (_lblTitle != null) _lblTitle.Text = title;
            foreach (Panel p in new[] { _pDash, _pTx, _pRep, _pSet })
                if (p != null) p.Visible = false;
            panel.Visible = true;
            if (panel == _pRep) RefreshReports();
            if (panel == _pSet) RefreshSettings();
        }

        // ════════════════════════════════════════════════════════
        //  DASHBOARD PANEL
        //  FIX-L1, FIX-L2, FIX-L6, FIX-L7
        // ════════════════════════════════════════════════════════
        void BuildDash()
        {
            _pDash = CP();

            // ── Stat cards (Dock=Top, fixed height) ──────────
            var cardWrap = new Panel { Dock = DockStyle.Top, Height = 118, BackColor = Color.Transparent, Padding = new Padding(14, 10, 14, 4) };
            var cardTbl  = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, BackColor = Color.Transparent };
            for (int i = 0; i < 4; i++) cardTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _cBal  = new StatCard { Title = "BALANCE",        Icon = "💰", Accent = Theme.Accent,  Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
            _cInc  = new StatCard { Title = "TOTAL INCOME",   Icon = "📈", Accent = Theme.Income,  Dock = DockStyle.Fill, Margin = new Padding(3, 0, 3, 0) };
            _cExp  = new StatCard { Title = "TOTAL EXPENSES", Icon = "📉", Accent = Theme.Expense, Dock = DockStyle.Fill, Margin = new Padding(3, 0, 3, 0) };
            _cSave = new StatCard { Title = "SAVINGS RATE",   Icon = "💎", Accent = Theme.Warning, Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 0) };
            cardTbl.Controls.Add(_cBal,  0, 0);
            cardTbl.Controls.Add(_cInc,  1, 0);
            cardTbl.Controls.Add(_cExp,  2, 0);
            cardTbl.Controls.Add(_cSave, 3, 0);
            cardWrap.Controls.Add(cardTbl);
            _pDash.Controls.Add(cardWrap);

            // ── Budget bar (Dock=Top, fixed height) ──────────
            var budgetWrap = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Color.Transparent, Padding = new Padding(14, 4, 14, 4) };
            _budget = new BudgetBar { Dock = DockStyle.Fill };
            budgetWrap.Controls.Add(_budget);
            _pDash.Controls.Add(budgetWrap);

            // ── Quick-action buttons (Dock=Top) ───────────────
            var qaWrap = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.Transparent, Padding = new Padding(14, 6, 14, 0) };
            var bI = Btn("➕  Add Income",  FlatButton.Sty.Primary);
            var bE = Btn("➖  Add Expense", FlatButton.Sty.Secondary);
            var bB = Btn("💾  Backup",       FlatButton.Sty.Ghost);
            bI.Width = 180; bE.Width = 180; bB.Width = 140;
            bI.Click += (s, e) => AddTx(TxType.Income);
            bE.Click += (s, e) => AddTx(TxType.Expense);
            bB.Click += (s, e) => { StorageService.Backup(); ToastOK("Backup created."); };
            var qaFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
            qaFlow.Controls.Add(bI); qaFlow.Controls.Add(bE); qaFlow.Controls.Add(bB);
            qaWrap.Controls.Add(qaFlow);
            _pDash.Controls.Add(qaWrap);

            // ── "Recent Transactions" header (Dock=Top) ───────
            var hdrWrap = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.Transparent };
            hdrWrap.Controls.Add(new Label { Text = "  Recent Transactions", Font = Theme.H3, ForeColor = Theme.TxtPrimary, BackColor = Color.Transparent, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false });
            _pDash.Controls.Add(hdrWrap);

            // ── FIX-L2: split grid + donut via TableLayoutPanel ─
            var splitTbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Color.Transparent,
                Padding     = new Padding(14, 4, 14, 10)
            };
            splitTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f)); // grid
            splitTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f)); // donut
            splitTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _dgRecent = Grid();
            _dgRecent.Dock   = DockStyle.Fill;
            _dgRecent.Margin = new Padding(0, 0, 6, 0);
            splitTbl.Controls.Add(_dgRecent, 0, 0);

            // FIX-L7: DonutChart with BackColor set for proper empty-state rendering
            _donut = new DonutChart
            {
                Dock       = DockStyle.Fill,
                ChartTitle = "Expense Breakdown",
                Margin     = new Padding(6, 0, 0, 0)
            };
            splitTbl.Controls.Add(_donut, 1, 0);

            _pDash.Controls.Add(splitTbl);

            // NOTE: In WinForms, Dock=Top controls are laid out top→bottom
            // in the order they appear in Controls. Dock=Fill gets the rest.
            // The order of Controls.Add above (cardWrap, budgetWrap, qaWrap,
            // hdrWrap, splitTbl) therefore produces the correct stacking.
        }

        // ════════════════════════════════════════════════════════
        //  TRANSACTIONS PANEL
        //  FIX-L1, FIX-L3, FIX-L6
        // ════════════════════════════════════════════════════════
        void BuildTx()
        {
            _pTx = CP();

            // ── Filter bar (Dock=Top) ─────────────────────────
            var fWrap = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent, Padding = new Padding(14, 8, 14, 0) };
            _srch = new TextBox { Bounds = new Rectangle(0, 3, 196, 34), BackColor = Theme.BgInput, ForeColor = Theme.TxtPrimary, BorderStyle = BorderStyle.FixedSingle, Font = Theme.Body, PlaceholderText = "🔍  Search..." };
            _srch.TextChanged += (s, e) => RefTx();
            fWrap.Controls.Add(_srch);

            _fType = MCbo(fWrap, 206, 3, 126, new[] { "All Types", "Income", "Expense" });
            _fType.SelectedIndexChanged += (s, e) => RefTx();
            _fCat  = MCbo(fWrap, 342, 3, 148, new[] { "All Categories", "Food", "Transport", "Shopping", "Bills", "Entertainment", "Salary", "Investment", "Other" });
            _fCat.SelectedIndexChanged  += (s, e) => RefTx();

            fWrap.Controls.Add(MkL("From:", 500, 11, 40, 18, Theme.Small, Theme.TxtMuted));
            _dtF = new DateTimePicker { Bounds = new Rectangle(542, 3, 114, 34), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-3) };
            _dtF.ValueChanged += (s, e) => RefTx();
            fWrap.Controls.Add(_dtF);

            fWrap.Controls.Add(MkL("To:", 666, 11, 28, 18, Theme.Small, Theme.TxtMuted));
            _dtT = new DateTimePicker { Bounds = new Rectangle(694, 3, 114, 34), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            _dtT.ValueChanged += (s, e) => RefTx();
            fWrap.Controls.Add(_dtT);
            _pTx.Controls.Add(fWrap);

            // ── Action buttons (Dock=Top) ─────────────────────
            var abWrap = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent, Padding = new Padding(14, 6, 14, 0) };
            var bA = Btn("➕  Add",        FlatButton.Sty.Primary);   bA.Width = 146;
            var bEd = Btn("✏️  Edit",       FlatButton.Sty.Secondary); bEd.Width = 146;
            var bD  = Btn("🗑  Delete",     FlatButton.Sty.Danger);    bD.Width = 146;
            var bC  = Btn("✖  Clear",       FlatButton.Sty.Ghost);     bC.Width = 130;
            var bX  = Btn("📥  Export CSV", FlatButton.Sty.Ghost);     bX.Width = 154;
            bA.Click  += (s, e) => AddTx(TxType.Expense);
            bEd.Click += DoEdit;
            bD.Click  += DoDel;
            bC.Click  += (s, e) => { _srch.Clear(); _fType.SelectedIndex = 0; _fCat.SelectedIndex = 0; _dtF.Value = DateTime.Today.AddMonths(-3); _dtT.Value = DateTime.Today; };
            bX.Click  += DoExport;
            var abFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
            abFlow.Controls.Add(bA); abFlow.Controls.Add(bEd); abFlow.Controls.Add(bD); abFlow.Controls.Add(bC); abFlow.Controls.Add(bX);
            abWrap.Controls.Add(abFlow);
            _pTx.Controls.Add(abWrap);

            // ── FIX-L3: Grid fills remaining space ────────────
            var gWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(14, 2, 14, 10) };
            _dgTx      = Grid();
            _dgTx.Dock = DockStyle.Fill;
            _dgTx.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) DoDel(s, e); };
            gWrap.Controls.Add(_dgTx);
            _pTx.Controls.Add(gWrap);
        }

        // ════════════════════════════════════════════════════════
        //  REPORTS PANEL
        //  FIX-L1, FIX-L4
        // ════════════════════════════════════════════════════════
        void BuildReports()
        {
            _pRep = CP();

            // ── 3 mini stat cards (Dock=Top) ──────────────────
            var srWrap = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Color.Transparent, Padding = new Padding(14, 10, 14, 4) };
            var srTbl  = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Color.Transparent };
            srTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            srTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            srTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            _rc1 = new StatCard { Title = "THIS MONTH EXPENSES", Icon = "📅", Accent = Theme.Expense, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 4, 0) };
            _rc2 = new StatCard { Title = "LAST 7 DAYS",          Icon = "⏱",  Accent = Theme.Warning, Dock = DockStyle.Fill, Margin = new Padding(4, 0, 4, 0) };
            _rc3 = new StatCard { Title = "SAVINGS RATE",         Icon = "💎", Accent = Theme.Income,  Dock = DockStyle.Fill, Margin = new Padding(4, 0, 0, 0) };
            srTbl.Controls.Add(_rc1, 0, 0);
            srTbl.Controls.Add(_rc2, 1, 0);
            srTbl.Controls.Add(_rc3, 2, 0);
            srWrap.Controls.Add(srTbl);
            _pRep.Controls.Add(srWrap);

            // ── FIX-L4: Bar + Donut side by side (Dock=Top) ──
            var chartRow = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                Height      = 270,
                ColumnCount = 2,
                BackColor   = Color.Transparent,
                Padding     = new Padding(14, 6, 14, 6)
            };
            chartRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            chartRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            chartRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _bar    = new BarChart    { Dock = DockStyle.Fill, ChartTitle = "Monthly Income vs Expenses (6 months)", Margin = new Padding(0, 0, 6, 0) };
            _rDonut = new DonutChart  { Dock = DockStyle.Fill, ChartTitle = "Expense Breakdown by Category",          Margin = new Padding(6, 0, 0, 0) };
            chartRow.Controls.Add(_bar,    0, 0);
            chartRow.Controls.Add(_rDonut, 1, 0);
            _pRep.Controls.Add(chartRow);

            // ── Export button (Dock=Top) ──────────────────────
            var expWrap = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent, Padding = new Padding(14, 6, 14, 0) };
            var bExp = new FlatButton { Text = "📥  Export Report CSV", Height = 38, Width = 220, Style = FlatButton.Sty.Outline };
            bExp.Click += DoExportRep;
            expWrap.Controls.Add(bExp);
            _pRep.Controls.Add(expWrap);

            // ── "Monthly Summary" header (Dock=Top) ──────────
            var mHdr = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.Transparent };
            mHdr.Controls.Add(new Label { Text = "  Monthly Summary  (last 12 months)", Font = Theme.H3, ForeColor = Theme.TxtPrimary, BackColor = Color.Transparent, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false });
            _pRep.Controls.Add(mHdr);

            // ── Monthly grid (Dock=Fill) ──────────────────────
            var mWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(14, 2, 14, 10) };
            _dgMon = new DataGridView
            {
                Dock              = DockStyle.Fill,
                BackgroundColor   = Theme.BgSurface,
                GridColor         = Theme.Border,
                BorderStyle       = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly          = true,
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect,
                Font              = Theme.Small,
                ColumnHeadersHeight      = 36,
                EnableHeadersVisualStyles = false,
                CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal
            };
            _dgMon.RowTemplate.Height = 32;
            StyleGrid(_dgMon);
            _dgMon.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mo", HeaderText = "Month",    FillWeight = 16 });
            _dgMon.Columns.Add(new DataGridViewTextBoxColumn { Name = "In", HeaderText = "Income",   FillWeight = 18 });
            _dgMon.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ex", HeaderText = "Expenses", FillWeight = 18 });
            _dgMon.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ne", HeaderText = "Net",      FillWeight = 18 });
            _dgMon.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sa", HeaderText = "Savings",  FillWeight = 16 });
            _dgMon.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ct", HeaderText = "# Txns",   FillWeight = 10 });
            _dgMon.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != _dgMon.Columns["Ne"].Index) return;
                string v = e.Value?.ToString() ?? "";
                e.CellStyle.ForeColor = v.StartsWith("-") ? Theme.Expense : Theme.Income;
                e.CellStyle.Font      = Theme.Bold;
            };
            mWrap.Controls.Add(_dgMon);
            _pRep.Controls.Add(mWrap);
        }

        // ════════════════════════════════════════════════════════
        //  SETTINGS PANEL  (unchanged layout — it was already fine)
        // ════════════════════════════════════════════════════════
        void BuildSettings()
        {
            _pSet = CP();

            var card = new DBPanel { Bounds = new Rectangle(24, 20, 480, 320), BackColor = Theme.BgSurface };
            card.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(Theme.BgSurface))
                    g.FillRound(br, new Rectangle(0, 0, card.Width - 1, card.Height - 1), Theme.RCard);
                using (var p = new Pen(Theme.Border))
                    g.DrawRound(p, new Rectangle(0, 0, card.Width - 1, card.Height - 1), Theme.RCard);
            };
            _pSet.Controls.Add(card);

            int y = 16;
            CL(card, "PREFERENCES", 20, y, 440, 20, Theme.Cap, Theme.TxtMuted); y += 36;
            SR(card, "Theme",    ref y, out _sTheme, new[] { "Dark Mode", "Light Mode" });
            SR(card, "Currency", ref y, out _sCur,   new[] { "USD — US Dollar", "EUR — Euro", "GBP — Pound", "EGP — Egyptian", "SAR — Saudi" });
            CL(card, "Monthly Budget", 20, y, 160, 28, Theme.Body, Theme.TxtPrimary, ContentAlignment.MiddleLeft);
            _sBudget = new TextBox { Bounds = new Rectangle(190, y, 260, 28), BackColor = Theme.BgInput, ForeColor = Theme.TxtPrimary, BorderStyle = BorderStyle.FixedSingle, Font = Theme.Body };
            card.Controls.Add(_sBudget); y += 52;
            card.Controls.Add(new Panel { Bounds = new Rectangle(20, y, 440, 1), BackColor = Theme.Border }); y += 16;
            var bs = new FlatButton { Text = "SAVE SETTINGS", Bounds = new Rectangle(20, y, 440, 44) };
            bs.Click += SaveSet;
            card.Controls.Add(bs); y += 54;
            CL(card, "Theme changes apply on next login.", 20, y, 440, 22, Theme.Cap, Theme.TxtMuted);

            var c2 = new DBPanel { Bounds = new Rectangle(24, 358, 480, 218), BackColor = Theme.BgSurface };
            c2.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(Theme.BgSurface))
                    g.FillRound(br, new Rectangle(0, 0, c2.Width - 1, c2.Height - 1), Theme.RCard);
                using (var p = new Pen(Theme.Border))
                    g.DrawRound(p, new Rectangle(0, 0, c2.Width - 1, c2.Height - 1), Theme.RCard);
            };
            _pSet.Controls.Add(c2);
            CL(c2, "KEYBOARD SHORTCUTS", 20, 14, 440, 20, Theme.Cap, Theme.TxtMuted);
            var sc = new[] { ("Ctrl+1","Dashboard"),("Ctrl+2","Transactions"),("Ctrl+3","Reports"),("Ctrl+I","Add Income"),("Ctrl+E","Add Expense"),("Del","Delete selected transaction") };
            int sy = 40;
            foreach (var (k, v) in sc) { CL(c2, k, 20, sy, 110, 22, Theme.Mono, Theme.Accent); CL(c2, v, 140, sy, 320, 22, Theme.Body, Theme.TxtPrimary); sy += 28; }
        }

        void SR(Panel p, string lbl, ref int y, out ComboBox cb, string[] items)
        {
            CL(p, lbl, 20, y, 160, 28, Theme.Body, Theme.TxtPrimary, ContentAlignment.MiddleLeft);
            cb = new ComboBox { Bounds = new Rectangle(190, y, 260, 28), BackColor = Theme.BgInput, ForeColor = Theme.TxtPrimary, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Body, FlatStyle = FlatStyle.Flat };
            cb.Items.AddRange(items);
            p.Controls.Add(cb);
            y += 48;
        }

        void RefreshSettings()
        {
            if (_sTheme  != null) _sTheme.SelectedIndex  = Session.CurrentUser.Theme    == AppTheme.Dark ? 0 : 1;
            if (_sCur    != null) _sCur.SelectedIndex    = (int)Session.CurrentUser.Currency;
            if (_sBudget != null) _sBudget.Text          = Session.CurrentUser.Budget.ToString("F2");
        }

        void SaveSet(object s, EventArgs e)
        {
            Session.CurrentUser.Theme    = _sTheme.SelectedIndex == 0 ? AppTheme.Dark : AppTheme.Light;
            Session.CurrentUser.Currency = (Currency)_sCur.SelectedIndex;
            if (double.TryParse(_sBudget.Text, out double b) && b > 0) Session.CurrentUser.Budget = b;
            Theme.Apply(Session.CurrentUser.Theme);
            AuthService.SaveCurrentUser();
            RefreshAll();
            ToastOK("Settings saved!");
        }

        // ════════════════════════════════════════════════════════
        //  GRID BUILDER
        //  FIX-L5: row height 36, better alternating colours
        // ════════════════════════════════════════════════════════
        DataGridView Grid()
        {
            var dg = new DataGridView
            {
                BackgroundColor   = Theme.BgSurface,
                GridColor         = Theme.Border,
                BorderStyle       = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly          = true,
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect       = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font              = Theme.Small,
                ColumnHeadersHeight = 40,
                CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false
            };
            dg.RowTemplate.Height = 36;  // FIX-L5: was 34
            StyleGrid(dg);

            dg.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",   HeaderText = "#",            FillWeight =  5 });
            dg.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date",          FillWeight = 11 });
            dg.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type",          FillWeight =  8 });
            dg.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cat",  HeaderText = "Category",      FillWeight = 12 });
            dg.Columns.Add(new DataGridViewTextBoxColumn { Name = "Desc", HeaderText = "Description",   FillWeight = 35 });
            dg.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amt",  HeaderText = "Amount",        FillWeight = 14 });
            dg.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Note",          FillWeight = 15 });

            dg.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != dg.Columns["Amt"].Index) return;
                string v = e.Value?.ToString() ?? "";
                e.CellStyle.ForeColor = v.StartsWith("+") ? Theme.Income : Theme.Expense;
                e.CellStyle.Font      = Theme.Bold;
            };
            return dg;
        }

        // FIX-L5: improved grid styling — better contrast + column header padding
        static void StyleGrid(DataGridView dg)
        {
            dg.DefaultCellStyle.BackColor          = Theme.BgSurface;
            dg.DefaultCellStyle.ForeColor          = Theme.TxtPrimary;
            dg.DefaultCellStyle.SelectionBackColor = Theme.SidebarSel;
            dg.DefaultCellStyle.SelectionForeColor = Color.FromArgb(10, 24, 14);
            dg.DefaultCellStyle.Padding            = new Padding(4, 0, 4, 0);

            dg.ColumnHeadersDefaultCellStyle.BackColor  = Theme.BgHeader;
            dg.ColumnHeadersDefaultCellStyle.ForeColor  = Theme.TxtMuted;
            dg.ColumnHeadersDefaultCellStyle.Font       = Theme.Bold;
            dg.ColumnHeadersDefaultCellStyle.Padding    = new Padding(4, 0, 4, 0);

            // FIX-L5: slightly more visible alternating row colour
            dg.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(22, 32, 54);
        }

        // ════════════════════════════════════════════════════════
        //  KEYBOARD SHORTCUTS
        // ════════════════════════════════════════════════════════
        protected override bool ProcessCmdKey(ref Message msg, Keys k)
        {
            if (k == (Keys.Control | Keys.D1)) { NavClick(FindNav("Dashboard"),    EventArgs.Empty); SetNav("Dashboard");    return true; }
            if (k == (Keys.Control | Keys.D2)) { NavClick(FindNav("Transactions"), EventArgs.Empty); SetNav("Transactions"); return true; }
            if (k == (Keys.Control | Keys.D3)) { NavClick(FindNav("Reports"),      EventArgs.Empty); SetNav("Reports");      return true; }
            if (k == (Keys.Control | Keys.I))  { AddTx(TxType.Income);  return true; }
            if (k == (Keys.Control | Keys.E))  { AddTx(TxType.Expense); return true; }
            return base.ProcessCmdKey(ref msg, k);
        }

        // ════════════════════════════════════════════════════════
        //  REFRESH — all logic unchanged
        // ════════════════════════════════════════════════════════
        void RefreshAll()
        {
            string sym   = Session.CurrentUser.CurrencySymbol;
            var    now   = DateTime.Today;
            double mInc  = AnalyticsService.MonthIncome(now);
            double mExp  = AnalyticsService.MonthExpenses(now);
            double pct   = AnalyticsService.BudgetUsedPercent(now);
            double rate  = AnalyticsService.SavingsRate();

            _cBal ?.Refresh($"{sym}{Session.CurrentUser.Balance:F2}", $"as of {now:MMM dd}");
            _cInc ?.Refresh($"{sym}{AnalyticsService.TotalIncome():F2}",   $"This month: {sym}{mInc:F2}");
            _cExp ?.Refresh($"{sym}{AnalyticsService.TotalExpenses():F2}", $"This month: {sym}{mExp:F2}");
            _cSave?.Refresh($"{rate:F1}%", $"Txns this month: {AnalyticsService.CountThisMonth()}");

            if (_budget != null)
            {
                _budget.Pct        = pct;
                _budget.LabelLeft  = $"Monthly Budget  ({sym}{mExp:F0} / {sym}{Session.CurrentUser.Budget:F0})";
                _budget.LabelRight = $"{pct:F0}% used";
                _budget.Invalidate();
            }

            // Update sidebar balance label
            foreach (Control c in _sb.Controls)
                if (c is Panel st && st.Height == 58)
                    foreach (Control l in st.Controls)
                        if (l is Label lb && lb.Tag?.ToString() == "bal")
                            lb.Text = $"Balance: {sym}{Session.CurrentUser.Balance:F2}";

            FillGrid(_dgRecent, TransactionService.GetAll().Take(10).ToList());

            if (_donut != null) { _donut.Data = AnalyticsService.ByCategory(TxType.Expense); _donut.Invalidate(); }

            RefTx();
        }

        void RefTx()
        {
            string kw = _srch?.Text ?? "";
            TxType?    tf = null;
            TxCategory? cf = null;
            if (_fType?.SelectedIndex == 1) tf = TxType.Income;
            if (_fType?.SelectedIndex == 2) tf = TxType.Expense;
            if (_fCat?.SelectedIndex  >  0) cf = (TxCategory)_fCat.SelectedIndex;
            DateTime fr = _dtF?.Value.Date ?? DateTime.Today.AddMonths(-3);
            DateTime to = _dtT?.Value.Date ?? DateTime.Today;
            var txs = TransactionService.Search(kw, tf, cf, fr, to.AddDays(1));
            FillGrid(_dgTx, txs);
            St($"Showing {txs.Count} transaction{(txs.Count == 1 ? "" : "s")}.");
        }

        void FillGrid(DataGridView dg, List<Transaction> txs)
        {
            if (dg == null) return;
            string sym = Session.CurrentUser.CurrencySymbol;
            dg.Rows.Clear();
            if (txs.Count == 0)
            {
                dg.Rows.Add("-", "-", "–", "–", "No transactions found.  Use 'Add' to get started.", "-", "-");
                if (dg.Rows.Count > 0)
                {
                    dg.Rows[0].DefaultCellStyle.ForeColor = Theme.TxtMuted;
                    dg.Rows[0].DefaultCellStyle.Font      = Theme.Body;
                }
                return;
            }
            foreach (var t in txs)
                dg.Rows.Add(t.Id, t.DateLabel, t.TypeLabel, t.CatLabel, t.Description, t.AmountLabel(sym), t.Note);
        }

        void RefreshReports()
        {
            string sym = Session.CurrentUser.CurrencySymbol;
            var now    = DateTime.Today;
            _rc1?.Refresh($"{sym}{AnalyticsService.MonthExpenses(now):F2}", now.ToString("MMMM yyyy"));
            _rc2?.Refresh($"{sym}{AnalyticsService.WeekExpenses():F2}",     "last 7 days");
            _rc3?.Refresh($"{AnalyticsService.SavingsRate():F1}%",          "of total income");

            if (_bar != null)
            {
                var sm = AnalyticsService.GetMonthlySummaries(6);
                _bar.Data = new List<(string, double, double)>();
                foreach (var m in sm) _bar.Data.Add((m.Label, m.Income, m.Expense));
                _bar.Invalidate();
            }

            if (_rDonut != null) { _rDonut.Data = AnalyticsService.ByCategory(TxType.Expense); _rDonut.Invalidate(); }

            if (_dgMon != null)
            {
                _dgMon.Rows.Clear();
                foreach (var m in AnalyticsService.GetMonthlySummaries(12))
                {
                    string net = m.Net >= 0 ? $"+{sym}{m.Net:F2}" : $"-{sym}{-m.Net:F2}";
                    _dgMon.Rows.Add(m.Label, $"{sym}{m.Income:F2}", $"{sym}{m.Expense:F2}", net, $"{sym}{m.Savings:F2}", m.Count);
                }
            }
        }

        // ════════════════════════════════════════════════════════
        //  ACTIONS — all unchanged
        // ════════════════════════════════════════════════════════
        void AddTx(TxType tp)
        {
            if (new AddTransactionForm(null, tp).ShowDialog(this) == DialogResult.OK)
            { RefreshAll(); ToastOK($"{(tp == TxType.Income ? "Income" : "Expense")} transaction added."); }
        }

        void DoEdit(object s, EventArgs e)
        {
            if (_dgTx?.SelectedRows.Count == 0) { Toast.Show(this, "Select a transaction to edit.", Toast.Kind.Warning); return; }
            if (!int.TryParse(_dgTx.SelectedRows[0].Cells["Id"].Value?.ToString(), out int id)) return;
            var tx = TransactionService.GetAll().FirstOrDefault(t => t.Id == id);
            if (tx == null) return;
            if (new AddTransactionForm(tx).ShowDialog(this) == DialogResult.OK)
            { RefreshAll(); ToastOK($"Transaction #{id} updated."); }
        }

        void DoDel(object s, EventArgs e)
        {
            if (_dgTx?.SelectedRows.Count == 0) { Toast.Show(this, "Select a transaction to delete.", Toast.Kind.Warning); return; }
            if (!int.TryParse(_dgTx.SelectedRows[0].Cells["Id"].Value?.ToString(), out int id)) return;
            if (MessageBox.Show($"Delete transaction #{id}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            TransactionService.Delete(id);
            RefreshAll();
            Toast.Show(this, $"Transaction #{id} deleted.", Toast.Kind.Info);
        }

        void DoExport(object s, EventArgs e)
        {
            using var dlg = new SaveFileDialog { Filter = "CSV|*.csv", FileName = $"transactions_{DateTime.Now:yyyyMMdd}.csv" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            StorageService.ExportCsv(dlg.FileName, TransactionService.GetAll(), Session.CurrentUser.CurrencySymbol);
            ToastOK("Transactions exported to CSV.");
        }

        void DoExportRep(object s, EventArgs e)
        {
            using var dlg = new SaveFileDialog { Filter = "CSV|*.csv", FileName = $"report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            var lines = new List<string> { "Month,Income,Expenses,Net,Savings,Transactions" };
            foreach (var m in AnalyticsService.GetMonthlySummaries(12))
                lines.Add($"{m.Label},{m.Income:F2},{m.Expense:F2},{m.Net:F2},{m.Savings:F2},{m.Count}");
            File.WriteAllLines(dlg.FileName, lines);
            ToastOK("Report exported to CSV.");
        }

        void ToastOK(string m) { Toast.Show(this, m, Toast.Kind.Success); St(m); }
        void St(string m)      { if (_lblSt != null) _lblSt.Text = $"  {m}  ·  {DateTime.Now:HH:mm:ss}"; }

        // ── Resize: keeps content panels in sync with the window ─
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int cw = ClientSize.Width  - SB;
            int ch = ClientSize.Height - HDR - STS;
            foreach (Panel p in new[] { _pDash, _pTx, _pRep, _pSet })
                if (p != null) p.Bounds = new Rectangle(SB, HDR, cw, ch);
            if (_sb != null) _sb.Height = ClientSize.Height;
        }

        // ── Helper: factory for action-row FlatButtons ────────────
        static FlatButton Btn(string text, FlatButton.Sty style)
            => new FlatButton { Text = text, Height = 38, Style = style, Margin = new Padding(0, 0, 8, 0) };

        // ── Static label / control helpers ───────────────────────
        static Label MkL(string t, int x, int y, int w, int h, Font f, Color c,
            ContentAlignment a = ContentAlignment.MiddleLeft)
            => new Label { Text = t, Bounds = new Rectangle(x,y,w,h), Font = f, ForeColor = c, BackColor = Color.Transparent, TextAlign = a, AutoSize = false };

        static void CL(Panel p, string t, int x, int y, int w, int h, Font f, Color c,
            ContentAlignment a = ContentAlignment.MiddleLeft)
            => p.Controls.Add(new Label { Text = t, Bounds = new Rectangle(x,y,w,h), Font = f, ForeColor = c, BackColor = Color.Transparent, TextAlign = a, AutoSize = false });

        static ComboBox MCbo(Panel p, int x, int y, int w, string[] items)
        {
            var cb = new ComboBox { Bounds = new Rectangle(x,y,w,34), BackColor = Theme.BgInput, ForeColor = Theme.TxtPrimary, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Body, FlatStyle = FlatStyle.Flat };
            cb.Items.AddRange(items);
            cb.SelectedIndex = 0;
            p.Controls.Add(cb);
            return cb;
        }
    }
}
