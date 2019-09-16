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
    public partial class ExportCount : Form
    {
        internal bool CountChecked => cbCount.Checked;
        internal int Count => (int)nbCount.Value;
        internal bool ShowFromTo => cbShowFromTo.Checked;
        internal DataTable Table;

        public ExportCount(object[] headers, DataTable table)
        {
            InitializeComponent();
            Table = table;
            SetHeaders(headers);
            cbShowFromTo.Checked = Properties.Settings.Default.CountFromTo;
            SetCheckedType();
        }

        private void SetHeaders(object[] headers)
        {
            ComboBox[] comboBoxes = new ComboBox[] { cmbColumn, CmbSecondFirstColumn, cmbSecondSecondColumn };
            CmbSecondFirstColumn.SelectedIndexChanged -= CmbSecondFirstColumn_SelectedIndexChanged;
            cmbSecondSecondColumn.SelectedIndexChanged -= CmbSecondFirstColumn_SelectedIndexChanged;
            foreach (ComboBox comboBox in comboBoxes)
            {
                comboBox.Items.AddRange(headers);
                comboBox.SelectedIndex = 0;
            }

            CmbSecondFirstColumn.SelectedIndexChanged += CmbSecondFirstColumn_SelectedIndexChanged;
            cmbSecondSecondColumn.SelectedIndexChanged += CmbSecondFirstColumn_SelectedIndexChanged;
            CmbSecondFirstColumn_SelectedIndexChanged(null, null);
        }

        private void SetCheckedType()
        {
            RbOneColumn.Checked = Properties.Settings.Default.CountSelectedType == 0;
            RbTwoColumns.Checked = !RbOneColumn.Checked;
            //RbColumn_CheckedChanged(null, null);
        }

        internal string getSelectedValue()
        {
            return cmbColumn.SelectedItem.ToString();
        }

        internal int getColumnIndex()
        {
            return cmbColumn.SelectedIndex;
        }

        private void cbCount_CheckedChanged(object sender, EventArgs e)
        {
            nbCount.Visible = cbCount.Checked;
        }

        private void ExportCount_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.CountFromTo = cbShowFromTo.Checked;
            Properties.Settings.Default.CountSelectedType = RbOneColumn.Checked ? 0 : 1;
            Properties.Settings.Default.Save();
        }

        private void RbColumn_CheckedChanged(object sender, EventArgs e)
        {
            GbOneColumn.Visible = RbOneColumn.Checked;
            GbTwoColumns.Visible = !RbOneColumn.Checked;
        }

        private void CmbSecondFirstColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            LblCount.Text = Table.AsEnumerable().Count(row => row[CmbSecondFirstColumn.SelectedItem.ToString()].ToString() == row[cmbSecondSecondColumn.SelectedItem.ToString()].ToString()).ToString();
        }
    }
}
