using System.Collections.Generic;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SelectHeader : Form
    {
        internal string Column => CmBHeaders.SelectedValue.ToString();

        internal SelectHeader(Dictionary<string, string> aliasColumnMapping, string header = null, string label = null)
        {
            InitializeComponent();
            if(header != null)
            {
                Text = header;
            }
            if(label != null)
            {
                dataSourceLabel.Text = label;
            }
            CmBHeaders.DataSource = new BindingSource(aliasColumnMapping, null);
            CmBHeaders.DisplayMember = "key";
            CmBHeaders.ValueMember = "value";
            CmBHeaders.SelectedIndex = 0;
        }
    }
}
