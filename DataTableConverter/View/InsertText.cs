using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class InsertText : Form
    {
        internal string NewText => TxTName.Text;

        internal InsertText(string header, string description, string oldName = "")
        {
            InitializeComponent();
            Text = header;
            LblDescription.Text = description;
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
