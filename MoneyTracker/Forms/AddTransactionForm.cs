using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MoneyTracker.Models;
using MoneyTracker.Services;
using MoneyTracker.UI;
using MoneyTracker.UI.Controls;

namespace MoneyTracker.Forms
{
    public class AddTransactionForm : Form
    {
        private readonly Transaction _edit;
        private ComboBox       _type,_cat;
        private TextBox        _amt,_desc,_note;
        private DateTimePicker _dt;
        private Label          _st;

        public AddTransactionForm(Transaction edit=null,TxType def=TxType.Expense)
        {
            _edit=edit; Build(def); if(edit!=null)Pop(edit);
        }

        private void Build(TxType def)
        {
            bool isEdit=_edit!=null;
            Text=isEdit?"Edit Transaction":"Add Transaction";
            ClientSize=new Size(440,484); StartPosition=FormStartPosition.CenterParent;
            FormBorderStyle=FormBorderStyle.FixedDialog; MaximizeBox=false; MinimizeBox=false;
            BackColor=Theme.BgSurface; DoubleBuffered=true;

            int lx=22,fw=394,y=14;
            Lbl("Transaction Type",lx,y);y+=20;
            _type=Cbo(lx,y,fw,new[]{"💚  Income","🔴  Expense"});
            _type.SelectedIndex=def==TxType.Income?0:1;y+=44;
            Lbl("Category",lx,y);y+=20;
            _cat=Cbo(lx,y,fw,new[]{"🍔  Food","🚗  Transport","🛍  Shopping","📄  Bills","🎭  Entertainment","💼  Salary","📈  Investment","📦  Other"});
            _cat.SelectedIndex=0;y+=44;
            Lbl("Amount ($)",lx,y);y+=20;
            _amt=Inp(lx,y,fw);_amt.PlaceholderText="0.00";y+=44;
            Lbl("Description *",lx,y);y+=20;
            _desc=Inp(lx,y,fw);_desc.PlaceholderText="What was this for?";y+=44;
            Lbl("Note  (optional)",lx,y);y+=20;
            _note=Inp(lx,y,fw);_note.PlaceholderText="Extra details...";y+=44;
            Lbl("Date",lx,y);y+=20;
            _dt=new DateTimePicker{Bounds=new Rectangle(lx,y,fw,34),Format=DateTimePickerFormat.Short,Value=DateTime.Today};
            Controls.Add(_dt);y+=48;
            _st=new Label{Bounds=new Rectangle(lx,y,fw,20),Font=Theme.Small,ForeColor=Theme.Expense,BackColor=Color.Transparent,AutoSize=false};
            Controls.Add(_st);y+=26;

            var bs=new FlatButton{Text=isEdit?"SAVE CHANGES":"ADD TRANSACTION",Bounds=new Rectangle(lx,y,190,42)};
            bs.Click+=Save;Controls.Add(bs);
            var bc=new FlatButton{Text="Cancel",Bounds=new Rectangle(lx+198,y,196,42),Style=FlatButton.Sty.Secondary};
            bc.Click+=(s,e)=>Close();Controls.Add(bc);
        }

        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);
            using(var lgb=new LinearGradientBrush(new Rectangle(0,0,Width,4),Theme.Accent,Theme.AccentDark,LinearGradientMode.Horizontal))
                e.Graphics.FillRectangle(lgb,0,0,Width,4);}

        void Pop(Transaction t){
            _type.SelectedIndex=t.Type==TxType.Income?0:1;
            _cat.SelectedIndex=(int)t.Category-1;
            _amt.Text=t.Amount.ToString("F2");_desc.Text=t.Description;_note.Text=t.Note;_dt.Value=t.Date;}

        void Save(object s,EventArgs e){
            _st.Text="";
            if(!double.TryParse(_amt.Text.Trim(),out double amt)||amt<=0){_st.Text="Enter a valid positive amount.";return;}
            if(string.IsNullOrWhiteSpace(_desc.Text)){_st.Text="Description is required.";return;}
            var tx=_edit??new Transaction();
            tx.Type=_type.SelectedIndex==0?TxType.Income:TxType.Expense;
            tx.Category=(TxCategory)(_cat.SelectedIndex+1);
            tx.Amount=amt;tx.Description=_desc.Text.Trim();tx.Note=_note.Text.Trim();tx.Date=_dt.Value.Date;
            if(_edit==null)TransactionService.Add(tx);else TransactionService.Edit(tx);
            DialogResult=DialogResult.OK;Close();}

        void Lbl(string t,int x,int y)
            =>Controls.Add(new Label{Text=t,Bounds=new Rectangle(x,y,394,18),Font=Theme.Small,ForeColor=Theme.TxtMuted,BackColor=Color.Transparent,AutoSize=false});
        TextBox Inp(int x,int y,int w){var tb=new TextBox{Bounds=new Rectangle(x,y,w,32),BackColor=Theme.BgInput,ForeColor=Theme.TxtPrimary,BorderStyle=BorderStyle.FixedSingle,Font=Theme.Body};Controls.Add(tb);return tb;}
        ComboBox Cbo(int x,int y,int w,string[]items){var cb=new ComboBox{Bounds=new Rectangle(x,y,w,32),BackColor=Theme.BgInput,ForeColor=Theme.TxtPrimary,DropDownStyle=ComboBoxStyle.DropDownList,Font=Theme.Body,FlatStyle=FlatStyle.Flat};cb.Items.AddRange(items);Controls.Add(cb);return cb;}
    }
}
