using DataTableConverter.Classes.WorkProcs;
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
    public partial class UpLowCaseForm : Form
    {
        internal ProcUpLowCase Procedure;
        internal UpLowCaseForm(object[] header)
        {
            InitializeComponent();
            cmbOption.SelectedIndex = 0;
            clbHeaders.Items.AddRange(header);
        }


        internal bool allColumns()
        {
            return cbAllColumns.Checked;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if(clbHeaders.CheckedItems.Count > 0 || allColumns())
            {
                Procedure = new ProcUpLowCase(clbHeaders.CheckedItems.Cast<string>().ToArray(), allColumns(), cmbOption.SelectedIndex);
                DialogResult = DialogResult.OK;
            }
        }

        private void cbAllColumns_CheckedChanged(object sender, EventArgs e)
        {
            clbHeaders.Enabled = !cbAllColumns.Checked;
        }
    }
}
