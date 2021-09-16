using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcMerge : WorkProc
    {
        internal static readonly string ClassName = "Spalten zusammenfügen";
        internal enum ConditionColumn : int { Spalte = 0, Wert = 1, NichtLeer = 2, Format = 3 };
        internal enum ConditionalText { Right, Left, None };
        public DataTable Conditions;
        public MergeFormat Format;

        internal ProcMerge(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            InitConditions();
            Format = new MergeFormat();
        }

        internal ProcMerge()
        {
            InitConditions();
            Format = new MergeFormat();
        }

        private void InitConditions()
        {
            Conditions = new DataTable();
            Conditions.Columns.Add("Spalte", typeof(string));
            Conditions.Columns.Add("Wert", typeof(string));
            Conditions.Columns.Add("Wenn nicht leer", typeof(bool));
            Conditions.Columns.Add("Format", typeof(MergeFormat));
        }

        private IEnumerable<string> GetConditionHeaders()
        {
            IEnumerable<string> columnHeaders = Conditions.AsEnumerable().Select(row => row[(int)ConditionColumn.Spalte]?.ToString()).Where(header => !string.IsNullOrWhiteSpace(header));
            IEnumerable<string> formatHeaders = Conditions.AsEnumerable().SelectMany(row => (row[(int)ConditionColumn.Format] as MergeFormat)?.GetHeaders());
            return columnHeaders.Concat(formatHeaders).Distinct();
        }

        public override string[] GetHeaders()
        {
            IEnumerable<string> mergeFormatHeaders = Format.GetHeaders();
            IEnumerable<string> conditionHeaders = GetConditionHeaders();

            return mergeFormatHeaders.Concat(conditionHeaders).Distinct().ToArray();
        }

        internal static IEnumerable<string> GetHeaderOfFormula(string formula)
        {
            string regularExpressionPattern = @"\[(.*?)\]";
            Regex re = new Regex(regularExpressionPattern);

            return re.Matches(formula ?? string.Empty).Cast<Match>().Select(col => col.Groups[1].Value);
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            Format.RenameHeaders(oldName, newName);
            foreach (DataRow row in Conditions.Rows)
            {
                RenameFormatHeader(row, (int)ConditionColumn.Format, oldName, newName);
                RenameHeader(row, (int)ConditionColumn.Spalte, oldName, newName);
                (row[(int)ConditionColumn.Format] as MergeFormat)?.RenameHeaders(oldName, newName);
            }
        }

        internal static void RenameHeader(DataRow row, int index, string oldName, string newName)
        {
            row[index] = row[index].ToString() == oldName ? newName : row[index];
        }

        internal static void RenameFormatHeader(DataRow row, int index, string oldName, string newName)
        {
            if (row[index] is MergeFormat format)
            {
                format.Formula.Replace($"[{oldName}]", $"[{newName}]");
            }
            else
            {
                row[index] = row[index].ToString().Replace($"[{oldName}]", $"[{newName}]");
            }
        }

        internal static string RenameFormatHeader(string format, string oldName, string newName)
        {
            return format?.Replace($"[{oldName}]", $"[{newName}]");
        }

        //we only remove Conditions
        //for Format: a non existing column is seen as an empty column
        public override void RemoveHeader(string colName)
        {
            Conditions = Conditions.AsEnumerable().Where(condition => condition[(int)ConditionColumn.Spalte].ToString() != colName).ToTable(Conditions);
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {

                //column is alias
                string column = NewColumn;
                PrepareSingle(ref column, invokeForm, tableName, out string destinationColumn);

                //column is columnName now
                if (destinationColumn != null)
                {
                    List<string> aliases = invokeForm.DatabaseHelper.GetSortedColumnsAsAlias(tableName).Select(alias => alias.ToLower()).ToList();
                    List<KeyValuePair<long, string>> updates = new List<KeyValuePair<long, string>>();
                    using (System.Data.SQLite.SQLiteDataReader reader = invokeForm.DatabaseHelper.GetDataCommand(tableName, "id").ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DataRow match = Conditions.AsEnumerable().FirstOrDefault(condition =>
                            {
                                string alias = condition[(int)ConditionColumn.Spalte]?.ToString().ToLower();
                                string rowValue;
                                bool notEmpty = condition[(int)ConditionColumn.NichtLeer] == DBNull.Value ? false : (bool)condition[(int)ConditionColumn.NichtLeer];
                                return !string.IsNullOrWhiteSpace(alias)
                                    &&
                                    (
                                        ((rowValue = reader.GetValue(aliases.IndexOf(alias) + 1).ToString()) == condition[(int)ConditionColumn.Wert].ToString() && !notEmpty)
                                        ||
                                        (
                                            notEmpty
                                            &&
                                            !string.IsNullOrWhiteSpace(rowValue)
                                        )
                                    );
                            });
                            Dictionary<string, string> aliasValueMapping = new Dictionary<string, string>();
                            for (int i = 1; i < reader.FieldCount; i++)
                            {
                                aliasValueMapping.Add(aliases[i - 1], reader.GetValue(i).ToString());
                            }
                            MergeFormat format = match == null ? Format : match[(int)ConditionColumn.Format] as MergeFormat;
                            string result = format.IsStringFormat() ? GetFormat(aliasValueMapping, format.Formula, aliases, invokeForm) : GetFormat(aliasValueMapping, format, aliases);
                            updates.Add(new KeyValuePair<long, string>(reader.GetInt64(0), result));
                        }
                    }

                    invokeForm.DatabaseHelper.UpdateCells(updates, destinationColumn, tableName);
                }
            }
            else
            {
                invokeForm.MessagesOK(MessageBoxIcon.Warning, $"{ClassName}: Der Name der neu anzulegenden Spalte darf nicht leer sein!");
            }
        }

        private string GetFormat(Dictionary<string, string> sourceRow, MergeFormat format, List<string> tableColumns)
        {
            StringBuilder result = new StringBuilder();
            IEnumerable<string> formatHeaders = format.GetHeaders().Select(header => header.ToLower());
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            foreach (string header in formatHeaders)
            {
                dict.Add(header, tableColumns.Contains(header));
            }

            foreach (DataRow row in format.Table.AsEnumerable())
            {
                string column = row[(int)MergeFormat.MergeColumns.Column]?.ToString();
                bool columnIsEmpty = string.IsNullOrWhiteSpace(column);
                if (columnIsEmpty || dict[column])
                {
                    //could contain columns that are not in the table
                    IEnumerable<string> emptyHeaderOfRow = GetHeaderOfFormula(row[(int)MergeFormat.MergeColumns.Empty]?.ToString()).Select(header => header.ToLower()).Where(header => dict[header]);
                    bool emptyAllChecked = row[(int)MergeFormat.MergeColumns.EmptyAll] == DBNull.Value ? false : (bool)row[(int)MergeFormat.MergeColumns.EmptyAll];
                    bool emptyFullFilled = emptyHeaderOfRow.Count() == 0 || emptyAllChecked ? emptyHeaderOfRow.All(header =>
                    {
                        sourceRow.TryGetValue(header, out string res);
                        return string.IsNullOrWhiteSpace(res);
                    }) : emptyHeaderOfRow.Any(header =>
                    {
                        sourceRow.TryGetValue(header, out string res);
                        return string.IsNullOrWhiteSpace(res);
                    });

                    if (emptyFullFilled)
                    {
                        IEnumerable<string> notEmptyHeaderOfRow = GetHeaderOfFormula(row[(int)MergeFormat.MergeColumns.NotEmpty]?.ToString()).Select(header => header.ToLower()).Where(header => dict[header]);
                        bool notEmptyAllChecked = row[(int)MergeFormat.MergeColumns.NotEmptyAll] == DBNull.Value ? false : (bool)row[(int)MergeFormat.MergeColumns.NotEmptyAll];
                        bool notEmptyFullFilled = notEmptyHeaderOfRow.Count() == 0 || notEmptyAllChecked ? notEmptyHeaderOfRow.All(header =>
                        {
                            sourceRow.TryGetValue(header, out string res);
                            return string.IsNullOrWhiteSpace(res);
                        }) : notEmptyHeaderOfRow.Any(header =>
                        {
                            sourceRow.TryGetValue(header, out string res);
                            return string.IsNullOrWhiteSpace(res);
                        });
                        if (notEmptyFullFilled)
                        {
                            if (!columnIsEmpty)
                            {
                                if (sourceRow.TryGetValue(column, out string value))
                                {
                                    result.Append(value);
                                }

                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    result.Append(row[(int)MergeFormat.MergeColumns.Text]?.ToString());
                                }
                            }
                            else
                            {
                                result.Append(row[(int)MergeFormat.MergeColumns.Text]?.ToString());
                            }
                        }
                    }
                }
            }
            return result.ToString();
        }


        private string GetFormat(Dictionary<string, string> row, string formula, List<string> tableColumns, Form mainForm)
        {
            string[] columns = GetHeaderOfFormula(formula).Select(header => header.ToLower()).ToArray();
            Dictionary<FormatIdentifier, bool> emptyAfterHeader = GetEmptyAfterHeaders(columns, row, tableColumns);
            StringBuilder result = new StringBuilder();
            int counter = 0;
            StringBuilder stringBetween = new StringBuilder();
            bool emptyBefore = false;
            List<string[]> headersInBrackets = GetHeaderInBrackets(formula).ToList();
            if (headersInBrackets.Count == 0)
            {
                headersInBrackets.Add(new string[0]); //to not enter an ArrayOutOfBoundsException
            }
            int bracketCount = 0;
            ConditionalText directionBefore = ConditionalText.None;
            ConditionalText condDefault = ConditionalText.Right;
            for (int i = 0; i < formula.Length; i++)
            {
                char c = formula[i];

                if (c == '(')
                {
                    string value = headersInBrackets[bracketCount].FirstOrDefault(h => tableColumns.Contains(h) && !string.IsNullOrWhiteSpace(row[h]?.ToString())) ?? string.Empty;
                    if (value != string.Empty)
                    {
                        result.Append(row[value]?.ToString());
                    }
                    counter += headersInBrackets[bracketCount].Length;
                    i = formula.IndexOf(')', i);
                    bracketCount++;
                }
                else if (c == '[') //insert column value
                {
                    if (counter > columns.Length)
                    {
                        ErrorHelper.ShowError($"Ungültiges Format? Format:{formula}", mainForm);
                        continue;
                    }
                    string header = columns[counter];

                    string value = tableColumns.Contains(header) ? row[header]?.ToString() : string.Empty;
                    bool isEmpty = string.IsNullOrWhiteSpace(value);
                    ConditionalText direction = GetDirection(formula, i + header.Length + 1, out int newIndex, condDefault);

                    if ((direction == ConditionalText.Left && isEmpty
                        || (directionBefore == ConditionalText.Right) && emptyBefore && direction != ConditionalText.Left
                        )
                      )
                    {
                        stringBetween.Clear();
                    }

                    result.Append(stringBetween);

                    if (!isEmpty)
                    {
                        result.Append(value);
                    }



                    stringBetween.Clear();

                    if (emptyAfterHeader[new FormatIdentifier { Header = columns[counter], Index = counter }])
                    {
                        break;
                    }

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
            return stringBetween.Length != 0 ? result.Append(stringBetween).ToString() : result.ToString();
        }

        private Dictionary<FormatIdentifier, bool> GetEmptyAfterHeaders(string[] headers, Dictionary<string, string> row, List<string> tableColumns)
        {
            Dictionary<FormatIdentifier, bool> dict = new Dictionary<FormatIdentifier, bool>();
            Dictionary<string, bool> isEmpty = new Dictionary<string, bool>();
            foreach (string header in headers)
            {
                if (!isEmpty.ContainsKey(header))
                {
                    row.TryGetValue(header, out string value);
                    isEmpty.Add(header, !tableColumns.Contains(header) || string.IsNullOrWhiteSpace(value));
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                dict.Add(new FormatIdentifier { Header = headers[i], Index = i }, i != headers.Length && headers.Skip(i + 1).All(header => isEmpty[header]));
            }

            return dict;
        }

        private ConditionalText GetDirection(string formula, int startindex, out int newIndex, ConditionalText condDefault)
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
                    result = condDefault;
                    break;
            }
            return result;
        }

        private IEnumerable<string[]> GetHeaderInBrackets(string formula)
        {
            Regex re = new Regex(@"\((.*?)\)");
            MatchCollection matches = re.Matches(formula);

            for (int i = 0; i < matches.Count; i++)
            {
                yield return GetHeaderOfFormula(matches[i].Groups[1].Value.ToLower()).ToArray();
            }
        }

    }
}
