﻿using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class WorkflowHelper
    {

        internal static string getColumnsAsObjectArray(DataRow row, string[] columns, int[] subStringBegin, int[] subStringEnd, List<Tolerance> tolerances)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                #region Set Tolerances
                StringBuilder result = new StringBuilder(row[columns[i]].ToString().ToLower());
                foreach (Tolerance tol in tolerances)
                {
                    foreach (string t in tol.getColumnsAsArrayToLower())
                    {
                        result.Replace(t, tol.Name);
                    }
                }
                #endregion

                string resultString = result.ToString();

                #region Set Substring
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
                #endregion

                res.Append("|").Append(resultString);
            }
            return res.ToString();
        }

        internal static void checkHeaders(List<string> tableHeader, List<string> notFoundColumns, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                string headerText = headers[i];
                if (!tableHeader.Contains(headerText.ToLower()))
                {
                    notFoundColumns.Add(headerText);
                }
            }
        }

        internal static string[] removeEmptyHeaders(string[] headers)
        {
            return headers.Where(header => !string.IsNullOrWhiteSpace(header)).ToArray();
        }

        internal static WorkProc createWorkProc(int type, int id, int ordinal, string name)
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

        internal static void removeRowThroughCaseChange(List<Work> workflows, List<int> rowIndizes, int casId)
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

        internal static void insertRowThroughCaseChange(List<Work> workflows, int rowIndex, int casId)
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

        internal static void adjustDuplicateColumns(Case selectedItem, List<Work> workflows)
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
    }
}