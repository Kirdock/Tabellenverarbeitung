using DataTableConverter.Classes.WorkProcs;
using System;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class RoundForm : Form
    {
        internal ProcRound Procedure;

        internal RoundForm(object[] headers)
        {
            InitializeComponent();
            cbHeaders.Items.AddRange(headers);
            SetNewColumnVisibility();
            CmBRound.SelectedIndex = 0;
        }

        internal string[] GetSelectedHeaders()
        {
            return ViewHelper.GetSelectedHeaders(cbHeaders);
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, true);
        }

        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, false);
        }

        private void cbNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            SetNewColumnVisibility();
            if (cbOldColumn.Visible = !txtHeader.Visible)
            {
                txtHeader.Text = string.Empty;
            }
            else
            {
                cbOldColumn.Checked = false;
            }
        }

        private void SetNewColumnVisibility()
        {
            lblHeader.Visible = txtHeader.Visible = cbNewColumn.Checked;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            Procedure = new ProcRound(GetSelectedHeaders(), (int)numDec.Value, txtHeader.Text, CmBRound.SelectedIndex, cbOldColumn.Checked);
            DialogResult = DialogResult.OK;
        }
    }
}
