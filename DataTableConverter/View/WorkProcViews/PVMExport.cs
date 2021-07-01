using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View.WorkProcViews
{
    public partial class PVMExport : Form
    {
        internal string[] SelectedHeaders() => ViewHelper.GetSelectedHeaders(cbHeaders);
        internal SaveFormat SelectedFormat => RBCSV.Checked ? SaveFormat.CSV : RBDBASE.Checked ? SaveFormat.DBASE : SaveFormat.EXCEL;
        internal PVMExport(IEnumerable<string> aliases)
        {
            InitializeComponent();
            cbHeaders.Items.AddRange(aliases.ToArray());
            LoadSetting();
        }

        private void LoadSetting()
        {
            if (Properties.Settings.Default.ExportSpec != null)
            {
                foreach (string header in Properties.Settings.Default.ExportSpec)
                {
                    int index = FindItem(header);
                    if (index != -1)
                    {
                        cbHeaders.SetItemChecked(index, true);
                    }
                }
            }
        }

        private int FindItem(string header)
        {
            header = header.ToLower();
            for (int i = 0; i < cbHeaders.Items.Count; i++)
            {
                if (header.Equals(cbHeaders.Items[i].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, true);
        }


        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, false);
        }

        private void PVMExport_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ExportSpec = new System.Collections.ArrayList(cbHeaders.CheckedItems.Cast<string>().ToArray());
            Properties.Settings.Default.Save();
        }
    }
}
