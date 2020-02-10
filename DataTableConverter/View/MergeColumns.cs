using DataTableConverter.Classes;
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
    public partial class MergeColumns : Form
    {
        internal string Identifier => CmBHeaders.SelectedItem.ToString();
        internal int IdentifierIndex => CmBHeaders.SelectedIndex;
        internal List<PlusListboxItem> AdditionalColumns { get
            {
                return ClBHeaders.CheckedItems.Cast<PlusListboxItem>().Where(item => item.ToString() != Identifier).ToList();
            }
        }

        internal MergeColumns(object[] headers)
        {
            InitializeComponent();
            SetListBoxStyle();
            CmBHeaders.Items.AddRange(headers);
            List<PlusListboxItem> list = new List<PlusListboxItem>();
            foreach(string header in headers)
            {
                list.Add(new PlusListboxItem(header));
            }
            ClBHeaders.Items.AddRange(list.ToArray());
            CmBHeaders.SelectedIndex = 0;
            ViewHelper.ResizePlusListBox(ClBHeaders);
        }

        private void SetListBoxStyle()
        {
            ViewHelper.SetListBoxStyle(ClBHeaders);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if(ClBHeaders.CheckedIndices.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            SetChecked(true);
        }

        private void SetChecked(bool status)
        {
            for (int i = 0; i < ClBHeaders.Items.Count; i++)
            {
                ClBHeaders.SetItemChecked(i, status);
            }
        }

        private void BtnUncheckAll_Click(object sender, EventArgs e)
        {
            SetChecked(false);
        }

        private void ClBHeaders_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = ClBHeaders.IndexFromPoint(e.Location);
                if (index != -1)
                {
                    (ClBHeaders.Items[index] as PlusListboxItem).Next();
                    ClBHeaders.Refresh();
                }
            }
        }
    }
}
