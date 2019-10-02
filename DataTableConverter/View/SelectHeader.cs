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
    public partial class SelectHeader : Form
    {
        internal string Column => CmBHeaders.SelectedItem.ToString();

        internal SelectHeader(object[] headers)
        {
            InitializeComponent();
            CmBHeaders.Items.AddRange(headers);
            CmBHeaders.SelectedIndex = 0;
        }
    }
}
