using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View.WorkProcViews
{
    public partial class SeparateLoadEntries : Form
    {
        private string TableName;
        private IEnumerable<ExportCustomItem> items;
        internal ExportCustomItem SelectedItem;
        private IEnumerable<ExportCustomItem> Items => items.Where(item => item != null && item.Column == CmBColumns.SelectedItem.ToString());
        private readonly DatabaseHelper DatabaseHelper;

        internal SeparateLoadEntries(DatabaseHelper databaseHelper, ExportSeparate selectedItem, BindingList<ExportSeparate> items, object[] headers, string tableName)
        {
            InitializeComponent();
            DatabaseHelper = databaseHelper;
            TableName = tableName;
            SelectedItem = new ExportCustomItem(selectedItem.Name, selectedItem.Column);
            this.items = items.Where(item => item != selectedItem).Select(item => new ExportCustomItem(item.Name, item.Column, item.Table.AsEnumerable().Select(row => row[0].ToString()))).Concat(new ExportCustomItem[] { SelectedItem });
            CLBValues.Dict = Items;
            CmBColumns.Items.AddRange(headers);
            int index = CmBColumns.Items.IndexOf(selectedItem.Column);
            CmBColumns.SelectedIndex = index > -1 ? index : 0;
            ViewHelper.SetListBoxStyle(CLBValues);
            LoadHeaders(selectedItem.Table.AsEnumerable().Select(row => row[0].ToString()));
        }

        private void LoadHeaders(IEnumerable<string> headers)
        {
            CLBValues.ItemCheck -= CLBValues_ItemCheck;
            for (int i = 0; i < CLBValues.Items.Count; i++)
            {
                CountListboxItem item = CLBValues.Items[i] as CountListboxItem;
                if (headers.Contains(item.Value))
                {
                    CLBValues.SetItemChecked(i, true);
                    SelectedItem.Values[item.Value.ToString()] = true;
                }
            }
            CLBValues.ItemCheck += CLBValues_ItemCheck;
        }

        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            SetValues(true);
            SetCheckedListBox();
            LoadChecked();
        }

        private void BtnUncheckAll_Click(object sender, EventArgs e)
        {
            SetValues(false);
            SetCheckedListBox();
            LoadChecked();
        }

        private void LoadChecked()
        {
            CLBValues.ItemCheck -= CLBValues_ItemCheck;

            for (int i = 0; i < CLBValues.Items.Count; i++)
            {
                CLBValues.SetItemChecked(i, SelectedItem.Values[CLBValues.Items[i].ToString()]);
            }

            CLBValues.ItemCheck += CLBValues_ItemCheck;
        }

        private void SetCheckedListBox()
        {
            try
            {
                string alias = CmBColumns.SelectedItem.ToString(); //DataSource = AliasColumnMapping? NO, because it is used for a workflow
                Dictionary<string, int> pair = DatabaseHelper.GroupCountOfColumn(DatabaseHelper.GetColumnName(alias, TableName), TableName);

                CLBValues.BeginUpdate();
                CLBValues.Items.Clear();
                foreach (string key in pair.Keys)
                {
                    CLBValues.Items.Add(new CountListboxItem(0, key));
                }
                CLBValues.EndUpdate();

                ViewHelper.ResizeCountListBox(CLBValues);
            }
            catch { }
        }

        private void CmBColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            CLBValues.Items.Clear();
            SelectedItem.Column = CmBColumns.SelectedItem.ToString();
            SetCheckedListBox();
            SetValues(false);
        }

        private void SetValues(bool status, ExportCustomItem i = null)
        {
            ExportCustomItem item = i ?? SelectedItem;
            item.SetValues(CLBValues.Items.Cast<CountListboxItem>().Select(x => x.ToString()), status, Items);
        }

        private void CLBValues_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string changedValue = CLBValues.Items[e.Index].ToString();
            if (Properties.Settings.Default.SeparateSelectable)
            {
                SelectedItem.Values[changedValue] = e.NewValue == CheckState.Checked;
            }
            else if (e.CurrentValue == CheckState.Checked || !ListContainsCustomExportItem(changedValue))
            {
                SelectedItem.Values[changedValue] = e.NewValue == CheckState.Checked;
            }
            else
            {
                CLBValues.BeginInvoke(new MethodInvoker(() =>
                {
                    CLBValues.ItemCheck -= CLBValues_ItemCheck;
                    CLBValues.SetItemCheckState(e.Index, e.CurrentValue);
                    CLBValues.ItemCheck += CLBValues_ItemCheck;
                }));

            }
        }

        private bool ListContainsCustomExportItem(string value)
        {
            return Items.SelectMany(item => item.SelectedValues).Contains(value);
        }
    }
}
