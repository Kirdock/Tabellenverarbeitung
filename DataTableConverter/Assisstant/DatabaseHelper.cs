﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataTableConverter.Classes;
using DataTableConverter.Exceptions;
using DataTableConverter.Extensions;

namespace DataTableConverter.Assisstant
{
    class DatabaseHelper
    {
        private static readonly string DatabasePath = "Database.sqlite";
        private static readonly string TempDatabasePath = "TempDatabase.sqlite";
        private static readonly string FileNameColumn = "dateiname";
        private static readonly string DefaultTable = "main";
        private static readonly string MetaTableAffix = "_meta";
        internal static readonly string IdColumnName = "rowid"; //could be a problem as primary key if VACUUM command is executed
        private static string SortOrderColumnName = "__SORT_ORDER__";
        private static readonly string IndexAffix = "_INDEX";
        private static int SavePoints = 0, Pointer = 0; //0 => after main Table with data is created
        private static SQLiteConnection Connection, TempConnection; //Second file and connection is needed because of Trace
        private static SQLiteTraceEventHandler UpdateHandler = null;
        private static SQLiteTransaction Transaction, TempTransaction;
        private static readonly HashSet<string> IgnoreCommands = new HashSet<string>()
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

        internal static void Init()
        {
            SortOrderColumnName = IdColumnName == "rowid" ? SortOrderColumnName : IdColumnName;
            SQLiteFunction.RegisterFunction(typeof(SQLiteComparator)); //COLLATE NATURALSORT
            DatabaseHistory.CreateDatabase();
            CreateDatabase(DatabasePath);
            CreateDatabase(TempDatabasePath);
            ConnectMain();
            ConnectTemp();
        }

        internal static void CreateDatabase(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            SQLiteConnection.CreateFile(path);
            File.SetAttributes(path, FileAttributes.Hidden);            
        }

        private static void ConnectMain()
        {
            Connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;");
            Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        private static void ConnectTemp()
        {
            TempConnection = new SQLiteConnection($"Data Source={TempDatabasePath};Version=3;");
            TempConnection.Open();
            TempTransaction = TempConnection.BeginTransaction();
        }

        internal static void Close()
        {
            //Transaction.Commit();
            Transaction.Dispose();
            Connection.Close();
            DatabaseHistory.Close();
            DeleteDatabases();
        }

        private static void DeleteDatabases()
        {
            DeleteMainDatabase();
            DeleteTempDatabase();
        }

        /// <summary>
        /// Get alias of meta-table ordered by sortorder
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static List<string> GetSortedColumnsAsAlias(string tableName = "main")
        {
            List<string> aliases = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias from [{tableName + MetaTableAffix}] order by sortorder asc";
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    aliases.Add(reader.GetString(0));
                }
            }
            return aliases;
        }

        internal static void CreateTable(IEnumerable<string> columnsNames)
        {
            string tableName = "main";
            CreateTable(columnsNames, tableName, Connection);
        }

        internal static void CreateTable(IEnumerable<string> columnNames, string tableName)
        {
            CreateTable(columnNames, tableName, TempConnection);
        }

        private static void CreateTable(IEnumerable<string> columnNames, string tableName, SQLiteConnection connection)
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

        private static void CreateMetaData(string tableName, IEnumerable<string> columnNames)
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"DROP table if exists [{tableName + MetaTableAffix}]";
                command.ExecuteNonQuery();

                command.CommandText = $"CREATE table [{tableName + MetaTableAffix}] (column varchar(255) not null default '', alias varchar(255) not null default '', sortorder INTEGER primary key AUTOINCREMENT)";
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

        internal static void ReplaceTable(string newTable, string oldTable = "main")
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

