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
        private bool SameRowCount;

        internal MergeTable(object[] headersOriginal, object[] headersMerge, string filename, int sourceCount, int importCount)
        {
            InitializeComponent();
            SetListBoxStyle();
            Headers = headersOriginal;
            setCmbItems(cmbIdentifierOriginal, headersOriginal);
            setCmbItems(cmbIdentifierMerge, headersMerge);
            setListItems(headersMerge);
            lblImportTable.Text = filename;

            lblRowCountImport.ForeColor = lblRowCountSource.ForeColor = lblSourceTable.ForeColor = lblImportTableText.ForeColor = sourceCount == importCount ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            lblRowCountImport.Text = importCount.ToString();
            lblRowCountSource.Text = sourceCount.ToString();
            SameRowCount = sourceCount == importCount;
            markAll(true);
        }

        private void SetListBoxStyle()
        {
            ViewHelper.SetListBoxStyle(clbColumns);
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
            if (!SameRowCount)
            {
                DialogResult result = MessageHandler.MessagesYesNoCancel(MessageBoxIcon.Warning, "Die Zeilenanzahl der beiden Tabellen stimmt nicht überein! Trotzdem fortfahren?");
                if(result != DialogResult.Abort)
                {
                    DialogResult = result;
                }
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
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
    }
}
