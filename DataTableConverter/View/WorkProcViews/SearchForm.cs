using System;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View.WorkProcViews
{
    public partial class SearchForm : Form
    {
        internal string SearchText => TxtSearchText.Text;
        internal string Header => CmBHeader.SelectedItem.ToString();
        internal int From => (int)NbSearchFrom.Value;
        internal int To => (int)NbSearchTo.Value;
        internal string NewColumn => TxtSearchNewColumn.Text;
        internal bool CheckTotal => CBTotal.Checked;
        internal string Shortcut => TxtShortcut.Text;
        internal bool FromToSelected => RbFromTo.Checked;
        private string[] Headers;

        internal SearchForm(string[] headers)
        {
            InitializeComponent();
            Headers = headers;
            CmBHeader.Items.AddRange(headers);
            CmBHeader.SelectedIndex = 0;
            RbFromTo.Checked = true;
        }

        private void bestätigenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtSearchNewColumn.Text))
            {
                if (RbFromTo.Checked && NbSearchFrom.Value > NbSearchTo.Value)
                {
                    MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Startnummer muss kleiner als Endnummer sein!");
                }
                else if (RbShortcut.Checked && string.IsNullOrWhiteSpace(TxtShortcut.Text))
                {
                    MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Bitte geben Sie eine Kennung ein.");
                }
                else if (Headers.Contains(TxtSearchNewColumn.Text))
                {
                    DialogResult res = MessageHandler.MessagesYesNoCancel(this, MessageBoxIcon.Warning, "Es gibt bereits eine Spalte mit derselben Bezeichnung.\nTrotzdem ausführen?");
                    if (res == DialogResult.Yes)
                    {
                        DialogResult = DialogResult.OK;
                    }
                }
                else
                {
                    DialogResult = DialogResult.OK;
                }
            }
            else if (RbShortcut.Checked)
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Bitte geben Sie einen Namen für die neue Spalte ein.");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void RbFromTo_CheckedChanged(object sender, EventArgs e)
        {
            GBFromTo.Visible = RbFromTo.Checked;
            GBShortcut.Visible = RbShortcut.Checked;
        }
    }
}
