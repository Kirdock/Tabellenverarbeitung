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
    public partial class SubstringForm : Form
    {
        internal ProcSubstring Procedure;

        internal SubstringForm(object[] headers)
        {
            InitializeComponent();
            cbHeaders.Items.AddRange(headers);
            SetNewColumnVisibility(false);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if(cbNewColumn.Checked && string.IsNullOrWhiteSpace(txtNewColumn.Text))
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an.");
            }
            else if(cbNewColumn.Checked && cbHeaders.Items.Contains(txtNewColumn.Text))
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Der Spaltenname wird bereits verwendet. Bitte verwenden Sie einen anderen.");
            }
            else if(cbHeaders.CheckedItems.Count < 1)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte wählen Sie zumindest eine Spatle aus, auf die der Vorgang angewendet werden soll.");
            }
            else
            {
                Procedure = new ProcSubstring(cbHeaders.CheckedItems.Cast<string>().ToArray(), txtNewColumn.Text, cbOldColumn.Checked, (int)nbStart.Value, (int)nbEnd.Value, txtSubstringText.Text, cbSubstringText.Checked, CBReverse.Checked);
                DialogResult = DialogResult.OK;
            }
        }

        private void cbNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            bool checkState = ((CheckBox)sender).Checked;
            if (cbOldColumn.Visible = !checkState)
            {
                txtNewColumn.Text = string.Empty;
            }
            else
            {
                cbOldColumn.Checked = false;
            }
            SetNewColumnVisibility(checkState);
        }

        private void SetNewColumnVisibility(bool status)
        {
            lblNewColumn.Visible = txtNewColumn.Visible = status;
        }

        private void cbSubstringText_CheckedChanged(object s, EventArgs e)
        {
            var sender = s as CheckBox;
            txtSubstringText.Visible = sender.Checked;
            if (!sender.Checked)
            {
                txtSubstringText.Text = string.Empty;
            }
        }

        private void nbStart_ValueChanged(object sender, EventArgs e)
        {
            if (nbEnd.Value < nbStart.Value && nbEnd.Value != 0)
            {
                nbStart.Value = nbEnd.Value;
            }
        }

        private void nbEnd_ValueChanged(object sender, EventArgs e)
        {
            if (nbEnd.Value < nbStart.Value && nbEnd.Value != 0)
            {
                nbEnd.Value = Convert.ToInt32(nbEnd.Text) > nbEnd.Value ? 0 : nbStart.Value;
            }
        }
    }
}
