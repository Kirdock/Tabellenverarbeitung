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
        internal ExportCustom(object[] headers)
        {
            InitializeComponent();
            cmbColumn.Items.AddRange(headers);
            cmbColumn.SelectedIndex = 0;
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
    }
}
