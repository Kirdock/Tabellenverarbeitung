using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    public enum State { DeleteRow, InsertRow, DeleteColumn, InsertColumn, ValueChange, NewTable, HeaderChange, CellValueChange, ColumnValuesChange, AddColumnsAndRows, DeleteColumnsAndRows, HeadersChange }
    public enum ProcedureState { Trim, Merge, User, Duplicate }
    public enum FormulaState { Merge, Procedure, Export}
    public enum ImportState { Append, Merge, Header, None}
    class HistoryStates
    {
    }
}
