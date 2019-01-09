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
    public partial class RoundForm : Form
    {
        internal RoundForm(object[] headers)
        {
            InitializeComponent();
            cbHeaders.Items.AddRange(headers);
            SetNewColumnVisibility();
        }

        internal string[] GetSelectedHeaders()
        {
            return ViewHelper.GetSelectedHeaders(cbHeaders);
        }

        internal int GetDecimals()
        {
            return (int)numDec.Value;
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
            if (!txtHeader.Visible)
            {
                txtHeader.Text = string.Empty;
            }
        }

        private void SetNewColumnVisibility()
        {
            lblHeader.Visible = txtHeader.Visible = cbNewColumn.Checked;
        }

        internal string NewColumn()
        {
            return txtHeader.Text;
        }
    }
}
