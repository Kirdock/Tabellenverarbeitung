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
        internal int MinLength  {
            get
            {
                return CBMinLength.Checked ? (int)NumMinLength.Value : -1;
            }
        }
        internal string Column => CmBHeaders.SelectedItem.ToString();

        public MaxRowLengthForm(object[] headers)
        {
            InitializeComponent();
            CBMinLength_CheckedChanged(null, null);
            CmBHeaders.Items.AddRange(headers);
            CmBHeaders.SelectedIndex = 0;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            bool shortcutEmpty = string.IsNullOrWhiteSpace(TxtShortcut.Text);
            bool newColumnEmpty = string.IsNullOrWhiteSpace(TxtNewColumn.Text);
            if (shortcutEmpty && !newColumnEmpty || newColumnEmpty && !shortcutEmpty)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte füllen Sie entweder \"Name der neuen Spalte\" und \"Kürzel\" oder keinen dieser Werte aus");
            }
            else if((shortcutEmpty || newColumnEmpty) && CBMinLength.Checked)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte füllen Sie \"Name der neuen Spalte\" und \"Kürzel\" aus");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void CBMinLength_CheckedChanged(object sender, EventArgs e)
        {
            NumMinLength.Visible = LblColumn.Visible = CmBHeaders.Visible = CBMinLength.Checked;
        }
    }
}
