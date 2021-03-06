﻿using DataTableConverter.Classes.WorkProcs;
using System;
using System.Data;
using System.Linq;
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
            Table.Columns[(int)ProcReplaceWhole.ColumnIndex.Column].DefaultValue = headers.First();

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
            if(Table.Rows.Count == 0)
            {
                Table.Rows.Add(new object[] { Table.Columns[(int)ProcReplaceWhole.ColumnIndex.Column].DefaultValue });
            }
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
