using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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