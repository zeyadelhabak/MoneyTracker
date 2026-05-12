using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace MoneyTracker.UI.Controls
{
    // ══════════════════════════════════════════════════════════
    //  BAR CHART
    // ══════════════════════════════════════════════════════════
    public class BarChart : Panel
    {
        public string ChartTitle { get; set; } = "Monthly Overview";
        public List<(string L, double Inc, double Exp)> Data { get; set; }
            = new List<(string, double, double)>();

        public BarChart(){DoubleBuffered=true;}

        protected override void OnPaint(PaintEventArgs e)
        {
            var g=e.Graphics;
            g.SmoothingMode    =SmoothingMode.AntiAlias;
            g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
            g.Clear(Theme.BgCard);
            using(var br=new SolidBrush(Theme.TxtPrimary))
                g.DrawString(ChartTitle,Theme.H3,br,14,10);

            if(Data==null||Data.Count==0){Empty(g,"No data yet — add transactions.");return;}

            int mg=54,bot=Height-44,top=46,cW=Width-mg-14,cH=bot-top;
            double mx=Data.Max(d=>Math.Max(d.Inc,d.Exp)); if(mx==0)mx=1;
            int n=Data.Count, slW=cW/n, bW=Math.Max(8,(slW-18)/2);

            for(int i=0;i<=4;i++){
                int y=top+(int)(cH*i/4.0);
                using(var p=new Pen(Theme.Border,1){DashStyle=DashStyle.Dot}) g.DrawLine(p,mg,y,Width-14,y);
                double v=mx*(4-i)/4;
                using(var br=new SolidBrush(Theme.TxtMuted))
                    g.DrawString(v>=1000?$"{v/1000:F0}k":$"{v:F0}",Theme.Cap,br,2,y-9);}

            for(int i=0;i<n;i++){
                var d=Data[i]; int x0=mg+i*slW+(slW-bW*2-6)/2;
                void Bar(int x,double v,Color c){
                    int h=(int)(v/mx*cH); if(h<1)return;
                    var r=new Rectangle(x,bot-h,bW,h);
                    using(var lb=new LinearGradientBrush(new Rectangle(x,bot-h,bW,Math.Max(h,1)),
                        ControlPaint.Light(c,0.25f),c,LinearGradientMode.Vertical))
                        g.FillRectangle(lb,r);
                    using(var br=new SolidBrush(ControlPaint.Light(c,0.5f)))
                        g.FillRectangle(br,x,bot-h,bW,3);}
                Bar(x0,       d.Inc,Theme.Income);
                Bar(x0+bW+6,  d.Exp,Theme.Expense);
                using(var br=new SolidBrush(Theme.TxtMuted))
                using(var sf=new StringFormat{Alignment=StringAlignment.Center})
                    g.DrawString(d.L,Theme.Cap,br,new RectangleF(mg+i*slW,bot+4,slW,22),sf);}

            int lx=Width-185,ly=11;
            g.FillRound(new SolidBrush(Theme.Income), new Rectangle(lx,    ly+2,12,10),2);
            g.FillRound(new SolidBrush(Theme.Expense),new Rectangle(lx+78,ly+2,12,10),2);
            using(var br=new SolidBrush(Theme.TxtMuted))
            {g.DrawString("Income",Theme.Cap,br,lx+16,ly-1);g.DrawString("Expense",Theme.Cap,br,lx+94,ly-1);}
        }

        void Empty(Graphics g,string m){
            using(var br=new SolidBrush(Theme.TxtMuted))
            using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})
                g.DrawString(m,Theme.Body,br,new RectangleF(0,0,Width,Height),sf);}

        protected override void OnResize(EventArgs e){base.OnResize(e);Invalidate();}
    }

    // ══════════════════════════════════════════════════════════
    //  DONUT CHART
    // ══════════════════════════════════════════════════════════
    public class DonutChart : Panel
    {
        public string ChartTitle { get; set; } = "Expenses by Category";
        public Dictionary<string,double> Data { get; set; } = new Dictionary<string,double>();

        public DonutChart(){DoubleBuffered=true;}

        protected override void OnPaint(PaintEventArgs e)
        {
            var g=e.Graphics;
            g.SmoothingMode    =SmoothingMode.AntiAlias;
            g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
            g.Clear(Theme.BgCard);
            using(var br=new SolidBrush(Theme.TxtPrimary))
                g.DrawString(ChartTitle,Theme.H3,br,14,10);
            if(Data==null||Data.Count==0){
                // FIX-L7: proper empty state placeholder
                int cx2=Width/2, cy2=Height/2;
                // Icon
                using(var f2=new Font("Segoe UI",28f))
                using(var br=new SolidBrush(Color.FromArgb(50,Theme.TxtMuted)))
                    g.DrawString("📊",f2,br,
                        new RectangleF(cx2-24,cy2-48,48,48));
                // Primary message
                using(var br=new SolidBrush(Theme.TxtMuted))
                using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})
                    g.DrawString("No expense data yet",Theme.Bold,br,
                        new RectangleF(20,cy2,Width-40,24),sf);
                // Sub message
                using(var br=new SolidBrush(Color.FromArgb(140,Theme.TxtMuted)))
                using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})
                    g.DrawString("Add expense transactions to see your breakdown",Theme.Cap,br,
                        new RectangleF(20,cy2+26,Width-40,18),sf);
                return;}

            double total=Data.Values.Sum();
            int cx=120,cy=Height/2+8,r=Math.Min(cx-20,cy-30),ri=r-28;
            float start=-90f; int ci=0;
            var items=Data.ToList();
            foreach(var kv in items){
                float sweep=(float)(kv.Value/total*360.0);
                using(var br=new SolidBrush(Theme.Chart[ci%Theme.Chart.Length]))
                    g.FillPie(br,cx-r,cy-r,r*2,r*2,start,sweep);
                start+=sweep; ci++;}

            using(var br=new SolidBrush(Theme.BgCard)) g.FillEllipse(br,cx-ri,cy-ri,ri*2,ri*2);
            using(var br=new SolidBrush(Theme.TxtPrimary))
            using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})
                g.DrawString($"${total:F0}",Theme.Bold,br,new RectangleF(cx-46,cy-16,92,32),sf);
            using(var br=new SolidBrush(Theme.TxtMuted))
            using(var sf=new StringFormat{Alignment=StringAlignment.Center})
                g.DrawString("total",Theme.Cap,br,new RectangleF(cx-46,cy+10,92,16),sf);

            int lx=cx+r+14,ly=cy-r; ci=0;
            foreach(var kv in items){
                if(ly>Height-18)break;
                var col=Theme.Chart[ci%Theme.Chart.Length];
                g.FillRound(new SolidBrush(col),new Rectangle(lx,ly+4,10,10),2);
                using(var br=new SolidBrush(Theme.TxtPrimary))
                    g.DrawString(kv.Key,Theme.Small,br,lx+16,ly);
                using(var br=new SolidBrush(Theme.TxtMuted))
                    g.DrawString($"${kv.Value:F0}  ({kv.Value/total*100:F0}%)",Theme.Cap,br,lx+16,ly+14);
                ly+=34; ci++;}
        }
        protected override void OnResize(EventArgs e){base.OnResize(e);Invalidate();}
    }

    // ══════════════════════════════════════════════════════════
    //  LINE CHART
    // ══════════════════════════════════════════════════════════
    public class LineChart : Panel
    {
        public string ChartTitle { get; set; } = "Trend";
        public List<(string L, double Inc, double Exp)> Data { get; set; }
            = new List<(string, double, double)>();

        public LineChart(){DoubleBuffered=true;}

        protected override void OnPaint(PaintEventArgs e)
        {
            var g=e.Graphics;
            g.SmoothingMode    =SmoothingMode.AntiAlias;
            g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
            g.Clear(Theme.BgCard);
            using(var br=new SolidBrush(Theme.TxtPrimary))
                g.DrawString(ChartTitle,Theme.H3,br,14,10);
            if(Data==null||Data.Count<2){
                using(var br=new SolidBrush(Theme.TxtMuted))
                using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})
                    g.DrawString("Add more months of data to see trend.",Theme.Body,br,new RectangleF(0,0,Width,Height),sf);
                return;}

            int mg=56,bot=Height-44,top=44,cW=Width-mg-14,cH=bot-top;
            double mx=Data.Max(d=>Math.Max(d.Inc,d.Exp)); if(mx==0)mx=1;
            for(int i=0;i<=4;i++){
                int y=top+(int)(cH*i/4.0);
                using(var p=new Pen(Theme.Border,1){DashStyle=DashStyle.Dot}) g.DrawLine(p,mg,y,Width-14,y);
                double v=mx*(4-i)/4;
                using(var br=new SolidBrush(Theme.TxtMuted))
                    g.DrawString(v>=1000?$"{v/1000:F0}k":$"{v:F0}",Theme.Cap,br,2,y-9);}

            int n=Data.Count; float step=cW/(float)(n-1);

            void Line(Func<(string L, double Inc, double Exp),double> val,Color col){
                var pts=new PointF[n];
                for(int i=0;i<n;i++){float x=mg+i*step;float y=top+(float)((1-val(Data[i])/mx)*cH);pts[i]=new PointF(x,y);}
                var poly=new PointF[n+2];
                poly[0]=new PointF(pts[0].X,bot);
                for(int i=0;i<n;i++)poly[i+1]=pts[i];
                poly[n+1]=new PointF(pts[n-1].X,bot);
                using(var br=new SolidBrush(Color.FromArgb(30,col))) g.FillPolygon(br,poly);
                using(var p=new Pen(col,2.5f){LineJoin=LineJoin.Round,StartCap=LineCap.Round,EndCap=LineCap.Round})
                    g.DrawLines(p,pts);
                foreach(var pt in pts)
                    using(var br=new SolidBrush(col)) g.FillEllipse(br,pt.X-4,pt.Y-4,8,8);}

            Line(d=>d.Inc, Theme.Income);
            Line(d=>d.Exp, Theme.Expense);

            float sw=Math.Max(step,40f);
            for(int i=0;i<n;i++)
                using(var br=new SolidBrush(Theme.TxtMuted))
                using(var sf=new StringFormat{Alignment=StringAlignment.Center})
                    g.DrawString(Data[i].L,Theme.Cap,br,new RectangleF(mg+i*step-sw/2f,bot+4,sw,22),sf);

            int lx=Width-185,ly=11;
            using(var p=new Pen(Theme.Income, 2.5f)) g.DrawLine(p,lx,   ly+7,lx+14,   ly+7);
            using(var p=new Pen(Theme.Expense,2.5f)) g.DrawLine(p,lx+78,ly+7,lx+78+14,ly+7);
            using(var br=new SolidBrush(Theme.TxtMuted))
            {g.DrawString("Income",Theme.Cap,br,lx+18,ly-1);g.DrawString("Expense",Theme.Cap,br,lx+96,ly-1);}
        }
        protected override void OnResize(EventArgs e){base.OnResize(e);Invalidate();}
    }
}
