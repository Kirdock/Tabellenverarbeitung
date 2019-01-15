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
        internal List<string> AdditionalColumns { get
            {
                List<string> items = ClBHeaders.CheckedItems.Cast<string>().ToList();
                items.Remove(Identifier);
                return items;
            }
        }

        internal MergeColumns(object[] headers)
        {
            InitializeComponent();
            CmBHeaders.Items.AddRange(headers);
            ClBHeaders.Items.AddRange(headers);
            CmBHeaders.SelectedIndex = 0;
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
    }
}
