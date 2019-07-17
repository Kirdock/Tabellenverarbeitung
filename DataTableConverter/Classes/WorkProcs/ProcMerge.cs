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
        internal DataTable Conditions;
        internal MergeFormat Format;

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
                    MergeFormat format = match == null ? Format : match[(int)ConditionColumn.Format] as MergeFormat;
                    
                    row[column] = GetFormat(row, format, table.Columns);
                }
            }
            else
            {
                MessageHandler.MessagesOK(System.Windows.Forms.MessageBoxIcon.Warning, $"{ClassName}: Der Name der neu anzulegenden Spalte darf nicht leer sein!");
            }
        }

        private string GetFormat(DataRow row, MergeFormat format, DataColumnCollection tableColumns)
        {
            return string.Empty;
        }

    }
}
