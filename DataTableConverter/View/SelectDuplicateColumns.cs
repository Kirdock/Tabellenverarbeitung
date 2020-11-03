﻿using System;
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
    public partial class SelectDuplicateColumns : Form
    {
        internal DataTable Table { get; set; }
        internal static readonly string IgnoreColumn = "Ignorieren";
        private int ComboBoxIndex;
        internal SelectDuplicateColumns(string[] caseHeaders, Dictionary<string,string> importTableColumnAliasMapping, bool mustBeAssigned, string heading = null, string firstColumnName = null)
        {
            InitializeComponent();
            if(heading != null)
            {
                Text = heading;
            }
            setDataGridView(caseHeaders, importTableColumnAliasMapping, mustBeAssigned, firstColumnName);
            ViewHelper.AdjustComboBoxGridView(dgDuplicate, ComboBoxIndex, importTableColumnAliasMapping.Values.Cast<object>().ToArray());
            ViewHelper.SetDataGridViewStyle(dgDuplicate);
        }

        private void setDataGridView(string[] caseHeaders, Dictionary<string, string> importTableColumnAliasMapping, bool mustBeAssigned, string firstColumnName)
        {
            DataTable table = new DataTable { TableName = "Duplicates" };
            table.Columns.Add(firstColumnName ?? "Spalte");
            table.Columns.Add("Zuweisung");
            Dictionary<string,string> newHeaders;
            
            if (mustBeAssigned)
            {
                newHeaders = importTableColumnAliasMapping;
            }
            else
            {
                newHeaders = new Dictionary<string, string>
                {
                    { IgnoreColumn, null },
                };
                foreach(KeyValuePair<string,string> pair in importTableColumnAliasMapping)
                {
                    newHeaders.Add(pair.Key, pair.Value);
                }
            }

            dgDuplicate.DataSource = table;

            DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn
            {
                DataSource = new BindingSource(newHeaders,null),
                DisplayMember = "key",
                ValueMember = "value",
                DataPropertyName = "Zuweisung",
                HeaderText = "Zuweisung "
            };

            ComboBoxIndex = dgDuplicate.Columns.Count;
            dgDuplicate.Columns.Add(cmb);
            string firstValue = newHeaders.First().Value;
            foreach (string header in caseHeaders)
            {
                table.Rows.Add(new object[] { header, firstValue });
            }
            dgDuplicate.Columns[0].ReadOnly = true;
            dgDuplicate.Columns[1].Visible = false;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            setDataTable();
            DataTable tab = getDataSource();
            bool isValid = true;
            foreach(DataRow row in tab.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.ItemArray[1]?.ToString()))
                {
                    isValid = false;
                }
            }
            if (isValid)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private DataTable getDataSource()
        {
            ViewHelper.EndDataGridViewEdit(dgDuplicate);
            return ((DataTable)dgDuplicate.DataSource).Copy();
        }

        private void setDataTable()
        {
            ViewHelper.EndDataGridViewEdit(dgDuplicate);
            Table = ((DataTable)dgDuplicate.DataSource).Copy();

        }
    }
}
