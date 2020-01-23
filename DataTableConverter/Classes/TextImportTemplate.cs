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
        public DataTable Table;
        public int Encoding;
        public bool ContainsHeaders;
        public string StringSeparator; //deprecated but used for migration
        public List<string> separators;
        public List<string> Separators
        {
            get
            {
                return separators ?? (StringSeparator != null ? new List<string> { StringSeparator } : new List<string>());
            }
            set
            {
                separators = value;
            }
        }
        public string BeginSeparator;
        public string EndSeparator;
        public SelectedSeparatedState SelectedSeparated;
        public int Variant;

        internal TextImportTemplate() { }
    }
}
