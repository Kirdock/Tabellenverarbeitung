using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class MergeTable : Form
    {
        private bool SameRowCount;
        internal IEnumerable<string> SelectedColumns => clbColumns.CheckedItems.Cast<KeyValuePair<string,string>>().Select(x => x.Value);
        internal string OriginalIdentifierColumnName => cmbIdentifierOriginal.SelectedValue.ToString();
        internal string ImportIdentifierColumnName => cmbIdentifierMerge.SelectedValue.ToString();


        internal MergeTable(Dictionary<string,string> originalHeaders, Dictionary<string,string> importHeaders, string filename, int sourceCount, int importCount)
        {
            //key: alias, value: columnName
            InitializeComponent();
            
            SetListBoxStyle();
            SetCmbItems(cmbIdentifierOriginal, originalHeaders);
            SetCmbItems(cmbIdentifierMerge, importHeaders);
            SetListItems(importHeaders);
            lblImportTable.Text = filename;

            lblRowCountImport.ForeColor = lblRowCountSource.ForeColor = lblSourceTable.ForeColor = lblImportTableText.ForeColor = sourceCount == importCount ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            lblRowCountImport.Text = importCount.ToString();
            lblRowCountSource.Text = sourceCount.ToString();
            SameRowCount = sourceCount == importCount;
            MarkAll(true);
        }

        private void SetListBoxStyle()
        {
            ViewHelper.SetListBoxStyle(clbColumns);
        }

        private void SetListItems(Dictionary<string,string> items)
        {
            ListBox box = clbColumns;
            box.DataSource = new BindingSource(items, null);
            box.DisplayMember = "key";
            box.ValueMember = "value";
        }

        private void SetCmbItems(ComboBox cmb, Dictionary<string,string> items)
        {
            cmb.DataSource = new BindingSource(items, null);
            cmb.DisplayMember = "key";
            cmb.ValueMember = "value";
            cmb.SelectedIndex = 0;
        }

        private void btnTakeOver_Click(object sender, EventArgs e)
        {
            if (!SameRowCount)
            {
                DialogResult result = this.MessagesYesNoCancel(MessageBoxIcon.Warning, "Die Zeilenanzahl der beiden Tabellen stimmt nicht überein! Trotzdem fortfahren?");
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
        

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            MarkAll(true);
        }

        private void MarkAll(bool status)
        {
            for (int i = 0; i < clbColumns.Items.Count; i++)
            {
                clbColumns.SetItemChecked(i, status);
            }
        }

        private void BtnRemoveAll_Click(object sender, EventArgs e)
        {
            MarkAll(false);
        }
    }
}
