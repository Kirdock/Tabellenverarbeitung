using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcMerge : WorkProc
    {
        internal static readonly string ClassName = "Spalten zusammenfügen";
        internal enum ConditionColumn : int { Spalte = 0, Wert = 1, Format = 2 };
        internal enum ConditionalText { Right, Left, None};
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
            Conditions.Columns.Add("Spalte", typeof(string));
            Conditions.Columns.Add("Wert", typeof(string));
            Conditions.Columns.Add("Format", typeof(string));
        }

        internal ProcMerge(string formula, DataTable conditions)
        {
            Formula = formula;
            Conditions = conditions;
        }

        public override string[] GetHeaders()
        {
            List<string> headers = new List<string>();
            GetHeaderOfFormula(Formula, headers);
            foreach(DataRow row in Conditions.Rows)
            {
                GetHeaderOfFormula(row[(int)ConditionColumn.Format].ToString(), headers);
                headers.Add(row[(int)ConditionColumn.Spalte].ToString());
            }
            return WorkflowHelper.RemoveEmptyHeaders(headers.Distinct());
        }

        private void GetHeaderOfFormula(string formula, List<string> headers)
        {
            string regularExpressionPattern = @"\[(.*?)\]";
            Regex re = new Regex(regularExpressionPattern);

            MatchCollection matches = re.Matches(formula);

            for (int i = 0; i < matches.Count; i++)
            {
                headers.Add(matches[i].Groups[1].Value.Split(new char[] { '|' }).First());
            }
        }

        private List<string[]> GetHeaderInBrackets(string formula)
        {
            List<string[]> headers = new List<string[]>();
            string regularExpressionPattern = @"\((.*?)\)";
            Regex re = new Regex(regularExpressionPattern);

            MatchCollection matches = re.Matches(formula);

            for (int i = 0; i < matches.Count; i++)
            {
                List<string> headerInBrackets = new List<string>();
                GetHeaderOfFormula(matches[i].Groups[1].Value, headerInBrackets);
                headers.Add(headerInBrackets.ToArray());
            }
            return headers;
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

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow)
        {
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                int column = table.Columns.Count;
                DataHelper.AddColumn(NewColumn, table);

                foreach (DataRow row in table.Rows)
                {
                    string formula = Formula;
                    foreach (DataRow condition in Conditions.Rows)
                    {
                        if (row[condition[(int)ConditionColumn.Spalte].ToString()].ToString() == condition[(int)ConditionColumn.Wert].ToString())
                        {
                            formula = condition[(int)ConditionColumn.Format].ToString();
                        }
                    }
                    row[column] = GetFormat(row, formula);
                }
            }
            else
            {
                MessageHandler.MessagesOK(System.Windows.Forms.MessageBoxIcon.Warning, $"{ClassName}: Der Name der neu anzulegenden Spalte darf nicht leer sein!");
            }
        }

        private string GetFormat(DataRow row, string formula)
        {
            List<string> headers = new List<string>();
            GetHeaderOfFormula(formula, headers);
            string[] columns = headers.ToArray();
            StringBuilder format = new StringBuilder();
            int counter = 0;
            StringBuilder stringBetween = new StringBuilder();
            bool emptyBefore = false;
            List<string[]> headersInBrackets = GetHeaderInBrackets(formula);
            if(headersInBrackets.Count == 0)
            {
                headersInBrackets.Add(new string[0]); //to not enter an ArrayOutOfBoundsException
            }
            int bracketCount = 0;
            ConditionalText directionBefore = ConditionalText.None;
            for (int i = 0; i < formula.Length; i++)
            {
                char c = formula[i];

                if (c == '(')
                {
                    string value = string.Empty;
                    foreach (string header in headersInBrackets[bracketCount])
                    {
                        string result;
                        if (!string.IsNullOrWhiteSpace((result = row[header]?.ToString())))
                        {
                            value = result;
                            break;
                        }
                    }
                    format.Append(value);
                    counter += headersInBrackets[bracketCount].Length;
                    i = formula.IndexOf(')', i);
                    bracketCount++;
                }
                else if (c == '[') //insert column value
                {
                    if(counter > columns.Length)
                    {
                        ErrorHelper.LogMessage($"Ungültiges Format? Format:{formula}");
                        continue;
                    }
                    string header = columns[counter];

                    string value = row[header]?.ToString();
                    bool isEmpty = string.IsNullOrWhiteSpace(value);

                    ConditionalText direction = GetDirection(formula, i + header.Length + 1, out int newIndex);

                    if(    direction == ConditionalText.Left && isEmpty
                        || directionBefore == ConditionalText.Right && emptyBefore
                      )
                    {
                        stringBetween.Clear();
                    }

                    format.Append(stringBetween);

                    if (!isEmpty)
                    {
                        format.Append(value);
                    }

                    stringBetween.Clear();
                    i = newIndex;
                    counter++;
                    emptyBefore = isEmpty;
                    directionBefore = direction;
                }
                else
                {
                    stringBetween.Append(c);
                }
            }
            return stringBetween.Length != 0 ? format.Append(stringBetween).ToString() : format.ToString();
        }

        private ConditionalText GetDirection(string formula, int startindex, out int newIndex)
        {
            newIndex = formula.IndexOf(']', startindex);
            string value = formula.Substring(startindex, newIndex - startindex).Trim().ToLower();
            ConditionalText result;
            switch (value)
            {
                case "|r":
                    result = ConditionalText.Right;
                    break;

                case "|l":
                    result = ConditionalText.Left;
                    break;

                default:
                    result = ConditionalText.None;
                    break;
            }
            return result;
        }
    }
}
