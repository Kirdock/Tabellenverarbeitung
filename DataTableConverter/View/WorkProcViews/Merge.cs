using CheckComboBoxTest;
using DataTableConverter.Classes;
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
    public partial class Merge : Form
    {
        internal string Formula { get { return txtFormula.Text; } }
        internal ProcMerge Proc;
        internal ViewHelper UIHelper;

        internal Merge(object[] headers, ContextMenuStrip ctxRow)
        {
            InitializeComponent();
            cbHeaders.Items.AddRange(headers);
            Proc = new ProcMerge();
            InitDataGridView(headers);
            UIHelper = new ViewHelper(ctxRow, null, null);
            UIHelper.AddContextMenuToDataGridView(dgvMerge, false);
        }

        private void InitDataGridView(object[] headers)
        {
            dgvMerge.DataSource = null;
            dgvMerge.DataSource = Proc.Conditions;
            DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn()
            {
                DataPropertyName = "Spalte",
                HeaderText = "Spalte "
            };
            col.Items.AddRange(headers);
            dgvMerge.Columns[(int)ProcMerge.ConditionColumn.Spalte].Visible = false;
            dgvMerge.Columns.Add(col);
            col.DisplayIndex = 0;
        }

        private void addColumn(string column)
        {
            if (!txtFormula.Text.Contains(column))
            {
                string separator = txtFormula.Text.Length > 0 ? " " : string.Empty;
                txtFormula.Text = $"{txtFormula.Text}{separator}[{column}]";
            }
        }

        private void removeColumn(string column)
        {
            txtFormula.Text = txtFormula.Text.Replace($" [{column}]", string.Empty).Replace($"[{column}] ", string.Empty);
        }

        private void CloseForm()
        {
            if (string.IsNullOrWhiteSpace(txtHeader.Text))
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an!");
            }
            else if (!cbMergeOldColumn.Checked && IsDuplicate(txtHeader.Text))
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Spalte mit diesem Namen.\nBitte geben Sie einen anderen an");
            }
            else
            {
                Proc.NewColumn = txtHeader.Text;
                Proc.Formula = txtFormula.Text;
                Proc.CopyOldColumn = cbMergeOldColumn.Checked;
                dgvMerge.BindingContext[dgvMerge.DataSource].EndCurrentEdit();
                DialogResult = DialogResult.OK;
            }
        }

        private bool IsDuplicate(string text)
        {
            return cbHeaders.Items.Contains(text);
        }

        private void cbHeaders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                addColumn(cbHeaders.Items[e.Index].ToString());
            }
            else
            {
                removeColumn(cbHeaders.Items[e.Index].ToString());
            }
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
            new SettingForm(SettingForm.Tabs.Help).Show();
        }
    }
}
