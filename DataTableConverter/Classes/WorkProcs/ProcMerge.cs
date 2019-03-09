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
        internal enum ConditionColumn : int { Spalte = 0, Wert = 1, Format = 2 };
        internal DataTable Conditions;

        internal ProcMerge(int ordinal, int id, string name) : base(ordinal, id, name) {
            InitConditions();
        }

        internal ProcMerge()
        {
            InitConditions();
        }

        private void InitConditions()
        {
            Conditions = new DataTable();
            Conditions.Columns.Add("Spalte");
            Conditions.Columns.Add("Wert");
            Conditions.Columns.Add("Format");
        }

        internal ProcMerge(string formula, DataTable conditions)
        {
            Formula = formula;
            Conditions = conditions;
        }

        public override string[] GetHeaders()
        {
            HashSet<string> headers = new HashSet<string>();
            GetHeaderOfFormula(Formula, headers);
            foreach(DataRow row in Conditions.Rows)
            {
                GetHeaderOfFormula(row[(int)ConditionColumn.Format].ToString(), headers);
                headers.Add(row[(int)ConditionColumn.Spalte].ToString());
            }
            return WorkflowHelper.RemoveEmptyHeaders(headers.ToArray());
        }

        private void GetHeaderOfFormula(string formula, HashSet<string> headers)
        {
            string regularExpressionPattern = @"\[(.*?)\]";
            Regex re = new Regex(regularExpressionPattern);

            MatchCollection matches = re.Matches(formula);

            for (int i = 0; i < matches.Count; i++)
            {
                headers.Add(matches[i].Value.Substring(1, matches[i].Value.Length - 2));
            }
        }

        public override void renameHeaders(string oldName, string newName)
        {
            Formula = ReplaceHeader(Formula,oldName, newName);
            foreach(DataRow row in Conditions.Rows)
            {
                row[(int)ConditionColumn.Format] = ReplaceHeader(row[(int)ConditionColumn.Format].ToString(),oldName, newName);
                row[(int)ConditionColumn.Spalte] = row[(int)ConditionColumn.Spalte].ToString() == oldName ? newName : row[(int)ConditionColumn.Spalte];
            }
        }

        private string ReplaceHeader(string source, string oldName, string newName)
        {
            return source.Replace($"[{oldName}]", $"[{newName}]");
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            sortingOrder = string.Empty; //base(table, sortingOrder, columns)
            int column = table.Columns.Count;
            DataHelper.addColumn(NewColumn, table);

            foreach (DataRow row in table.Rows)
            {
                string formula = Formula;
                foreach(DataRow condition in Conditions.Rows)
                {
                    if(row[condition[(int)ConditionColumn.Spalte].ToString()].ToString() == condition[(int)ConditionColumn.Wert].ToString())
                    {
                        formula = condition[(int)ConditionColumn.Format].ToString();
                    }
                }
                row[column] = GetFormat(row, formula);
            }
        }

        private string GetFormat(DataRow row, string formula)
        {
            HashSet<string> headers = new HashSet<string>();
            GetHeaderOfFormula(formula, headers);
            string[] columns = headers.ToArray();
            string format = string.Empty;
            int counter = columns.Length - 1;
            bool skipWhenEmpty = false;
            for (int i = formula.Length - 1; i >= 0; i--)
            {
                
                char c = formula[i];

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
                    string header = columns[counter];
                    skipWhenEmpty = string.IsNullOrWhiteSpace(row[header]?.ToString());
                    if (!skipWhenEmpty)
                    {
                        format = row[header] + format;
                    }

                    i -= (header.Length + 1);
                    counter--;
                }
            }
            return format;
        }

    }
}
