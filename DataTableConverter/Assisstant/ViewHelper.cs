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

        internal static void insertClipboardToDataGridView(DataGridView myDataGridView)
        {
            myDataGridView.BindingContext[myDataGridView.DataSource].EndCurrentEdit();
            DataTable table = ((DataTable)myDataGridView.DataSource).Copy();

            DataObject o = (DataObject)Clipboard.GetDataObject();
            int columnCount = table.Columns.Count;
            if (o.GetDataPresent(DataFormats.Text))
            {
                string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                foreach (string pastedRow in pastedRows)
                {
                    string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });
                    try
                    {
                        DataRow row = table.NewRow();
                        row.ItemArray = pastedRowCells.Length > columnCount ? pastedRowCells.Take(columnCount).ToArray() : pastedRowCells;
                        table.Rows.Add(row);
                    }
                    catch { } //Keine Übereinstimmung mit dem ColumnTyp z.B. int
                }
                myDataGridView.DataSource = null;
                myDataGridView.DataSource = table;
            }
        }

        internal static List<int> SelectedRows(DataGridView sender)
        {
            List<int> selectedRows = sender.SelectedCells.Cast<DataGridViewCell>().Select(cell => cell.RowIndex).Where(row => row != sender.Rows.Count - 1).Distinct().ToList();
            selectedRows.Sort();
            return selectedRows;
        }
    }
}
