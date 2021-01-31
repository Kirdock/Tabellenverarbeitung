namespace DataTableConverter.Classes
{
    public enum State { DeleteRow, InsertRow, DeleteColumn, InsertColumn, ValueChange, NewTable, HeaderChange, CellValueChange, AddColumnsAndRows, DeleteColumnsAndRows, HeadersChange, OrderIndexChange }
    public enum FormulaState { Procedure, Export }
    public enum ImportState { Append, Merge, Header, None }
    public enum OrderType { Reverse, Normal, Windows }
    class HistoryStates
    {
    }
}
