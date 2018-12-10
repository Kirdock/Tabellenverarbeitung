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
    public partial class SetString : Form
    {
        public SetString()
        {
            InitializeComponent();
        }

        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
            }
        }

        internal string getName()
        {
            return txtName.Text;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text))
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
