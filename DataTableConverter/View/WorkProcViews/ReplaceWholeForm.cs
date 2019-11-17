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
    public partial class ReplaceWholeForm : Form
    {
        internal DataTable Table;
        private ViewHelper UIHelper;
        private int ComboBoxIndex;

        internal ReplaceWholeForm(object[] headers, ContextMenuStrip ctxRow)
        {
            InitializeComponent();
            InitDataTable(headers);
            UIHelper = new ViewHelper(ctxRow, null, null);
            UIHelper.AddContextMenuToDataGridView(dgTable, this, false);
            ViewHelper.AdjustComboBoxGridView(dgTable, ComboBoxIndex, headers);
            ViewHelper.SetDataGridViewStyle(dgTable);
        }

        private void InitDataTable(object[] headers)
        {
            Table = new DataTable();
            ProcReplaceWhole.SetColumns(Table);

            dgTable.DataSource = Table;

            DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn
            {
                DataSource = headers,
                DataPropertyName = Table.Columns[(int)ProcReplaceWhole.ColumnIndex.Column].ColumnName,
                HeaderText = Table.Columns[(int)ProcReplaceWhole.ColumnIndex.Column].ColumnName + " "
            };
            dgTable.Columns.Add(cmb);
            dgTable.Columns[(int)ProcReplaceWhole.ColumnIndex.Column].Visible = false;
            ComboBoxIndex = cmb.DisplayIndex = 0;
            
        }

        private void CloseForm()
        {
            ViewHelper.EndDataGridViewEdit(dgTable);
            DialogResult = DialogResult.OK;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void ReplaceWholeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UIHelper.Clear();
        }
    }
}
