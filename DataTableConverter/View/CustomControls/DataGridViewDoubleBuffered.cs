using System.Windows.Forms;

namespace DataTableConverter
{
    class DataGridViewDoubleBuffered : DataGridView
    {

        public DataGridViewDoubleBuffered()
        {
            DoubleBuffered = true;
        }

    }
}