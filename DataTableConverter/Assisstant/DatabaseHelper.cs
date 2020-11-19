using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataTableConverter.Assisstant.SQL_Functions;
using DataTableConverter.Classes;
using DataTableConverter.Extensions;

namespace DataTableConverter.Assisstant
{
    class DatabaseHelper
    {
        private readonly string DatabaseDirectory = "";
        private readonly string TempDatabasePath;
        private readonly string DatabasePath;
        private readonly string FileNameColumn = "dateiname";
        private readonly string DefaultTable = "main";
        private readonly string MetaTableAffix = "_meta";
        internal readonly string IdColumnName = "rowid"; //could be a problem as primary key if VACUUM command is executed
        private string SortOrderColumnName = "__SORT_ORDER__";
        private readonly string IndexAffix = "_INDEX";
        private int SavePoints, Pointer; //0 => after main Table with data is created
        private SQLiteConnection Connection, TempConnection; //Second file and connection is needed because of Trace
        private SQLiteTraceEventHandler UpdateHandler = null;
        private SQLiteTransaction Transaction, TempTransaction;
        internal ExportHelper ExportHelper;
        private readonly DatabaseHistory DatabaseHistory;

        private readonly HashSet<string> IgnoreCommands = new HashSet<string>()
        {
            "SELECT",
            "BEGIN",
            "ROLLBACK",
            "SAVEPOINT",
            "DROP",
            "CREATE"
        };
        //Autoincrement only works on primary keys. BUT primary keys can be updated
        //Warning: Selection of "rowid" is equal to selecting primary key
        //Problem with DataTables to edit data: if there is a change in a row, the update statement contains all columns of the row, not only the changed ones

        internal DatabaseHelper(string databaseName = null)
        {
            bool createMainDatabase = databaseName == null;
            databaseName = (databaseName ?? "Database");

            DatabaseHistory = new DatabaseHistory(this, DatabaseDirectory, databaseName);

            DatabasePath = Path.Combine(DatabaseDirectory, databaseName + ".sqlite");
            TempDatabasePath = Path.Combine(DatabaseDirectory, databaseName + "_temp.sqlite");
            Init(createMainDatabase);
        }

        private void Init(bool createMainDatabase)
        {
            Reset();
            SortOrderColumnName = IdColumnName == "rowid" ? SortOrderColumnName : IdColumnName;
            SQLiteFunction.RegisterFunction(typeof(SQLiteComparator)); //COLLATE NATURALSORT
            SQLiteFunction.RegisterFunction(typeof(ParseDecimal)); //PARSEDECIMAL(myValue)
            SQLiteFunction.RegisterFunction(typeof(NumberToString)); //TOSTRING(myValue, myFormat)

            if (createMainDatabase)
            {
                CreateDatabase(DatabasePath);
            }
            CreateDatabase(TempDatabasePath);
            ConnectMain();
            ConnectTemp();
        }

        internal void CreateDatabase(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            SQLiteConnection.CreateFile(path);
            File.SetAttributes(path, FileAttributes.Hidden);            
        }

        private void ConnectMain()
        {
            Connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;");
            Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        private void ConnectTemp()
        {
            TempConnection = new SQLiteConnection($"Data Source={TempDatabasePath};Version=3;");
            TempConnection.Open();
            TempTransaction = TempConnection.BeginTransaction();
        }

        internal int CompareColumnsCount(string columnName1, string columnName2, string tableName = "main")
        {
            int count = 0;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM [{tableName}] where [{columnName1}] = [{columnName2}]";
                count = Convert.ToInt32(command.ExecuteScalar());
            }
            return count;
        }

        internal void Close()
        {
            //Transaction.Commit();
            Transaction.Dispose();
            Connection.Close();
            DatabaseHistory.Close();
            DeleteDatabases();
        }

        private void DeleteDatabases()
        {
            DeleteMainDatabase();
            DeleteTempDatabase();
        }

