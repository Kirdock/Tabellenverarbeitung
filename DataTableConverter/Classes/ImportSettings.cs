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
        internal bool ContainsHeaders;

        internal ImportSettings(int codePage, string textBegin, string textEnd, bool containsHeaders, object[] headers)
        {
            CodePage = codePage;
            TextBegin = textBegin;
            TextEnd = textEnd;
            ContainsHeaders = containsHeaders;
            Headers = headers.Cast<string>().ToList();
        }

        internal ImportSettings(string separator, int codePage, bool containsHeaders, object[] headers)
        {
            CodePage = codePage;
            Separator = separator;
            ContainsHeaders = containsHeaders;
            Headers = headers.Cast<string>().ToList();
        }

        internal ImportSettings(List<int> values, List<string> headers, int codePage)
        {
            Values = values;
            Headers = headers;
            CodePage = codePage;
        }
    }
}
