using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class PlusListboxItem
    {
        internal bool Checked;
        internal object value;

        public PlusListboxItem(bool @checked, object value)
        {
            Checked = @checked;
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }


    }
}
