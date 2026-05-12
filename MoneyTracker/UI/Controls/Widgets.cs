using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MoneyTracker.UI.Controls
{
    // ══════════════════════════════════════════════════════════
    //  STAT CARD
    // ══════════════════════════════════════════════════════════
    public class StatCard : Panel
    {
        public string Title  { get; set; } = "";
        public string Value  { get; set; } = "$0.00";
        public string Icon   { get; set; } = "";
        public string Sub    { get; set; } = "";
        public Color  Accent { get; set; } = Theme.Accent;

        public StatCard() { DoubleBuffered=true; Margin=new Padding(6); }

        public void Refresh(string v, string sub="") { Value=v; Sub=sub; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g=e.Graphics;
            g.SmoothingMode    =SmoothingMode.AntiAlias;
            g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;

            using(var br=new SolidBrush(Theme.BgCard))
                g.FillRound(br, new Rectangle(2,2,Width-4,Height-4), Theme.RCard);

            using(var br=new SolidBrush(Accent))
                g.FillRectangle(br,2,2,5,Height-4);

            using(var lgb=new LinearGradientBrush(new Rectangle(8,2,Width-10,Height/3),
                Color.FromArgb(22,Accent), Color.Transparent, LinearGradientMode.Vertical))
                g.FillRectangle(lgb,8,2,Width-10,Height/3);

            string hdr=(Icon!=""?Icon+"  ":"")+Title;
            using(var br=new SolidBrush(Theme.TxtMuted))
                g.DrawString(hdr, Theme.Cap, br, new RectangleF(16,10,Width-22,18));

            float fs=Value.Length>9?15f:20f;
            using(var f=new Font("Segoe UI",fs,FontStyle.Bold))
            using(var br=new SolidBrush(Accent))
                g.DrawString(Value,f,br,new RectangleF(16,28,Width-22,50));

            if(!string.IsNullOrEmpty(Sub))
                using(var br=new SolidBrush(Theme.TxtHint))
                    g.DrawString(Sub,Theme.Cap,br,new RectangleF(16,Height-22,Width-22,20));
        }
        protected override void OnResize(EventArgs e){base.OnResize(e);Invalidate();}
    }

    // ══════════════════════════════════════════════════════════
    //  BUDGET BAR
    // ══════════════════════════════════════════════════════════
    public class BudgetBar : Panel
    {
        public double Pct   { get; set; }
        public string LabelLeft  { get; set; } = "Monthly Budget";
        public string LabelRight { get; set; } = "0%";

        public BudgetBar(){DoubleBuffered=true;Height=58;}

        protected override void OnPaint(PaintEventArgs e)
        {
            var g=e.Graphics;
            g.SmoothingMode    =SmoothingMode.AntiAlias;
            g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
            g.Clear(Theme.BgCard);

            using(var br=new SolidBrush(Theme.TxtMuted))
                g.DrawString(LabelLeft,Theme.Small,br,0,2);

            bool over=Pct>=100;
            using(var br=new SolidBrush(over?Theme.Expense:Theme.TxtPrimary))
            using(var sf=new StringFormat{Alignment=StringAlignment.Far})
                g.DrawString(LabelRight,Theme.Bold,br,new RectangleF(0,0,Width,22),sf);

            int th=12,ty=26;
            using(var br=new SolidBrush(Theme.Border))
                g.FillRound(br,new Rectangle(0,ty,Width,th),6);

            double p=Math.Min(100,Math.Max(0,Pct));
            int fw=(int)(Width*p/100.0);
            if(fw>4)
            {
                Color fill=p<60?Theme.Income:p<85?Theme.Warning:Theme.Expense;
                using(var lgb=new LinearGradientBrush(new Rectangle(0,ty,Math.Max(fw,1),th),
                    ControlPaint.Light(fill,0.3f),fill,LinearGradientMode.Horizontal))
                    g.FillRound(lgb,new Rectangle(0,ty,fw,th),6);
            }

            string sub=over?"⚠  Budget exceeded!":$"{100-p:F0}% remaining";
            using(var br=new SolidBrush(over?Theme.Expense:Theme.TxtMuted))
                g.DrawString(sub,Theme.Cap,br,0,ty+15);
        }
        protected override void OnResize(EventArgs e){base.OnResize(e);Invalidate();}
    }

    // ══════════════════════════════════════════════════════════
    //  TOAST
    // ══════════════════════════════════════════════════════════
    public class Toast : Form
    {
        public enum Kind{Success,Error,Warning,Info}
        private Toast(){}

        public static void Show(Form owner,string msg,Kind kind=Kind.Success,int ms=3200)
        {
            Color bg,accent; string icon;
            switch(kind){
                case Kind.Error:   bg=Color.FromArgb(36,8,8); accent=Theme.Expense;  icon="✘"; break;
                case Kind.Warning: bg=Color.FromArgb(34,26,6);accent=Theme.Warning;  icon="⚠"; break;
                case Kind.Info:    bg=Color.FromArgb(6,20,38);accent=Theme.Info;     icon="ℹ"; break;
                default:           bg=Color.FromArgb(6,28,14);accent=Theme.Success;  icon="✔"; break;
            }
            var t=new Form{FormBorderStyle=FormBorderStyle.None,StartPosition=FormStartPosition.Manual,
                ShowInTaskbar=false,TopMost=true,BackColor=Color.FromArgb(16,22,38),Opacity=0,Width=342,Height=62};
            if(owner!=null){var r=owner.RectangleToScreen(owner.ClientRectangle);
                t.Location=new Point(r.Right-t.Width-22,r.Bottom-t.Height-24);}
            t.Paint+=(s,pe)=>{
                var g=pe.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
                g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
                using(var br=new SolidBrush(bg))
                    g.FillRound(br,new Rectangle(1,1,t.Width-2,t.Height-2),9);
                using(var p=new Pen(accent,1.5f))
                    g.DrawRound(p,new Rectangle(1,1,t.Width-2,t.Height-2),9);
                using(var br=new SolidBrush(accent))
                    g.FillRectangle(br,1,1,4,t.Height-2);
                using(var f=new Font("Segoe UI",15f,FontStyle.Bold))
                using(var br=new SolidBrush(accent))
                    g.DrawString(icon,f,br,14,18);
                using(var br=new SolidBrush(Color.FromArgb(218,226,244)))
                    g.DrawString(msg,Theme.Bold,br,46,10);
                using(var br=new SolidBrush(Color.FromArgb(80,218,226,244)))
                    g.DrawString(DateTime.Now.ToString("HH:mm"),Theme.Cap,br,46,32);};
            t.Click+=(s,ex)=>t.Close();
            t.Show(owner);
            var fi=new Timer{Interval=25};
            fi.Tick+=(s,ex)=>{t.Opacity=Math.Min(1.0,t.Opacity+0.14);if(t.Opacity>=1){fi.Stop();fi.Dispose();}};
            fi.Start();
            var ac=new Timer{Interval=ms};
            ac.Tick+=(s,ex)=>{ac.Stop();ac.Dispose();if(t.IsDisposed)return;
                var fo=new Timer{Interval=25};
                fo.Tick+=(fs,fe)=>{if(t.IsDisposed){fo.Stop();fo.Dispose();return;}
                    t.Opacity-=0.09;if(t.Opacity<=0){fo.Stop();fo.Dispose();t.Close();}};
                fo.Start();};
            ac.Start();
        }
    }

    // ══════════════════════════════════════════════════════════
    //  UTILITY
    // ══════════════════════════════════════════════════════════
    public class DBPanel : Panel { public DBPanel(){DoubleBuffered=true;} }
}
