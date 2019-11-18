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
    public partial class SelectEncoding : Form
    {
        internal int FileEncoding => (int)CmBEncoding.SelectedValue;
        public SelectEncoding()
        {
            InitializeComponent();
            ViewHelper.SetEncodingCmb(CmBEncoding);
        }
    }
}
