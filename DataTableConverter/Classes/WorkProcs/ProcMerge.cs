using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
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
        internal enum ConditionalText { Right, Left, Both, None};
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

        public override void removeHeader(string colName)
        {
            Conditions = Conditions.AsEnumerable().Where(condition => condition[(int)ConditionColumn.Spalte].ToString() != colName).ToTable(Conditions);
        }

        private string ReplaceHeader(string source, string oldName, string newName)
        {
            return source.Replace($"[{oldName}]", $"[{newName}]");
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType)
        {
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                int column = table.Columns.IndexOf(NewColumn);
                if (CopyOldColumn && column > -1)
                {
                    table.CopyColumns(new string[] { NewColumn });
                }
                else if(!CopyOldColumn && column == -1)
                {
                    column = table.Columns.Count;
                    table.TryAddColumn(NewColumn);
                }

                foreach (DataRow row in table.Rows)
                {
                    var match = Conditions.AsEnumerable().FirstOrDefault(condition => !string.IsNullOrWhiteSpace(condition[(int)ConditionColumn.Spalte]?.ToString()) && row[condition[(int)ConditionColumn.Spalte].ToString()].ToString() == condition[(int)ConditionColumn.Wert].ToString());
                    string formula = match == null ? Formula : match[(int)ConditionColumn.Format].ToString();
                    
                    row[column] = GetFormat(row, formula, table.Columns);
                }
            }
            else
            {
                MessageHandler.MessagesOK(System.Windows.Forms.MessageBoxIcon.Warning, $"{ClassName}: Der Name der neu anzulegenden Spalte darf nicht leer sein!");
            }
        }

        private string GetFormat(DataRow row, string formula, DataColumnCollection tableColumns)
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
            ConditionalText condDefault = Properties.Settings.Default.DefaultFormular == 0 ? ConditionalText.None : Properties.Settings.Default.DefaultFormular == 1 ? ConditionalText.Left : ConditionalText.Right;
            for (int i = 0; i < formula.Length; i++)
            {
                char c = formula[i];

                if (c == '(')
                {
                    string value = headersInBrackets[bracketCount].FirstOrDefault(h => tableColumns.Contains(h) && !string.IsNullOrWhiteSpace(row[h]?.ToString())) ?? string.Empty;
                    if (value != string.Empty)
                    {
                        format.Append(row[value].ToString());
                    }
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

                    string value = tableColumns.Contains(header) ? row[header]?.ToString() : string.Empty;
                    bool isEmpty = string.IsNullOrWhiteSpace(value);

                    ConditionalText direction = GetDirection(formula, i + header.Length + 1, out int newIndex, condDefault);

                    //if ((directionBefore == ConditionalText.Right || directionBefore == ConditionalText.Both) && (direction == ConditionalText.Left || direction == ConditionalText.Both))
                    //{
                    //    if (emptyBefore && isEmpty)
                    //    {
                    //        stringBetween.Clear();
                    //    }
                    //}
                    if ((    (direction == ConditionalText.Left || direction == ConditionalText.Both) && isEmpty
                        || (directionBefore == ConditionalText.Right || directionBefore == ConditionalText.Both )&& emptyBefore
                        )
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

        private ConditionalText GetDirection(string formula, int startindex, out int newIndex, ConditionalText condDefault = ConditionalText.None)
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

                case "|lr":
                case "|rl":
                    result = ConditionalText.Both;
                    break;
                default:
                    result = condDefault;
                    break;
            }
            return result;
        }
    }
}
