// =============================================================
// FlatButton.cs — Owner-drawn rounded button
//
//  TEXT-DOUBLING ROOT CAUSE & FIX
//  ────────────────────────────────
//  Two-part fix required:
//
//  Part A: SetStyle(UserPaint | AllPaintingInWmPaint | DoubleBuffer)
//    → WinForms no longer draws the Win32 button label at all.
//
//  Part B: Walk the parent chain for the REAL erase colour.
//    When the button is inside a Transparent FlowLayoutPanel,
//    Parent?.BackColor == Color.Transparent, and filling with
//    Transparent does nothing.  The old text frame ghosts through.
//    Fix: find the first ancestor with an opaque BackColor.
// =============================================================
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MoneyTracker.UI.Controls
{
    public class FlatButton : Button
    {
        public enum Sty { Primary, Secondary, Danger, Ghost, Outline }
        private Sty  _sty = Sty.Primary;
        private bool _hover, _down;

        public Sty Style
        {
            get { return _sty; }
            set { _sty = value; Invalidate(); }
        }

        public FlatButton()
        {
            // Part A — own the entire paint surface
            SetStyle(
                ControlStyles.UserPaint             |
                ControlStyles.AllPaintingInWmPaint  |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            // Suppress any remaining Win32 / VisualStyles rendering
            FlatStyle                         = FlatStyle.Flat;
            FlatAppearance.BorderSize         = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            UseVisualStyleBackColor           = false;

            Cursor = Cursors.Hand;
            Font   = Theme.Bold;
            Height = 40;
        }

        // Part B — walk parent chain to get the real opaque background colour
        private Color GetEraseColor()
        {
            Control c = Parent;
            while (c != null)
            {
                if (c.BackColor != Color.Transparent && c.BackColor.A == 255)
                    return c.BackColor;
                c = c.Parent;
            }
            return Theme.BgBase;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Do NOT call base.OnPaint — we own everything.
            var g = e.Graphics;
            g.SmoothingMode      = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint  = TextRenderingHint.ClearTypeGridFit;

            // Resolve colours
            Color bg, fg, bdr;
            switch (_sty)
            {
                case Sty.Secondary: bg=Theme.BgCard;       fg=Theme.TxtPrimary; bdr=Color.Transparent; break;
                case Sty.Danger:    bg=Theme.Danger;       fg=Color.White;      bdr=Color.Transparent; break;
                case Sty.Ghost:     bg=Color.Transparent;  fg=Theme.TxtMuted;   bdr=Theme.Border;      break;
                case Sty.Outline:   bg=Color.Transparent;  fg=Theme.Accent;     bdr=Theme.Accent;      break;
                default:            bg=Theme.Accent;       fg=Color.FromArgb(6,18,8); bdr=Color.Transparent; break;
            }

            if      (_down  && bg != Color.Transparent) bg = ControlPaint.Dark(bg, 0.18f);
            else if (_hover && bg != Color.Transparent) bg = ControlPaint.Dark(bg, 0.09f);

            // 1. Erase entire surface with the real parent background (Part B fix)
            using (var br = new SolidBrush(GetEraseColor()))
                g.FillRectangle(br, ClientRectangle);

            // 2. Draw rounded button body
            var rc = new Rectangle(1, 1, Width - 2, Height - 2);
            if (bg != Color.Transparent)
                using (var br = new SolidBrush(bg))
                    g.FillRound(br, rc, Theme.RBtn);

            // 3. Draw border for Ghost / Outline
            if (bdr != Color.Transparent)
                using (var pen = new Pen(bdr, 1.5f))
                    g.DrawRound(pen, rc, Theme.RBtn);

            // 4. Draw text exactly once
            using (var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags   = StringFormatFlags.NoWrap,
                Trimming      = StringTrimming.EllipsisCharacter
            })
            using (var br = new SolidBrush(fg))
                g.DrawString(Text, Font, br,
                    new RectangleF(3, 3, Width - 6, Height - 6), sf);

            // 5. Focus cue
            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(g,
                    new Rectangle(4, 4, Width - 8, Height - 8));
        }

        protected override void OnMouseEnter(EventArgs e)     { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e)     { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down  = true;  Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e)   { _down  = false; Invalidate(); base.OnMouseUp(e);   }
        protected override void OnTextChanged(EventArgs e)    { Invalidate(); base.OnTextChanged(e); }

        internal static GraphicsPath RoundPath(Rectangle r, int rad)
        {
            if (rad < 1) rad = 1;
            int d = rad * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X,          r.Y,          d, d, 180, 90);
            p.AddArc(r.Right - d,  r.Y,          d, d, 270, 90);
            p.AddArc(r.Right - d,  r.Bottom - d, d, d,   0, 90);
            p.AddArc(r.X,          r.Bottom - d, d, d,  90, 90);
            p.CloseFigure();
            return p;
        }
    }

    public static class G
    {
        public static void FillRound(this Graphics g, Brush br, Rectangle r, int rad)
        { using (var p = FlatButton.RoundPath(r, rad)) g.FillPath(br, p); }

        public static void DrawRound(this Graphics g, Pen pen, Rectangle r, int rad)
        { using (var p = FlatButton.RoundPath(r, rad)) g.DrawPath(pen, p); }

        public static void FillRound(this Graphics g, Brush br, RectangleF r, int rad)
        {
            if (rad < 1) rad = 1;
            float d = rad * 2f;
            var path = new GraphicsPath();
            path.AddArc(r.X,          r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d,  r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d,  r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,          r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            using (path) g.FillPath(br, path);
        }
    }
}
