using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcSearch : WorkProc
    {
        internal static readonly string ClassName = "Suchen";
        public string SearchText;
        public string Header;
        public int From, To;
        public bool TotalSearch;
        public string Shortcut;

        public ProcSearch(int ordinal, int id, string name) : base(ordinal, id, name){}

        public ProcSearch(string searchText, string header, int from, int to, string newColumn, bool totalSearch)
        {
            SearchText = searchText;
            Header = header;
            From = from;
            To = to;
            NewColumn = newColumn;
            TotalSearch = totalSearch;
        }

        public ProcSearch(string searchText, string header, string newColumn, bool totalSearch, string shortcut)
        {
            SearchText = searchText;
            Header = header;
            TotalSearch = totalSearch;
            Shortcut = shortcut;
            NewColumn = newColumn;
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            int index = table.Columns.IndexOf(Header);
            if (index != -1 && From <= To )
            {
                if (!string.IsNullOrWhiteSpace(NewColumn))
                {
                    string col = table.Columns.Contains(NewColumn) ? NewColumn : table.TryAddColumn(NewColumn);
                    
                    Func<string, string, bool> search;
                    if (TotalSearch)
                    {
                        search = SearchTotal;
                    }
                    else
                    {
                        search = PartialSearch;
                    }
                    if (string.IsNullOrWhiteSpace(Shortcut))
                    {
                        int counter = From;
                        bool found = false;
                        foreach (DataRow row in table.GetSortedTable(sortingOrder, orderType))
                        {
                            if (found)
                            {
                                if (counter > To)
                                {
                                    break;
                                }
                                else
                                {
                                    row[col] = counter;
                                    counter++;
                                }
                            }
                            else if (search.Invoke(row[index].ToString(), SearchText))
                            {
                                row[col] = counter;
                                found = true;
                                counter++;
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRow row in table.GetSortedTable(sortingOrder, orderType))
                        {
                            if (search.Invoke(row[index].ToString(), SearchText))
                            {
                                row[col] = Shortcut;
                            }
                        }
                    }
                }
            }
        }

        private bool SearchTotal(string value, string searchText)
        {
            return value == searchText;
        }

        private bool PartialSearch(string value, string searchText)
        {
            return value.Contains(searchText);
        }

        public override string[] GetHeaders()
        {
            return new string[] { Header };
        }

        public override void RemoveHeader(string colName)
        {
            if(colName == Header)
            {
                Header = string.Empty;
            }
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            if(oldName == Header)
            {
                Header = newName;
            }
        }
    }
}
