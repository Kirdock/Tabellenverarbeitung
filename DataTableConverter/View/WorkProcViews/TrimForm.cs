using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View.WorkProcViews
{
    public partial class TrimForm : Form
    {
        internal ProcTrim Proc;
        public TrimForm()
        {
            InitializeComponent();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if(TxtTrimText.Text == string.Empty)
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Das Zeichen darf nicht leer sein");
            }
            else
            {
                ProcTrim.TrimType type = RbTrimStart.Checked ? ProcTrim.TrimType.Start : RbTrimEnd.Checked ? ProcTrim.TrimType.End : ProcTrim.TrimType.Both;
                Proc = new ProcTrim(TxtTrimText.Text, type, CbTrimDeleteDouble.Checked);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
