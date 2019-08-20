using DataTableConverter.Classes.WorkProcs;
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
    public partial class HeaderSelect : Form
    {
        internal string Headers;
        internal HeaderSelect(object[] headers, string oldHeaders)
        {
            InitializeComponent();
            ClBHeaders.Items.AddRange(headers);
            SetChecked(oldHeaders);
            SetListBoxStyle();
        }

        private void SetListBoxStyle()
        {
            ViewHelper.SetListBoxStyle(ClBHeaders);
        }

        private void SetChecked(string oldHeaders)
        {
            foreach (string header in ProcMerge.GetHeaderOfFormula(oldHeaders))
            {
                int index;
                if ((index = ClBHeaders.Items.IndexOf(header)) > -1)
                {
                    ClBHeaders.SetItemChecked(index, true);
                }
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            StringBuilder result = new StringBuilder();
            if(ClBHeaders.CheckedItems.Count > 0)
            {
                result.Append("[").Append(string.Join("] [", ClBHeaders.CheckedItems.Cast<string>())).Append("]");
            }
            Headers = result.ToString();
            DialogResult = DialogResult.OK;
        }
    }
}
