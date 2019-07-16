using DataTableConverter.Classes.WorkProcs;
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
    public partial class PaddingForm : Form
    {
        internal ProcPadding Proc;
        public PaddingForm(object[] headers)
        {
            InitializeComponent();
            cbHeadersPad.Items.AddRange(headers);
            Proc = new ProcPadding();
            InitDataGridView(headers);
            SetNewColumnVisibility(false);
            ViewHelper.SetDataGridViewStyle(dgvPadConditions);
        }

        private void InitDataGridView(object[] headers)
        {
            dgvPadConditions.DataSource = Proc.Conditions;
            DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn()
            {
                HeaderText = "Spalte ",
                DataPropertyName = "Spalte"
            };
            col.Items.AddRange(headers);
            dgvPadConditions.Columns[0].Visible = false;
            dgvPadConditions.Columns.Add(col);
            col.DisplayIndex = 0;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if(TxtCharacter.Text.Length == 0)
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie ein Zeichen ein!");
            }
            else
            {
                dgvPadConditions.BindingContext[dgvPadConditions.DataSource].EndCurrentEdit();
                Proc.Character = TxtCharacter.Text[0];
                Proc.OperationSide = RbLeft.Checked ? ProcPadding.Side.Left : ProcPadding.Side.Right;
                Proc.Counter = (int)nbPadCount.Value;
                Proc.NewColumn = txtNewColumnPad.Text;
                Proc.CopyOldColumn = cbOldColumn.Checked;
                cbHeadersPad.CheckedItems.Cast<string>().ToList().ForEach(header => Proc.Columns.Rows.Add(header));
                DialogResult = DialogResult.OK;
            }
        }

        private void cbPadNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            bool checkState = ((CheckBox)sender).Checked;
            if (cbOldColumn.Visible = !checkState)
            {
                txtNewColumnPad.Text = string.Empty;
            }
            else
            {
                cbOldColumn.Checked = false;
            }
            SetNewColumnVisibility(checkState);
        }

        private void SetNewColumnVisibility(bool status)
        {
            lblPadNewColumn.Visible = txtNewColumnPad.Visible = status;
        }
    }
}
