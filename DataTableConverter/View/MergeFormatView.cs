using DataTableConverter.Classes;
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
        private MergeFormat Format;
        internal MergeFormatView(MergeFormat format, object[] headers = null)
        {
            InitializeComponent();
            Format = format;
            txtFormula.Text = format.Formula;
            ViewHelper.SetDataGridViewStyle(dgTable);
            try
            {
                format.Table.AcceptChanges();
            }
            catch { }
            dgTable.DataSource = format.Table;
            if (headers != null)
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
                
                dgTable.Columns[0].DisplayIndex = 8;
                
                col.DisplayIndex = 0;
                boxCol.DisplayIndex = 4;
                boxCol2.DisplayIndex = 7;
                dgTable.Columns[(int)MergeFormat.MergeColumns.Empty].ReadOnly = true;
                dgTable.Columns[(int)MergeFormat.MergeColumns.NotEmpty].ReadOnly = true;
            }
            dgTable.Refresh();
            dgTable.Update();
            SetSize(format.Table.Rows.Count == 0);
        }

        private void SetSize(bool isStringFormat)
        {
            if (isStringFormat)
            {
                Size = new Size(Size.Width, 244);
            }
            else
            {
                Size = Properties.Settings.Default.MergeFormatViewSize;
            }
            RBExtended.Checked = !isStringFormat;
            RBSimple.Checked = isStringFormat;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(dgTable);
            Format.Formula = txtFormula.Text;

            if (RBSimple.Checked)
            {
                (dgTable.DataSource as DataTable).Rows.Clear();
                (dgTable.DataSource as DataTable).AcceptChanges();
            }
            else
            {
                Format.Formula = string.Empty;
            }
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
                HeaderSelect form = new HeaderSelect(Headers, e.ColumnIndex == 0 ? table.Rows[e.RowIndex][(int)MergeFormat.MergeColumns.Empty]?.ToString() : table.Rows[e.RowIndex][(int)MergeFormat.MergeColumns.NotEmpty].ToString());

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (e.ColumnIndex == 0) //Empty Column
                    {
                        table.Rows[e.RowIndex][(int)MergeFormat.MergeColumns.Empty] = form.Headers;
                    }
                    else //Not Empty Column
                    {
                        table.Rows[e.RowIndex][(int)MergeFormat.MergeColumns.NotEmpty] = form.Headers;
                    }
                    dgTable.Refresh();
                }
            }
        }

        private void MergeFormatView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Format.Table.Rows.Count != 0)
            {
                Properties.Settings.Default.MergeFormatViewSize = Size;
                Properties.Settings.Default.Save();
            }
            
            if (DialogResult != DialogResult.OK)
            {
                DataTable table = (dgTable.DataSource as DataTable);
                if (table.GetChanges() != null)
                {
                    DialogResult res = MessageHandler.MessagesYesNo(MessageBoxIcon.Information, "Möchten Sie die Änderung wirklich verwerfen?");
                    e.Cancel = res == DialogResult.No;
                    if (!e.Cancel)
                    {
                        table.RejectChanges();
                    }
                }
            }
            if (!e.Cancel)
            {
                dgTable.DataSource = null;
            }
        }


        private void RB_CheckedChanged(object sender, EventArgs e)
        {
            PanelSimple.Visible = RBSimple.Checked;
            PanelExtended.Visible = RBExtended.Checked;
        }
    }
}
