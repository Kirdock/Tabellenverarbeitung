﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    public enum State { DeleteRow, InsertRow, DeleteColumn, InsertColumn, ValueChange, NewTable, HeaderChange, CellValueChange, AddColumnsAndRows, DeleteColumnsAndRows, HeadersChange }
    public enum FormulaState { Procedure, Export}
    public enum ImportState { Append, Merge, Header, None}
    public enum OrderType { Reverse, Normal, Windows}
    class HistoryStates
    {
    }
}
