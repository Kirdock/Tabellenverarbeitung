using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class ExcelSheets : Form
    {
        internal ExcelSheets(string[] sheets, string headerText = null)
        {
            InitializeComponent();
            cList.Items.AddRange(sheets);
            ViewHelper.SetListBoxStyle(cList);
            Text = headerText ?? Text;
        }

        internal int[] GetSheets()
        {
            int[] selectedIndizes = new int[cList.CheckedItems.Count];
            int pointer = 0;
            for(int i = 0; i < cList.Items.Count; i++)
            {
                if (cList.GetItemChecked(i))
                {
                    selectedIndizes[pointer++] = i;
                }
            }
            return selectedIndizes;
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
