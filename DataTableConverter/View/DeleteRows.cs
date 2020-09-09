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
    public partial class DeleteRows : Form
    {
        internal int[] Range;
        internal string ColumnText, Column;
        internal bool EqualsText => CBEquals.Checked;

        internal DeleteRows(int max, string[] headers)
        {
            InitializeComponent();
            NbEnd.Maximum = max;
            CmBHeaders.Items.AddRange(headers);
            CmBHeaders.SelectedIndex = 0;
        }

        private void BtnConfirmMulti_Click(object sender, EventArgs e)
        {
            if(NbStart.Value > NbEnd.Value)
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Error, "Die Startposition muss kleiner als die Endposition sein!");
            }
            else
            {
                Range = Enumerable.Range((int)NbStart.Value - 1, (int)NbEnd.Value).ToArray();
                DialogResult = DialogResult.OK;
            }
        }

        private void BtnConfirmSingle_Click(object sender, EventArgs e)
        {
            Range = new int[] { (int)NbSingle.Value - 1 };
            DialogResult = DialogResult.OK;
        }

        private void BtnConfirmMatchText_Click(object sender, EventArgs e)
        {
            ColumnText = TxtValue.Text;
            Column = (string)CmBHeaders.SelectedItem;
            DialogResult = DialogResult.OK;
        }
    }
}
