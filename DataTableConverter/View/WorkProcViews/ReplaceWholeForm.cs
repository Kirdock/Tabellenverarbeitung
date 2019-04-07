using CheckComboBoxTest;
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
    public partial class ReplaceWholeForm : Form
    {
        internal string ReplaceText => txtText.Text;
        internal string[] SelectedHeaders => ViewHelper.GetSelectedHeaders(cbHeaders);

        internal ReplaceWholeForm(object[] headers)
        {
            InitializeComponent();
            cbHeaders.Items.AddRange(headers);
        }

        private void txtFormula_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                CloseForm();
            }
        }

        private void CloseForm()
        {
            DialogResult = DialogResult.OK;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            CloseForm();
        }
    }
}
