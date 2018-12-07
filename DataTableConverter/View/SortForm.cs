using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SortForm : Form
    {
        public string SortString;
        private readonly string DescString = "↓";
        private readonly string AscString = "↑";
        Dictionary<string, string> Orders;

        internal SortForm(object[] items, string orderBefore)
        {
            InitializeComponent();
            Orders = new Dictionary<string, string>();
            clBoxHeaders.Items.AddRange(items);
            adjustListBox(orderBefore);
        }

        private void adjustListBox(string orderBefore)
        {
            if (!string.IsNullOrWhiteSpace(orderBefore))
            {
                //Regex re = new Regex(@"\[(.*?)\]");
                //MatchCollection matches = re.Matches(orderBefore);
                //string[] headers = new string[matches.Count];
                //for (int i = 0; i < matches.Count; i++)
                //{
                //    string header = matches[i].Value.Substring(1, matches[i].Value.Length - 2);
                //    int index;
                //    if ((index = clBoxHeaders.Items.IndexOf(header)) != -1)
                //    {
                //        clBoxHeaders.SetItemChecked(index, true);
                //    }
                //}
                clBoxHeaders.ItemCheck -= clBoxHeaders_ItemCheck;

                string[] headersInformation = orderBefore.Split(new string[] { ","}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string info in headersInformation)
                {
                    string[] headerInfo = info.Split(new string[] { "] " }, StringSplitOptions.RemoveEmptyEntries);
                    string header = headerInfo[0].Trim().Substring(1);
                    string order = headerInfo[1];
                    int index;
                    if ((index = clBoxHeaders.Items.IndexOf(header)) != -1)
                    {
                        clBoxHeaders.SetItemChecked(index, true);
                        lBoxSelectedHeaders.Items.Add(header);
                        Orders.Add(header, order.ToUpper() == "ASC" ? AscString : DescString);
                    }
                }
                clBoxHeaders.ItemCheck += clBoxHeaders_ItemCheck;
            }
        }

        private void clBoxHeaders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            object value = clBoxHeaders.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                Orders.Add(value.ToString(), AscString);
                lBoxSelectedHeaders.Items.Add(value);
            }
            else
            {
                Orders.Remove(value.ToString());
                lBoxSelectedHeaders.Items.Remove(value);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            lBoxSelectedHeaders.Items.Cast<object>().ToList().ForEach(x => builder.Append("[").Append(x).Append("] ").Append(Orders[x.ToString()] == AscString ? "ASC" : "DESC").Append(", "));
            SortString = builder.ToString().Substring(0,builder.Length-2);
            DialogResult = DialogResult.OK;
        }

        private void lBoxSelectedHeaders_DoubleClick(object sender, EventArgs e)
        {
            if(lBoxSelectedHeaders.SelectedIndex != -1)
            {
                clBoxHeaders.SetItemChecked(clBoxHeaders.Items.IndexOf(lBoxSelectedHeaders.SelectedItem), false);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if ( index > 0)
            {
                setUpOrDown(index, -1);
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if (index < lBoxSelectedHeaders.Items.Count-1)
            {
                setUpOrDown(index, 1);
            }
        }

        private void setUpOrDown(int index, int value)
        {
            object temp = lBoxSelectedHeaders.Items[index];
            lBoxSelectedHeaders.Items[index] = lBoxSelectedHeaders.Items[index + value];
            lBoxSelectedHeaders.Items[index + value] = temp;
            lBoxSelectedHeaders.SelectedIndex = index + value;
        }

        private void btnTop_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if (index > 0)
            {
                lBoxSelectedHeaders.Items.Insert(0, lBoxSelectedHeaders.Items[index]);
                lBoxSelectedHeaders.Items.RemoveAt(index + 1);
                lBoxSelectedHeaders.SelectedIndex = 0;
            }
        }

        private void btnBottom_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if (index < lBoxSelectedHeaders.Items.Count-1)
            {
                lBoxSelectedHeaders.Items.Add(lBoxSelectedHeaders.Items[index]);
                lBoxSelectedHeaders.Items.RemoveAt(index);
                lBoxSelectedHeaders.SelectedIndex = lBoxSelectedHeaders.Items.Count-1;
            }
        }

        private void lBoxSelectedHeaders_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Define the default color of the brush as black.
            Brush myBrush = Brushes.Black;

            string value = lBoxSelectedHeaders.Items[e.Index].ToString();
            e.Graphics.DrawString($"{value} {Orders[value]}",
                e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }


        private void lBoxSelectedHeaders_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                lBoxSelectedHeaders.SelectedIndex = lBoxSelectedHeaders.IndexFromPoint(e.Location);
                string value = lBoxSelectedHeaders.SelectedItem.ToString();
                Orders[value] = Orders[value] == AscString ? DescString : AscString;
                lBoxSelectedHeaders.Refresh();
            }
        }
    }
}
