using System.Collections.Generic;
using System.Linq;

namespace DataTableConverter.Classes
{
    class ImportSettings
    {
        internal int CodePage;
        internal string TextBegin = null;
        internal string TextEnd = null;
        internal List<string> Separators = null;
        internal List<int> Values = null;
        internal List<string> Headers = null;
        internal bool ContainsHeaders;
        internal bool HasRowBreaks;

        internal ImportSettings(int codePage, string textBegin, string textEnd, bool containsHeaders, object[] headers)
        {
            CodePage = codePage;
            TextBegin = textBegin;
            TextEnd = textEnd;
            ContainsHeaders = containsHeaders;
            Headers = headers.Cast<string>().ToList();
        }

        internal ImportSettings(List<string> separators, int codePage, bool containsHeaders, object[] headers)
        {
            CodePage = codePage;
            Separators = separators;
            ContainsHeaders = containsHeaders;
            Headers = headers.Cast<string>().ToList();
        }

        internal ImportSettings(List<int> values, List<string> headers, int codePage, bool hasRowBreaks)
        {
            Values = values;
            Headers = headers;
            CodePage = codePage;
            HasRowBreaks = hasRowBreaks;
        }
    }
}
