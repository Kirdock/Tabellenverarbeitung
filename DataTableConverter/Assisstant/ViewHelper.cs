﻿using CheckComboBoxTest;
using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.View;
using DataTableConverter.View.CustomControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DataTableConverter
{
    class ViewHelper
    {
        private EventHandler CtxRowDeleteRowHandler, CtxRowClipboard, CtxRowInsertRowHandler;
        private ContextMenuStrip CtxRow;
        private ToolStripItem ClipboardItem, DeleteRowItem, InsertRowItem;
        private Action<object, EventArgs> MyFunction;
        private List<Work> Workflows;
        internal int SelectedCase { get; set; }
        private static readonly string LockIcon = "\uD83D\uDD12";

        internal static void AdjustComboBoxGridView(DataGridView dataGridView, int comboBoxIndex, object[] headers)
        {
            dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView.CellFormatting += (sender, e) => DataGridView_CellFormatting(e, comboBoxIndex, headers);
            dataGridView.EditingControlShowing += dataGridView_EditingControlShowing;
        }

        private static void DataGridView_CellFormatting(DataGridViewCellFormattingEventArgs e, int comboBoxIndex, object[] headers)
        {
            if (e.ColumnIndex == comboBoxIndex && string.IsNullOrWhiteSpace(e.Value?.ToString()))
            {
                e.Value = headers[0];
            }
        }

        private static void dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox)
            {
                ComboBox ctl = e.Control as ComboBox;
                ctl.Enter -= new EventHandler(ctl_Enter);
                ctl.Enter += new EventHandler(ctl_Enter);
            }
        }

        private static void ctl_Enter(object sender, EventArgs e)
        {
            (sender as ComboBox).DroppedDown = true;
        }

        public ViewHelper(ContextMenuStrip ctxrow, Action<object, EventArgs> myfunction, List<Work> workflows)
        {
            CtxRow = ctxrow;
            MyFunction = myfunction;
            ClipboardItem = CtxRow.Items.Cast<ToolStripItem>().First(x => x.Name == "clipboardItem");
            DeleteRowItem = CtxRow.Items.Cast<ToolStripItem>().First(x => x.Name == "deleteRowItem");
            InsertRowItem = CtxRow.Items.Cast<ToolStripItem>().First(x => x.Name == "insertRowItem");
            Workflows = workflows;
        }

        internal static void AddNumerationToDataGridView(object sender, DataGridViewRowPostPaintEventArgs e, Font font, decimal pages = 1)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1 + (pages - 1) * Properties.Settings.Default.MaxRows).ToString();

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

        internal static void HandleDataGridViewNumber(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Tx_KeyPress);
            if (((DataGridView)sender).CurrentCell.ColumnIndex >= 1)
            {
                TextBox tx = e.Control as TextBox;
                tx.KeyPress += new KeyPressEventHandler(Tx_KeyPress);
            }
        }

        internal static void ResizeCountListBox(CountListbox clbValues)
        {
            int maxWidth = 0;

            for (int i = 0; i < clbValues.Items.Count; i++)
            {
                int testWidth = TextRenderer.MeasureText(clbValues.Items[i].ToString() + $" (Anzahl: {(clbValues.Items[i] as CountListboxItem)?.Count})",
                                                            clbValues.Font).Width;
                if (testWidth > maxWidth)
                {
                    maxWidth = testWidth;
                }
            }

            clbValues.HorizontalExtent = maxWidth + 50;
        }

        internal static void ResizePlusListBox(PlusListbox clBHeaders)
        {
            int maxWidth = 0;

            for (int i = 0; i < clBHeaders.Items.Count; i++)
            {
                int testWidth = TextRenderer.MeasureText(clBHeaders.Items[i].ToString(),
                                                            clBHeaders.Font).Width;
                if (testWidth > maxWidth)
                {
                    maxWidth = testWidth;
                }
            }

            clBHeaders.HorizontalExtent = maxWidth + 20;
        }

        internal static void Tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
        }

        internal static void InsertClipboardToDataGridView(DataGridView myDataGridView, int rowIndex, Form mainForm, DataGridViewCellValidatingEventHandler dgEvent = null, Action<object, DataGridViewCellEventArgs> myfunc = null)
        {
            rowIndex = rowIndex < 0 ? 0 : rowIndex;
            Thread thread = new Thread(() =>
            {
                bool errorMessageShown = false;
                EndDataGridViewEdit(myDataGridView);
                DataTable table = ((DataTable)myDataGridView.DataSource);

                DataObject o = (DataObject)Clipboard.GetDataObject();
                int columnCount = table.Columns.Count;
                DataTable clipboardTable = new DataTable();
                if (o.GetDataPresent(DataFormats.Text))
                {
                    string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                    foreach (string pastedRow in pastedRows)
                    {
                        string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });
                        for (int i = clipboardTable.Columns.Count; i < pastedRowCells.Length; i++)
                        {
                            clipboardTable.Columns.Add($"Spalte{i + 1}", typeof(string));
                        }
                        clipboardTable.Rows.Add(pastedRowCells);
                    }

                    ClipboardForm form = new ClipboardForm(clipboardTable);
                    DialogResult result = DialogResult.Cancel;
                    mainForm.Invoke(new MethodInvoker(() =>
                    {
                        result = form.ShowDialog(mainForm);
                    }));
                    if (result == DialogResult.OK)
                    {
                        foreach (DataRow row in form.getTable().Rows)
                        {
                            try
                            {
                                DataRow newRow = table.NewRow();
                                object[] itemArray = row.ItemArray.Clone() as object[];
                                newRow.ItemArray = itemArray.Length > table.Columns.Count ? itemArray.Take(table.Columns.Count).ToArray() : itemArray;
                                table.Rows.InsertAt(newRow, rowIndex);
                                rowIndex++;
                            }
                            catch (Exception ex)
                            {
                                if (!errorMessageShown)
                                {
                                    mainForm.MessagesOK(MessageBoxIcon.Error, "Typen stimmen nicht überein! Es kann kein Text in ein Nummernfeld eingegeben werden!");
                                    errorMessageShown = true;
                                    ErrorHelper.LogMessage(ex, mainForm, false);
                                }
                            } //Error, wenn Typen wie int nicht übereinstimmen
                        }
                        if (dgEvent != null)
                        {
                            myDataGridView.CellValidating -= dgEvent;
                        }
                        myDataGridView.BeginInvoke(new MethodInvoker(() =>
                        {
                            myDataGridView.DataSource = null;
                            myDataGridView.DataSource = table;
                            myfunc?.Invoke(null, null);
                            if (dgEvent != null)
                            {
                                myDataGridView.CellValidating += dgEvent;
                            }
                        }));
                    }
                }
            });
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

        }


        internal static Dictionary<string, SortOrder> GenerateSortingList(string orderBefore)
        {
            Dictionary<string, SortOrder> dict = new Dictionary<string, SortOrder>();
            if (!string.IsNullOrWhiteSpace(orderBefore))
            {

                string[] headersInformation = orderBefore.Replace("COLLATE NATURALSORT ", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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

        internal void AddContextMenuToDataGridView(DataGridView view, Form mainForm, bool clipboard)
        {
            view.MouseClick += (sender, e) => DataGridView_MouseClick((DataGridView)sender, e, mainForm, clipboard);
        }

        private void DataGridView_MouseClick(DataGridView view, MouseEventArgs e, Form mainForm, bool clipboard)
        {
            if (e.Button == MouseButtons.Right)
            {
                Clear();
                int selectedRow = view.HitTest(e.X, e.Y).RowIndex;
                int selectedColumn = view.HitTest(e.X, e.Y).ColumnIndex;

                if (selectedColumn > -1 && selectedRow > -1 && !view.SelectedCells.Contains(view[selectedColumn, selectedRow]))
                {
                    view.SelectedCells.Cast<DataGridViewCell>().ToList().ForEach(cell => cell.Selected = false);
                    view[selectedColumn, selectedRow].Selected = true;
                }

                ClipboardItem.Click += CtxRowClipboard = (sender2, e2) => InsertClipboardToDataGridView(view, selectedRow, mainForm);

                if (DeleteRowItem.Visible = (selectedRow > -1 && selectedRow != view.Rows.Count - 1))
                {

                    List<int> selectedRows = SelectedRows(view);
                    DeleteRowItem.Text = (selectedRows.Count > 1) ? "Zeilen löschen" : "Zeile löschen";

                    DeleteRowItem.Click += CtxRowDeleteRowHandler = (sender2, e2) => DeleteRowClick(view, selectedRows);
                    InsertRowItem.Click += CtxRowInsertRowHandler = (sender2, e2) => InsertRowClick(view, selectedRow);
                }
                ClipboardItem.Visible = clipboard;

                CtxRow.Show(view, new Point(e.X, e.Y));

            }
        }

        internal void Clear()
        {
            if (CtxRowDeleteRowHandler != null)
            {
                DeleteRowItem.Click -= CtxRowDeleteRowHandler;
                InsertRowItem.Click -= CtxRowInsertRowHandler;
            }

            if (CtxRowClipboard != null)
            {
                ClipboardItem.Click -= CtxRowClipboard;
            }
        }

        private void DeleteRowClick(DataGridView view, List<int> rowIndizes)
        {
            for (int i = rowIndizes.Count - 1; i >= 0; i--)
            {
                view.Rows.RemoveAt(rowIndizes[i]);
            }
            if (view.DataSource != null)
            {
                EndDataGridViewEdit(view);
            }
            if (view.Name == "dgCaseColumns")
            {
                RemoveRowThroughCaseChange(Workflows, rowIndizes, SelectedCase);
            }
            MyFunction?.Invoke(null, null);
        }

        private void RemoveRowThroughCaseChange(List<Work> workflows, List<int> rowIndizes, int casId)
        {
            foreach (Work work in workflows)
            {
                work.Procedures.Where(proc => proc.ProcedureId == casId && proc.GetType() == typeof(Classes.WorkProcs.ProcDuplicate)).ToList().ForEach((caseProc) =>
                {
                    List<string> columns = caseProc.DuplicateColumns.ToList();
                    foreach (int index in rowIndizes.OrderByDescending(x => x))
                    {
                        columns.RemoveAt(index);
                    }
                    caseProc.DuplicateColumns = columns.ToArray();
                });
            }
        }

        private void InsertRowClick(DataGridView view, int rowIndex)
        {
            EndDataGridViewEdit(view);
            DataTable table = (DataTable)view.DataSource;
            DataRow row = table.NewRow();
            table.Rows.InsertAt(row, rowIndex);
            view.DataSource = table;

            if (view.Name == "dgCaseColumns")
            {
                InsertRowThroughCaseChange(Workflows, rowIndex, SelectedCase);
            }
            MyFunction?.Invoke(null, null);
        }

        private void InsertRowThroughCaseChange(List<Work> workflows, int rowIndex, int casId)
        {
            foreach (Work work in workflows)
            {
                work.Procedures.Where(proc => proc.ProcedureId == casId).ToList().ForEach((caseProc) =>
                {
                    List<string> columns = caseProc.DuplicateColumns.ToList();
                    columns.Insert(rowIndex, string.Empty);
                    caseProc.DuplicateColumns = columns.ToArray();
                });
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

        internal static void AddRemoveHeaderThroughCheckedListBox(DataGridView sender, ItemCheckEventArgs e, CheckedListBox headers)
        {
            DataTable Table = (DataTable)sender.DataSource;
            if (e.NewValue == CheckState.Checked)
            {
                Table.Rows.Add(headers.Items[e.Index]);
            }
            else
            {
                bool found = false;
                for (int i = Table.Rows.Count - 1; i >= 0 && !found; i--)
                {
                    if (Table.Rows[i][0].ToString() == headers.Items[e.Index].ToString())
                    {
                        Table.Rows.RemoveAt(i);
                        found = true;
                    }
                }
            }
        }

        internal static string AdjustSort(string adjustSort, string column, string newColumn)
        {
            if (string.IsNullOrWhiteSpace(adjustSort))
            {
                return adjustSort;
            }
            if (newColumn == null)
            {
                string colString = $"[{column}] COLLATE NATURALSORT";
                int indexFrom = adjustSort.IndexOf(colString);
                if (indexFrom != -1)
                {
                    int indexTo = adjustSort.IndexOf(",", column.Length + indexFrom);
                    adjustSort = adjustSort.Remove(indexFrom, indexTo == -1 ? adjustSort.Length : (indexTo - indexFrom + 2)); //+2 weil nach "," noch ein Leerzeichen ist
                }
            }
            else
            {
                adjustSort = adjustSort.Replace($"[{column}]", $"[{newColumn}]");
            }
            return adjustSort;
        }

        internal static void CheckAllItemsOfCheckedCombobox(CheckedComboBox cbHeaders, bool status)
        {
            for (int i = 0; i < cbHeaders.Items.Count; i++)
            {
                cbHeaders.SetItemChecked(i, status);
            }
        }

        internal static string[] GetSelectedHeaders(CheckedComboBox cbHeaders)
        {
            string[] columns = new string[cbHeaders.CheckedItems.Count];
            int counter = 0;
            for (int i = 0; i < cbHeaders.Items.Count; i++)
            {
                if (cbHeaders.GetItemChecked(i))
                {
                    columns[counter] = cbHeaders.Items[i].ToString();
                    counter++;
                }
            }
            return columns;
        }

        internal static void DrawLock(ListBox listBox, DrawItemEventArgs e, bool lockStatus)
        {
            if (e.Index != -1)
            {
                // Draw the background of the ListBox control for each item.
                e.DrawBackground();
                // Define the default color of the brush as black.
                Brush myBrush = Brushes.Black;

                string value = listBox.Items[e.Index].ToString();
                e.Graphics.DrawString($"{value} {(lockStatus ? LockIcon : string.Empty)}",
                    e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        internal static void SetLock(MouseEventArgs e, dynamic list, ListBox sender, Action selectedIndexChanged)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = sender.IndexFromPoint(e.Location);
                list[index].Locked = !list[index].Locked;
                sender.SelectedIndex = index;
                sender.Refresh();
                selectedIndexChanged.Invoke();
            }
        }

        internal static int[] SelectedRowsOfDataGridView(DataGridView view)
        {
            return view.SelectedCells.Cast<DataGridViewCell>().Select(cell => cell.RowIndex).Where(row => row != view.Rows.Count - 1).Distinct().ToArray();
        }

        internal static void SetControlColor(Control control)
        {
            control.BackColor = control.Enabled ? Color.White : Properties.Settings.Default.Locked;
            //foreach (Control c in GetControlHierarchy(control))
            //{
            //    c.ForeColor = SystemColors.ControlText;
            //}
        }

        internal static void SetDataGridViewStyle(DataGridView table)
        {
            table.DefaultCellStyle.Font = new Font(table.DefaultCellStyle.Font.Name, Properties.Settings.Default.TableFontSize);
            table.RowTemplate.Height = Properties.Settings.Default.RowHeight;
            foreach (DataGridViewRow row in table.Rows)
            {
                row.Height = Properties.Settings.Default.RowHeight;
            }
        }

        internal static void SetListBoxStyle(ListBox listBox)
        {
            listBox.Font = new Font(listBox.Font.Name, Properties.Settings.Default.ListBoxFontSize);
            listBox.ItemHeight = Properties.Settings.Default.ListBoxRowHeight;
        }

        internal static void EndDataGridViewEdit(DataGridView dgTable)
        {
            dgTable.BindingContext[dgTable.DataSource].EndCurrentEdit();
        }

        internal static void SetEncodingCmb(ComboBox cmbEncoding, bool emptySelectable = false)
        {
            cmbEncoding.DataSource = (emptySelectable ? (new NewEncodingInfo[] { new NewEncodingInfo(string.Empty, 0) }).Concat(GetEncodings()) : GetEncodings()).ToArray();
            cmbEncoding.DisplayMember = "DisplayName";
            cmbEncoding.ValueMember = "CodePage";
            cmbEncoding.SelectedValue = Properties.Settings.Default.Encoding;
        }

        private static IEnumerable<NewEncodingInfo> GetEncodings()
        {
            return Encoding.GetEncodings().Select(encoding => new NewEncodingInfo(encoding.DisplayName, encoding.CodePage)).OrderBy(encoding => encoding.DisplayName, new NaturalStringComparer(SortOrder.Ascending));
        }

        internal static void SetComboboxWithDictionary(ComboBox cmb, Dictionary<string, string> items)
        {
            cmb.DataSource = new BindingSource(items, null);
            cmb.DisplayMember = "key";
            cmb.ValueMember = "value";
            cmb.SelectedIndex = 0;
        }
    }
}
