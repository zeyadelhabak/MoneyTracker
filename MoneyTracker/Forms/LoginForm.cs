using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using MoneyTracker.Services;
using MoneyTracker.UI;
using MoneyTracker.UI.Controls;

namespace MoneyTracker.Forms
{
    // ══════════════════════════════════════════════════════════
    //  LOGIN FORM
    // ══════════════════════════════════════════════════════════
    public class LoginForm : Form
    {
        private TextBox    _user, _pass;
        private FlatButton _btnLogin, _btnEye;
        private Label      _status;
        private bool       _showPwd;

        public LoginForm()
        {
            StorageService.Init();
            Theme.Apply(Models.AppTheme.Dark);
            Build();
        }

        private void Build()
        {
            Text="MoneyTracker"; ClientSize=new Size(480,580);
            StartPosition=FormStartPosition.CenterScreen;
            FormBorderStyle=FormBorderStyle.FixedSingle;
            MaximizeBox=false; BackColor=Theme.BgBase;
            DoubleBuffered=true; KeyPreview=true;

            int cfx=46, cw=388;
            L("💰",cfx,26,44,44,new Font("Segoe UI",26f,FontStyle.Bold),Theme.Accent);
            L("MoneyTracker",cfx+50,32,300,34,Theme.H2,Theme.TxtPrimary);
            L("Professional Finance Manager",cfx,72,cw,20,Theme.Small,Theme.TxtMuted);

            var card=new DBPanel{Bounds=new Rectangle(cfx-10,106,cw+20,406),BackColor=Theme.BgSurface};
            card.Paint+=(s,pe)=>{
                var g=pe.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
                using(var br=new SolidBrush(Theme.BgSurface))
                    g.FillRound(br,new Rectangle(0,0,card.Width-1,card.Height-1),Theme.RCard);
                using(var br=new SolidBrush(Theme.Accent))
                    g.FillRectangle(br,0,0,card.Width,4);
                using(var p=new Pen(Theme.Border))
                    g.DrawRound(p,new Rectangle(0,0,card.Width-1,card.Height-1),Theme.RCard);};
            Controls.Add(card);

            int cy=22;
            CL(card,"SIGN IN",0,cy,cw+20,26,Theme.Bold,Theme.Accent,ContentAlignment.MiddleCenter); cy+=42;
            CL(card,"Username",22,cy,360,18,Theme.Small,Theme.TxtMuted); cy+=20;
            _user=CInp(card,22,cy,360); _user.PlaceholderText="Enter your username"; cy+=48;
            CL(card,"Password",22,cy,360,18,Theme.Small,Theme.TxtMuted); cy+=20;
            _pass=CInp(card,22,cy,316); _pass.UseSystemPasswordChar=true; _pass.PlaceholderText="Enter your password";

            _btnEye=new FlatButton{Text="👁",Bounds=new Rectangle(342,cy,36,34),Style=FlatButton.Sty.Ghost,Font=new Font("Segoe UI",12f)};
            _btnEye.Click+=(s,e)=>{_showPwd=!_showPwd;_pass.UseSystemPasswordChar=!_showPwd;_btnEye.Text=_showPwd?"🙈":"👁";};
            card.Controls.Add(_btnEye); cy+=52;

            _btnLogin=new FlatButton{Text="SIGN IN",Bounds=new Rectangle(22,cy,360,44)};
            _btnLogin.Click+=DoLogin; card.Controls.Add(_btnLogin); cy+=56;

            card.Controls.Add(new Panel{Bounds=new Rectangle(22,cy,360,1),BackColor=Theme.Border}); cy+=14;
            CL(card,"Don't have an account?",0,cy,cw+20,20,Theme.Small,Theme.TxtMuted,ContentAlignment.MiddleCenter); cy+=22;

            var bReg=new FlatButton{Text="CREATE FREE ACCOUNT",Bounds=new Rectangle(22,cy,360,40),Style=FlatButton.Sty.Outline};
            bReg.Click+=(s,e)=>new RegisterForm().ShowDialog(this);
            card.Controls.Add(bReg); cy+=52;

            _status=new Label{Bounds=new Rectangle(0,cy,cw+20,28),Font=Theme.Small,ForeColor=Theme.Expense,
                BackColor=Color.Transparent,TextAlign=ContentAlignment.MiddleCenter,AutoSize=false};
            card.Controls.Add(_status);

            L("v2.0  ·  Secure & Encrypted  ·  2025",0,532,480,22,Theme.Cap,Theme.TxtHint,ContentAlignment.MiddleCenter);
            KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Return)DoLogin(s,e);};
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using(var lgb=new LinearGradientBrush(new Rectangle(0,0,Width,4),Theme.Accent,Theme.AccentDark,LinearGradientMode.Horizontal))
                e.Graphics.FillRectangle(lgb,0,0,Width,4);
        }

        private void DoLogin(object s,EventArgs e)
        {
            _status.Text="";
            string u=_user.Text.Trim(),p=_pass.Text;
            if(u==""&&p==""){Err("Please enter your username and password.");return;}
            if(u==""){Err("Please enter your username.");return;}
            if(p==""){Err("Please enter your password.");return;}
            _btnLogin.Text="Signing in..."; _btnLogin.Enabled=false;
            switch(AuthService.Login(u,p)){
                case AuthResult.Ok:
                    _user.Clear();_pass.Clear();
                    Theme.Apply(Models.Session.CurrentUser.Theme);
                    _btnLogin.Text="SIGN IN";_btnLogin.Enabled=true;
                    new DashboardForm().ShowDialog(this); break;
                case AuthResult.NotFound:
                    Err("Username not found. Please register first.");
                    _btnLogin.Text="SIGN IN";_btnLogin.Enabled=true;_user.Focus(); break;
                case AuthResult.BadPassword:
                    Err("Incorrect Password. Please try again.");
                    _btnLogin.Text="SIGN IN";_btnLogin.Enabled=true;_pass.Clear();_pass.Focus(); break;
                default:
                    Err("Login failed. Please try again.");
                    _btnLogin.Text="SIGN IN";_btnLogin.Enabled=true; break;}
        }

        void Err(string m){_status.ForeColor=Theme.Expense;_status.Text="⚠  "+m;}

        void L(string t,int x,int y,int w,int h,Font f,Color c,ContentAlignment a=ContentAlignment.MiddleLeft)
            =>Controls.Add(new Label{Text=t,Bounds=new Rectangle(x,y,w,h),Font=f,ForeColor=c,BackColor=Color.Transparent,TextAlign=a,AutoSize=false});
        static void CL(Panel p,string t,int x,int y,int w,int h,Font f,Color c,ContentAlignment a=ContentAlignment.MiddleLeft)
            =>p.Controls.Add(new Label{Text=t,Bounds=new Rectangle(x,y,w,h),Font=f,ForeColor=c,BackColor=Color.Transparent,TextAlign=a,AutoSize=false});
        static TextBox CInp(Panel p,int x,int y,int w){
            var tb=new TextBox{Bounds=new Rectangle(x,y,w,34),BackColor=Theme.BgInput,ForeColor=Theme.TxtPrimary,BorderStyle=BorderStyle.FixedSingle,Font=Theme.Body};
            p.Controls.Add(tb);return tb;}
    }

    // ══════════════════════════════════════════════════════════
    //  REGISTER FORM
    // ══════════════════════════════════════════════════════════
    public class RegisterForm : Form
    {
        private TextBox _u,_em,_p,_c; private Label _st;

        public RegisterForm(){Build();}

        private void Build()
        {
            Text="MoneyTracker — Create Account";ClientSize=new Size(480,596);
            StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;
            MaximizeBox=false;BackColor=Theme.BgBase;DoubleBuffered=true;

            int cfx=46,cw=388;
            L("Create Account",cfx,22,cw,36,Theme.H2,Theme.TxtPrimary);
            L("Join MoneyTracker — free, private, professional.",cfx,60,cw,20,Theme.Small,Theme.TxtMuted);

            var card=new DBPanel{Bounds=new Rectangle(cfx-10,94,cw+20,454),BackColor=Theme.BgSurface};
            card.Paint+=(s,pe)=>{var g=pe.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
                using(var br=new SolidBrush(Theme.BgSurface))g.FillRound(br,new Rectangle(0,0,card.Width-1,card.Height-1),Theme.RCard);
                using(var br=new SolidBrush(Theme.Accent))g.FillRectangle(br,0,0,card.Width,4);
                using(var p=new Pen(Theme.Border))g.DrawRound(p,new Rectangle(0,0,card.Width-1,card.Height-1),Theme.RCard);};
            Controls.Add(card);

            int cy=22;
            CL(card,"CREATE ACCOUNT",0,cy,cw+20,26,Theme.Bold,Theme.Accent,ContentAlignment.MiddleCenter);cy+=42;
            Row(card,"Username  (3–31 chars, letters / digits / _)",ref cy,out _u,false);
            Row(card,"Email Address",ref cy,out _em,false);
            Row(card,"Password  (minimum 6 characters)",ref cy,out _p,true);
            Row(card,"Confirm Password",ref cy,out _c,true); cy+=4;

            var bc=new FlatButton{Text="CREATE ACCOUNT",Bounds=new Rectangle(22,cy,360,44)};
            bc.Click+=DoCreate;card.Controls.Add(bc);cy+=54;
            var bb=new FlatButton{Text="Back to Login",Bounds=new Rectangle(22,cy,360,38),Style=FlatButton.Sty.Ghost};
            bb.Click+=(s,e)=>Close();card.Controls.Add(bb);cy+=50;
            _st=new Label{Bounds=new Rectangle(0,cy,cw+20,28),Font=Theme.Small,ForeColor=Theme.Expense,BackColor=Color.Transparent,TextAlign=ContentAlignment.MiddleCenter,AutoSize=false};
            card.Controls.Add(_st);
        }

        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);
            using(var lgb=new LinearGradientBrush(new Rectangle(0,0,Width,4),Theme.Accent,Theme.AccentDark,LinearGradientMode.Horizontal))
                e.Graphics.FillRectangle(lgb,0,0,Width,4);}

        void Row(Panel p,string lbl,ref int y,out TextBox tb,bool pwd){
            CL(p,lbl,22,y,360,18,Theme.Small,Theme.TxtMuted);y+=20;
            tb=new TextBox{Bounds=new Rectangle(22,y,360,34),BackColor=Theme.BgInput,ForeColor=Theme.TxtPrimary,BorderStyle=BorderStyle.FixedSingle,Font=Theme.Body,UseSystemPasswordChar=pwd};
            p.Controls.Add(tb);y+=48;}

        void DoCreate(object s,EventArgs e){
            _st.Text="";string u=_u.Text.Trim(),em=_em.Text.Trim(),p=_p.Text,c=_c.Text;
            if(u==""||em==""||p==""){Err("All fields are required.");return;}
            if(!AuthService.ValidateUsername(u)){Err("Username: 3-31 chars, letters/digits/_");return;}
            if(!AuthService.ValidatePassword(p)){Err("Password must be at least 6 characters.");return;}
            if(p!=c){Err("Passwords do not match.");return;}
            switch(AuthService.Register(u,p,em)){
                case AuthResult.Ok:
                    MessageBox.Show("Account created! You can now sign in.","Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    Close();break;
                case AuthResult.UserExists:Err("Username already taken.");break;
                default:Err("Registration failed.");break;}}

        void Err(string m){_st.ForeColor=Theme.Expense;_st.Text="⚠  "+m;}
        void L(string t,int x,int y,int w,int h,Font f,Color c,ContentAlignment a=ContentAlignment.MiddleLeft)
            =>Controls.Add(new Label{Text=t,Bounds=new Rectangle(x,y,w,h),Font=f,ForeColor=c,BackColor=Color.Transparent,TextAlign=a,AutoSize=false});
        static void CL(Panel p,string t,int x,int y,int w,int h,Font f,Color c,ContentAlignment a=ContentAlignment.MiddleLeft)
            =>p.Controls.Add(new Label{Text=t,Bounds=new Rectangle(x,y,w,h),Font=f,ForeColor=c,BackColor=Color.Transparent,TextAlign=a,AutoSize=false});
    }

    // shared helper
    public class DBPanel:Panel{public DBPanel(){DoubleBuffered=true;}}
}
