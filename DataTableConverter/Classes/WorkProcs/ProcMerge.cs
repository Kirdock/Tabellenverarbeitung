using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcMerge : WorkProc
    {
        internal static readonly string ClassName = "Spalten zusammenfügen";
        internal ProcMerge(int ordinal, int id, string name) : base(ordinal, id, name) { }

        internal ProcMerge(string formula)
        {
            Formula = formula;
        }

        public override string[] GetHeaders()
        {
            string regularExpressionPattern = @"\[(.*?)\]";
            Regex re = new Regex(regularExpressionPattern);

            MatchCollection matches = re.Matches(Formula);
            
            string[] headers = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                headers[i] = matches[i].Value.Substring(1, matches[i].Value.Length - 2);
            }
            return WorkflowHelper.removeEmptyHeaders(headers);
        }

        public override void renameHeaders(string oldName, string newName)
        {
            Formula = Formula.Replace($"[{oldName}]", $"[{newName}]");
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            sortingOrder = string.Empty; //base(table, sortingOrder, columns)
            string[] columns = GetHeaders();
            int column = table.Columns.Count;
            DataHelper.addColumn(NewColumn, table);

            columns = columns.Select(h => h.ToLower()).ToArray();

            List<string> tableHeaders = DataHelper.getHeadersToLower(table);


            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                DataRow row = table.Rows[rowIndex];
                string format = "";

                int counter = columns.Length - 1;

                bool skipWhenEmpty = false;
                for (int i = Formula.Length - 1; i >= 0; i--)
                {
                    string header = columns[counter];
                    int index = tableHeaders.IndexOf(header.ToLower());
                    char c = Formula[i];

                    if (c != ']')
                    {
                        if (skipWhenEmpty)
                        {
                            continue;
                        }
                        else
                        {
                            format = c + format;
                        }
                    }
                    else
                    {
                        skipWhenEmpty = string.IsNullOrWhiteSpace(row[index]?.ToString());
                        if (!skipWhenEmpty)
                        {
                            format = row[index] + format;
                        }

                        i -= (header.Length + 1);
                        counter--;
                    }
                }

                table.Rows[rowIndex].SetField(column, format);
            }
        }

    }
}
