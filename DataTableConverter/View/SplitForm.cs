using System;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SplitForm : Form
    {
        internal SplitForm(object[] headers)
        {
            InitializeComponent();
            cmbHeaders.Items.AddRange(headers);
            cmbHeaders.SelectedIndex = 0;
        }

        internal string getSplitString()
        {
            return txtSplitString.Text;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (txtSplitString.Text.Length == 0)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Länge von 0 ist ungültig!");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        internal string getSelectedHeader()
        {
            return cmbHeaders.SelectedItem.ToString();
        }
    }
}
