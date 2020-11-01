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
        private static readonly string DatabasePath = "MyDatabase.sqlite";
        private static readonly string FileNameColumn = "dateiname";
        internal static readonly string DefaultTable = "main";
        private static readonly string MetaTable = "meta";
        private static int SavePoints = 0, Pointer = 0; //0 => after main Table with data is created
        private static SQLiteConnection Connection, TempConnection; //Second file and connection is needed because of Trace
        private static SQLiteTraceEventHandler UpdateHandler = null;
        private static readonly HashSet<string> TempTables = new HashSet<string>(); //HashSet is not sorted
        private static SQLiteTransaction Transaction;
        private static readonly HashSet<string> IgnoreCommands = new HashSet<string>()
        {
            "SELECT",
            "BEGIN",
            "ROLLBACK",
            "SAVEPOINT"
        };
        //Problem with DataTables to edit data: if there is a change in a row, the update statement contains all columns of the row, not only the changed ones

        internal static void CreateDatabase()
        {
            SQLiteConnection.CreateFile(DatabasePath); //create a new Database on every start or a new main table is opened
            File.SetAttributes(DatabasePath, FileAttributes.Hidden);
            DatabaseHistory.CreateDatabase();
            Connect();
        }

        internal static void Connect()
        {
            Connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;");
            Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        internal static void Close()
        {
            //Transaction.Commit();
            Transaction.Dispose();
            Connection.Close();
            DatabaseHistory.Close();
        }

        internal static List<string> HeadersOfTable(string tableName = "main")
        {
            List<string> headers = new List<string>();
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"SELECT alias FROM {MetaTable} order by sortorder";
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
            TempTables.Add(tableName);
        }

        private static void CreateTable(IEnumerable<string> headers, string tableName, SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand(connection)
            {
                CommandText = $"drop table if exists {tableName}"
            };
            command.ExecuteNonQuery();

            command.CommandText = $"create table {tableName} (ID INTEGER PRIMARY KEY AUTOINCREMENT, {string.Join(" varchar(255) not null default '',", headers)} varchar(255) not null default '')";
            command.ExecuteNonQuery();

            CreateMetaData(tableName, headers, connection); //what should I do with temp-databases. Do they have only one Meta-Table? (e.g. I only rename/adjust/concat first table)
        }

        private static void CreateMetaData(string tableName, IEnumerable<string> headers, SQLiteConnection connection)
        {
            //tableName needed?
        }

        internal static void ReplaceTable(string newTable, string oldTable = "main")
        {
            //on replace: delete Main-Database file
            //              rename temp file to main
            //              rename main table to "main" table
            //              delete other tables that are not main table
            //              set savePoint
            //              Connection has to be closed and replaced and before committed

            Delete(oldTable);
            RenameTable(newTable, oldTable);
            DatabaseHistory.Reset();
        }

        internal static void RenameTable(string from, string to)
        {
            if (Tables.TryGetValue(from, out List<string> headers))
            {
                SQLiteCommand command = new SQLiteCommand(Connection)
                {
                    CommandText = $"ALTER TABLE $from $to"
                };
                command.Parameters.Add(new SQLiteParameter("$from", from));
                command.Parameters.Add(new SQLiteParameter("$to", to));
                command.ExecuteNonQuery();

                if (Tables.ContainsKey(to))
                {
                    Tables.Remove(to);
                }
                Tables.Add(to, headers);
                Tables.Remove(from);
            }
            else
            {
                throw new TableNotFoundException(from);
            }
        }

        internal static SQLiteCommand PrepareInsert(string tableName = "main")
        {
            if (Tables.TryGetValue(tableName, out List<string> headers))
            {
                string headerString = GetHeaderString(headers);
                string valueString = GetValueString(headers);
                SQLiteCommand command = new SQLiteCommand(Connection)
                {
                    CommandText = $"insert into {tableName} ({headerString}) values ({valueString})"
                };

                foreach (string header in headers)
                {
                    command.Parameters.Add(new SQLiteParameter($"${header}"));
                }
                return command;
            }
            else
            {
                throw new TableNotFoundException(tableName);
            }
        }

        internal static void CreateFromDataTable(string tableName, DataTable table)
        {
            Delete(tableName, true);
            SQLiteCommand cmd = Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM {tableName}";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
            adapter.Update(table);
            CreateMetaData(tableName, table.HeadersOfDataTableAsString(), TempConnection);
            Tables.Add(tableName, new List<string>(table.HeadersOfDataTableAsString()));
            table.Dispose();

        }

        internal static void InsertRow(Dictionary<string, string> row, string tableName)
        {
            string headerString = GetHeaderString(row.Keys);
            string valueString = GetValueString(row.Keys);
            SQLiteCommand command = new SQLiteCommand(Connection)
            {
                CommandText = $"insert into {tableName} ({headerString}) values ({valueString})"
            };

            foreach (string header in row.Keys)
            {
                command.Parameters.Add(new SQLiteParameter($"${header}", row[header]));
            }
            command.ExecuteNonQuery();
        }

        internal static void InsertRow(List<string> headers, string[] values, string tableName)
        {
            string headerString = GetHeaderString(headers);
            string valueString = GetValueString(headers);
            SQLiteCommand command = new SQLiteCommand(Connection)
            {
                CommandText = $"insert into {tableName} ({headerString}) values ({valueString})"
            };

            for(int i = 0; i < values.Length; i++)
            {
                command.Parameters.Add(new SQLiteParameter($"${headers[i]}", values[i]));
            }
            command.ExecuteNonQuery();
        }

        internal static bool ContainsColumn(string tableName, string column)
        {
            if(Tables.TryGetValue(tableName, out List<string> headers))
            {
                return headers.Contains(column);
            }
            else
            {
                throw new TableNotFoundException(tableName);
            }
        }

        internal static void AddColumn(string tableName, string column, string defaultValue = "")
        {
            if (Tables.TryGetValue(tableName, out List<string> headers))
            {
                SQLiteCommand command = new SQLiteCommand(Connection)
                {
                    CommandText = $"ALTER TABLE {tableName} ADD COLUMN $column varchar(255) NOT NULL DEFAULT $defaultValue"
                };
                command.Parameters.Add(new SQLiteParameter($"$column", column));
                command.Parameters.Add(new SQLiteParameter("$defaultValue", defaultValue));

                command.ExecuteNonQuery();
                headers.Add(column);
            }
            else
            {
                throw new TableNotFoundException(tableName);
            }
        }

        private static string GetValueString(IEnumerable<string> headers)
        {
            return "trim($" + string.Join("),trim($", headers) + ")"; //trim on insert
        }

        private static string GetHeaderString(IEnumerable<string> headers)
        {
            return string.Join(",", headers);
        }

        internal static DataTable GetData(string tableName = "main", string order = "", int offset = 0)
        {
            if (Tables.TryGetValue(tableName, out List<string> headers))
            {
                string headerString = string.Join(",", headers);
                //select explicit instead of everything because of column order
                string CommandText = $"SELECT id,{headerString} FROM main LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset} order by {order}";
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(CommandText, Connection);

                DataTable dt = new DataTable();
                sqlda.Fill(dt);
                return dt;
            }
            else
            {
                throw new TableNotFoundException(tableName);
            }
        }

        private static void AddColumnIfNotExists(string tableName, string column, string defaultValue = "")
        {
            if(!ContainsColumn(tableName, column))
            {
                AddColumn(tableName, column, defaultValue);
            }
        }

        internal static void ConcatTable(string originalTable, string newTable, string fileNameBefore, string filename)
        {
            AddColumnIfNotExists(originalTable, FileNameColumn, fileNameBefore);
            AddColumnIfNotExists(newTable, FileNameColumn, filename);
            if (Tables.TryGetValue(newTable, out List<string> headers))
            {
                foreach (string header in headers)
                {
                    AddColumnIfNotExists(originalTable, header);
                }
            }
            else
            {
                throw new TableNotFoundException(newTable);
            }
            SQLiteCommand command = new SQLiteCommand(Connection)
            {
                CommandText = $"INSERT INTO {originalTable} SELECT * FROM {newTable}" //should temp Tables be deleted? It should be the same. Either the temp table is saved or all insert statements in history
                                                                                        //NO IT'S NOT!! When I rollback, the temp table is deleted
            };
            command.ExecuteNonQuery();
            Delete(newTable);
        }

        private static void Delete(string tableName, bool ifExists = false)
        {
            SQLiteCommand command = new SQLiteCommand(Connection)
            {
                CommandText = $"DROP TABLE {(ifExists ? "if exists" : string.Empty)} {tableName}"
            };
            command.ExecuteNonQuery();
            if (Tables.ContainsKey(tableName))
            {
                Tables.Remove(tableName);
            }
        }

        internal static void RenameColumns(string oldTable, string importTable)
        {
            //we can rename it differently
            //we don't touch the column names
            //instead we have a meta-table
            //meta table contains: column_name (foreign_key and primary_key); alias (alias for this column); order


            //order of imported Headers is important. Can't use HashSet
            SQLiteCommand command = new SQLiteCommand(Connection)
            {
                CommandText = $"ALTER TABLE {oldTable} RENAME COLUMN $oldCol TO $column"
            };
            SQLiteParameter parameterOld = new SQLiteParameter("$oldColumn");
            SQLiteParameter parameterNew = new SQLiteParameter("$column");
            command.Parameters.Add(parameterOld);
            command.Parameters.Add(parameterNew);
            if (Tables.TryGetValue(oldTable, out List<string> originalColumns) && Tables.TryGetValue(importTable, out List<string> importColumns))
            {
                for(int i = 0; i < originalColumns.Count && i < importColumns.Count; ++i) //first rename to a unique identifier in order to tackle name duplications on rename
                {
                    parameterOld.Value = Guid.NewGuid().ToString();
                    parameterNew.Value = Guid.NewGuid().ToString();

                    command.ExecuteNonQuery();
                }

                for (int i = 0; i < originalColumns.Count && i < importColumns.Count; ++i)
                {
                    parameterOld.Value = originalColumns[i];
                    parameterNew.Value = importColumns[i];

                    command.ExecuteNonQuery();
                }
            }
            else
            {
                throw new TableNotFoundException(oldTable, importTable);
            }
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

        }
    }
}
