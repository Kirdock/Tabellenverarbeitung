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
    public partial class DivideForm : Form
    {
        internal ProcDivide Proc;
        internal string[] SelectedHeaders => ViewHelper.GetSelectedHeaders(cbHeaders);
        internal DivideForm(List<string> headers)
        {
            InitializeComponent();
            cbHeaders.Items.AddRange(headers.ToArray());
            SetNewColumnVisibility();
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
            if(NumDivisor.Value == 0)
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Division durch 0 nicht möglich!");
            }
            else
            {
                Proc = new ProcDivide(SelectedHeaders, NumDivisor.Value, txtHeader.Text, cbOldColumn.Checked, checkBoxDecimals.Checked);
                DialogResult = DialogResult.OK;
            }
        }
    }
}