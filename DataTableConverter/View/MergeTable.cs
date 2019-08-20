using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class MergeTable : Form
    {
        private object[] Headers;

        internal MergeTable(object[] headersOriginal, object[] headersMerge, string filename, int sourceCount, int importCount)
        {
            InitializeComponent();
            SetListBoxStyle();
            Headers = headersOriginal;
            setCmbItems(cmbIdentifierOriginal, headersOriginal);
            setCmbItems(cmbIdentifierMerge, headersMerge);
            setListItems(headersMerge);
            setSettings();
            setOrderVisibility(chbRememberOrder.Checked);
            lblImportTable.Text = filename;

            lblRowCountImport.ForeColor = lblRowCountSource.ForeColor = lblSourceTable.ForeColor = lblImportTableText.ForeColor = sourceCount == importCount ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            lblRowCountImport.Text = importCount.ToString();
            lblRowCountSource.Text = sourceCount.ToString();
        }

        private void SetListBoxStyle()
        {
            ViewHelper.SetListBoxStyle(clbColumns);
        }

        private void setSettings()
        {
            chbRememberOrder.Checked = Properties.Settings.Default.RememberOrder;
        }

        private void saveSettings()
        {
            Properties.Settings.Default.RememberOrder = chbRememberOrder.Checked;
            if (chbRememberOrder.Checked)
            {
                Properties.Settings.Default.RememberOrderName = txtOrder.Text;
            }
            Properties.Settings.Default.Save();
        }

        private void setListItems(object[] items)
        {
            clbColumns.Items.AddRange(items);
        }

        private void setCmbItems(ComboBox cmb, object[] items)
        {
            cmb.Items.AddRange(items);
            cmb.SelectedIndex = 0;
        }

        internal int getSelectedOriginal()
        {
            return cmbIdentifierOriginal.SelectedIndex;
        }

        internal int getSelectedMerge()
        {
            return cmbIdentifierMerge.SelectedIndex;
        }

        internal string[] getSelectedColumns()
        {
            return clbColumns.CheckedItems.Cast<object>()
                                 .Select(x => x.ToString())
                                 //.Where(x => x != cmbIdentifierMerge.SelectedItem.ToString())
                                 .ToArray();
        }

        private void btnTakeOver_Click(object sender, EventArgs e)
        {
            bool valid = true;
            if (chbRememberOrder.Checked)
            {
                if (txtOrder.Text == string.Empty)
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Der Spaltenname darf nicht leer sein!");
                    valid = false;
                }
                else if (Headers.Any(s => s.ToString().Equals(txtOrder.Text, StringComparison.OrdinalIgnoreCase)) || getSelectedColumns().Any(s => s.Equals(txtOrder.Text, StringComparison.OrdinalIgnoreCase))){
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Dieser Spaltenname ist bereits vergeben.\nBitte wählen Sie einen anderen");
                    valid = false;
                }
            }
            if(valid)
            {
                closeForm();
            }
        }

        private void closeForm()
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            markAll(true);
        }

        private void markAll(bool status)
        {
            for (int i = 0; i < clbColumns.Items.Count; i++)
            {
                clbColumns.SetItemChecked(i, status);
            }
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            markAll(false);
        }

        private void chbRememberOrder_CheckedChanged(object sender, EventArgs e)
        {
            setOrderVisibility(chbRememberOrder.Checked);
            if (!chbRememberOrder.Checked)
            {
                txtOrder.Text = string.Empty;
            }
            else
            {
                txtOrder.Text = Properties.Settings.Default.RememberOrderName;
            }
        }

        internal string OrderColumnName()
        {
            return txtOrder.Text;
        }

        private void setOrderVisibility(bool status)
        {
            lblOrder.Visible = txtOrder.Visible = status;
        }

        private void MergeTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveSettings();
        }
    }
}
