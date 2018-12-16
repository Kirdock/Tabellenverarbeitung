using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataTableConverter
{
    class ViewHelper
    {
        private EventHandler ctxRowDeleteRowHandler, ctxRowClipboard, ctxRowInsertRowHandler;
        private ContextMenuStrip ctxRow;
        private ToolStripItem clipboardItem, deleteRowItem, insertRowItem;
        private Action<object, EventArgs> myFunction;
        private List<Work> Workflows;
        internal int selectedCase { get; set; }

        public ViewHelper(ContextMenuStrip ctxrow, Action<object,EventArgs> myfunction, List<Work> workflows)
        {
            ctxRow = ctxrow;
            myFunction = myfunction;
            clipboardItem = ctxRow.Items.Cast<ToolStripItem>().First(x=> x.Name == "clipboardItem");
            deleteRowItem = ctxRow.Items.Cast<ToolStripItem>().First(x => x.Name == "deleteRowItem");
            insertRowItem = ctxRow.Items.Cast<ToolStripItem>().First(x => x.Name == "insertRowItem");
            Workflows = workflows;
        }

        internal static void addNumerationToDataGridView(object sender, DataGridViewRowPostPaintEventArgs e, Font font)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            Font f = new Font(font.Name, font.Size, FontStyle.Bold);

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };
            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, f, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        internal static void handleDataGridViewNumber(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(tx_KeyPress);
            if (((DataGridView)sender).CurrentCell.ColumnIndex >= 1)
            {
                TextBox tx = e.Control as TextBox;
                tx.KeyPress += new KeyPressEventHandler(tx_KeyPress);
            }
        }

        internal static void tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
        }

        internal static void insertClipboardToDataGridView(DataGridView myDataGridView, int rowIndex)
        {
            myDataGridView.BindingContext[myDataGridView.DataSource].EndCurrentEdit();
            DataTable table = ((DataTable)myDataGridView.DataSource).Copy();

            DataObject o = (DataObject)Clipboard.GetDataObject();
            int columnCount = table.Columns.Count;
            DataTable clipboardTable = new DataTable();
            if (o.GetDataPresent(DataFormats.Text))
            {
                string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                foreach (string pastedRow in pastedRows)
                {
                    string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });
                    for(int i = clipboardTable.Columns.Count; i < pastedRowCells.Length; i++)
                    {
                        clipboardTable.Columns.Add($"Spalte{i+1}");
                    }
                    clipboardTable.Rows.Add(pastedRowCells);
                }

                ClipboardForm form = new ClipboardForm(clipboardTable);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    foreach(DataRow row in form.getTable().Rows)
                    {
                        try
                        {
                            DataRow newRow = table.NewRow();
                            object[] itemArray = row.ItemArray.Clone() as object[];
                            newRow.ItemArray = itemArray.Length > table.Columns.Count ? itemArray.Take(table.Columns.Count).ToArray() : itemArray;
                            table.Rows.InsertAt(newRow, rowIndex);
                            rowIndex++;
                        }
                        catch { } //Error, wenn Typen wie int nicht übereinstimmen
                    }
                    myDataGridView.DataSource = null;
                    myDataGridView.DataSource = table;
                }
            }
        }

        internal static DataView getSortedView(string order, DataTable table)
        {
            Dictionary<string, SortOrder> dict = generateSortingList(order);
            if (dict.Count == 0)
            {
                return table.DefaultView;
            }
            else
            {
                var enumerable = table.AsEnumerable();
                var firstElement = dict.First();
                var enum2 = enumerable.OrderBy(field => field.Field<string>(firstElement.Key), new NaturalStringComparer(firstElement.Value));
                dict.Remove(firstElement.Key);
                foreach (var column in dict)
                {
                    enum2 = enum2.ThenBy(field => field.Field<string>(column.Key), new NaturalStringComparer(column.Value));
                }
                return enum2.AsDataView();
            }
        }

        internal static Dictionary<string, SortOrder> generateSortingList(string orderBefore)
        {
            Dictionary<string, SortOrder> dict = new Dictionary<string, SortOrder>();
            if (!string.IsNullOrWhiteSpace(orderBefore))
            {

                string[] headersInformation = orderBefore.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string info in headersInformation)
                {
                    string[] headerInfo = info.Split(new string[] { "] " }, StringSplitOptions.RemoveEmptyEntries);
                    string header = headerInfo[0].Trim().Substring(1);
                    SortOrder order = headerInfo[1].ToUpper() == "ASC" ? SortOrder.Ascending : SortOrder.Descending;
                    dict.Add(header, order);
                }
            }
            return dict;
        }

        internal void addContextMenuToDataGridView(DataGridView view,bool clipboard)
        {
            view.MouseClick +=(sender, e)=> dataGridView_MouseClick((DataGridView)sender, e, clipboard);
        }

        private void dataGridView_MouseClick(DataGridView view, MouseEventArgs e, bool clipboard)
        {
            if (e.Button == MouseButtons.Right)
            {
                clear();
                int selectedRow = view.HitTest(e.X, e.Y).RowIndex;
                int selectedColumn = view.HitTest(e.X, e.Y).ColumnIndex;

                if (selectedColumn > -1 && selectedRow > -1 && !view.SelectedCells.Contains(view[selectedColumn, selectedRow]))
                {
                    view.SelectedCells.Cast<DataGridViewCell>().ToList().ForEach(cell => cell.Selected = false);
                    view[selectedColumn, selectedRow].Selected = true;
                }

                clipboardItem.Click += ctxRowClipboard = (sender2, e2) => insertClipboardToDataGridView(view, selectedRow);

                if (deleteRowItem.Visible = (selectedRow > -1 && selectedRow != view.Rows.Count - 1))
                {

                    List<int> selectedRows = SelectedRows(view);
                    deleteRowItem.Text = (selectedRows.Count > 1) ? "Zeilen löschen" : "Zeile löschen";

                    deleteRowItem.Click += ctxRowDeleteRowHandler = (sender2, e2) => deleteRowClick(view, selectedRows);
                    insertRowItem.Click += ctxRowInsertRowHandler = (sender2, e2) => insertRowClick(view, selectedRow);
                }
                clipboardItem.Visible = clipboard;
                
                ctxRow.Show(view, new Point(e.X, e.Y));

            }
        }

        internal void clear()
        {
            if (ctxRowDeleteRowHandler != null)
            {
                deleteRowItem.Click -= ctxRowDeleteRowHandler;
                insertRowItem.Click -= ctxRowInsertRowHandler;
            }
            
            if (ctxRowClipboard != null)
            {
                clipboardItem.Click -= ctxRowClipboard;
            }
        }

        private void deleteRowClick(DataGridView view, List<int> rowIndizes)
        {
            for (int i = rowIndizes.Count - 1; i >= 0; i--)
            {
                view.Rows.RemoveAt(rowIndizes[i]);
            }
            if (view.DataSource != null)
            {
                view.BindingContext[view.DataSource].EndCurrentEdit();
            }
            if (view.Name == "dgCaseColumns")
            {
                WorkflowHelper.removeRowThroughCaseChange(Workflows, rowIndizes, selectedCase);
                myFunction(null, null);
            }
        }

        private void insertRowClick(DataGridView view, int rowIndex)
        {
            view.BindingContext?[view.DataSource].EndCurrentEdit();
            DataTable table = (DataTable)view.DataSource;
            DataRow row = table.NewRow();
            table.Rows.InsertAt(row, rowIndex);
            view.DataSource = table;

            if (view.Name == "dgCaseColumns")
            {
                WorkflowHelper.insertRowThroughCaseChange(Workflows, rowIndex, selectedCase);
                myFunction(null, null);
            }
        }


        internal static List<int> SelectedRows(DataGridView sender)
        {
            List<int> selectedRows = sender.SelectedCells.Cast<DataGridViewCell>().Select(cell => cell.RowIndex).Where(row => row != sender.Rows.Count - 1).Distinct().ToList();
            selectedRows.Sort();
            return selectedRows;
        }

        internal static List<int> SelectedColumns(DataGridView sender)
        {
            List<int> selectedColumns = sender.SelectedCells.Cast<DataGridViewCell>().Select(cell => cell.ColumnIndex).Where(col => col != sender.Columns.Count - 1).Distinct().ToList();
            selectedColumns.Sort();
            return selectedColumns;
        }

        internal static void addRemoveHeaderThroughCheckedListBox(DataGridView sender, ItemCheckEventArgs e, CheckedListBox headers)
        {
            if (e.NewValue == CheckState.Checked)
            {
                sender.Rows.Add(headers.Items[e.Index]);
            }
            else
            {
                bool found = false;
                for (int i = sender.Rows.Count - 1; i >= 0 && !found; i--)
                {
                    if (sender.Rows[i].Cells[0] == headers.Items[e.Index])
                    {
                        sender.Rows.RemoveAt(i);
                        found = true;
                    }
                }
            }
        }

        internal static string adjustSort(string adjustSort, string column, string newColumn)
        {
            if (newColumn == null)
            {
                int indexFrom = adjustSort.IndexOf($"[{column}]");
                if (indexFrom != -1)
                {
                    int indexTo = adjustSort.IndexOf(",", column.Length + 2 + indexFrom);
                    adjustSort = adjustSort.Remove(indexFrom, indexTo == -1 ? adjustSort.Length : (indexTo - indexFrom + 2)); //+2 weil nach "," noch ein Leerzeichen ist
                }
            }
            else
            {
                adjustSort = adjustSort.Replace($"[{column}]", $"[{newColumn}]");
            }
            return adjustSort;
        }
    }
}
