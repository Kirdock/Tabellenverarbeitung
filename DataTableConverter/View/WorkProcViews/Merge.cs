using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class Merge : Form
    {
        private object[] Headers;
        internal ProcMerge Proc;
        internal ViewHelper UIHelper;

        internal Merge(object[] headers, ContextMenuStrip ctxRow)
        {
            InitializeComponent();
            Headers = headers;
            Proc = new ProcMerge();
            InitDataGridView(headers);
            UIHelper = new ViewHelper(ctxRow, null, null);
            UIHelper.AddContextMenuToDataGridView(dgvMerge, this, false);
            ViewHelper.SetDataGridViewStyle(dgvMerge);

        }

        private void InitDataGridView(object[] headers)
        {
            dgvMerge.DataSource = null;
            dgvMerge.DataSource = Proc.Conditions.DefaultView;
            DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn()
            {
                DataPropertyName = "Spalte",
                HeaderText = "Spalte ",
                DisplayIndex = 0
            };
            DataGridViewButtonColumn boxCol = new DataGridViewButtonColumn
            {
                Text = "Format",
                UseColumnTextForButtonValue = true
            };
            col.Items.AddRange(headers);

            for (int i = 0; i < dgvMerge.Columns.Count; i++)
            {
                dgvMerge.Columns[i].DisplayIndex = i;
            }

            dgvMerge.Columns.Add(col);
            dgvMerge.Columns.Add(boxCol);
            dgvMerge.Columns[(int)ProcMerge.ConditionColumn.Spalte].Visible = false;
            dgvMerge.Columns[(int)ProcMerge.ConditionColumn.Spalte].DisplayIndex = 4;


            col.DisplayIndex = 0;
            boxCol.DisplayIndex = 5;

            ViewHelper.AdjustComboBoxGridView(dgvMerge, 1, headers);
        }

        private void CloseForm()
        {
            if (string.IsNullOrWhiteSpace(txtHeader.Text))
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an!");
            }
            else if (!cbMergeOldColumn.Checked && IsDuplicate(txtHeader.Text))
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Spalte mit diesem Namen.\nBitte geben Sie einen anderen an");
            }
            else
            {
                Proc.NewColumn = txtHeader.Text;
                Proc.CopyOldColumn = cbMergeOldColumn.Checked;
                ViewHelper.EndDataGridViewEdit(dgvMerge);
                DialogResult = DialogResult.OK;
            }
        }

        private bool IsDuplicate(string text)
        {
            return Headers.Cast<string>().Contains(text, StringComparer.OrdinalIgnoreCase);
        }


        private void btnConfirm_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void Merge_FormClosing(object sender, FormClosingEventArgs e)
        {
            UIHelper.Clear();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new SettingForm(SettingForm.Tabs.Help).Show(this);
        }

        private void BtnFormat_Click(object sender, EventArgs e)
        {
            MergeFormatView view = new MergeFormatView(Proc.Format, Headers);
            if (view.ShowDialog(this) == DialogResult.OK)
            {
                txtFormula.Text = Proc.Format.ToString();
            }
        }

        private void dgvMerge_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1 && dgvMerge[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell && string.IsNullOrWhiteSpace(e.Value?.ToString()))
            {
                e.Value = "Format";
            }
        }

        private void dgvMerge_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Check Button Click
            if (e.ColumnIndex > -1 && e.RowIndex > -1 && dgvMerge[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell)
            {
                ViewHelper.EndDataGridViewEdit(dgvMerge);
                DataTable table = (dgvMerge.DataSource as DataView).Table;

                var row = (dgvMerge.Rows[e.RowIndex].DataBoundItem as DataRowView).Row;

                if (!(row[(int)ProcMerge.ConditionColumn.Format] is MergeFormat))
                {
                    row[(int)ProcMerge.ConditionColumn.Format] = new MergeFormat();
                }

                MergeFormatView view = new MergeFormatView(row[(int)ProcMerge.ConditionColumn.Format] as MergeFormat, Headers);
                view.ShowDialog(this);
                dgvMerge.Refresh();
            }
        }

        private void dgvMerge_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (dgvMerge.DataSource != null)
            {
                dgvMerge.CommitEdit(DataGridViewDataErrorContexts.CurrentCellChange);
                ViewHelper.EndDataGridViewEdit(dgvMerge);
                DataTable table = (dgvMerge.DataSource as DataView).Table;
                table.Rows[e.Row.Index - 1][(int)ProcMerge.ConditionColumn.Format] = new MergeFormat();
                if (dgvMerge.EditingControl is TextBox)
                {
                    TextBox box = ((TextBox)dgvMerge.EditingControl);
                    box.SelectionStart = box.Text.Length;
                    box.SelectionLength = 0;
                }

            }
        }
    }
}
