using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        internal SearchForm(string[] headers)
        {
            InitializeComponent();
            CmBHeader.Items.AddRange(headers);
            CmBHeader.SelectedIndex = 0;
        }

        private void bestätigenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtSearchNewColumn.Text) && NbSearchFrom.Value > NbSearchTo.Value)
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Startnummer muss kleiner als Endnummer sein!");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
