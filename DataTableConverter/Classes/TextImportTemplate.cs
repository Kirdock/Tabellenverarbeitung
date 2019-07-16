using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    [Serializable()]
    class TextImportTemplate
    {
        public enum SelectedSeparatedState { Tab, TabCharacter, Between}
        internal DataTable Table;
        internal int Encoding;
        internal bool ContainsHeaders;
        internal string StringSeparator;
        internal string BeginSeparator;
        internal string EndSeparator;
        internal SelectedSeparatedState SelectedSeparated;
        internal int Variant;


        internal TextImportTemplate() { }

        
    }
}
