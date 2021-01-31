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
