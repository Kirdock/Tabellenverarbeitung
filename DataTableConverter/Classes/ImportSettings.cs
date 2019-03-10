using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class ImportSettings
    {
        internal int CodePage;
        internal string TextBegin = null;
        internal string TextEnd = null;
        internal string Separator = null;
        internal List<int> Values = null;
        internal List<string> Headers = null;

        internal ImportSettings(int codePage, string textBegin, string textEnd)
        {
            CodePage = codePage;
            TextBegin = textBegin;
            TextEnd = textEnd;
        }

        internal ImportSettings(string separator, int codePage)
        {
            CodePage = codePage;
            Separator = separator;
        }

        internal ImportSettings(List<int> values, List<string> headers, int codePage)
        {
            Values = values;
            Headers = headers;
            CodePage = codePage;
        }
    }
}
