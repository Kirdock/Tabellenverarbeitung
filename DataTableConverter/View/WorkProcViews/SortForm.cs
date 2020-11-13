using DataTableConverter.Classes;
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
        private readonly Dictionary<string, string> Orders;
        private readonly BindingSource ListElements;
        internal OrderType OrderType => GetOrderType();

        internal SortForm(Dictionary<string,string> aliasColumnMapping, string orderBefore, OrderType orderType)
        {
            InitializeComponent();
            SetListBoxStyle();

            ListElements = new BindingSource(new Dictionary<string, string>(), null);
            lBoxSelectedHeaders.DataSource = ListElements;
            lBoxSelectedHeaders.DisplayMember = "key";
            lBoxSelectedHeaders.ValueMember = "value";
            Orders = new Dictionary<string, string>();
            ListBox box = clBoxHeaders;
            box.DataSource = new BindingSource(aliasColumnMapping, null);
            box.DisplayMember = "key";
            box.ValueMember = "value";

            
            adjustListBox(orderBefore);
            SetOrderType(orderType);
        }

        private void SetListBoxStyle()
        {
            ListBox[] listBoxes = new ListBox[]
            {
                clBoxHeaders,
                lBoxSelectedHeaders
            };
            foreach (ListBox listBox in listBoxes)
            {
                ViewHelper.SetListBoxStyle(listBox);
            }
        }

        private void SetOrderType(OrderType orderType)
        {
            switch (orderType)
            {
                case OrderType.Reverse:
                    rbReverse.Checked = true;
                    break;

                case OrderType.Windows:
                default:
                    rbWindows.Checked = true;
                    break;
            }
        }

        private OrderType GetOrderType()
        {
            OrderType type;
            if (rbWindows.Checked)
            {
                type = OrderType.Windows;
            }
            else
            {
                type = OrderType.Reverse;
            }
            return type;
        }

        private void adjustListBox(string orderBefore)
        {
            if (!string.IsNullOrWhiteSpace(orderBefore))
            {
                clBoxHeaders.ItemCheck -= clBoxHeaders_ItemCheck;

                string[] headersInformation = orderBefore.Split(new string[] { ","}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string info in headersInformation)
                {
                    string[] headerInfo = info.Split(new string[] { "] " }, StringSplitOptions.RemoveEmptyEntries);
                    string columnName = headerInfo[0].Trim().Substring(1);
                    string order = headerInfo[1];
                    
                    clBoxHeaders.SelectedValue = columnName;
                    int index = clBoxHeaders.SelectedIndex;
                    if (index != -1)
                    {
                        clBoxHeaders.SetItemChecked(index, true);
                        ListElements.Add(clBoxHeaders.SelectedItem);
                        Orders.Add(columnName, order.ToUpper() == "ASC" ? AscString : DescString);
                    }
                }
                clBoxHeaders.ItemCheck += clBoxHeaders_ItemCheck;
            }
        }

        private void clBoxHeaders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            KeyValuePair<string, string> pair = (KeyValuePair<string, string>)clBoxHeaders.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                Orders.Add(pair.Key, AscString);
                ListElements.Add(pair);
            }
            else
            {
                Orders.Remove(pair.Key);
                ListElements.Remove(pair);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            foreach(KeyValuePair<string,string> pair in ((Dictionary<string, string>)ListElements.DataSource))
            {
                builder.Append("[").Append(pair.Value).Append("]").Append(Orders[pair.Key] == AscString ? "ASC" : "DESC").Append(", ");
            }
            
            SortString = builder.Length >= 2 ? builder.ToString().Substring(0,builder.Length-2) : string.Empty;
            DialogResult = DialogResult.OK;
        }

        private void lBoxSelectedHeaders_DoubleClick(object sender, EventArgs e)
        {
            if(lBoxSelectedHeaders.SelectedIndex != -1)
            {
                Orders.Remove(lBoxSelectedHeaders.SelectedValue.ToString());
                clBoxHeaders.SelectedValue = lBoxSelectedHeaders.SelectedValue;

                clBoxHeaders.SetItemChecked(clBoxHeaders.SelectedIndex, false);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if (index > 0)
            {
                setUpOrDown(index, -1);
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if (index < ListElements.Count-1)
            {
                setUpOrDown(index, 1);
            }
        }

        private void setUpOrDown(int index, int value)
        {
            KeyValuePair<string,string> pair = (KeyValuePair<string, string>)ListElements[index];
            ListElements[index] = ListElements[index + value];
            ListElements[index + value] = pair;
            lBoxSelectedHeaders.SelectedIndex = index + value;
        }

        private void btnTop_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if (index > 0)
            {

                ListElements.Insert(0, ListElements[index]);
                ListElements.RemoveAt(index + 1);
                lBoxSelectedHeaders.SelectedIndex = 0;
            }
        }

        private void btnBottom_Click(object sender, EventArgs e)
        {
            int index = lBoxSelectedHeaders.SelectedIndex;
            if (index < ListElements.Count-1)
            {
                ListElements.Add(ListElements[index]);
                ListElements.RemoveAt(index);
                lBoxSelectedHeaders.SelectedIndex = ListElements.Count-1;
            }
        }

        private void lBoxSelectedHeaders_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                // Draw the background of the ListBox control for each item.
                e.DrawBackground();
                // Define the default color of the brush as black.
                Brush myBrush = Brushes.Black;

                string alias = ((KeyValuePair<string,string>)ListElements[e.Index]).Key;
                e.Graphics.DrawString($"{alias} {Orders[alias]}",
                    e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }


        private void lBoxSelectedHeaders_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = lBoxSelectedHeaders.IndexFromPoint(e.Location);
                if(index > -1)
                {
                    lBoxSelectedHeaders.SelectedIndex = index;
                    KeyValuePair<string,string> pair= (KeyValuePair<string,string>)lBoxSelectedHeaders.SelectedItem;
                    Orders[pair.Key] = Orders[pair.Key] == AscString ? DescString : AscString;
                    lBoxSelectedHeaders.Refresh();
                }
            }
        }
    }
}
