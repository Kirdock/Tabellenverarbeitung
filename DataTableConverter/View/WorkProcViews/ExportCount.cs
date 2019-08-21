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

        public ExportCount(object[] headers)
        {
            InitializeComponent();
            cmbColumn.Items.AddRange(headers);
            cmbColumn.SelectedIndex = 0;
            cbShowFromTo.Checked = Properties.Settings.Default.CountFromTo;
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
            Properties.Settings.Default.Save();
        }
    }
}
