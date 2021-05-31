using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class Formula : Form
    {
        internal bool OldColumn => cbOldColumn.Checked;

        internal Formula(IEnumerable<string> aliases)
        {
            InitializeComponent();
            SetNewColumnVisibility();

            cbHeaders.Items.AddRange(aliases.ToArray());
        }

        internal string[] SelectedHeaders()
        {
            return ViewHelper.GetSelectedHeaders(cbHeaders);
        }

        private void SetNewColumnVisibility()
        {
            lblHeader.Visible = txtHeader.Visible = cbNewColumn.Checked;
        }

        internal string HeaderName()
        {
            return txtHeader.Text;
        }


        private void txtFormula_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CloseForm();
            }
        }

        private void CloseForm()
        {
            if (cbNewColumn.Checked && string.IsNullOrWhiteSpace(txtHeader.Text))
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an!");
            }
            else if (cbNewColumn.Checked && IsDuplicate(txtHeader.Text))
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Spalte mit diesem Namen.\nBitte geben Sie einen anderen an");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private bool IsDuplicate(string text)
        {
            return cbHeaders.Items.Cast<string>().Contains(text, StringComparer.OrdinalIgnoreCase);
        }

        private void cbNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            SetNewColumnVisibility();
            cbOldColumn.Visible = !cbNewColumn.Checked;
            if (!cbNewColumn.Checked)
            {
                txtHeader.Text = string.Empty;
            }
            else
            {
                cbOldColumn.Checked = false;
            }
        }

        private void Formula_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ExportSpec = new System.Collections.ArrayList(cbHeaders.CheckedItems.Cast<string>().ToArray());
            Properties.Settings.Default.Save();
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, true);
        }


        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, false);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            CloseForm();
        }
    }
}
