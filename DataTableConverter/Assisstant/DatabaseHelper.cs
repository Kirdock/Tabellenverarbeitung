using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataTableConverter.Assisstant.SQL_Functions;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
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
            SQLiteFunction.RegisterFunction(typeof(Round)); //ROUND2(myValue, type, decimalCount)
            SQLiteFunction.RegisterFunction(typeof(CountString)); //COUNTSTRING(myValue, mySubstring)
            SQLiteFunction.RegisterFunction(typeof(GetSplit)); //GETSPLIT(myValue, mySubstring, myIndex)
            SQLiteFunction.RegisterFunction(typeof(CustomUppercase)); //CUSTOMUPPERCASE(myValue, myOption)
            SQLiteFunction.RegisterFunction(typeof(CustomTrim)); //CUSTOMTRIM(myValue, myCharacters, isDeleteDouble, trimType)
            SQLiteFunction.RegisterFunction(typeof(CustomSubstring)); //CUSTOMSUBSTRING(myValue, replaceText, start, end, isReplace, isReverse)
            SQLiteFunction.RegisterFunction(typeof(DataTableConverter.Assisstant.SQL_Functions.Padding)); //PADDING(value, type, count, character)

            if (createMainDatabase)
            {
                CreateDatabase(DatabasePath);
            }
            CreateDatabase(TempDatabasePath);
            ConnectMain();
            ConnectTemp();
        }

        internal void RoundColumns(string[] sourceColumns, string[] destinationColumns, int type, int decimals, string tableName)
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < sourceColumns.Length; ++i)
                {
                    builder.Append("[").Append(destinationColumns[i]).Append("]=ROUND2([").Append(sourceColumns[i]).Append("],$type,$decimals), ");
                }
                builder.Remove(builder.Length - 2, 2);
                command.Parameters.AddWithValue("$type", type);
                command.Parameters.AddWithValue("$decimals", decimals);
                command.CommandText = $"UPDATE [{tableName}] SET {builder}";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Padding for columns depending on given conditions (if column = value)
        /// </summary>
        /// <param name="sourceColumns"></param>
        /// <param name="destinationColumns"></param>
        /// <param name="conditions"></param>
        /// <param name="operationSide"></param>
        /// <param name="counter"></param>
        /// <param name="character"></param>
        /// <param name="tableName"></param>
        internal void Padding(string[] sourceColumns, string[] destinationColumns, DataTable conditions, ProcPadding.Side operationSide, int counter, char character, string tableName)
        {
            //UPDATE [{tableName}] SET destination1 = CASE
            //        WHEN cond1Col = cond1Val or ... THEN PADDING(source1, type, counter, character)
            //                       ELSE source1
            //        END
            TranslateAliasOfDataRowAndRemoveEmpty(conditions, (int)ProcPadding.ConditionColumn.Spalte, tableName);

            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] SET ");
                if (conditions.Rows.Count == 0)
                {
                    for (int i = 0; i < sourceColumns.Length; ++i)
                    {
                        builder.Append("[").Append(destinationColumns[i]).Append("] = PADDING([").Append(sourceColumns[i]).Append("],$side,$counter,$char),");
                    }
                }
                else
                {
                    for (int i = 0; i < sourceColumns.Length; ++i)
                    {
                        builder.Append("CASE WHEN ");
                        foreach (DataRow rep in conditions.AsEnumerable())
                        {
                            string column = rep[(int)ProcPadding.ConditionColumn.Spalte].ToString();
                            string value = rep[(int)ProcPadding.ConditionColumn.Wert].ToString();
                            builder.Append("[").Append(column).Append("]=? or ");
                            command.Parameters.Add(new SQLiteParameter() { Value = value });
                        }
                        builder.Remove(builder.Length - 3, 3);
                        builder.Append("THEN PADDING([").Append(sourceColumns[i]).Append("],$side,$counter,$char)");
                        builder.Append(" ELSE [").Append(sourceColumns[i]).Append("] END ,");
                    }
                }
                command.Parameters.AddWithValue("$side", (int)operationSide);
                command.Parameters.AddWithValue("$counter", counter);
                command.Parameters.AddWithValue("$char", character);
                builder.Remove(builder.Length - 1, 1);
                command.CommandText = builder.ToString();
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Renames the column alias to column name on a given index
        /// </summary>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <param name="tableName"></param>
        private void TranslateAliasOfDataRowAndRemoveEmpty(DataTable table, int index, string tableName)
        {
            foreach(DataRow row in table.AsEnumerable())
            {
                string column = row[index].ToString();
                if (string.IsNullOrWhiteSpace(column))
                {
                    row.Delete();
                }
                else
                {
                    row[index] = GetColumnName(column, tableName);
                }
            }
            table.AcceptChanges();
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

        /// <summary>
        /// Creates table and meta table
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="tableName"></param>
        internal void CreateTable(IEnumerable<string> columnNames, string tableName)
        {
            CreateTable(columnNames, tableName, TempConnection);
        }

        /// <summary>
        /// Creates table and meta table
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
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

                command.CommandText = $"CREATE table [{tableName + MetaTableAffix}] (column varchar(255) not null default '' COLLATE NATURALSORT, alias varchar(255) COLLATE NATURALSORT, sortorder INTEGER primary key AUTOINCREMENT)";
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

        internal SQLiteCommand InsertRow(Dictionary<string, string> row, string tableName = "main", SQLiteCommand cmd = null)
        {
            SQLiteConnection connection = GetConnection(tableName);
            SQLiteCommand command = cmd;
            if (cmd == null)
            {
                command = connection.CreateCommand();
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
            }
            else
            {
                int i = 0;
                foreach(KeyValuePair<string, string> pair in row)
                {
                    command.Parameters[i].Value = pair.Value.ToString();
                    i++;
                }
            }
            command.ExecuteNonQuery();
            return command;
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

        internal SQLiteCommand InsertRow(IEnumerable<string> eHeaders, SQLiteDataReader reader, string tableName, SQLiteCommand cmd = null, SQLiteConnection connection = null)
        {
            string[] headers = eHeaders.ToArray();
            string headerString = GetHeaderString(headers);

            SQLiteCommand command = cmd;
            if (cmd == null)
            {
                command = CreateInsertRowCommand(headers, tableName, reader, connection);
            }
            else
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    command.Parameters[i].Value = reader.GetString(i);
                }
            }
            command.ExecuteNonQuery();
            return cmd;
        }

        private SQLiteCommand CreateInsertRowCommand(IEnumerable<string> eHeaders, string tableName, SQLiteDataReader reader, SQLiteConnection connection = null)
        {
            string[] headers = eHeaders.ToArray();
            string headerString = GetHeaderString(headers);

            SQLiteCommand command = (connection ?? GetConnection(tableName)).CreateCommand();
            command.CommandText = $"INSERT into [{tableName}] ({headerString}) values ({GetValueString(headers.Length)})";

            for (int i = 0; i < reader.FieldCount; i++)
            {
                command.Parameters.Add(new SQLiteParameter() { Value = reader.GetString(i) });
            }
            return command;
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

        internal string AddColumnAt(int index, string alias, string tableName = "main")
        {
            string columnName = AddColumnFixedAlias(alias, tableName);
            MoveColumnToIndex(index, columnName, tableName);
            return columnName;
        }

        internal void MoveColumnToIndex(int index, string columnName, string tableName = "main")
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                index = index + 1; //ID begins at 1
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = -(sortorder)";
                command.ExecuteNonQuery();
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = ? where column = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = index });
                command.Parameters.Add(new SQLiteParameter() { Value = columnName });
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                int counter = 2;
                List<KeyValuePair<int, int>> updates = new List<KeyValuePair<int, int>>();
                command.CommandText = $"SELECT sortorder from [{tableName + MetaTableAffix}]";
                using(SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        updates.Add(new KeyValuePair<int, int>(reader.GetInt32(0), counter));
                        counter++;
                    }
                }

                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = ? where sortorder = ?";
                command.Parameters.Add(new SQLiteParameter());
                command.Parameters.Add(new SQLiteParameter());
                foreach(KeyValuePair<int, int> pair in updates)
                {
                    command.Parameters[0].Value = pair.Key;
                    command.Parameters[1].Value = pair.Value;
                    command.ExecuteNonQuery();
                }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="tableName"></param>
        /// <param name="destinationTableColumnAliasMapping"></param>
        /// <returns>Column name of created column</returns>
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
        /// /// <returns>Column name of created column</returns>
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
        internal bool CreateIndexOn(string tableName, string columnName, Form1 invokeForm = null, bool unique = true)
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

        private string GetHeaderString(IEnumerable<string> headers)
        {

            return "[" + string.Join("],[", headers) + "]";
        }

        private string GetHeaderStringWithAlias(Dictionary<string,string> columnAliasMapping)
        {
            StringBuilder builder = new StringBuilder();
            foreach(KeyValuePair<string,string> pair in columnAliasMapping)
            {
                builder.Append('[').Append(pair.Value).Append("] AS [").Append(pair.Key).Append("],");
            }
            return builder.Remove(builder.Length-1,1).ToString();
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

        private string GetSortedSelectString(string headerString, string order, OrderType orderType, int limit, int offset, bool includeId, string tableName, string whereStatement = "", string idAlias = null)
        {
            string selectString = "SELECT ";
            if (includeId)
            {
                selectString += $"{IdColumnName} AS [{idAlias ?? IdColumnName}],";
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

        private string GetSelectString(string headerString, bool includeId, string tableName)
        {
            return $"SELECT {(includeId ? $"{IdColumnName}," : string.Empty)}{headerString} FROM [{tableName}]";
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
        /// <para>Key: alias</para>
        /// Value: columnName
        /// <para>Note: This Dictionary is insertion order except you delete keys, then the "empty" space is filled by a new ".Add"</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal Dictionary<string, string> GetAliasColumnMapping(string tableName = "main")
        {
            Dictionary<string, string> headerMapping = new Dictionary<string, string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias, column from [{tableName + MetaTableAffix}] where alias is not null order by sortorder";
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

        internal int PVMSplit(string sourceFilePath, Form1 invokeForm, int encoding, string invalidColumnName, string order, OrderType orderType, string tableName = "main")
        {
            string directory = Path.GetDirectoryName(sourceFilePath);
            string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            
            SplitAndSavePVM(tableName, invalidColumnName, directory, fileName + Properties.Settings.Default.FailAddressText, encoding, order, orderType, invokeForm, false);
            return SplitAndSavePVM(tableName, invalidColumnName, directory, fileName + Properties.Settings.Default.RightAddressText, encoding, order, orderType, invokeForm, true); //return count of rows
        }

        private int SplitAndSavePVM(string tableName, string invalidColumnName, string directory, string fileName, int encoding, string order, OrderType orderType, Form1 invokeForm, bool saveValidRows)
        {
            int rowCount = 0;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                Dictionary<string,string> columnAliasMapping = GetAliasColumnMapping(tableName);
                command.CommandText = $"SELECT {GetHeaderStringWithAlias(columnAliasMapping)} from [{tableName}] where [{invalidColumnName}] {(saveValidRows ? "!=" : "=")} ?";
                command.Parameters.Add(new SQLiteParameter() { Value = Properties.Settings.Default.FailAddressValue });

                rowCount = ExportHelper.Save(directory, fileName, null, encoding, Properties.Settings.Default.PVMSaveFormat, order, orderType, invokeForm, command, null, tableName);
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

            command.CommandText += $"{GetHeaderStringWithAlias(columnAliasMapping)} from [{tableName}]";

            return command;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="aliases"></param>
        /// <returns>Select command with rowid and given aliases</returns>
        internal SQLiteCommand GetDataCommand(string tableName, string[] aliases)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();

            command.CommandText = $"SELECT rowid, {GetHeaderString(GetColumnNames(aliases, tableName))} from [{tableName}]";

            return command;
        }

        internal SQLiteCommand GetDataCommand(string tableName, string order, OrderType orderType, string orderAlias = null)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            Dictionary<string, string> columnAliasMapping = GetAliasColumnMapping(tableName);

            command.CommandText = GetSortedSelectString(GetHeaderStringWithAlias(columnAliasMapping), order, orderType, -1, -1, orderAlias != null, tableName, string.Empty, orderAlias);

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

        internal string[] GetColumnNames(string[] aliases, string tableName)
        {
            string[] columnNames = new string[aliases.Length];
            aliases = aliases.OrderBy(alias => alias, new NaturalStringComparer()).ToArray();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("alias = ?");
                for(int i = 1; i < aliases.Length; ++i)
                {
                    builder.Append(" or alias = ?");
                    command.Parameters.Add(new SQLiteParameter() { Value = aliases[i] });
                }
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] where {builder} order by alias";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        columnNames[i] = reader.GetString(0);
                        ++i;
                    }
                }
            }
            return columnNames;
        }

        internal List<string> GetColumnNames(string tableName = "main", bool includeDeleted = false)
        {
            List<string> columnNames = new List<string>();
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] {(includeDeleted ? string.Empty : "where alias is not null")} order by sortorder";
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aliases"></param>
        /// <param name="tableName"></param>
        /// <returns>Colum names of new columns</returns>
        internal string[] CopyColumns(string[] aliases, string tableName)
        {
            for(int i = 0; i < aliases.Length; ++i)
            {
                aliases[i] = CopyColumn(aliases[i], tableName);
            }
            return aliases;
        }

        /// <summary>
        /// Adds a column. If the column already exists the user is asked if he wants to override it
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="invokeForm"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns>Status if creation was successfull</returns>
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
                else
                {
                    columnName = null;
                }
            }
            else
            {
                columnName = AddColumnFixedAlias(alias, tableName);
            }
            return inserted;
        }

        /// <summary>
        /// Adds columns. If one column already exists the user is asked if he wants to override it
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="originalAlias"></param>
        /// <param name="invokeForm"></param>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <returns>Status if all columns were created</returns>
        internal bool AddColumnsWithDialog(string alias, string[] originalAlias, Form invokeForm, string tableName, out string[] columnNames)
        {
            bool result = true;
            columnNames = new string[originalAlias.Length];
            for(int i = 0; i < originalAlias.Length; ++i)
            {
                result &= AddColumnWithDialog($"{originalAlias[i]}_{alias}", invokeForm, tableName, out string columnName);
                columnNames[i] = columnName;
            }
            return result;
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

        internal void UpdateCells(IEnumerable<KeyValuePair<string, string>> updates, string column, string tableName = "main")
        {
            if (updates.Count() != 0)
            {
                using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
                {
                    command.CommandText = $"UPDATE [{tableName}] set [{column}] = ? where [{IdColumnName}] = ?";
                    command.Parameters.Add(new SQLiteParameter());
                    command.Parameters.Add(new SQLiteParameter());
                    foreach (KeyValuePair<string, string> pair in updates)
                    {
                        command.Parameters[0].Value = pair.Key;
                        command.Parameters[1].Value = pair.Value;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        internal void UpdateCell(string value, string alias, string id, bool aliasIsColumnName = false, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string columnName = aliasIsColumnName ? alias : GetColumnName(alias, tableName);
                command.CommandText = $"UPDATE [{tableName}] set [{columnName}] = ? where [{IdColumnName}] = ?";
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
            IEnumerable<string> columns = GetAliasColumnMapping(sourceTable).Keys;
            CreateTable(columns, "main", connection);
            using (SQLiteCommand command = GetDataCommand(sourceTable))
            {
                using(SQLiteDataReader reader = command.ExecuteReader())
                {
                    SQLiteCommand insertCommand = null;
                    while (reader.Read())
                    {
                        insertCommand = InsertRow(columns, reader, "main", insertCommand, connection);
                    }
                }
            }
            transaction.Commit();
            connection.Close();
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
                //SQLiteCommand sumCommand = null;
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

        internal void EmptyColumnByCondition(string sourceColumn, string destinationColumn, string compareAlias, string tableName)
        {
            string compareColumn = GetColumnName(compareAlias, tableName);
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] SET [{destinationColumn}] = CASE WHEN [{sourceColumn}] = [{compareColumn}] THEN '' ELSE [{sourceColumn}]";
                command.ExecuteNonQuery();
            }
        }

        private int GetMaxOccurenciesOfString(string sourceAlias, string subString, string tableName)
        {
            int max = 0;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"Select max(Select COUNTSTRING([{GetColumnName(sourceAlias, tableName)}], ?) from [${tableName}])";
                command.Parameters.Add(new SQLiteParameter() { Value = subString });
                max = (int)command.ExecuteScalar();
            }
            return max;
        }

        internal void SplitColumnByString(string sourceAlias, string newColumn, string splitText, string tableName = "main")
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                int max = GetMaxOccurenciesOfString(sourceAlias, splitText, tableName);
                StringBuilder builder = new StringBuilder($"Update [{tableName}] set ");
                for (int i = 1; i <= max; i++)
                {
                    string createdColumn = AddColumnWithAdditionalIfExists($"{newColumn}{i}", string.Empty, tableName);
                    builder.Append($"[{createdColumn}] = GETSPLIT([{createdColumn}], $splitText,{i - 1}),");
                }
                command.CommandText = builder.Remove(builder.Length - 1, 1).ToString();
                command.Parameters.AddWithValue("$splitText", splitText);
                command.ExecuteNonQuery();
            }
        }

        internal void Enumerate(string destinationColumn, int start, int end, bool repeat, string order, OrderType orderType, string tableName = "main")
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] SET [{IdColumnName}] = - [{IdColumnName}]";
                command.ExecuteNonQuery();

                command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, -1, true, tableName);

                List<KeyValuePair<string, string>> updates = new List<KeyValuePair<string, string>>();
                using(SQLiteDataReader reader = command.ExecuteReader())
                {
                    int count = start;
                    bool noEnd = end != 0;
                    while (reader.Read())
                    {
                        updates.Add(new KeyValuePair<string, string>(reader.GetString(0), count.ToString()));
                        
                        count++;
                        if (noEnd && count > end)
                        {
                            if (repeat)
                            {
                                count = start;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                UpdateCells(updates, destinationColumn, tableName);
            }
        }

        internal void ReplaceColumnValues(IEnumerable<DataRow> distinctDataTale, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] set ");

                foreach (DataRow replaceRow in distinctDataTale)
                {
                    builder.Append($"[{GetColumnName(replaceRow[(int)ProcReplaceWhole.ColumnIndex.Column].ToString(), tableName)}] = ?,");
                    command.Parameters.Add(new SQLiteParameter() { Value = replaceRow[(int)ProcReplaceWhole.ColumnIndex.Value].ToString() });
                }

                command.CommandText = builder.Remove(builder.Length - 1, 1).ToString();
                command.ExecuteNonQuery();
            }
        }

        internal void SetCustomUppercase(string[] aliases, int option, string tableName = "main")
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string[] columns = GetColumnNames(aliases, tableName);
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] set ");
                foreach(string column in columns)
                {
                    builder.Append($"[{column}] = CUSTOMUPPERCASE([{column}], {option}),");
                }
                command.CommandText = builder.Remove(builder.Length - 1, 1).ToString();
                command.ExecuteNonQuery();
            }
        }

        internal void Trim(string characters, string[] columnAlias, bool deleteDouble, ProcTrim.TrimType type, string tableName = "main")
        {
            string[] columns = columnAlias == null ? GetColumnNames(tableName).ToArray() : GetColumnNames(columnAlias, tableName);
            using(SQLiteCommand commmand = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] set ");
                foreach(string column in columns)
                {
                    builder.Append($"[{column}]=CUSTOMTRIM([{column}],$characters,$deleteDouble, $type),");
                }
                commmand.Parameters.AddWithValue("$characters", characters);
                commmand.Parameters.AddWithValue("$deleteDouble", deleteDouble);
                commmand.Parameters.AddWithValue("$type", type == ProcTrim.TrimType.Start ? 0 : type == ProcTrim.TrimType.End ? 1 : 2);
                commmand.CommandText = builder.Remove(builder.Length - 1, 1).ToString();
                commmand.ExecuteNonQuery();
            }
        }

        internal void SearchAndShortcut(string sourceColumn, string destinationColumn, bool totalSearch, string searchText, string shortcut, int from, int to, string order, OrderType orderType, string tableName = "main")
        {
            Func<string, string, bool> search;
            if (totalSearch)
            {
                search = SearchTotal;
            }
            else
            {
                search = PartialSearch;
            }
            
            if (string.IsNullOrWhiteSpace(shortcut))
            {
                int counter = from;
                bool found = false;


                using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
                {
                    command.CommandText = GetSortedSelectString(GetHeaderString(new string[] { sourceColumn }), order, orderType, -1, -1, true, tableName);

                    List<KeyValuePair<string, string>> updates = new List<KeyValuePair<string, string>>();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (found)
                            {
                                if (counter > to)
                                {
                                    break;
                                }
                                else
                                {
                                    updates.Add(new KeyValuePair<string, string>(reader.GetString(0), counter.ToString()));
                                    counter++;
                                }
                            }
                            else if (search.Invoke(reader.GetString(1), searchText))
                            {
                                updates.Add(new KeyValuePair<string, string>(reader.GetString(0), counter.ToString()));
                                found = true;
                                counter++;
                            }
                        }
                    }
                    UpdateCells(updates, destinationColumn, tableName);
                }
            }
            else
            {
                SetShortcut(searchText, shortcut, sourceColumn, destinationColumn, totalSearch, tableName);
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

        private void SetShortcut(string searchText, string shortcut, string sourceColumn, string destinationColumn, bool totalSearch, string tableName)
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] set [{sourceColumn}] = ? where [{destinationColumn}] {(totalSearch ? "= ?" : "like %?%")}";
                command.Parameters.Add(new SQLiteParameter() { Value = shortcut });
                command.Parameters.Add(new SQLiteParameter() { Value = searchText });
                command.ExecuteNonQuery();
            }
        }

        internal void Substring(string[] sourceColumns, string[] destinationColumns, string replaceText, int start, int end, bool replaceChecked, bool reverseCheck, string tableName = "main")
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] SET ");
                for (int i = 0; i < sourceColumns.Length; ++i)
                {
                    builder.Append("[").Append(destinationColumns[i]).Append("] = CUSTOMSUBSTRING([").Append(sourceColumns[i]).Append("],$replaceText,$start,$end, $replaceChecked, $reverseCheck),");
                }
                command.Parameters.AddWithValue("$replaceText", replaceText);
                command.Parameters.AddWithValue("$start", start);
                command.Parameters.AddWithValue("$end", end);
                command.Parameters.AddWithValue("$replaceChecked", replaceChecked);
                command.Parameters.AddWithValue("$reverseCheck", reverseCheck);
                command.ExecuteNonQuery();
            }
        }

        internal void SearchAndReplace(string[] sourceColumns, string[] destinationColumns, bool checkTotal, bool checkWord, bool leaveEmpty, string replaceEmptyString, string replaceWholeText, bool containsEmpty, bool containsReplaceWhole, IEnumerable<DataRow> replaceWithoutEmpty, string tableName)
        {
            //3 dimensions
            // id, column (destinationColumn), value
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSelectString(GetHeaderString(sourceColumns), true, tableName);

                List<KeyValuePair<string, string[]>> updates = new List<KeyValuePair<string, string[]>>();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string[] newValues = new string[sourceColumns.Length];

                        for (int i = 0; i < sourceColumns.Length; i++)
                        {

                            string index = destinationColumns[i];
                            string value = reader.GetString(i + 1);
                            string result = value;
                            bool changed = false;

                            if (checkTotal)
                            {
                                DataRow foundRows = replaceWithoutEmpty.FirstOrDefault(replace => replace[0].ToString() == value);
                                if (foundRows != null)
                                {
                                    result = foundRows[1].ToString();
                                    changed = true;
                                }
                            }
                            else if (checkWord)
                            {
                                foreach (DataRow rep in replaceWithoutEmpty)
                                {
                                    string pattern = @"(?<=^|[\s>])" + Regex.Escape(rep[0].ToString()) + @"(?!\w)";
                                    if (Regex.IsMatch(result, pattern))
                                    {
                                        result = Regex.Replace(result, pattern, rep[1].ToString());
                                        changed = true;
                                    }
                                }
                            }
                            else
                            {
                                foreach (DataRow rep in replaceWithoutEmpty)
                                {
                                    string pattern = rep[0].ToString();
                                    if (result.Contains(pattern))
                                    {
                                        result = result.Replace(pattern, rep[1].ToString());
                                        changed = true;
                                    }
                                }
                            }

                            if (containsEmpty && !changed && result == string.Empty)
                            {
                                result = replaceEmptyString;
                                changed = true;
                            }
                            if (containsReplaceWhole && !changed && result != string.Empty)
                            {
                                result = replaceWholeText;
                                changed = true;
                            }
                            newValues[i] = !changed && leaveEmpty ? string.Empty : ProcTrim.Trim(result);
                        }
                        updates.Add(new KeyValuePair<string, string[]>(reader.GetString(0), newValues));
                    }
                }
                UpdateRows(updates, destinationColumns, tableName);
            }
        }

        private void UpdateRows(List<KeyValuePair<string, string[]>> updates, string[] destinationColumns, string tableName)
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] SET ");
                foreach(string column in destinationColumns)
                {
                    builder.Append($"[{column}]=?");
                    command.Parameters.Add(new SQLiteParameter());
                }
                builder.Append($" WHERE [{IdColumnName}]=?");
                command.Parameters.Add(new SQLiteParameter());
                command.CommandText = builder.ToString();
                foreach (KeyValuePair<string, string[]> update in updates)
                {
                    for(int i = 0; i < destinationColumns.Length; ++i)
                    {
                        command.Parameters[i].Value = update.Value[i];
                    }
                    command.Parameters[command.Parameters.Count - 1].Value = update.Key;
                    command.ExecuteNonQuery();
                }
            }
        }

        internal SQLiteCommand SplitTableOnColumnsCommand(string[] sourceColumns, string order, OrderType orderType, string tableName = "main")
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            command.CommandText = GetSortedSelectString(GetHeaderString(sourceColumns), order, orderType, -1, -1, false, tableName);
            return command;
        }

        internal bool ExistsValueInColumn(string column, string value, string tableName, string idColumn, out string id)
        {
            object result = null;
            id = null;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT [{idColumn}] from [{tableName}] where [{column}] = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                result = command.ExecuteScalar();
                if(result != null)
                {
                    id = result.ToString();
                }
            }
            return result != null;
        }
    }
}