        /// <summary>
        /// Get alias of meta-table ordered by sortorder
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal List<string> GetSortedColumnsAsAlias(string tableName = "main")
        {
            List<string> aliases = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias from [{tableName + MetaTableAffix}] order by sortorder asc";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        aliases.Add(reader.GetString(0));
                    }
                }
            }
            return aliases;
        }

        internal void CreateTable(IEnumerable<string> columnsNames)
        {
            string tableName = "main";
            CreateTable(columnsNames, tableName, Connection);
        }

        internal void CreateTable(IEnumerable<string> columnNames, string tableName)
        {
            CreateTable(columnNames, tableName, TempConnection);
        }

        private void CreateTable(IEnumerable<string> columnNames, string tableName, SQLiteConnection connection)
        {

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"DROP table if exists [{tableName}]";
                command.ExecuteNonQuery();

                string colType = "varchar(255) not null default '' COLLATE NATURALSORT";
                command.CommandText = $"CREATE table [{tableName}] ({SortOrderColumnName} INTEGER PRIMARY KEY AUTOINCREMENT, [{string.Join($"] {colType},[", columnNames)}] {colType})";
                
                command.ExecuteNonQuery();
            };
            

            CreateMetaData(tableName, columnNames);
        }

        private void CreateMetaData(string tableName, IEnumerable<string> columnNames, SQLiteConnection connection = null)
        {
            using(SQLiteCommand command = (connection ?? GetConnection(tableName)).CreateCommand())
            {
                command.CommandText = $"DROP table if exists [{tableName + MetaTableAffix}]";
                command.ExecuteNonQuery();

                command.CommandText = $"CREATE table [{tableName + MetaTableAffix}] (column varchar(255) not null default '', alias varchar(255), sortorder INTEGER primary key AUTOINCREMENT)";
                command.ExecuteNonQuery();

                command.CommandText = $"INSERT INTO [{tableName + MetaTableAffix}] (column, alias) values ($column, $alias)";
                command.Parameters.Add(new SQLiteParameter("$column"));
                command.Parameters.Add(new SQLiteParameter("$alias"));

                foreach (string columnName in columnNames)
                {
                    command.Parameters[0].Value = command.Parameters[1].Value = columnName;
                    command.ExecuteNonQuery();
                }
            }
        }

        internal void ReplaceTable(string newTable, string oldTable = "main")
        {
            DeleteMainDatabase();
            Reset();
            if (UpdateHandler != null)
            {
                Connection.Trace -= UpdateHandler;
            }
            
            RenameTable(newTable, oldTable);
            CommitTemp();

            RenameTempDatabase();

            ConnectMain();
            
            CreateDatabase(TempDatabasePath);
            UpdateHandler = Update;
            Connection.Trace += UpdateHandler;

            DatabaseHistory.Close();
            DatabaseHistory.CreateDatabase();
            ConnectTemp();
        }

        private void CommitTemp()
        {
            TempTransaction.Commit();
        }

        private void Reset()
        {
            SavePoints = Pointer = -1;
        }

        private void DeleteMainDatabase()
        {
            Connection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(DatabasePath);
        }

        private void DeleteTempDatabase()
        {
            TempConnection.Close();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            File.Delete(TempDatabasePath);
        }

        private void RenameTempDatabase()
        {
            TempConnection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            File.Move(TempDatabasePath, DatabasePath);
        }

        internal void RenameTable(string from, string to)
        {
            using (SQLiteCommand command = GetConnection(from).CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{from}] rename to [{to}]";
                command.ExecuteNonQuery();

                command.CommandText = $"ALTER TABLE [{from + MetaTableAffix}] rename to [{to + MetaTableAffix}]";
                command.ExecuteNonQuery();
            }
        }

        internal string InsertRow(Dictionary<string, string> row = null, string tableName = "main")
        {
            string id = string.Empty;
            
            SQLiteConnection connection = GetConnection(tableName);
            using (SQLiteCommand command = connection.CreateCommand())
            {
                if (row == null || row.Keys.Count == 0)
                {
                    command.CommandText = $"INSERT into [{tableName}] DEFAULT VALUES";
                }
                else
                {
                    command.CommandText = $"INSERT into [{tableName}] ({GetHeaderString(row.Keys)}) values ({GetValueString(row.Keys.Count)})";
                    foreach (string header in row.Keys)
                    {
                        command.Parameters.Add(new SQLiteParameter() { Value = row[header] });
                    }
                }
                command.ExecuteNonQuery();
                id = connection.LastInsertRowId.ToString();
            }
            return id;
        }

        /// <summary>
        /// Insert empty row before given id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal void InsertRowAt(string id, string tableName = "main")
        {
            //from id till end, increase id by 1
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] SET [{IdColumnName}] = - ([{IdColumnName}] + 1) WHERE [{IdColumnName}] >= ?;" //everything greater and equal than the given id is increased by one (it is set to negative to prevent duplicate IDs)
                                    + $" UPDATE [{tableName}] SET [{IdColumnName}] = - [{IdColumnName}] WHERE [{IdColumnName}] < 0"; //After everything is set set negative to positive
                command.Parameters.Add(new SQLiteParameter() { Value = id });

                command.ExecuteNonQuery();
            }
            InsertRow(new Dictionary<string, string>() { { IdColumnName, id } }, tableName);
        }

        internal SQLiteCommand InsertRow(IEnumerable<string> eHeaders, object[] values, string tableName, SQLiteCommand cmd = null)
        {
            string[] headers = eHeaders.ToArray();
            string headerString = GetHeaderString(headers);
            
            SQLiteCommand command = cmd;
            if (cmd == null)
            {
                command = GetConnection(tableName).CreateCommand();
                command.CommandText = $"INSERT into [{tableName}] ({headerString}) values ({GetValueString(headers.Length)})";

                for (int i = 0; i < values.Length; i++)
                {
                    command.Parameters.Add(new SQLiteParameter() { Value = values[i].ToString() });
                }
                command.ExecuteNonQuery();
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    command.Parameters[i].Value = values[i].ToString();
                }
                command.ExecuteNonQuery();
            }
            return cmd;
        }

        internal bool ContainsAlias(string tableName, string column)
        {
            bool status = false;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM [{tableName + MetaTableAffix}] WHERE alias = $column COLLATE NOCASE";
                command.Parameters.Add(new SQLiteParameter("$column", column));
                int count = Convert.ToInt32(command.ExecuteScalar());
                status = count != 0;
            }
            return status;
        }

        private SQLiteConnection GetConnection(string tableName)
        {
            return tableName == DefaultTable ? Connection : TempConnection;
        }

        internal void AddColumn(string tableName, string column, string defaultValue = "")
        {
            AddColumnWithAlias(column, column, tableName, defaultValue);
        }

        internal string RenameAlias(string from, string to, string tableName = "main")
        {
            string newAlias = to;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                int counter = 1;
                while(ContainsAlias(tableName, newAlias))
                {
                    newAlias = to + counter;
                    counter++;
                }
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] set alias = ? where alias = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = to });
                command.Parameters.Add(new SQLiteParameter() { Value = from });
                command.ExecuteNonQuery();
            }
            return newAlias;
        }

        //it is not possible to remove a column from a table
        //but with my structure we just need to delete the entry in the meta-table.
        //Then future SELECT statemetns don't contain the column
        internal void DeleteColumnThroughAlias(string alias, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET alias = null where alias = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = alias });
                command.ExecuteNonQuery();
            }
        }

        private void AddColumnWithAlias(string columnName, string alias, string tableName, string defaultValue)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{tableName}] ADD COLUMN [{columnName}] varchar(255) NOT NULL DEFAULT [{defaultValue}]";
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.CommandText = $"INSERT INTO [{tableName + MetaTableAffix}] (column, alias) values ($column, $alias)";
                command.Parameters.Add(new SQLiteParameter("$column", columnName));
                command.Parameters.Add(new SQLiteParameter("$alias", alias));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds all columnNames to a given table. If the columnName already exists, an additional column will be created including a counter after the name
        /// Example: Name1, Name2, ...
        /// </summary>
        /// <param name="columnsNames"></param>
        /// <param name="defaultValue"></param>
        /// <param name="tableName"></param>
        /// <param name="destinationTableColumnAliasMapping"></param>
        internal void AddColumnsWithAdditionalIfExists(IEnumerable<string> columnsNames, string defaultValue, out string[] newColumnNames, string tableName = "main")
        {
            Dictionary<string,string> destinationTableColumnAliasMapping = GetAliasColumnMapping(tableName);
            newColumnNames = new string[columnsNames.Count()];
            int index = 0;
            foreach (string columnName in columnsNames)
            {
                newColumnNames[index] = AddColumnWithAdditionalIfExists(columnName, defaultValue, tableName, destinationTableColumnAliasMapping);
                ++index;
            }
        }

        internal string AddColumnWithAdditionalIfExists(string columnName, string defaultValue = "", string tableName = "main", Dictionary<string,string> destinationTableColumnAliasMapping = null)
        {
            destinationTableColumnAliasMapping = destinationTableColumnAliasMapping  ?? GetAliasColumnMapping(tableName);
            string newAlias = columnName;
            int counter;

            for (counter = 1; destinationTableColumnAliasMapping.Keys.Any(alias => alias.Equals(newAlias, StringComparison.OrdinalIgnoreCase)); counter++)
            {
                newAlias = columnName + counter;
            }
            return AddColumnFixedAlias(newAlias, tableName, defaultValue, destinationTableColumnAliasMapping.Values);
        }

        /// <summary>
        /// Add column to table. If column name is already taken, a counter is increased but alias is not changed
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="tableName"></param>
        /// <param name="defaultValue"></param>
        internal string AddColumnFixedAlias(string alias, string tableName = "main", string defaultValue = "", IEnumerable<string> columnNames = null)
        {
            columnNames = columnNames ?? GetColumnNames(tableName, true);
            string columnName = alias;
            for(int counter = 1;  columnNames.Any( col => col.Equals(columnName,StringComparison.OrdinalIgnoreCase)); ++counter)
            {
                columnName = alias + counter;
            }
            AddColumnWithAlias(columnName, alias, tableName, defaultValue);
            return columnName;
        }

        /// <summary>
        /// Creates an index on a given column in a given table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="invokeForm"></param>
        /// <param name="unique"></param>
        /// <returns>abort-status</returns>
        private bool CreateIndexOn(string tableName, string columnName, Form1 invokeForm = null, bool unique = true)
        {
            bool abort = false;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                try
                {
                    command.CommandText = $"CREATE {(unique ? "UNIQUE" : string.Empty)} index if not exists $index on [{tableName}]($colName)";
                    command.Parameters.Add(new SQLiteParameter("$colName", columnName));
                    command.Parameters.Add(new SQLiteParameter("$index", columnName + IndexAffix + tableName)); //tableName because drop index does not specify a table
                    command.ExecuteNonQuery();
                }
                catch
                {
                    DialogResult result = MessageHandler.MessagesYesNo(invokeForm, MessageBoxIcon.Warning, "Die angegebene identifizierende Spalte der geladenen Tabelle enthält Duplikate.\nTrotzdem fortfahren?");
                    abort = result == DialogResult.Yes;
                }
            }
            return abort;
        }

        private void DeleteIndexOn(string tableName, string columnName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"DROP Index if exists ?";
                command.Parameters.Add(new SQLiteParameter() { Value = columnName + IndexAffix + tableName });
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Add columns of import table depending on a given identifier on destination and import table
        /// </summary>
        /// <param name="importTable"></param>
        /// <param name="importColumnNames"></param>
        /// <param name="destinationIdentifierColumn"></param>
        /// <param name="importIdentifierColumn"></param>
        /// <param name="destinationTableColumnAliasMapping"></param>
        /// <param name="importTableColumnAliasMapping"></param>
        /// <param name="destinationTable"></param>
        internal bool PVMImport(string importTable, string[] importColumnNames, string destinationIdentifierColumn, string importIdentifierColumn, Form1 invokeForm = null, string destinationTable = "main")
        {
            bool abort;
            if(abort = CreateIndexOn(destinationTable, destinationIdentifierColumn, invokeForm))
            {
                AddColumnsWithAdditionalIfExists(importColumnNames.Where(col => col != importIdentifierColumn), string.Empty, out string[] destinationColumnNames, destinationTable);

                //int rowCount = GetRowCount(importTable);
                int sortOrder = 0;
                using (SQLiteCommand destinationCommand = GetConnection(destinationTable).CreateCommand())
                {
                    string headerString = GetHeaderString(destinationColumnNames);
                    destinationCommand.CommandText = $"INSERT into [{destinationTable}] ({SortOrderColumnName},{headerString}) values ({GetValueString(destinationColumnNames.Length +1 )})"; //+1 because of sortOrder

                    for (int i = 0; i < destinationColumnNames.Length + 1; i++) //+1 because of sortOrder
                    {
                        destinationCommand.Parameters.Add(new SQLiteParameter());
                    }

                    using (SQLiteCommand command = GetConnection(importTable).CreateCommand())
                    {
                        command.CommandText = $"SELECT {GetHeaderString(importColumnNames)} from [{importTable}] LIMIT {Properties.Settings.Default.MaxRows} OFFSET ?";
                        command.Parameters.Add(new SQLiteParameter());
                        bool hasRows = true;
                        int offset = 0;
                        while (hasRows)
                        {
                            command.Parameters[0].Value = offset;
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {

                                int rowCount = 0;
                                while (reader.Read())
                                {
                                    destinationCommand.Parameters[0].Value = sortOrder;
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        destinationCommand.Parameters[i + 1].Value = reader.GetString(i);
                                    }
                                    destinationCommand.ExecuteNonQuery();
                                    sortOrder++;

                                    ++rowCount;
                                }

                                hasRows = hasRows || rowCount < Properties.Settings.Default.MaxRows;
                                offset += (int)Properties.Settings.Default.MaxRows;
                            }
                        }
                    }
                }
            }
            Delete(importTable);
            //remove index on destinationTable? (destinationColumn + IndexAffix)
            return abort;
        }

        private string GetValueString(int count)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < count - 1; ++i)
            {
                builder.Append("?,");
            }
            return builder.Append('?').ToString();
        }

        private string GetValueString(IEnumerable<string> headers)
        {
            return "$" + string.Join(",$", headers);
        }

        private string GetHeaderString(IEnumerable<string> headers)
        {

            return "[" + string.Join("],[", headers) + "]";
        }

        internal DataTable GetData(string tableName, int offset = 0)
        {
            DataTable dt = new DataTable();
            string headerString = string.Join(",", GetSortedHeadersIncludeAsAlias(tableName));

            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT {headerString} FROM [{tableName}] LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}";
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(command);
                sqlda.Fill(dt);
            }

            return dt;
        }

        internal DataTable GetData(string order, OrderType orderType, int offset, string tableName = "main")
        {
            DataTable dt = new DataTable();
            string headerString = string.Join(",", GetSortedHeadersIncludeAsAlias(tableName));
            //select explicit instead of everything because of column order
            //use alias from metaTable
            //heading1 as Surname, ...

            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSortedSelectString(headerString, order, orderType, (int)Properties.Settings.Default.MaxRows, offset, true, tableName);

                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(command);
                sqlda.Fill(dt);
                if (orderType == OrderType.Reverse)
                {
                    dt.Columns.Remove("rnumber");
                }
            }

            return dt;
        }

        private string GetSortedSelectString(string headerString, string order, OrderType orderType, int limit, int offset, bool includeId, string tableName, string whereStatement = "")
        {
            string selectString = "SELECT ";
            if (includeId)
            {
                selectString += $"{IdColumnName} AS {IdColumnName},";
            }

            if (orderType == OrderType.Reverse)
            {
                int half = GetRowCount(tableName) / 2;
                selectString += $"{headerString}, ROW_NUMBER() OVER(ORDER BY {order}) as rnumber from [{tableName}] {whereStatement} ORDER BY case when rnumber > {half}  then(rnumber - ({half}-0.5)) when rnumber <= {half} then rnumber end"; //append ASC or DESC
            }
            else if (orderType == OrderType.Windows && order != string.Empty)
            {
                selectString += $"{headerString} FROM [{tableName}] {whereStatement} ORDER BY {order}";
            }
            else
            {
                selectString += $"{headerString} FROM [{tableName}] {whereStatement} ORDER BY {SortOrderColumnName}";
            }
            if(limit != -1)
            {
                selectString += $" LIMIT {limit} OFFSET {offset}";
            }
            return selectString;
        }

        private List<string> GetSortedHeadersIncludeAsAlias(string tableName = "main")
        {
            List<string> headers = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column, alias from [{tableName + MetaTableAffix}] order by sortorder asc";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        headers.Add($"[{reader.GetString(0)}] AS [{reader.GetString(1)}]");
                    }
                }
            }
            return headers;
        }

        private DataTable GetSortedHeadersAsDataTable(out SQLiteDataAdapter adapter, string tableName = "main")
        {
            DataTable table = new DataTable();
            adapter = null;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT * from [{tableName + MetaTableAffix}] order by sortorder";
                adapter = new SQLiteDataAdapter(command);
                adapter.Fill(table);
                SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
                adapter.UpdateCommand = builder.GetUpdateCommand();
            }
            return table;
        }

        private bool AddColumnIfNotExists(string tableName, string column, string defaultValue = "")
        {
            bool status;
            if(! (status = ContainsAlias(tableName, column)))
            {
                AddColumn(tableName, column, defaultValue);
            }
            return status;
        }

        /// <summary>
        /// Get a Key-Value based Collection of alias and column
        /// Key: alias
        /// Value: columnName
        /// Note: This Dictionary is insertion order except you delete keys, then the "empty" space is filled by a new ".Add"
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal Dictionary<string, string> GetAliasColumnMapping(string tableName = "main")
        {
            Dictionary<string, string> headerMapping = new Dictionary<string, string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias, column from [{tableName + MetaTableAffix}] where alias != null order by sortorder";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        headerMapping.Add(reader.GetString(0), reader.GetString(1));
                    }
                }
            }
            return headerMapping;
        }

        internal void ConcatTable(string newTable, string fileNameBefore, string filename, string destinationTable = "main")
        {
            AddColumnIfNotExists(destinationTable, FileNameColumn, fileNameBefore);
            AddColumnIfNotExists(newTable, FileNameColumn, filename);
            List<string> headers = GetSortedColumnsAsAlias(newTable);
            string newTableHeaderString = GetHeaderString(headers);
            SQLiteConnection destinationConnection = GetConnection(destinationTable);
            
            foreach (string header in headers)
            {
                AddColumnIfNotExists(destinationTable, header);
            }

            Dictionary<string,string> destinationHeaders = GetAliasColumnMapping(destinationTable);
            List<string> headerMapping = new List<string>();

            foreach (string header in headers)
            {
                headerMapping.Add(destinationHeaders[header]);
            }
            //map headers of newTable to original column name of originalTable

            string headerString = GetHeaderString(headerMapping);

            using (SQLiteCommand destinationCommand = destinationConnection.CreateCommand())
            {
                destinationCommand.CommandText = $"INSERT into [{destinationTable}] ({headerString}) values ({GetValueString(headerMapping.Count)})";
                for(int i = 0; i < headerMapping.Count; ++i)
                {
                    destinationCommand.Parameters.Add(new SQLiteParameter());
                }

                using (SQLiteCommand command = GetConnection(newTable).CreateCommand())
                {
                    int rowCount = GetRowCount(newTable);
                    int offset = 0;
                    while (offset < rowCount)
                    {
                        DataTable table = new DataTable();
                        command.CommandText = $"SELECT {newTableHeaderString} from [{newTable}] LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}";
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(table);

                        foreach (DataRow row in table.AsEnumerable())
                        {
                            for (int i = 0; i < table.Columns.Count; ++i)
                            {
                                destinationCommand.Parameters[i].Value = row[i].ToString();
                            }
                            destinationCommand.ExecuteNonQuery();
                        }

                        offset += (int)Properties.Settings.Default.MaxRows;
                    }
                }
            }

            Delete(newTable);
        }

        internal int GetRowCount(string tableName = "main")
        {
            int rowCount = 0;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand()) {
                command.CommandText = $"SELECT count(*) from [{tableName}]";
                rowCount = Convert.ToInt32(command.ExecuteScalar());
            }
            return rowCount;
        }

        internal void Delete(string tableName, bool ifExists = false)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                //delete table
                command.CommandText = $"DROP TABLE {(ifExists ? "if exists" : string.Empty)} [{tableName}]";
                command.ExecuteNonQuery();

                //delete related meta-table
                command.CommandText = $"DROP TABLE {(ifExists ? "if exists" : string.Empty)} [{tableName + MetaTableAffix}]";
                command.ExecuteNonQuery();
            }
        }

        internal void RenameColumns(string importTable, string destinationTable = "main")
        {
            List<string> headers = GetSortedColumnsAsAlias(importTable);
            DataTable table = GetSortedHeadersAsDataTable(out SQLiteDataAdapter adapter, destinationTable);
            for (int i = 0; i < table.Rows.Count && i < headers.Count; ++i)
            {
                table.Rows[i]["alias"] = headers[i];
            }
            adapter?.Update(table);
        }

        internal void SetSavepoint()
        {
            if(UpdateHandler == null)
            {
                UpdateHandler = Update;
                Connection.Trace += UpdateHandler;
            }
            CreateSavePoint();
        }

        private void CreateSavePoint()
        {
            ++SavePoints;
            ++Pointer;
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"SAVEPOINT \"{SavePoints}\"";
                command.ExecuteNonQuery();
            }
            DatabaseHistory.CreateSavePoint(SavePoints);
        }

        private void Update(object sender, TraceEventArgs e)
        {
            if (!IgnoreCommands.Contains(GetCommandType(e.Statement)))
            {
                DatabaseHistory.Log(Pointer, ref SavePoints, e.Statement);
            }
        }

        private string GetCommandType(string command)
        {
            int index = command.IndexOf(" ");
            return command.Substring(0, index == -1 ? command.Length -1 : index).ToUpper();
        }

        internal int PVMSplit(string sourceFilePath, Form1 invokeForm, int encoding, string invalidColumnName, string tableName = "main")
        {
            string directory = Path.GetDirectoryName(sourceFilePath);
            string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            
            SplitAndSavePVM(tableName, invalidColumnName, directory, fileName + Properties.Settings.Default.FailAddressText, encoding, invokeForm, false);
            return SplitAndSavePVM(tableName, invalidColumnName, directory, fileName + Properties.Settings.Default.RightAddressText, encoding, invokeForm, true); //return count of rows
        }

        private int SplitAndSavePVM(string tableName, string invalidColumnName, string directory, string fileName, int encoding, Form1 invokeForm, bool saveValidRows)
        {
            int rowCount = 0;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                Dictionary<string,string> columnAliasMapping = GetAliasColumnMapping(tableName);
                command.CommandText = $"SELECT {GetHeaderString(columnAliasMapping.Values)} from [{tableName}] where $column {(saveValidRows ? "!=" : "=")} $value limit {Properties.Settings.Default.MaxRows} offset $offset";
                command.Parameters.Add(new SQLiteParameter("$offset"));
                command.Parameters.Add(new SQLiteParameter("$column", invalidColumnName));
                command.Parameters.Add(new SQLiteParameter("$value", Properties.Settings.Default.FailAddressValue));

                rowCount = ExportHelper.Save(directory, fileName, null, encoding, Properties.Settings.Default.PVMSaveFormat, invokeForm, command, null, tableName);
            }
            return rowCount;
        }

        internal SQLiteCommand GetDataCommand(string tableName, string orderAlias = null)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            Dictionary<string, string> columnAliasMapping = GetAliasColumnMapping(tableName);
            command.CommandText = "SELECT ";
            if (orderAlias != null)
            {
                 command.CommandText += $"rowid AS [{orderAlias}],";
            }

            command.CommandText += $"{GetHeaderString(columnAliasMapping.Values)} from [{tableName}] limit {Properties.Settings.Default.MaxRows} offset $offset";
            command.Parameters.Add(new SQLiteParameter("$offset"));

            return command;
        }

        internal void DeleteInvalidRows(string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string columnName = GetColumnName(Properties.Settings.Default.InvalidColumnName, tableName);
                command.CommandText = $"DELETE FROM [{tableName}] where $column = $value";
                command.Parameters.Add(new SQLiteParameter("$column", columnName));
                command.Parameters.Add(new SQLiteParameter("$alias", Properties.Settings.Default.FailAddressValue));
                command.ExecuteNonQuery();
            }
        }

        internal string GetColumnName(string alias, string tableName)
        {
            string columnName = string.Empty;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] where alias = $alias";
                command.Parameters.Add(new SQLiteParameter("$alias", alias));
                columnName = command.ExecuteScalar()?.ToString();
            }
            return columnName;
        }

        internal List<string> GetColumnNames(string tableName = "main", bool includeDeleted = false)
        {
            List<string> columnNames = new List<string>();
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] {(includeDeleted ? string.Empty : "where alias != null")} order by sortorder";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnNames.Add(reader.GetString(0));
                    }
                }
            }
            return columnNames;
        }

        /// <summary>
        /// Renames the given column to column+affix and creates a new one with alias
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="tableName"></param>
        /// <returns>columnName of the created column</returns>
        internal string CopyColumn(string alias, string tableName)
        {
            //pre condition alias exists
            RenameAlias(alias, alias + Properties.Settings.Default.OldAffix);
            return AddColumnWithAdditionalIfExists(alias, string.Empty, tableName);
        }

        internal bool AddColumnWithDialog(string alias, Form invokeForm, string tableName, out string columnName)
        {
            bool inserted = true;
            columnName = GetColumnName(alias, tableName);
            if (columnName != null)
            {
                inserted = MessageHandler.MessagesYesNo(invokeForm, MessageBoxIcon.Warning, $"Es gibt bereits eine Spalte mit der Bezeichnung \"{alias}\".\nSpalte überschreiben?") == DialogResult.Yes;
                if (inserted) //if yes override everything with empty
                {
                    SetColumnValues(alias, string.Empty, tableName);
                }
            }
            else
            {
                columnName = AddColumnFixedAlias(alias, tableName);
            }
            return inserted;
        }

        internal void InsertDataPerColumnValue(string columnName, OrderType orderType, int limit, string sourceTable, string destinationTable)
        {
            List<string> groupColumn = GroupColumn(columnName, orderType, sourceTable);

            using (SQLiteCommand command = GetConnection(sourceTable).CreateCommand())
            {
                using (SQLiteCommand insertCommand = GetConnection(destinationTable).CreateCommand())
                {
                    List<string> columnNames = GetColumnNames(sourceTable);
                    string headerString = GetHeaderString(columnNames);

                    insertCommand.CommandText = $"INSERT INTO [{destinationTable}] ({headerString}) values ({GetValueString(columnNames.Count)})";
                    for(int i = 0; i < columnNames.Count; ++i)
                    {
                        insertCommand.Parameters.Add(new SQLiteParameter());
                    }
                    command.CommandText = GetSortedSelectString(headerString, $"[{columnName}] asc", orderType, limit, 0, false, sourceTable, $"where [{columnName}] = ?");
                    command.Parameters.Add(new SQLiteParameter());
                    foreach (string column in groupColumn)
                    {
                        command.Parameters[0].Value = column;
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    insertCommand.Parameters[i].Value = reader.GetString(i);
                                }
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
                
        }
        internal List<string> GroupColumn(string columnName, OrderType orderType, string tableName = "main")
        {
            List<string> list = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSortedSelectString($"[{columnName}]", $"[{columnName}] asc", orderType, -1, 0, false, tableName, $"group by [{columnName}]");
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader.GetString(0));
                    }
                }
            }
            return list;
        }

        internal Dictionary<string, int> GroupCountOfColumn(string columnName, string tableName = "main")
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT [{columnName}], count(*) from [{tableName}] group by [{columnName}] order by [{columnName}]";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dict.Add(reader.GetString(0), reader.GetInt32(1));
                    }
                }
            }
            return dict;
        }

        internal void SplitTableOnRowValue(Dictionary<string, string[]> dict, string column, string tableName = "main")
        {
            SQLiteConnection connection = GetConnection(tableName);
            Dictionary<string, string> aliasColumnMapping = GetAliasColumnMapping(tableName);
            string headerString = GetHeaderString(aliasColumnMapping.Keys);
            string valueString = GetValueString(aliasColumnMapping.Count);
            using (SQLiteCommand command = connection.CreateCommand())
            {
                CreateIndexOn(tableName, column, null, false);
                command.CommandText = $"SELECT ({headerString}) from [{tableName}] where [{column}] = ?";
                command.Parameters.Add(new SQLiteParameter());
                foreach (KeyValuePair<string, string[]> pair in dict)
                {
                    command.Parameters[0].Value = pair.Key;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        using (SQLiteCommand insertCommand = TempConnection.CreateCommand())
                        {
                            //0 => tableName
                            //1 => fileName
                            insertCommand.CommandText = $"INSERT INTO [{pair.Value[0]}] ({headerString}) values ({valueString})";
                            for (int i = 0; i < aliasColumnMapping.Count; i++)
                            {
                                insertCommand.Parameters.Add(new SQLiteParameter());
                            }
                            while (reader.Read())
                            {
                                for(int i = 0; i < reader.FieldCount; i++)
                                {
                                    insertCommand.Parameters[i].Value = reader.GetString(i);
                                }
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        internal int[] GetMaxColumnLength(string tableName)
        {
            string[] columnNames = GetColumnNames(tableName).ToArray();
            int[] max = new int[columnNames.Length];
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                for(int i = 0; i < columnNames.Length; i++)
                {
                    command.CommandText = $"SELECT max(length([{columnNames[i]}])) from [{tableName}]";
                    max[i] = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return max;
        }

        internal void DictionaryToDataTable(Dictionary<string, int> dict, string columnName, bool showFromTo, string tableName, int sourceRowCount)
        {
            SQLiteCommand command = null;
            if (showFromTo)
            {
                string[] columns = new string[] { columnName, "Anzahl", "Von", "Bis" };
                CreateTable(columns, tableName);

                int count = 1;
                foreach (KeyValuePair<string, int> item in dict)
                {
                    int newCount = count + item.Value;
                    command = InsertRow(columns, new object[] { item.Key, item.Value.ToString(), count.ToString(), (newCount - 1).ToString() }, tableName, command);
                    count = newCount;
                }
            }
            else
            {
                string[] columns = new string[] { columnName, "Anzahl" };
                CreateTable(columns, tableName);
                foreach (KeyValuePair<string, int> item in dict)
                {
                    command = InsertRow(columns, new object[] { item.Key, item.Value.ToString() }, tableName, command);
                }
            }
            InsertRow(new string[] { columnName }, new object[] { sourceRowCount.ToString() }, tableName, command);
        }

        internal void DeleteRow(string id, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"DELETE from [{tableName}] where rowid = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = id });
                command.ExecuteNonQuery();
            }
        }

        internal void Undo()
        {
            if (Pointer > 0)
            {
                Pointer--;
                GoToSavePoint(Pointer);
            }
        }

        internal void Redo()
        {
            if (Pointer < SavePoints)
            {
                Connection.Trace -= UpdateHandler;
                
                DatabaseHistory.Redo(Pointer);
                SetSavepoint();

                Connection.Trace += UpdateHandler;
            }
        }

        internal void ExecuteCommand(string sql, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        private void GoToSavePoint(int savePoint)
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"ROLLBACK TO \"{savePoint}\"";
                command.ExecuteNonQuery();
            }
        }

        internal void UpdateCell(string value, string alias, string id, bool aliasIsColumnName = false, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string columnName = aliasIsColumnName ? alias : GetColumnName(alias, tableName);
                command.CommandText = $"UPDATE [{tableName}] set [{columnName}] = ? where rowid = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                command.Parameters.Add(new SQLiteParameter() { Value = id });
                command.ExecuteNonQuery();
            }
        }

        internal void SetColumnValues(string alias, string newValue, string tableName = "main")
        {
            string columnName = GetColumnName(alias, tableName);
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] set [{columnName}] = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = newValue });
                command.ExecuteNonQuery();
            }
        }

        internal void SetCheckSum(string columnName, Action updateLoadingBar, Form invokeForm, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                int offset = 0;

                command.CommandText = $"SELECT rowid, [{columnName}] from [{tableName}] LIMIT {Properties.Settings.Default.MaxRows} OFFSET ?";
                command.Parameters.Add(new SQLiteParameter());
                bool hasRows = true;
                bool askAgain = true;
                while (hasRows)
                {
                    command.Parameters[0].Value = offset;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        int newRows = 0;
                        while (reader.Read())
                        {
                            newRows++;
                            string value = reader.GetString(1);
                            int checkSum = ChecksumEAN9(value);
                            if (checkSum == -1 && askAgain)
                            {
                                DialogResult result = MessageHandler.MessagesYesNo(invokeForm, MessageBoxIcon.Warning, $"Ungültige Zahl in Zeile {offset + newRows}. Trotzdem fortfahren?");
                                if (result == DialogResult.No)
                                {
                                    break;
                                }
                                else
                                {
                                    askAgain = false;
                                }
                            }
                            else
                            {
                                UpdateCell(value + checkSum, columnName, reader.GetString(0), true, tableName);
                            }
                            updateLoadingBar();
                        }
                        offset += newRows;
                        hasRows = reader.HasRows && newRows < Properties.Settings.Default.MaxRows;
                    }
                }
            }
        }

        /// <summary>
        /// Before a new main instance with a table is opened, copy the table into it's own file
        /// </summary>
        /// <param name="tableName"></param>
        internal void CopyToNewDatabaseFile(string tableName, string sourceTable = "main")
        {
            string path = Path.Combine(DatabaseDirectory, tableName + ".sqlite");
            CreateDatabase(path);
            SQLiteConnection connection = new SQLiteConnection($"Data Source={path};Version=3;");
            connection.Open();
            SQLiteTransaction transaction = connection.BeginTransaction();
            //SQLiteConnection mainConnection;
            //bool isMain = sourceTable == "main";
            //if (isMain)
            //{
            //    mainConnection = Connection;
            //    mainConnection.Trace -= UpdateHandler;
            //}
            //else
            //{
            //    mainConnection = TempConnection;
            //}

            //using (SQLiteCommand command = mainConnection.CreateCommand())
            //{
            //    command.CommandText = $"ATTACH database [{path}] as [{tableName}]";
            //    command.ExecuteNonQuery();

            //    string colType = "varchar(255) not null default '' COLLATE NATURALSORT";
            //    Dictionary<string, string> aliasColumnMapping = GetAliasColumnMapping(sourceTable);
            //    command.CommandText = $"CREATE table [{tableName}].main ({SortOrderColumnName} INTEGER PRIMARY KEY AUTOINCREMENT, [{string.Join($"] {colType},[", aliasColumnMapping.Keys)}] {colType})";

            //    command.CommandText = $"INSERT into [{tableName}].main SELECT {GetHeaderString(aliasColumnMapping.Values)} from [{sourceTable}]"; //not SELECT * because in sourceTable there may be "deleted" columns
            //    command.ExecuteNonQuery();


            //    command.CommandText = $"DETACH database [{tableName}]";
            //    command.ExecuteNonQuery();
            //}
            //if (isMain)
            //{
            //    mainConnection.Trace += UpdateHandler;
            //}
            Dictionary<string, string> aliasColumnMapping = GetAliasColumnMapping(sourceTable);
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"ATTACH database [{DatabasePath}] as main"; //probably everything not available because it is not commited
                command.ExecuteNonQuery();

                string colType = "varchar(255) not null default '' COLLATE NATURALSORT";
                command.CommandText = $"CREATE table main ({SortOrderColumnName} INTEGER PRIMARY KEY AUTOINCREMENT, [{string.Join($"] {colType},[", aliasColumnMapping.Keys)}] {colType})";

                command.CommandText = $"INSERT into main SELECT [{IdColumnName}], {GetHeaderString(aliasColumnMapping.Values)} from main.[{sourceTable}]"; //not SELECT * because in sourceTable there may be "deleted" columns
                command.ExecuteNonQuery();


                command.CommandText = $"DETACH database main";
                command.ExecuteNonQuery();
            }
            CreateMetaData(tableName, aliasColumnMapping.Keys, connection);
            transaction.Commit();
            connection.Close();

            Delete(tableName);
        }

        /// <summary>
        /// Rows with the same value in the given column "identifier" are merged.
        /// <para>Columns are either summed, counted or a new one is created</para>
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="additionalColumns"></param>
        /// <param name="updateLoadingBar"></param>
        /// <returns></returns>
        internal void MergeRows(string columnName, List<PlusListboxItem> additionalColumns, bool separator, Form1 invokeForm, Action updateLoadingBar, string tableName = "main")
        {
            //additionalColumns[...].Value is the columnName
            Dictionary<string, string> aliasColumnMapping = GetAliasColumnMapping(tableName);
            string separatorText = separator ? "#,0.###" : string.Empty;
            CreateIndexOn(tableName, columnName, invokeForm, false);
            string[] sumColumns = additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Sum).Select(item => item.Value).ToArray();
            string[] countColumns = additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Count).Select(item => item.Value).ToArray();
            string[] appendArray = additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Nothing).Select(item => item.Value).ToArray();
            if (countColumns.Length != 0)
            {
                SetRowCountByIdentifier(columnName, countColumns, tableName);
            }
            if(sumColumns.Length != 0)
            {
                SetSumByIdentifier(columnName, sumColumns, tableName, separatorText);
            }
            
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string headerString = appendArray.Length == 0 ? string.Empty : "t1.[" + string.Join("], t1.[", appendArray.Concat(sumColumns)) + "]";
                command.CommandText = $"SELECT t1.[{IdColumnName}], t1.[{columnName}], {headerString} from [{tableName}] t1 join (Select t2.[{columnName}] from [{tableName}] t2 group by t2.[{columnName}] having count(*) > 1) t2 on t1.[{columnName}] = t2.[{columnName}] order by [{columnName}]";
                int offset = 2; //ofset till the values of "headerString"
                int sumOffset = offset + appendArray.Length;
                bool containsSumColumns = sumColumns.Length != 0;
                SQLiteCommand sumCommand = null;
                using(SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        #region Init
                        //Dictionary<string, decimal> sum = new Dictionary<string, decimal>(); //Dictionary instead of single variable because we can sum several columns

                        string id = reader.GetString(0);
                        string nameBefore = reader.GetString(1);
                        int counter = 1;
                        
                        //for (int i = 0; i < sumColumns.Length; ++i)
                        //{
                        //    decimal.TryParse(reader.GetString(i + sumOffset), out decimal result);
                        //    sum.Add(sumColumns[i], result);
                        //}
                        #endregion

                        while (reader.Read())
                        {
                            string name = reader.GetString(1);
                            if(name != nameBefore)
                            {
                                //sumCommand = UpdateRow(id, sum.Keys.ToArray(), sum.Values.Select(value => value.ToString(separatorText)).ToArray(), tableName, sumCommand);

                                #region InitNew
                                id = reader.GetString(0); //newId
                                
                                //for (int i = 0; i < sumColumns.Length; ++i)
                                //{
                                //    decimal.TryParse(reader.GetString(i + sumOffset), out decimal result);
                                //    sum[sumColumns[i]] = result;
                                //}
                                counter = 1;
                                #endregion
                            }
                            else
                            {
                                if (appendArray.Length != 0)
                                {
                                    string[] newColumnNames = new string[appendArray.Length];
                                    string[] newRowValues = new string[appendArray.Length];
                                    for (int i = 0; i < appendArray.Length; ++i)
                                    {
                                        string newAlias = appendArray[i] + counter;
                                        string colName;

                                        if (!aliasColumnMapping.ContainsKey(newAlias))
                                        {
                                            colName = AddColumnFixedAlias(newAlias, tableName);
                                            aliasColumnMapping.Add(newAlias, colName);
                                        }
                                        else
                                        {
                                            colName = aliasColumnMapping[newAlias];
                                        }
                                        newColumnNames[i] = colName;
                                        newRowValues[i] = reader.GetString(i + offset);
                                    }
                                    UpdateRow(id, newColumnNames, newRowValues, tableName);
                                }
                                //for (int i = 0; i < sumColumns.Length; ++i)
                                //{
                                //    if(decimal.TryParse(reader.GetString(i + sumOffset), out decimal result))
                                //    {
                                //        sum[sumColumns[i]] += result;
                                //    }
                                    
                                //}
                                counter++;
                                DeleteRow(id, tableName);
                            }
                            nameBefore = name;
                        }
                        //UpdateRow(id, sum.Keys.ToArray(), sum.Values.Select(value => value.ToString(separatorText)).ToArray(), tableName, sumCommand);
                    }
                }
            }

            DeleteIndexOn(tableName, columnName);
        }

        private void SetSumByIdentifier(string identifier, string[] columnNames, string tableName, string separatorText)
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                foreach(string columnName in columnNames)
                {
                    command.CommandText = $"UPDATE [{tableName}] set [{columnName}] = TOSTRING(" +
                        $"(select sum(PARSEDECIMAL(t2.[{columnName}])) from [{tableName}] t2 group by t2.[{identifier}] having [{tableName}].[{IdColumnName}]= t2.[{IdColumnName}])," +
                        $"\"{separatorText})\")";
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetRowCountByIdentifier(string identifier, string[] columnNames, string tableName)
        {
            //count (just unique rows): update [{tableName}] set col1 = "1", col2 = "1",... where [{IdColumnName}] in (Select t2.[{IdColumnName}] from [{tableName}] t2 group by t2.[{Identifier}] having count(*) = 1)
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] set {string.Join("=", columnNames)} = (Select count(t2.[{identifier}]) from [{tableName}] t2 group by t2.[{identifier}] having [{tableName}].[{IdColumnName}] = t2.[{IdColumnName}]) "; //col1 = col2 = ... works
                command.ExecuteNonQuery();
            }
        }

        private SQLiteCommand UpdateRow(string id, string[] columnNames, string[] columnValues, string tableName, SQLiteCommand command = null)
        {
            
            if(command == null)
            {
                command = GetConnection(tableName).CreateCommand();
                command.CommandText = $"UPDATE [{tableName}] SET {GetUpdateCommandText(columnNames)} WHERE [{IdColumnName}] = ?";
                foreach(string value in columnValues)
                {
                    command.Parameters.Add(new SQLiteParameter() { Value = value });
                }
                command.Parameters.Add(new SQLiteParameter() { Value = id });
            }
            else
            {
                for (int i = 0; i < columnValues.Length; ++i)
                {
                    command.Parameters[i].Value = columnValues[i];
                }
                command.Parameters[command.Parameters.Count - 1].Value = id;
            }
            command.ExecuteNonQuery();
            return command;
        }

        private string GetUpdateCommandText(string[] columnNames)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < columnNames.Length -1; ++i)
            {
                AppendColumn(builder, columnNames[i]).Append(",");
            }
            AppendColumn(builder, columnNames[columnNames.Length - 1]);

            return builder.ToString();

            StringBuilder AppendColumn(StringBuilder bd, string column)
            {
                return bd.Append("[").Append(column).Append("]=?");
            }
        }

        private int ChecksumEAN9(string data)
        {
            int result = -1;
            if (int.TryParse(data, out int _))
            {
                int sum1 = 0;
                for (int i = data.Length - 2; i >= 0; i -= 2)
                {
                    sum1 += (int)char.GetNumericValue(data[i]);
                }

                int sum2 = 0;
                for (int i = data.Length - 1; i >= 0; i -= 2)
                {
                    sum2 += (int)char.GetNumericValue(data[i]);
                }

                int checksum_digit = 10 - ((sum1 + (sum2 * 3)) % 10);

                result = checksum_digit == 10 ? 0 : checksum_digit;
            }
            return result;
        }

        internal string GetRowWithMaxCharacters(string order, OrderType orderType, out int index, string tableName = "main")
        {
            string id = null;
            index = 0;
            
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT [{IdColumnName}], max({GetLengthCalcString(tableName)}) from [{tableName}]";
                id = command.ExecuteScalar()?.ToString();

                if (id != null)
                {
                    command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, 0, true, tableName);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        for (; reader.Read() && reader.GetString(0) != id; ++index) { }
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// Searches for a value in a given column.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="alias"></param>
        /// <param name="strictMatch"></param>
        /// <param name="order"></param>
        /// <param name="orderType"></param>
        /// <param name="tableName"></param>
        /// <returns>index of the row that matches the value</returns>
        internal int SearchValue(string value, string alias, bool strictMatch, string order, OrderType orderType, string tableName = "main")
        {
            int index = -1;
            //strictMatch == true: column = value NOCASE
            //strictMatch == false: column like %value% //nocase not needed; default of like is case insensitive
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, 0, true, tableName, $"where [{GetColumnName(alias, tableName)}] {(strictMatch ? "= ? COLLATE NOCASE" : "like %?%")}");
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                string id = command.ExecuteScalar()?.ToString();
                if (id != null)
                {
                    command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, 0, true, tableName);
                    command.Parameters.Clear();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        for (; reader.Read() && reader.GetString(0) != id; ++index) { }
                    }
                }
            }
            return index;
        }

        private string GetLengthCalcString(string tableName)
        {
            List<string> columnNames = GetColumnNames(tableName);
            return "length(" + string.Join(")+(", columnNames) + ")";
        }

        internal void UpdateRowsWithMinCharacters(string columnName, int minLength, string value, string destinationColumn, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] set [{destinationColumn}] = ? where length({columnName}) >= ?";
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                command.Parameters.Add(new SQLiteParameter() { Value = minLength });
            }
        }

        internal int DeleteRowByMatch(string value, string columnName, bool strictMatch, string tableName = "main")
        {
            int deleteCount;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"DELETE from [{tableName}] where [{columnName}] {(strictMatch ? "=? COLLATE NOCASE" : "like %?%")}";
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                deleteCount = Convert.ToInt32(command.ExecuteScalar());
            }
            return deleteCount;
        }

        //internal void CheckHeaders(List<string> tableHeader, List<string> notFoundColumns, string[] aliases, string tableName = "main")
        //{
        //    Dictionary<string,string> aliasColumnMapping = GetAliasColumnMapping(tableName);
        //    notFoundColumns.AddRange( aliases.Where(header => !tableHeader.Contains(header, System.StringComparer.OrdinalIgnoreCase)));
        //}
    }
}