            SetSavepoint();
            ConnectTemp();
        }

        private static void CommitTemp()
        {
            TempTransaction.Commit();
        }

        private static void Reset()
        {
            SavePoints = Pointer = 0;
        }

        private static void DeleteMainDatabase()
        {
            Connection.Close();
            File.Delete(DatabasePath);
        }

        private static void DeleteTempDatabase()
        {
            TempConnection.Close();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            File.Delete(TempDatabasePath);
        }

        private static void RenameTempDatabase()
        {
            TempConnection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            File.Move(TempDatabasePath, DatabasePath);
        }

        internal static void RenameTable(string from, string to)
        {
            using (SQLiteCommand command = GetConnection(from).CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{from}] rename to [{to}]";
                command.ExecuteNonQuery();

                command.CommandText = $"ALTER TABLE [{from + MetaTableAffix}] rename to [{to + MetaTableAffix}]";
                command.ExecuteNonQuery();
            }
        }

        internal static void CreateFromDataTable(string tableName, DataTable table)
        {
            Delete(tableName, true);
            string[] columnNames = table.HeadersOfDataTableAsString();
            CreateTable(columnNames, tableName);

            SQLiteCommand command = null;
            foreach(DataRow row in table.AsEnumerable())
            {
                command = InsertRow(columnNames, row.ItemArray, tableName, command);
            }
            command?.Dispose();
            table.Dispose();

        }

        internal static void InsertRow(Dictionary<string, string> row, string tableName)
        {
            string headerString = GetHeaderString(row.Keys);
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"INSERT into [{tableName}] ({headerString}) values ({GetValueString(row.Keys.Count)})";
                foreach (string header in row.Keys)
                {
                    command.Parameters.Add(new SQLiteParameter() { Value = row[header] });
                }
                command.ExecuteNonQuery();
            };
        }

        internal static SQLiteCommand InsertRow(IEnumerable<string> eHeaders, object[] values, string tableName, SQLiteCommand cmd = null)
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

        internal static bool ContainsAlias(string tableName, string column)
        {
            SQLiteConnection connection = GetConnection(tableName);
            bool status = false;
            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM [{tableName + MetaTableAffix}] WHERE alias = $column";
                command.Parameters.Add(new SQLiteParameter("$column", column));
                int count = Convert.ToInt32(command.ExecuteScalar());
                status = count != 0;
            }
            return status;
        }

        private static SQLiteConnection GetConnection(string tableName)
        {
            return tableName == DefaultTable ? Connection : TempConnection;
        }

        internal static void AddColumn(string tableName, string column, string defaultValue = "")
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{tableName}] ADD COLUMN [{column}] varchar(255) NOT NULL DEFAULT [{defaultValue}]";
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.CommandText = $"INSERT INTO [{tableName + MetaTableAffix}] (column, alias) values ($column, $alias)";
                command.Parameters.Add(new SQLiteParameter("$column", column));
                command.Parameters.Add(new SQLiteParameter("$alias", column));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds all columnNames to a given table. If the columnName already exists, an additional column will be created including a counter after the name
        /// Example: Name1, Name2, ...
        /// </summary>
        /// <param name="columnsNames"></param>
        /// <param name="defaultValue"></param>
        /// <param name="destinationTable"></param>
        /// <param name="destinationTableColumnAliasMapping"></param>
        internal static void AddColumnsWithAdditionalIfExists(IEnumerable<string> columnsNames, string defaultValue, string destinationTable, Dictionary<string, string> destinationTableColumnAliasMapping, out string[] newColumnNames)
        {
            //if columnName or column with alias exists, create a new one with +count (while loop; while exists)
            HashSet<string> aliasAndColumnNames = new HashSet<string>(destinationTableColumnAliasMapping.Keys);
            aliasAndColumnNames.UnionWith(destinationTableColumnAliasMapping.Values);
            newColumnNames = new string[columnsNames.Count()];
            int i = 0;
            foreach (string columnName in columnsNames)
            {

                string newAlias = columnName;
                if (aliasAndColumnNames.Contains(columnName))
                {
                    newAlias = columnName + 1;
                    for (int counter = 2; aliasAndColumnNames.Contains(newAlias); ++counter)
                    {
                        newAlias = columnName + counter;
                    }
                }
                AddColumn(destinationTable, newAlias, defaultValue);
                newColumnNames[i] = newAlias;
                ++i;
            }
        }

        /// <summary>
        /// Creates an index on a given column in a given table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="invokeForm"></param>
        /// <param name="unique"></param>
        /// <returns>abort-status</returns>
        private static bool CreateIndexOn(string tableName, string columnName, Form1 invokeForm = null, bool unique = true)
        {
            bool abort = false;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                try
                {
                    command.CommandText = $"CREATE {(unique ? "UNIQUE" : string.Empty)} index if not exists $index on [{tableName}]($colName)";
                    command.Parameters.Add(new SQLiteParameter("$colName", columnName));
                    command.Parameters.Add(new SQLiteParameter("$index", columnName + IndexAffix));
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
        internal static bool PVMImport(string importTable, string[] importColumnNames, string destinationIdentifierColumn, string importIdentifierColumn, Dictionary<string,string> destinationTableColumnAliasMapping, Form1 invokeForm = null, string destinationTable = "main")
        {
            bool abort;
            if(abort = CreateIndexOn(destinationTable, destinationIdentifierColumn, invokeForm))
            {
                AddColumnsWithAdditionalIfExists(importColumnNames.Where(col => col != importIdentifierColumn), string.Empty, destinationTable, destinationTableColumnAliasMapping, out string[] destinationColumnNames);

                int rowCount = GetRowCount(importTable);
                int sortOrder = 0;
                using (SQLiteCommand destinationCommand = GetConnection(destinationTable).CreateCommand())
                {
                    string headerString = GetHeaderString(destinationColumnNames);
                    destinationCommand.CommandText = $"UPDATE TABLE [{destinationTable}] ({SortOrderColumnName},{headerString}) values ({GetValueString(destinationColumnNames.Length +1 )})"; //+1 because of sortOrder

                    for (int i = 0; i < destinationColumnNames.Length + 1; i++) //+1 because of sortOrder
                    {
                        destinationCommand.Parameters.Add(new SQLiteParameter());
                    }

                    for (int offset = 0; offset < rowCount; offset += (int)Properties.Settings.Default.MaxRows)
                    {
                        using (DataTable table = GetData(importTable, importColumnNames, offset))
                        {
                            foreach (DataRow row in table.AsEnumerable())
                            {
                                destinationCommand.Parameters[0].Value = sortOrder;
                                for (int i = 0; i < row.ItemArray.Length; i++)
                                {
                                    destinationCommand.Parameters[i+1].Value = row[i].ToString();
                                }
                                sortOrder++;
                            }
                        }
                    }
                }
            }
            Delete(importTable);
            //remove index on destinationTable? (destinationColumn + IndexAffix)
            return abort;
        }

        private static string GetValueString(int count)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < count - 1; ++i)
            {
                builder.Append("?,");
            }
            return builder.Append('?').ToString();
        }

        private static string GetValueString(IEnumerable<string> headers)
        {
            return "$" + string.Join(",$", headers);
        }

        private static string GetHeaderString(IEnumerable<string> headers)
        {

            return "[" + string.Join("],[", headers) + "]";
        }

        private static DataTable GetData(string tableName, string[] columnNames, int offset)
        {
            DataTable table = new DataTable();
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string headerString = GetHeaderString(columnNames);
                command.CommandText = $"SELECT {headerString} from [{tableName}] LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                adapter.Fill(table);
            }
            return table;
        }

        internal static DataTable GetData(string tableName, int offset = 0)
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

        internal static DataTable GetData(string order, OrderType orderType, int offset, string tableName = "main")
        {
            DataTable dt = new DataTable();
            string headerString = string.Join(",", GetSortedHeadersIncludeAsAlias(tableName));
            //select explicit instead of everything because of column order
            //use alias from metaTable
            //heading1 as Surname, ...

            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                if(orderType == OrderType.Reverse)
                {
                    int half = GetRowCount(tableName)/2;
                    command.CommandText = $"SELECT {IdColumnName} AS {IdColumnName}, {headerString}, ROW_NUMBER() OVER(ORDER BY {order}) as rnumber from [{tableName}] ORDER BY case when rnumber > {half}  then(rnumber - ({half}-0.5)) when rnumber <= {half} then rnumber end LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}"; //append ASC or DESC
                }
                else if(orderType == OrderType.Windows && order != string.Empty)
                {
                    command.CommandText = $"SELECT {IdColumnName} AS {IdColumnName},{headerString} FROM [{tableName}] ORDER BY {order} LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}";
                }
                else
                {
                    command.CommandText = $"SELECT {IdColumnName} AS {IdColumnName},{headerString} FROM [{tableName}] ORDER BY {SortOrderColumnName} LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}";
                }

                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(command);
                sqlda.Fill(dt);
                if (orderType == OrderType.Reverse)
                {
                    dt.Columns.Remove("rnumber");
                }
            }

            return dt;
        }

        private static List<string> GetSortedHeadersIncludeAsAlias(string tableName = "main")
        {
            List<string> headers = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column, alias from [{tableName + MetaTableAffix}] order by sortorder asc";
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    headers.Add($"[{reader.GetString(0)}] AS [{reader.GetString(1)}]");
                }
            }
            return headers;
        }

        private static DataTable GetSortedHeadersAsDataTable(out SQLiteDataAdapter adapter, string tableName = "main")
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

        private static void AddColumnIfNotExists(string tableName, string column, string defaultValue = "")
        {
            if(!ContainsAlias(tableName, column))
            {
                AddColumn(tableName, column, defaultValue);
            }
        }

        /// <summary>
        /// Get a Key-Value based Collection of alias and column
        /// Key: alias
        /// Value: columnName
        /// Note: This Dictionary is insertion order except you delete keys, then the "empty" space is filled by a new ".Add"
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static Dictionary<string, string> GetColumnAliasMapping(string tableName = "main")
        {
            Dictionary<string, string> headerMapping = new Dictionary<string, string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias, column from [{tableName + MetaTableAffix}] order by sortorder";
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    headerMapping.Add(reader.GetString(0), reader.GetString(1));
                }
            }
            return headerMapping;
        }

        internal static void ConcatTable(string newTable, string fileNameBefore, string filename, string destinationTable = "main")
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

            Dictionary<string,string> destinationHeaders = GetColumnAliasMapping(destinationTable);
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
            
            //SQLiteCommand command = new SQLiteCommand(Connection)
            //{
            //    CommandText = $"INSERT INTO {originalTable} SELECT * FROM {newTable}"
            //};
            //command.ExecuteNonQuery();

            Delete(newTable);
        }

        internal static int GetRowCount(string tableName = "main")
        {
            int rowCount = 0;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand()) {
                command.CommandText = $"SELECT count(*) from [{tableName}]";
                rowCount = Convert.ToInt32(command.ExecuteScalar());
            }
            return rowCount;
        }

        internal static void Delete(string tableName, bool ifExists = false)
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

        internal static void RenameColumns(string importTable, string destinationTable = "main")
        {
            List<string> headers = GetSortedColumnsAsAlias(importTable);
            DataTable table = GetSortedHeadersAsDataTable(out SQLiteDataAdapter adapter, destinationTable);
            for (int i = 0; i < table.Rows.Count && i < headers.Count; ++i)
            {
                table.Rows[i]["alias"] = headers[i];
            }
            adapter?.Update(table);
        }

        internal static void SetSavepoint()
        {
            if(UpdateHandler == null)
            {
                UpdateHandler = Update;
                Connection.Trace += UpdateHandler;
            }
            CreateSavePoint();
        }

        private static void Update(object sender, TraceEventArgs e)
        {
            if (!IgnoreCommands.Contains(GetCommandType(e.Statement)))
            {
                DatabaseHistory.Log(Pointer, ref SavePoints, e.Statement);
            }
        }

        private static string GetCommandType(string command)
        {
            int index = command.IndexOf(" ");
            return command.Substring(0, index == -1 ? command.Length -1 : index).ToUpper();
        }

        private static void CreateSavePoint()
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"SAVEPOINT \"{SavePoints}\"";
                command.ExecuteNonQuery();
            }
            ++SavePoints;
            ++Pointer;
            DatabaseHistory.CreateSavePoint(SavePoints);
        }

        internal static int PVMSplit(string path, Form1 invokeForm, int encoding, string invalidColumnAlias, string tableName = "main")
        {
            
            string invalidColumnName = GetColumnName(invalidColumnAlias ?? Properties.Settings.Default.InvalidColumnName, tableName);
            SplitAndSavePVM(tableName, invalidColumnName, path, path.AppendFileName(Properties.Settings.Default.FailAddressText), encoding, invokeForm, false);
            return SplitAndSavePVM(tableName, invalidColumnName, path, path.AppendFileName(Properties.Settings.Default.RightAddressText), encoding, invokeForm, true); //return count of rows
        }

        private static int SplitAndSavePVM(string tableName, string invalidColumnName, string originalFilePath, string fileName, int encoding, Form1 invokeForm, bool saveValidRows)
        {
            int rowCount = 0;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                Dictionary<string,string> columnAliasMapping = GetColumnAliasMapping(tableName);
                command.CommandText = $"SELECT {GetHeaderString(columnAliasMapping.Values)} from [{tableName}] where $column {(saveValidRows ? "!=" : "=")} $value limit {Properties.Settings.Default.MaxRows} offset $offset";
                command.Parameters.Add(new SQLiteParameter("$offset"));
                command.Parameters.Add(new SQLiteParameter("$column", invalidColumnName));
                command.Parameters.Add(new SQLiteParameter("$value", Properties.Settings.Default.FailAddressValue));


                //Create file with all headers (columnAliasMapping.Keys)
                rowCount = ExportHelper.Save(originalFilePath, fileName, encoding, Properties.Settings.Default.PVMSaveFormat, invokeForm, command, null, tableName);
            }
            return rowCount;
        }

        internal static SQLiteCommand GetDataCommand(string tableName)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            Dictionary<string, string> columnAliasMapping = GetColumnAliasMapping(tableName);
            command.CommandText = $"SELECT {GetHeaderString(columnAliasMapping.Values)} from [{tableName}] limit {Properties.Settings.Default.MaxRows} offset $offset";
            command.Parameters.Add(new SQLiteParameter("$offset"));

            return command;
        }

        internal static void DeleteInvalidRows(string tableName = "main")
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

        private static string GetColumnName(string alias, string tableName)
        {
            string columnName = string.Empty;
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] where alias = $alias";
                command.Parameters.Add(new SQLiteParameter("$alias", alias));
                columnName = command.ExecuteScalar().ToString();
            }
            return columnName;
        }

        private static List<string> GetColumnNames(string tableName)
        {
            List<string> columnNames = new List<string>();
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] order by sortorder";
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    columnNames.Add(reader.GetString(0));
                }
            }
            return columnNames;
        }

        internal static int[] GetMaxColumnLength(string tableName)
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

        internal static void Undo()
        {
            Pointer--;
            GoToSavePoint(Pointer);
        }

        internal static void Redo()
        {
            if (Pointer < SavePoints)
            {
                Connection.Trace -= UpdateHandler;
                

                DatabaseHistory.Redo(Pointer);
                Pointer++;

                Connection.Trace += UpdateHandler;
            }
        }

        internal static void ExecuteCommand(string sql, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        private static void GoToSavePoint(int savePoint)
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"ROLLBACK TO \"{savePoint}\"";
                command.ExecuteNonQuery();
            }
        }

        internal static void UpdateCell(string value, string alias, string id, string tableName = "main")
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string columnName = GetColumnName(alias, tableName);
                command.CommandText = $"UPDATE [{tableName}] set [{columnName}] = ? where rowid = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                command.Parameters.Add(new SQLiteParameter() { Value = id });
                command.ExecuteNonQuery();
            }
        }
    }
}
