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
    public partial class NumerationForm : Form
    {
        internal ProcNumber Procedure;
        private object[] Headers;

        internal NumerationForm(object[] headers)
        {
            InitializeComponent();
            Headers = headers;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewColumn.Text))
            {
                if (Headers.Contains(txtNewColumn.Text))
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Der Spaltenname wird bereits verwendet. Bitte verwenden Sie einen anderen");
                }
                else
                {
                    Procedure = new ProcNumber(txtNewColumn.Text, (int)nbNumberStart.Value, (int)nbNumberEnd.Value, cbNumberRepeat.Checked);
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an");
            }
        }
    }
}
