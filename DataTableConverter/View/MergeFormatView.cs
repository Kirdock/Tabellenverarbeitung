﻿using DataTableConverter.Classes;
using DataTableConverter.View.WorkProcViews;
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
    public partial class MergeFormatView : Form
    {
        private object[] Headers;
        internal MergeFormatView(MergeFormat format, object[] headers = null)
        {
            InitializeComponent();
            dgTable.DataSource = format.Table;
            if(headers != null)
            {
                Headers = headers;
                DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn()
                {
                    DataPropertyName = "Spalte",
                    HeaderText = "Spalte "
                };
                col.Items.Add(string.Empty);
                col.Items.AddRange(headers);
                
                dgTable.Columns[0].Visible = false;
                dgTable.Columns.Add(col);

                DataGridViewButtonColumn boxCol = new DataGridViewButtonColumn
                {
                    Text = "Auswahl",
                    UseColumnTextForButtonValue = true
                };
                DataGridViewButtonColumn boxCol2 = new DataGridViewButtonColumn
                {
                    Text = "Auswahl",
                    UseColumnTextForButtonValue = true
                };
                dgTable.Columns.Add(boxCol);
                dgTable.Columns.Add(boxCol2);

                for (int i = 0; i < dgTable.Columns.Count; i++)
                {
                    dgTable.Columns[i].DisplayIndex = i;
                }
                
                dgTable.Columns[0].DisplayIndex = 6;
                dgTable.Columns[3].DisplayIndex = 5;
                
                col.DisplayIndex = 0;
                boxCol.DisplayIndex = 4;
                boxCol2.DisplayIndex = 6;
                dgTable.Columns[2].ReadOnly = true;
                dgTable.Columns[3].ReadOnly = true;

                Size = new Size(Size.Width+ 500,Size.Height+300);
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(dgTable);
            DialogResult = DialogResult.OK;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new SettingForm(SettingForm.Tabs.Help).Show();
        }

        private void dgTable_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1 && dgTable[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell && string.IsNullOrWhiteSpace(e.Value?.ToString()))
            {
                e.Value = "Auswahl";
            }
        }

        private void dgTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1 && dgTable[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell)
            {
                ViewHelper.EndDataGridViewEdit(dgTable);

                //ColumnIndex 0 --> Empty Column
                //ColumnIndex 1 --> Not Empty Column
                //new Form: CheckedListBox with headers to select. additional parameter is the column "[Title1] [Title2]..." to set checked
                //--> selected headders
                //set Value after ShowDialog
                DataTable table = dgTable.DataSource as DataTable;
                HeaderSelect form = new HeaderSelect(Headers, e.ColumnIndex == 0 ? table.Rows[e.RowIndex][2]?.ToString() : table.Rows[e.RowIndex][3].ToString());

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (e.ColumnIndex == 0) //Empty Column
                    {
                        table.Rows[e.RowIndex][2] = form.Headers;
                    }
                    else //Not Empty Column
                    {
                        table.Rows[e.RowIndex][3] = form.Headers;
                    }
                    dgTable.Refresh();
                }
            }
        }
    }
}