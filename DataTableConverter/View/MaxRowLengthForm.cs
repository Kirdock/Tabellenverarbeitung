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
    public partial class MaxRowLengthForm : Form
    {
        internal string Shortcut => TxtShortcut.Text;
        internal string NewColumn => TxtNewColumn.Text;
        public MaxRowLengthForm()
        {
            InitializeComponent();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            bool shortcutEmpty = string.IsNullOrWhiteSpace(TxtShortcut.Text);
            bool newColumnEmpty = string.IsNullOrWhiteSpace(TxtNewColumn.Text);
            if (shortcutEmpty && !newColumnEmpty || newColumnEmpty && !shortcutEmpty)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte füllen Sie entweder beide oder keine Werte aus");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
