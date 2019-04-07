﻿using DataTableConverter.Assisstant;
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
        internal int SelectedFormat { get { return CmBFormat.SelectedIndex; } }
        private readonly DataTable Table;
        internal Dictionary<string, List<string>> Files { get; set; }
        internal ExportCustom(object[] headers, DataTable table)
        {
            InitializeComponent();
            Files = new Dictionary<string, List<string>>();
            Table = table;
            cmbColumn.Items.AddRange(headers);
            cmbColumn.SelectedIndex = 0;
            SetData();
        }

        private void SetData()
        {
            CmBFormat.SelectedIndex = Properties.Settings.Default.ExportCustomFormat;
            CbSaveAll.Checked = Properties.Settings.Default.ExportCustomCheck;
            CbSaveAll_CheckedChanged(null, null);
        }

        internal int getColumnIndex()
        {
            return cmbColumn.SelectedIndex;
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                closeConfirmed();
            }
        }

        private void closeConfirmed()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            closeConfirmed();
        }

        private void CbSaveAll_CheckedChanged(object sender, EventArgs e)
        {
            gbFiles.Enabled = !CbSaveAll.Checked;
        }

        internal bool AllValues()
        {
            return CbSaveAll.Checked;
        }

        private void ExportCustom_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ExportCustomFormat = CmBFormat.SelectedIndex;
            Properties.Settings.Default.ExportCustomCheck = CbSaveAll.Checked;
            Properties.Settings.Default.Save();
        }

        internal List<string> ValuesOfColumn (){
            return clbValues.Items.Cast<string>().ToList();
        }

        private void cmbColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            clbValues.Items.Clear();
            clbValues.Items.AddRange(DataHelper.GetValuesOfColumnDistrinct(cmbColumn.SelectedItem.ToString(), Table));

            foreach(List<string> value in Files.Values)
            {
                value.Clear();
            }
        }

        private void cmbFiles_KeyDown(object sender, KeyEventArgs e)
        {
            int index = -1;
            if(e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(cmbFiles.Text) && !((index = cmbFiles.Items.IndexOf(cmbFiles.Text)) > -1))
            {
                AddFile(cmbFiles.Text);
            }
            if(index > -1)
            {
                cmbColumn.SelectedIndex = index;
            }
        }

        private void AddFile(string name)
        {
            cmbFiles.Items.Add(name);
            Files.Add(name, new List<string>());

            cmbFiles.SelectedIndex = cmbFiles.Items.IndexOf(name);
        }

        private void btnDeleteFile_Click(object sender, EventArgs e)
        {
            int index;
            if((index = cmbFiles.Items.IndexOf(cmbFiles.Text)) != -1)
            {
                Files.Remove(cmbFiles.Text);
                cmbFiles.Items.RemoveAt(index);

                ResetCheckedListBoxValues(clbValues, clbValues_ItemCheck);
                if (cmbFiles.Items.Count > 0)
                {
                    cmbFiles.SelectedIndex = 0;
                }
                else
                {
                    cmbFiles.Text = string.Empty;
                }
            }
        }

        private void clbValues_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(Files.TryGetValue(cmbFiles.Text, out List<string> values))
            {
                string changedValue = clbValues.Items[e.Index].ToString();
                if (e.NewValue == CheckState.Checked)
                {
                    values.Add(changedValue);
                }
                else
                {
                    values.Remove(changedValue);
                }
            }
            else if(!string.IsNullOrWhiteSpace(cmbFiles.Text))
            {
                AddFile(cmbFiles.Text);
                clbValues_ItemCheck(sender, e);
            }
        }

        private void cmbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetCheckedListBoxValues(clbValues, clbValues_ItemCheck);
            if (Files.TryGetValue(cmbFiles.Text, out List<string> values))
            {
                clbValues.ItemCheck -= clbValues_ItemCheck;
                foreach (string value in values) {
                    int index;
                    if((index = clbValues.Items.IndexOf(value)) > -1)
                    {
                        clbValues.SetItemChecked(index, true);
                    }
                }
                clbValues.ItemCheck += clbValues_ItemCheck;
            }
        }

        private void ResetCheckedListBoxValues(CheckedListBox box, ItemCheckEventHandler handler)
        {
            box.ItemCheck -= handler;
            foreach (int i in box.CheckedIndices)
            {
                box.SetItemChecked(i, false);
            }
            box.ItemCheck += handler;
        }

        private void cmbFiles_TextChanged(object sender, EventArgs e)
        {
            cmbFiles_SelectedIndexChanged(null, null);
        }
    }
}
