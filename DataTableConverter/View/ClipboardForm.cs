using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class ClipboardForm : Form
    {
        private int selectedRow, selectedColumn;
        internal ClipboardForm(DataTable table)
        {
            InitializeComponent();
            dgTable.DataSource = table.DefaultView;
            dgTable.ColumnDisplayIndexChanged += dgTable_ColumnDisplayIndexChanged;
            ViewHelper.SetDataGridViewStyle(dgTable);
        }

        private void bestätigenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgTable.CommitEdit(DataGridViewDataErrorContexts.Commit);
            DialogResult = DialogResult.OK;
        }

        internal DataTable getTable()
        {
            ViewHelper.EndDataGridViewEdit(dgTable);
            return ((DataView)dgTable.DataSource).ToTable();
        }



        private void zeileLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<int> selectedRows = ViewHelper.SelectedRows(dgTable);
            if (selectedRows.Count == 0)
            {
                selectedRows.Add(selectedRow);
            }
            foreach (int row in selectedRows.OrderByDescending(x => x))
            {
                getDataView().Table.Rows.RemoveAt(row);
            }
        }

        private DataView getDataView()
        {
            ViewHelper.EndDataGridViewEdit(dgTable);
            return (DataView)dgTable.DataSource;
        }

        private void spalteLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<int> selectedColumns = ViewHelper.SelectedColumns(dgTable);
            if (selectedColumns.Count == 0)
            {
                selectedColumns.Add(selectedColumn);
            }
            foreach (int col in selectedColumns.OrderByDescending(x => x))
            {
                getDataView().Table.Columns.RemoveAt(col);
            }
        }

        private void zeileHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int tableRowIndex = selectedRow == -1 ? 0 : getDataTableRowIndexOfDataGridView(selectedRow);
            DataTable table = getDataView().Table;
            DataRow row = table.NewRow();
            table.Rows.InsertAt(row, tableRowIndex);
        }

        private int getDataTableRowIndexOfDataGridView(int rowIndex)
        {
            DataRow oldRow = ((DataRowView)dgTable.Rows[rowIndex].DataBoundItem).Row;
            return ((DataView)dgTable.DataSource).Table.Rows.IndexOf(oldRow);
        }


        private void spalteHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newColumn = Microsoft.VisualBasic.Interaction.InputBox("Bitte Spaltennamen eingeben", "Spalte hinzufügen", string.Empty);
            if (!string.IsNullOrWhiteSpace(newColumn))
            {
                if (!dgTable.Columns.Cast<DataGridViewColumn>().Any(col => col.HeaderText == newColumn))
                {
                    dgTable.ColumnDisplayIndexChanged -= dgTable_ColumnDisplayIndexChanged;
                    DataTable table = getTable();
                    DataColumn col = table.Columns.Add(newColumn, typeof(string));
                    col.SetOrdinal(selectedColumn == -1 ? 0 : selectedColumn);
                    dgTable.DataSource = null;
                    dgTable.DataSource = table.DefaultView;
                    dgTable.ColumnDisplayIndexChanged += dgTable_ColumnDisplayIndexChanged;
                }
                else
                {
                    if (this.MessagesOkCancel(MessageBoxIcon.Warning, "Der Spaltenname wird bereits verwendet. Bitte geben Sie einen anderen ein.") == DialogResult.OK)
                    {
                        spalteHinzufügenToolStripMenuItem_Click(sender, e);
                    }
                }
            }
        }

        private void adjustDataGridView()
        {
            DataView view = getDataView();
            dgTable.DataSource = null;
            dgTable.DataSource = view;
        }

        private void dgTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectedRow = dgTable.HitTest(e.X, e.Y).RowIndex;
                selectedColumn = dgTable.HitTest(e.X, e.Y).ColumnIndex;
                if (selectedColumn == -1 || selectedRow == -1)
                {
                    setSelectedCells(false);
                }
                else if (selectedColumn > -1 && selectedRow > -1 && !dgTable.SelectedCells.Contains(dgTable[selectedColumn, selectedRow]))
                {
                    setSelectedCells(false);
                    dgTable[selectedColumn, selectedRow].Selected = true;
                }

                ctxRow.Show(dgTable, new Point(e.X, e.Y));
            }
        }

        private void spalteAufteilenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SplitForm form = new SplitForm(getTable()?.HeadersOfDataTable() ?? new object[0]);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                DataTable table = getDataView().Table;
                string splitString = form.getSplitString();
                string header = form.getSelectedHeader();
                int column = table.Columns.IndexOf(header);
                int newColumnIndizes = table.Columns.Count;

                int counter = 0;

                for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                {
                    if (table.Rows[rowIndex][column].ToString() == string.Empty) continue;

                    string[] cols = table.Rows[rowIndex][column].ToString().Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries);
                    while (cols.Length > counter)
                    {
                        counter++;
                        TryAddColumn(table, header, counter);
                    }
                    for (int i = 0; i < cols.Length; i++)
                    {
                        table.Rows[rowIndex][newColumnIndizes + i] = cols[i];
                    }
                }
                table.Columns.RemoveAt(column);

            }
            form.Dispose();
        }

        private string TryAddColumn(DataTable table, string header, int counter = 0)
        {
            string result;
            string name = counter == 0 ? header : header + counter;
            if (table.Columns.Contains(name))
            {
                counter++;
                result = TryAddColumn(table, header, counter);
            }
            else
            {
                result = name;
                table.Columns.Add(name, typeof(string));
            }
            return result;
        }

        private void dgTable_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (e.Column.Index != e.Column.DisplayIndex)
            {
                dgTable.BeginInvoke(new MethodInvoker(() =>
                {
                    dgTable.ColumnDisplayIndexChanged -= dgTable_ColumnDisplayIndexChanged;
                    ((DataView)dgTable.DataSource).Table.Columns[e.Column.Name].SetOrdinal(e.Column.DisplayIndex);
                    dgTable.ColumnDisplayIndexChanged += dgTable_ColumnDisplayIndexChanged;
                }));
            }
        }

        private void setSelectedCells(bool status)
        {
            dgTable.SelectedCells.Cast<DataGridViewCell>().ToList().ForEach(cell => cell.Selected = status);
        }
    }
}
