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
        internal ExportCustom(object[] headers)
        {
            InitializeComponent();
            cmbColumn.Items.AddRange(headers);
            cmbColumn.SelectedIndex = 0;
            SetData();
        }

        private void SetData()
        {
            CmBFormat.SelectedIndex = Properties.Settings.Default.ExportCustomFormat;
            txtSearch.Text = Properties.Settings.Default.ExportCustomText;
            CbSaveAll.Checked = Properties.Settings.Default.ExportCustomCheck;
            CbSaveAll_CheckedChanged(null, null);
        }

        internal int getColumnIndex()
        {
            return cmbColumn.SelectedIndex;
        }

        internal string getValue()
        {
            return txtSearch.Text;
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
            txtSearch.ReadOnly = CbSaveAll.Checked;
        }

        internal bool AllValues()
        {
            return CbSaveAll.Checked;
        }

        private void ExportCustom_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ExportCustomFormat = CmBFormat.SelectedIndex;
            Properties.Settings.Default.ExportCustomText = txtSearch.Text;
            Properties.Settings.Default.ExportCustomCheck = CbSaveAll.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
