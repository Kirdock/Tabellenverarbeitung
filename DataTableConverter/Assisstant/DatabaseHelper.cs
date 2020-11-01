using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
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
        private static int SavePoints = 0, Pointer = 0; //0 => after main Table with data is created
        private static SQLiteConnection Connection, TempConnection; //Second file and connection is needed because of Trace
        private static SQLiteTraceEventHandler UpdateHandler = null;
        private static SQLiteTransaction Transaction, TempTransaction;
        private static bool Lock = false;
        private static readonly HashSet<string> IgnoreCommands = new HashSet<string>()
        {
            "SELECT",
            "BEGIN",
            "ROLLBACK",
            "SAVEPOINT",
            "DROP"
        };
        //Problem with DataTables to edit data: if there is a change in a row, the update statement contains all columns of the row, not only the changed ones

        internal static void Init()
        {
            DatabaseHistory.CreateDatabase();
            CreateDatabase(DatabasePath);
            CreateDatabase(TempDatabasePath);
            ConnectMain();
            ConnectTemp();
        }

        internal static void CreateDatabase(string path)
        {
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
            TempTransaction = Connection.BeginTransaction();
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

        internal static List<string> HeadersOfTable(string tableName = "main")
        {
            List<string> headers = new List<string>();
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"SELECT alias FROM $table order by sortorder";
                command.Parameters.Add(new SQLiteParameter("$table", tableName + MetaTableAffix));
                var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    headers.Add(reader.GetString(0));
                }
            }
            return headers;
        }

        internal static void CreateTable(IEnumerable<string> headers)
        {
            string tableName = "main";
            CreateTable(headers, tableName, Connection);
        }

        internal static void CreateTable(IEnumerable<string> headers, string tableName)
        {
            CreateTable(headers, tableName, TempConnection);
        }

        private static void CreateTable(IEnumerable<string> headers, string tableName, SQLiteConnection connection)
        {

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"DROP table if exists $table";
                command.Parameters.Add(new SQLiteParameter("$table", tableName));
                command.ExecuteNonQuery();

                command.CommandText = $"CREATE table (ID INTEGER PRIMARY KEY AUTOINCREMENT, {string.Join(" varchar(255) not null default '',", headers)} varchar(255) not null default '')";
                command.ExecuteNonQuery();
            };
            

            CreateMetaData(tableName, headers); //what should I do with temp-databases. Do they have only one Meta-Table? (e.g. I only rename/adjust/concat first table)
        }

        private static void CreateMetaData(string tableName, IEnumerable<string> headers)
        {
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = "DROP table if exists $table";
                command.Parameters.Add(new SQLiteParameter("$table", tableName + MetaTableAffix));
                command.ExecuteNonQuery();

                command.CommandText = "CREATE table $table (ID INTEGER PRIMARY KEY AUTOINCREMENT, column varchar(255) not null default '', alias varchar(255) not null default '', sortorder INTEGER AUTOINCREMENT)";
                command.ExecuteNonQuery();

                command.CommandText = $"INSERT INTO $table (alias) values ($alias)";
                command.Parameters.Add(new SQLiteParameter("$alias"));

                foreach(string header in headers)
                {
                    command.Parameters[1].Value = header;
                    command.ExecuteNonQuery();
                }
            }
        }

        internal static void ReplaceTable(string newTable, string oldTable = "main")
        {
            DeleteMainDatabase();
            if (UpdateHandler != null)
            {
                Connection.Trace -= UpdateHandler;
            }
            
            RenameTable(newTable, oldTable);
            CommitTemp();
            RenameTempDatabase();
            Connection = TempConnection;
            Connection.Open();
            Reset();

            Transaction = Connection.BeginTransaction();
            CreateDatabase(TempDatabasePath);
            UpdateHandler = Update;
            Connection.Trace += UpdateHandler;

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
            DatabaseHistory.Reset();
        }

        private static void DeleteMainDatabase()
        {
            Connection.Close();
            File.Delete(DatabasePath);
        }

        private static void DeleteTempDatabase()
        {
            TempConnection.Close();
            File.Delete(TempDatabasePath);
        }

        private static void RenameTempDatabase()
        {
            TempConnection.Close();
            File.Move(TempDatabasePath, DatabasePath);
        }

        internal static void RenameTable(string from, string to)
        {
            SQLiteCommand command = new SQLiteCommand(Connection)
            {
                CommandText = $"ALTER TABLE $from $to"
            };
            command.Parameters.Add(new SQLiteParameter("$from", from));
            command.Parameters.Add(new SQLiteParameter("$to", to));
            command.ExecuteNonQuery();
        }

        internal static void CreateFromDataTable(string tableName, DataTable table)
        {
            Delete(tableName, true);
            string[] headers = table.HeadersOfDataTableAsString();
            CreateTable(headers, tableName);

            SQLiteCommand command = null;
            foreach(DataRow row in table.AsEnumerable())
            {
                command = InsertRow(headers, row.ItemArray, tableName, command);
            }

            table.Dispose();

        }

        internal static void InsertRow(Dictionary<string, string> row, string tableName)
        {
            string headerString = GetHeaderString(row.Keys);
            string valueString = GetValueString(row.Keys);
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"insert into $table ({headerString}) values ({valueString})";
                command.Parameters.Add(new SQLiteParameter("$table", tableName));
                foreach (string header in row.Keys)
                {
                    command.Parameters.Add(new SQLiteParameter($"${header}", row[header]));
                }
                command.ExecuteNonQuery();
            };
        }

        internal static SQLiteCommand InsertRow(IEnumerable<string> eHeaders, object[] values, string tableName, SQLiteCommand cmd = null)
        {
            string[] headers = eHeaders.ToArray();
            string headerString = GetHeaderString(headers);
            string valueString = GetValueString(headers);
            SQLiteCommand command = cmd;
            if (cmd == null)
            {
                command = new SQLiteCommand(Connection)
                {
                    CommandText = $"INSERT into $table ({headerString}) values ({valueString})"
                };
                command.Parameters.Add(new SQLiteParameter("$table", tableName));

                for (int i = 0; i < values.Length; i++)
                {
                    command.Parameters.Add(new SQLiteParameter($"${headers[i]}", values[i].ToString()));
                }
                command.ExecuteNonQuery();
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    command.Parameters[i+1].Value = values[i].ToString(); //+1 because tableName is 0
                }
                command.ExecuteNonQuery();
            }
            return cmd;
        }

        internal static bool ContainsColumn(string tableName, string column)
        {
            SQLiteConnection connection = GetConnection(tableName);
            bool status = false;
            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM $table WHERE alias = $column";
                command.Parameters.Add(new SQLiteParameter("$table", tableName + MetaTableAffix));
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
                command.CommandText = $"ALTER TABLE $table ADD COLUMN $column varchar(255) NOT NULL DEFAULT $defaultValue";
                command.Parameters.Add(new SQLiteParameter("$table", tableName));
                command.Parameters.Add(new SQLiteParameter("$column", column));
                command.Parameters.Add(new SQLiteParameter("$defaultValue", defaultValue));
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.CommandText = $"INSERT INTO $table (alias) values ($alias)";
                command.Parameters.Add(new SQLiteParameter("$table", tableName + MetaTableAffix));
                command.Parameters.Add(new SQLiteParameter("$alias", column));
                command.ExecuteNonQuery();
            }
        }

        private static string GetValueString(IEnumerable<string> headers)
        {
            return "$"+string.Join(",$", headers);
        }

        private static string GetHeaderString(IEnumerable<string> headers)
        {
            return string.Join(",", headers);
        }

        internal static DataTable GetData(string tableName = "main", string order = "", int offset = 0)
        {
            DataTable dt = new DataTable();
            string headerString = string.Join(",", GetSortedHeaders());
            //select explicit instead of everything because of column order
            //use alias
            //heading1 as Surname, ...

            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT id,{headerString}, FROM $table LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset} order by {order}";
                command.Parameters.Add(new SQLiteParameter("$table", tableName));
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(command);
                sqlda.Fill(dt);
                dt.Columns.Remove("rnumber");
            }

            return dt;
        }

        private static List<string> GetSortedHeaders(string tableName = "main")
        {
            List<string> headers = new List<string>();
            using(SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = "SELECT column, alias from $table order by sortorder asc";
                command.Parameters.Add(new SQLiteParameter("$table", tableName + MetaTableAffix));
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    headers.Add($"{reader.GetString(0)} AS {reader.GetString(1)}");
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
                command.CommandText = "SELECT * from $table order by sortorder asc";
                command.Parameters.Add(new SQLiteParameter("$table", tableName + MetaTableAffix));
                adapter = new SQLiteDataAdapter(command);
                adapter.Fill(table);
                SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
                adapter.UpdateCommand = builder.GetUpdateCommand();
            }
            return table;
        }

        private static void AddColumnIfNotExists(string tableName, string column, string defaultValue = "")
        {
            if(!ContainsColumn(tableName, column))
            {
                AddColumn(tableName, column, defaultValue);
            }
        }

        private static Dictionary<string, string> GetHeaderMapping(string tableName)
        {
            Dictionary<string, string> headerMapping = new Dictionary<string, string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = "SELECT alias, column from $table";
                command.Parameters.Add(new SQLiteParameter("$table", tableName + MetaTableAffix));
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    headerMapping.Add(reader.GetString(0), reader.GetString(1));
                }
            }
            return headerMapping;
        }

        internal static void ConcatTable(string originalTable, string newTable, string fileNameBefore, string filename)
        {
            AddColumnIfNotExists(originalTable, FileNameColumn, fileNameBefore);
            AddColumnIfNotExists(newTable, FileNameColumn, filename);
            List<string> headers = GetSortedHeaders(newTable);
            SQLiteConnection destinationConnection = GetConnection(originalTable);
            
            foreach (string header in headers)
            {
                AddColumnIfNotExists(originalTable, header);
            }

            Dictionary<string,string> destinationHeaders = GetHeaderMapping(originalTable);
            List<string> headerMapping = new List<string>();

            foreach (string header in headers)
            {
                headerMapping.Add(destinationHeaders[header]);
            }
            //map headers of newTable to original column name of originalTable

            string headerString = GetHeaderString(headerMapping);
            string valueString = GetValueString(headerMapping);

            SQLiteCommand destinationCommand = destinationConnection.CreateCommand();
            destinationCommand.CommandText = $"INSERT into $table ({headerString}) values ({valueString})";
            destinationCommand.Parameters.Add(new SQLiteParameter("$table", originalTable));
            foreach(string header in headerMapping)
            {
                destinationCommand.Parameters.Add(new SQLiteParameter($"${header}"));
            }

            using (SQLiteCommand command = GetConnection(newTable).CreateCommand())
            {
                command.CommandText = "SELECT count(*) fromm $table";
                command.Parameters.Add(new SQLiteParameter("$table", newTable));
                int rowCount = Convert.ToInt32(command.ExecuteScalar());
                int offset = 0;
                while (offset < rowCount)
                {
                    DataTable table = new DataTable();
                    command.CommandText = $"SELECT * from $table LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}";
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(table);

                    foreach (DataRow row in table.AsEnumerable())
                    {
                        for(int i = 0; i < table.Columns.Count; ++i)
                        {
                            destinationCommand.Parameters[i + 1].Value = row[i].ToString();
                        }
                        destinationCommand.ExecuteNonQuery();
                    }

                    offset += (int)Properties.Settings.Default.MaxRows;
                }
                destinationCommand.Dispose();
            }
            
            //SQLiteCommand command = new SQLiteCommand(Connection)
            //{
            //    CommandText = $"INSERT INTO {originalTable} SELECT * FROM {newTable}"
            //};
            //command.ExecuteNonQuery();

            Delete(newTable);
        }

        private static void Delete(string tableName, bool ifExists = false)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"DROP TABLE {(ifExists ? "if exists" : string.Empty)} {tableName}";
                command.ExecuteNonQuery();
            }
        }

        internal static void RenameColumns(string oldTable, string importTable)
        {
            List<string> headers = GetSortedHeaders(importTable);
            DataTable table = GetSortedHeadersAsDataTable(out SQLiteDataAdapter adapter, oldTable);
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
            //Additional lock during REDO
            if (!IgnoreCommands.Contains(GetCommandType(e.Statement)))
            {
                DatabaseHistory.Log(Pointer, ref SavePoints, e.Statement);
            }
        }

        private static string GetCommandType(string command)
        {
            return command.Substring(0, command.IndexOf(" "));
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

        internal static void Undo()
        {
            Pointer--;
            GoToSavePoint(Pointer);
        }

        private static void GoToSavePoint(int savePoint)
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"ROLLBACK TO \"{savePoint}\"";
                command.ExecuteNonQuery();
            }
        }

        internal static void Redo()
        {
            Lock = true;


            Lock = false;
        }
    }
}
