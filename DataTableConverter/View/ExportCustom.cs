using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class ExportCustom : Form
    {
        internal IEnumerable<ExportCustomItem> SelectedColumnItems => CmBFileNames.Items.Cast<ExportCustomItem>().Where(item => item.Column == cmbColumn.SelectedItem.ToString());
        internal IEnumerable<ExportCustomItem> Items => CmBFileNames.Items.Cast<ExportCustomItem>();
        private readonly DataTable Table;
        private Dictionary<string, Dictionary<string, int>> CacheDataTableGroupCount;
        private ExportCustomItem SelectedItem => (CmBFileNames.SelectedItem as ExportCustomItem);
        internal ExportCustom(object[] headers, DataTable table)
        {
            InitializeComponent();
            clbValues.Dict = SelectedColumnItems;
            CacheDataTableGroupCount = new Dictionary<string, Dictionary<string, int>>();
            SetListBoxStyle();
            Table = table;
            cmbColumn.Items.AddRange(headers);
            
            SetEnabled();
        }

        private void SetListBoxStyle()
        {
            ViewHelper.SetListBoxStyle(clbValues);
        }

        private void SetEnabled()
        {
            bool enabled = CmBFileNames.SelectedIndex != -1;
            Control[] controls = new Control[]
            {
                cmbColumn,
                CmBFormat,
                clbValues,
                CbSaveAll,
                CmBFileNames,
                BtnCheckAll,
                BtnUncheckAll
            };

            foreach(Control control in controls)
            {
                control.Enabled = enabled;
            }
            clbValues.Enabled = enabled && !CbSaveAll.Checked;
        }

        private void CloseConfirmed()
        {
            DialogResult = DialogResult.OK;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            CloseConfirmed();
        }

        private void CbSaveAll_CheckedChanged(object sender, EventArgs e)
        {
            SetEnabled();
            SelectedItem.CheckedAllValues = CbSaveAll.Checked;
        }

        private void cmbColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmBFileNames.SelectedIndex > -1)
            {
                SelectedItem.Column = cmbColumn.SelectedItem.ToString();
                SetListValues();
                SetValues(false);
                SetSumCount();
            }
        }

        private void SetListValues()
        {
            string identifier = cmbColumn.SelectedItem.ToString();

            Dictionary<string, int> pair;
            if (CacheDataTableGroupCount.ContainsKey(identifier))
            {
                pair = CacheDataTableGroupCount[identifier];
            }
            else
            {
                pair = Table.GroupCountOfColumn(Table.Columns.IndexOf(cmbColumn.SelectedItem.ToString()));
                CacheDataTableGroupCount.Add(identifier, pair);
            }
            clbValues.BeginUpdate();
            clbValues.Items.Clear();
            foreach (string key in pair.Keys.OrderBy(key => key, new NaturalStringComparer(SortOrder.Ascending)))
            {
                clbValues.Items.Add(new CountListboxItem(pair[key], key));
            }
            clbValues.EndUpdate();
            ViewHelper.ResizeCountListBox(clbValues);
        }

        private void btnDeleteFile_Click(object sender, EventArgs e)
        {
            if(CmBFileNames.SelectedIndex != -1)
            {
                CmBFileNames.Items.RemoveAt(CmBFileNames.SelectedIndex);
                if (CmBFileNames.Items.Count > 0)
                {
                    CmBFileNames.SelectedIndex = 0;
                }
                SetEnabled();
            }
        }

        private void clbValues_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string changedValue = clbValues.Items[e.Index].ToString();
            if (Properties.Settings.Default.SeparateSelectable)
            {
                SelectedItem.Values[changedValue] = e.NewValue == CheckState.Checked;
                SetSumCount((e.NewValue == CheckState.Checked ? 1 : -1) * (clbValues.Items[e.Index] as CountListboxItem).Count);
            }
            else if (e.CurrentValue == CheckState.Checked || !ListContainsCustomExportItem(changedValue))
            {
                SelectedItem.Values[changedValue] = e.NewValue == CheckState.Checked;
                SetSumCount((e.NewValue == CheckState.Checked ? 1 : -1) * (clbValues.Items[e.Index] as CountListboxItem).Count);
            }
            else
            {
                clbValues.BeginInvoke(new MethodInvoker(() =>
                {
                    clbValues.ItemCheck -= clbValues_ItemCheck;
                    clbValues.SetItemCheckState(e.Index, e.CurrentValue);
                    clbValues.ItemCheck += clbValues_ItemCheck;
                }));

            }
        }

        private bool ListContainsCustomExportItem(string value)
        {
            return SelectedColumnItems.SelectMany(item => item.SelectedValues).Contains(value);
        }

        private void CmBFileNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbColumn.SelectedIndexChanged -= cmbColumn_SelectedIndexChanged;
            cmbColumn.SelectedItem = SelectedItem.Column;
            cmbColumn.SelectedIndexChanged += cmbColumn_SelectedIndexChanged;

            CmBFormat.SelectedIndex = SelectedItem.Format;
            CbSaveAll.Checked = SelectedItem.CheckedAllValues;
            SetListValues();
            clbValues.ItemCheck -= clbValues_ItemCheck;

            for (int i = 0; i < clbValues.Items.Count; i++)
            {
                clbValues.SetItemChecked(i, SelectedItem.Values[clbValues.Items[i].ToString()]);
            }
            
            clbValues.ItemCheck += clbValues_ItemCheck;

            SetSumCount();
        }

        private void SetSumCount(int add = 0)
        {
            lblSumCount.Text = (clbValues.CheckedItems.Cast<CountListboxItem>().Sum(item => item.Count) + add).ToString();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string newText = Microsoft.VisualBasic.Interaction.InputBox("Bitte Dateinamen eingeben", "Dateiname", string.Empty);
            if (!string.IsNullOrWhiteSpace(newText))
            {
                if (CmBFileNames.Items.Cast<ExportCustomItem>().Select(item => item.Name).Contains(newText))
                {
                    this.MessagesOK(MessageBoxIcon.Warning, "Der Dateiname ist bereits vergeben!");
                    BtnAdd_Click(null, null);
                }
                else
                {
                    var item = new ExportCustomItem(newText, cmbColumn.Items[0].ToString());
                    cmbColumn.SelectedIndexChanged -= cmbColumn_SelectedIndexChanged;
                    cmbColumn.SelectedIndex = 0;
                    cmbColumn.SelectedIndexChanged += cmbColumn_SelectedIndexChanged;
                    SetListValues();
                    SetValues(false, item);
                    int index = CmBFileNames.Items.Count;
                    CmBFileNames.Items.Add(item);
                    CmBFileNames.SelectedIndex = index;
                    CmBFormat.SelectedIndex = cmbColumn.SelectedIndex = 0;
                    SetEnabled();
                }
            }
        }

        private void SetValues(bool status, ExportCustomItem i = null)
        {
            ExportCustomItem item = i ?? SelectedItem;
            item.SetValues(clbValues.Items.Cast<CountListboxItem>().Select(x => x.ToString()), status, SelectedColumnItems);
        }

        private void CmBFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedItem.Format = (sender as ComboBox).SelectedIndex;
        }

        private void BtnRename_Click(object sender, EventArgs e)
        {
            if(CmBFileNames.SelectedIndex > -1)
            {
                string newText = Microsoft.VisualBasic.Interaction.InputBox("Bitte den Dateinamen eingeben", "Dateiname", SelectedItem.Name);
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    if (newText != SelectedItem.Name && CmBFileNames.Items.Cast<ExportCustomItem>().Select(item => item.Name).Contains(newText))
                    {
                        this.MessagesOK(MessageBoxIcon.Warning, "Der Dateiname ist bereits vergeben!");
                        BtnRename_Click(null, null);
                    }
                    else
                    {
                        SelectedItem.Name = newText;
                        CmBFileNames.Items[CmBFileNames.SelectedIndex] = SelectedItem;
                    }
                }
            }
        }

        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            SetValues(true);
            CmBFileNames_SelectedIndexChanged(null, null);
        }

        private void BtnUncheckAll_Click(object sender, EventArgs e)
        {
            SetValues(false);
            CmBFileNames_SelectedIndexChanged(null, null);
        }
    }
}
