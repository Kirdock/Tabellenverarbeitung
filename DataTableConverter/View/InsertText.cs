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
    public partial class InsertText : Form
    {
        internal string NewText => TxTName.Text;

        internal InsertText(string oldName = "")
        {
            InitializeComponent();
            TxTName.Text = oldName;
        }

        private void TxTName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
