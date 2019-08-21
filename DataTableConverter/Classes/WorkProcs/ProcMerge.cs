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
        internal enum ConditionColumn : int { Spalte = 0, Wert = 1, NichtLeer = 2, Format = 3 };
        public DataTable Conditions;
        public MergeFormat Format;

        internal ProcMerge(int ordinal, int id, string name) : base(ordinal, id, name) {
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

            return re.Matches(formula).Cast<Match>().Select(col => col.Groups[1].Value);
        }

        public override void renameHeaders(string oldName, string newName)
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
            row[index] = row[index].ToString().Replace($"[{oldName}]", $"[{newName}]");
        }

        //we only remove Conditions
        //for Format: a non existing column is seen as an empty column
        public override void removeHeader(string colName)
        {
            Conditions = Conditions.AsEnumerable().Where(condition => condition[(int)ConditionColumn.Spalte].ToString() != colName).ToTable(Conditions);
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
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
                    DataRow match = Conditions.AsEnumerable().FirstOrDefault(condition =>
                    {
                        string value = condition[(int)ConditionColumn.Spalte]?.ToString();
                        string rowValue;
                        return !string.IsNullOrWhiteSpace(value)
                            &&
                            (
                                (rowValue = row[value].ToString()) == condition[(int)ConditionColumn.Wert].ToString()
                                ||
                                (
                                    (bool)condition[(int)ConditionColumn.NichtLeer]
                                    &&
                                    rowValue.Length > 0
                                )
                            );
                    });
                    MergeFormat format = match == null ? Format : match[(int)ConditionColumn.Format] as MergeFormat;
                    
                    row[column] = GetFormat(row, format, table.Columns);
                }
            }
            else
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, $"{ClassName}: Der Name der neu anzulegenden Spalte darf nicht leer sein!");
            }
        }

        private string GetFormat(DataRow sourceRow, MergeFormat format, DataColumnCollection tableColumns)
        {
            StringBuilder result = new StringBuilder();
            IEnumerable<string> formatHeaders = format.GetHeaders();
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            foreach(string header in formatHeaders)
            {
                dict.Add(header, tableColumns.Contains(header));
            }

            foreach(DataRow row in format.Table.AsEnumerable())
            {
                string column = row[(int)MergeFormat.MergeColumns.Column]?.ToString();
                bool columnIsEmpty = string.IsNullOrWhiteSpace(column);
                if (columnIsEmpty || dict[column])
                {
                    //could contain columns that are not in the table
                    IEnumerable<string> emptyHeaderOfRow = GetHeaderOfFormula(row[(int)MergeFormat.MergeColumns.Empty]?.ToString()).Where(header => dict[header]);
                    bool emptyFullFilled = emptyHeaderOfRow.All(header => string.IsNullOrWhiteSpace(sourceRow[header]?.ToString()));

                    if (emptyFullFilled)
                    {
                        IEnumerable<string> notEmptyHeaderOfRow = GetHeaderOfFormula(row[(int)MergeFormat.MergeColumns.NotEmpty]?.ToString()).Where(header => dict[header]);
                        bool notEmptyFullFilled = notEmptyHeaderOfRow.All(header => !string.IsNullOrWhiteSpace(sourceRow[header]?.ToString()));
                        if (notEmptyFullFilled)
                        {
                            if (!columnIsEmpty)
                            {
                                string value = sourceRow[column]?.ToString();
                                result.Append(value);
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

    }
}
