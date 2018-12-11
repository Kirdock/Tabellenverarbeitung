using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        }

        private void bestätigenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgTable.CommitEdit(DataGridViewDataErrorContexts.Commit);
            DialogResult = DialogResult.OK;
        }

        internal DataTable getTable()
        {
            dgTable.BindingContext[dgTable.DataSource].EndCurrentEdit();
            return ((DataView)dgTable.DataSource).ToTable();
        }



        private void zeileLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<int> selectedRows = ViewHelper.SelectedRows(dgTable);
            if(selectedRows.Count == 0)
            {
                selectedRows.Add(selectedRow);
            }
            foreach (int row in selectedRows.OrderByDescending(x => x))
            {
                dgTable.Rows.RemoveAt(row);
            }
        }

        private DataView getDataView()
        {
            dgTable.BindingContext[dgTable.DataSource].EndCurrentEdit();
            return (DataView)dgTable.DataSource;
        }

        private void spalteLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getDataView().Table.Columns.RemoveAt(selectedColumn);
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
                    DataColumn col = table.Columns.Add(newColumn);
                    col.SetOrdinal(selectedColumn == -1 ? 0 : selectedColumn);
                    dgTable.DataSource = null;
                    dgTable.DataSource = table.DefaultView;
                    dgTable.ColumnDisplayIndexChanged += dgTable_ColumnDisplayIndexChanged;
                }
                else
                {
                    if (MessageHandler.MessagesOkCancel(MessageBoxIcon.Warning,"Der Spaltenname wird bereits verwendet. Bitte geben Sie einen anderen ein.") == DialogResult.OK)
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
                if(selectedColumn == -1 || selectedRow == -1)
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
            SplitForm form = new SplitForm(DataHelper.getHeadersOfDataTable(getTable()));
            if(form.ShowDialog() == DialogResult.OK)
            {
                DataTable table = getDataView().Table;
                string splitString = form.getSplitString();
                string header = form.getSelectedHeader();
                int column = table.Columns.IndexOf(header);
                int newColumnIndizes = table.Columns.Count;

                DataHelper.addColumn(header + "1", table);
                DataHelper.addColumn(header + "2", table);

                foreach (DataRow row in table.Rows)
                {
                    if (row[column].ToString() == string.Empty) continue;

                    string[] cols = row[column].ToString().Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries);
                    row[newColumnIndizes] = cols[0];
                    row[newColumnIndizes+1] = cols.Length > 1 ? cols[1] : string.Empty;
                }
                table.Columns.RemoveAt(column);

            }
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
