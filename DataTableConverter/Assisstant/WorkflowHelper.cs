using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class WorkflowHelper
    {

        internal static string GetColumnsAsObjectArray(DataRow row, string[] columns, int[] subStringBegin, int[] subStringEnd, List<Tolerance> tolerances)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                #region Set Tolerances
                StringBuilder result = new StringBuilder(row[columns[i]].ToString().ToLower());
                if (tolerances != null)
                {
                    foreach (Tolerance tol in tolerances)
                    {
                        List<string> array = new List<string>(tol.getColumnsAsArrayToLower()) { tol.Name }.Distinct().ToList();
                        string replaceWith = array.Contains(string.Empty) ? string.Empty : tol.Name;
                        array.Remove(string.Empty);

                        foreach (string t in array)
                        {
                            result.Replace(t, replaceWith);
                        }
                    }
                }
                #endregion

                string resultString = result.ToString();

                #region Set Substring
                if (subStringBegin != null)
                {
                    int begin = subStringBegin[i];
                    int end = subStringEnd[i];
                    if (begin != 0 && end != 0 && end >= begin)
                    {
                        if (begin - 1 > resultString.Length)
                        {
                            resultString = string.Empty;
                        }
                        else
                        {
                            int count = end - begin + 1;
                            if (begin + count > resultString.Length)
                            {
                                count = resultString.Length - begin + 1;
                            }
                            resultString = resultString.Substring(begin - 1, count);
                        }
                    }
                }
                #endregion

                res.Append("|").Append(resultString);
            }
            return res.ToString();
        }

        internal static void CheckHeaders(List<string> tableHeader, List<string> notFoundColumns, string[] headers)
        {
            notFoundColumns.AddRange(headers.Where(header => !tableHeader.Contains(header, System.StringComparer.OrdinalIgnoreCase)));
        }

        internal static string[] RemoveEmptyHeaders(IEnumerable<string> headers)
        {
            return headers.Where(header => !string.IsNullOrWhiteSpace(header)).ToArray();
        }

        internal static WorkProc CreateWorkProc(int type, int id, int ordinal, string name)
        {
            WorkProc newProc;
            switch (type)
            {
                //System-Proc
                case 1:
                    switch (id)
                    {
                        case 2:
                            newProc = new ProcMerge(ordinal, id, name);
                            break;

                        case 3:
                            newProc = new ProcOrder(ordinal, id, name);
                            break;

                        case 4:
                            newProc = new ProcUpLowCase(ordinal, id, name);
                            break;

                        case 5:
                            newProc = new ProcRound(ordinal, id, name);
                            break;

                        case 6:
                            newProc = new ProcPadding(ordinal, id, name);
                            break;

                        case 7:
                            newProc = new ProcNumber(ordinal, id, name);
                            break;

                        case 8:
                            newProc = new ProcSubstring(ordinal, id, name);
                            break;

                        case 9:
                            newProc = new ProcReplaceWhole(ordinal, id, name);
                            break;

                        case 10:
                            newProc = new ProcAddTableColumns(ordinal, id, name);
                            break;

                        case 11:
                            newProc = new ProcCompare(ordinal, id, name);
                            break;

                        case 12:
                            newProc = new ProcPVMExport(ordinal, id, name);
                            break;

                        case 13:
                            newProc = new ProcCount(ordinal, id, name);
                            break;

                        case 14:
                            newProc = new ProcSeparate(ordinal, id, name);
                            break;

                        case 15:
                            newProc = new ProcSearch(ordinal, id, name);
                            break;

                        case 16:
                            newProc = new ProcSplit(ordinal, id, name);
                            break;

                        case 17:
                            newProc = new ProcUser(ordinal, id, name)
                            {
                                IsSystem = true,
                                Procedure = new Proc()
                            };
                            break;

                        case 18:
                            newProc = new ProcMergeRows(ordinal, id, name);
                            break;

                        case 1:
                        default:
                            newProc = new ProcTrim(ordinal, id, name);
                            break;
                    }
                    break;

                //Duplicate
                case 2:
                    newProc = new ProcDuplicate(ordinal, id, name);
                    break;

                //User-Proc
                default:
                    newProc = new ProcUser(ordinal, id, name);
                    break;
            }
            return newProc;
        }

        internal static void RemoveRowThroughCaseChange(List<Work> workflows, List<int> rowIndizes, int casId)
        {
            foreach(Work work in workflows)
            {
                work.Procedures.Where(proc => proc.ProcedureId == casId && proc.GetType() == typeof(ProcDuplicate)).ToList().ForEach((caseProc) =>
                {
                    List<string> columns = caseProc.DuplicateColumns.ToList();
                    foreach(int index in rowIndizes.OrderByDescending(x => x))
                    {
                        columns.RemoveAt(index);
                    }
                    caseProc.DuplicateColumns = columns.ToArray();
                });
            }
        }

        internal static void InsertRowThroughCaseChange(List<Work> workflows, int rowIndex, int casId)
        {
            foreach (Work work in workflows)
            {
                work.Procedures.Where(proc => proc.ProcedureId == casId).ToList().ForEach((caseProc) =>
                {
                    List<string> columns = caseProc.DuplicateColumns.ToList();
                    columns.Insert(rowIndex, string.Empty);
                    caseProc.DuplicateColumns = columns.ToArray();
                });
            }
        }

        internal static void AdjustDuplicateColumns(Case selectedItem, List<Work> workflows)
        {
            foreach (Work work in workflows)
            {
                work.Procedures.Where(proc => proc.ProcedureId == selectedItem.Id).ToList().ForEach((caseProc) =>
                {
                    if(selectedItem.Columns.Rows.Count > caseProc.DuplicateColumns.Length)
                    {
                        List<string> temp = caseProc.DuplicateColumns.ToList();
                        temp.Add(string.Empty);
                        caseProc.DuplicateColumns = temp.ToArray();
                    }
                });
            }
        }

        internal static List<WorkProc> CopyProcedures(List<WorkProc> proc)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, proc);

                memoryStream.Position = 0;

                return (List<WorkProc>)formatter.Deserialize(memoryStream);
            }
        }
    }
}
