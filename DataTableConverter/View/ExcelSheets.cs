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
    public partial class ExcelSheets : Form
    {
        internal ExcelSheets(string[] sheets)
        {
            InitializeComponent();
            cList.Items.AddRange(sheets);
        }

        internal string[] GetSheets()
        {
            return cList.CheckedItems.Cast<string>().ToArray();
        }
        

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            setChecked(true);
        }

        private void setChecked(bool status)
        {
            for (int i = 0; i < cList.Items.Count; i++)
            {
                cList.SetItemChecked(i, status);
            }
        }
        
        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            setChecked(false);
        }
    }
}
