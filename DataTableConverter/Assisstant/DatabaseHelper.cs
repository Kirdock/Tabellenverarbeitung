using DataTableConverter.Assisstant.SQL_Functions;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class DatabaseHelper
    {
        private readonly string DatabaseDirectory = "";
        private readonly string TempDatabasePath;
        private readonly string DatabasePath;
        private readonly string FileNameColumn = "dateiname";
        internal readonly static string DefaultTable = "main";
        private readonly string MetaTableAffix = "_meta";
        internal readonly string IdColumnName = "rowid"; //could be a problem as primary key if VACUUM command is executed
        private readonly string IndexAffix = "_INDEX";
        private int SavePoints, Pointer; //0 => after main Table with data is created
        private SQLiteConnection Connection, TempConnection; //Second file and connection is needed because of Trace
        private SQLiteTraceEventHandler UpdateHandler = null;
        private SQLiteTransaction Transaction, TempTransaction;
        internal ExportHelper ExportHelper;
        private readonly DatabaseHistory DatabaseHistory;
        private readonly string isNewRowIdentifier = "1";

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

        internal DatabaseHelper(string databaseName, bool createMain)
        {
            databaseName = databaseName ?? "Database";

            DatabaseHistory = new DatabaseHistory(this, DatabaseDirectory, databaseName);

            DatabasePath = Path.Combine(DatabaseDirectory, databaseName + ".sqlite");
            TempDatabasePath = Path.Combine(DatabaseDirectory, databaseName + "_temp.sqlite");
            Init(createMain);
        }

#if DEBUG
        internal void Commit()
        {
            Transaction.Commit();
        }
#endif

        private void Init(bool createMainDatabase)
        {
            Reset();
            SQLiteFunction.RegisterFunction(typeof(SQLiteSensitive)); // COLLATE CASESENSITIVE
            SQLiteFunction.RegisterFunction(typeof(SQLiteComparator)); //COLLATE NATURALSORT
            SQLiteFunction.RegisterFunction(typeof(SQLiteNoCase)); //COLLATE NO_CASE, fixes NOCASE of database (special characters not working)
            SQLiteFunction.RegisterFunction(typeof(NumberToString)); //TOSTRING(myValue, myFormat)
            SQLiteFunction.RegisterFunction(typeof(Round)); //ROUND2(myValue, type, decimalCount)
            SQLiteFunction.RegisterFunction(typeof(CountString)); //COUNTSTRING(myValue, mySubstring)
            SQLiteFunction.RegisterFunction(typeof(GetSplit)); //GETSPLIT(myValue, mySubstring, myIndex)
            SQLiteFunction.RegisterFunction(typeof(CustomUppercase)); //CUSTOMUPPERCASE(myValue, myOption)
            SQLiteFunction.RegisterFunction(typeof(CustomTrim)); //CUSTOMTRIM(myValue, myCharacters, isDeleteDouble, trimType)
            SQLiteFunction.RegisterFunction(typeof(CustomSubstring)); //CUSTOMSUBSTRING(myValue, replaceText, start, end, isReplace, isReverse)
            SQLiteFunction.RegisterFunction(typeof(SQL_Functions.Padding)); //PADDING(value, type, count, character)
            SQLiteFunction.RegisterFunction(typeof(Divide)); //DIVIDE(value, divisor)
            SQLiteFunction.RegisterFunction(typeof(ThousandSeparator)); //THOUSAND_SEPARATOR(value)

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
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < sourceColumns.Length; ++i)
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
                        builder.Append("[").Append(destinationColumns[i]).Append("] = CASE WHEN ");
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
            foreach (DataRow row in table.AsEnumerable())
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

        internal int CompareColumnsCount(string columnName1, string columnName2, string tableName)
        {
            int count = 0;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
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
        internal List<string> GetSortedColumnsAsAlias(string tableName)
        {
            List<string> aliases = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias from [{tableName + MetaTableAffix}] where alias is not null order by sortorder asc";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        aliases.Add(reader.GetValue(0).ToString());
                    }
                }
            }
            return aliases;
        }

        private List<string> GetSortedColumns(string tableName)
        {
            List<string> columns = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] where alias is not null order by sortorder asc";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(reader.GetValue(0).ToString());
                    }
                }
            }
            return columns;
        }

        /// <summary>
        /// Creates table and meta table
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="tableName"></param>
        internal void CreateTable(IEnumerable<string> columnNames, string tableName, bool naturalSort = true)
        {
            CreateTable(columnNames, tableName, TempConnection, naturalSort);
        }

        /// <summary>
        /// Creates table and meta table
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        private void CreateTable(IEnumerable<string> columnNames, string tableName, SQLiteConnection connection, bool naturalSort = true)
        {

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"DROP table if exists [{tableName}]";
                command.ExecuteNonQuery();

                string colType = "varchar(255) not null default '' COLLATE "+ (naturalSort ? "NATURALSORT" : "NO_CASE");
                command.CommandText = $"CREATE table [{tableName}] ({IdColumnName} INTEGER PRIMARY KEY AUTOINCREMENT, [{string.Join($"] {colType},[", columnNames)}] {colType})";

                command.ExecuteNonQuery();
            };


            CreateMetaData(tableName, columnNames, connection);
        }

        private void CreateMetaData(string tableName, IEnumerable<string> columnNames, SQLiteConnection connection = null)
        {
            using (SQLiteCommand command = (connection ?? GetConnection(tableName)).CreateCommand())
            {
                string metaTable = tableName + MetaTableAffix;
                command.CommandText = $"DROP table if exists [{metaTable}]";
                command.ExecuteNonQuery();

                command.CommandText = $"CREATE table [{metaTable}] (column varchar(255) not null default '' COLLATE NATURALSORT, alias varchar(255) COLLATE NATURALSORT, sortorder INTEGER primary key AUTOINCREMENT)";
                command.ExecuteNonQuery();

                command.CommandText = $"INSERT INTO [{metaTable}] (column, alias) values ($column, $alias)";
                command.Parameters.Add(new SQLiteParameter("$column"));
                command.Parameters.Add(new SQLiteParameter("$alias"));

                Func<string, string> operation = ImportOperation();

                foreach (string columnName in columnNames)
                {
                    command.Parameters[0].Value = columnName;
                    command.Parameters[1].Value = operation(columnName);
                    command.ExecuteNonQuery();
                }
            }
        }

        internal static Func<string, string> ImportOperation()
        {
            return Properties.Settings.Default.ImportHeaderUpperCase ? (Func<string, string>)(value => value.Replace('-', '\t').ToUpper().Replace('\t', '-') /*exception for this character*/) : value => value;
        }

        internal void ReplaceTable(string newTable, string oldTable)
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
            try
            {
                TempTransaction.Commit();
            }
            catch { }
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
            try
            {
                File.Delete(TempDatabasePath);
            }
            catch { }
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

        internal string InsertRow(string tableName, Dictionary<string, string> row = null)
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

        internal SQLiteCommand InsertRow(Dictionary<string, string> row, string tableName, SQLiteCommand cmd = null)
        {
            SQLiteConnection connection = GetConnection(tableName);
            SQLiteCommand command = cmd;
            if (cmd != null && cmd.Parameters.Count != row.Count)
            {
                command.Dispose();
                command = null;
            }
            if (command == null)
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
                foreach (KeyValuePair<string, string> pair in row)
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
        internal void InsertRowAt(string id, string tableName)
        {
            //from id till end, increase id by 1
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] SET [{IdColumnName}] = - ([{IdColumnName}] + 1) WHERE [{IdColumnName}] >= ?;" //everything greater and equal than the given id is increased by one (it is set to negative to prevent duplicate IDs)
                                    + $"UPDATE [{tableName}] SET [{IdColumnName}] = - [{IdColumnName}] WHERE [{IdColumnName}] < 0"; //After everything is set set negative to positive
                command.Parameters.Add(new SQLiteParameter() { Value = id });

                command.ExecuteNonQuery();
            }
            InsertRow(new Dictionary<string, string>() { { IdColumnName, id } }, tableName);
        }

        internal SQLiteCommand InsertDuplicateCommand(string[] headers, string tableName)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            string headerString = GetHeaderString(headers);
            command.CommandText = $"INSERT into [{tableName}] ({headerString}) values ({GetValueString(headers.Length)})";

            for (int i = 0; i < 2; i++)
            {
                command.Parameters.Add(new SQLiteParameter());
            }
            return command;
        }

        internal SQLiteCommand InsertRow(IEnumerable<string> eHeaders, object[] values, string tableName, SQLiteCommand command = null)
        {
            string[] headers = eHeaders.ToArray();
            if (headers.Length > values.Length)
            {
                headers = headers.Take(values.Length).ToArray();
                command = null;
            }
            if(command?.Parameters.Count != values.Length)
            {
                command = null;
            }

            if (command == null)
            {
                string headerString = GetHeaderString(headers);
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
            return command;
        }

        internal SQLiteCommand InsertRow(IEnumerable<string> eHeaders, SQLiteDataReader reader, string tableName, SQLiteCommand command = null, SQLiteConnection connection = null)
        {
            if (command == null)
            {
                command = CreateInsertRowCommand(eHeaders, tableName, reader, connection);
            }
            else
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    command.Parameters[i].Value = reader.GetValue(i).ToString();
                }
            }
            command.ExecuteNonQuery();
            return command;
        }

        private SQLiteCommand CreateInsertRowCommand(IEnumerable<string> eHeaders, string tableName, SQLiteDataReader reader, SQLiteConnection connection = null)
        {
            string[] headers = eHeaders.ToArray();
            string headerString = GetHeaderString(headers);

            SQLiteCommand command = (connection ?? GetConnection(tableName)).CreateCommand();
            command.CommandText = $"INSERT into [{tableName}] ({headerString}) values ({GetValueString(headers.Length)})";

            for (int i = 0; i < reader.FieldCount; i++)
            {
                command.Parameters.Add(new SQLiteParameter() { Value = reader.GetValue(i).ToString() });
            }
            return command;
        }

        internal bool ContainsAlias(string tableName, string column)
        {
            bool status = false;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM [{tableName + MetaTableAffix}] WHERE alias = $column COLLATE NO_CASE";
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

        internal string AddColumn(string tableName, string column, string defaultValue = "")
        {
            string columnName = GetValidColumnName(column, tableName);
            AddColumnWithAlias(columnName, column, tableName, defaultValue);
            return columnName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="tableName"></param>
        /// <returns>(adjusted) alias</returns>
        internal string RenameAlias(string from, string to, string tableName)
        {
            string newAlias = to;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                int counter = 1;
                while (ContainsAlias(tableName, newAlias))
                {
                    newAlias = to + counter;
                    counter++;
                }
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] set alias = ? where alias = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = newAlias });
                command.Parameters.Add(new SQLiteParameter() { Value = from });
                command.ExecuteNonQuery();
            }
            return newAlias;
        }

        //it is not possible to remove a column from a table
        //but with my structure we just need to delete the entry in the meta-table.
        //Then future SELECT statemetns don't contain the column
        internal void DeleteColumnThroughAlias(string alias, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET alias = null where alias = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = alias });
                command.ExecuteNonQuery();
            }
        }

        internal void DeleteColumn(string columnName, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET alias = null where column = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = columnName });
                command.ExecuteNonQuery();
            }
        }

        internal string AddColumnAtStart(string alias, string tableName)
        {
            string columnName = AddColumnFixedAlias(alias, tableName);

            MoveColumnToIndex(0, columnName, tableName);

            return columnName;
        }

        internal void MoveColumnToIndex(string aliasName, string columnName, string tableName)
        {
            MoveColumnToIndex(GetAliasIndex(aliasName, tableName), columnName, tableName);
        }

        internal void MoveColumnToIndex(int index, string columnName, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                int rangeStart;
                int rangeStop;
                int add;
                index++; //ID begins at 1

                //get old sortorder
                command.CommandText = $"SELECT sortorder from [{tableName + MetaTableAffix}] WHERE column = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = columnName });
                int oldIndex = Convert.ToInt32(command.ExecuteScalar());
                command.Parameters.Clear();

                if (oldIndex < index)
                {
                    rangeStart = oldIndex;
                    rangeStop = index;
                    add = -1;
                }
                else
                {
                    rangeStart = index;
                    rangeStop = oldIndex;
                    add = 1;
                }

                //multiply everything that is affected by -1 (needed because of constraint)
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = -(sortorder) where sortorder between ? and ?";
                command.Parameters.Add(new SQLiteParameter() { Value = rangeStart });
                command.Parameters.Add(new SQLiteParameter() { Value = rangeStop });
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                //move the column to the right index
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = ? where column = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = index });
                command.Parameters.Add(new SQLiteParameter() { Value = columnName });
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                //add or substract 1 and revert the multiplication of -1
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = (-sortorder) + ? where sortorder between ? and ?";
                command.Parameters.Add(new SQLiteParameter() { Value = add });
                command.Parameters.Add(new SQLiteParameter() { Value = -rangeStop });
                command.Parameters.Add(new SQLiteParameter() { Value = -rangeStart });
                command.ExecuteNonQuery();
            }
        }

        internal int GetAliasIndex(string aliasName, string tableName)
        {
            int index = 0;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT sortorder from [{tableName + MetaTableAffix}] WHERE alias = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = aliasName });
                index = Convert.ToInt32(command.ExecuteScalar());
            }
            return index;
        }

        internal void SwitchColumnIndex(string columnName1, string columnName2, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                //get old sortorder
                command.CommandText = $"SELECT sortorder from [{tableName + MetaTableAffix}] WHERE column = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = columnName1 });
                int index1 = Convert.ToInt32(command.ExecuteScalar());

                command.Parameters[0].Value = columnName2;
                int index2 = Convert.ToInt32(command.ExecuteScalar());
                command.Parameters.Clear();

                //multiply everything that is affected by -1 (needed because of constraint)
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = -(sortorder) where sortorder = ? or sortorder = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = index1 });
                command.Parameters.Add(new SQLiteParameter() { Value = index2 });
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                //move the column to the right index
                command.CommandText = $"UPDATE [{tableName + MetaTableAffix}] SET sortorder = ? where sortorder = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = index2 });
                command.Parameters.Add(new SQLiteParameter() { Value = -index1 });
                command.ExecuteNonQuery();

                command.Parameters[0].Value = index1;
                command.Parameters[1].Value = -index2;
                command.ExecuteNonQuery();
            }
        }

        internal void ApplyOrder(string columnName, string tableName)
        {
            using (SQLiteCommand command = GetDataCommandWithOrder(tableName, GenerateOrderAsc(columnName), OrderType.Windows))
            {
                Dictionary<long, int> updates = new Dictionary<long, int>();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    for (int i = 1; reader.Read(); ++i)
                    {
                        long val = -reader.GetInt64(0);
                        updates.Add(val, i);
                    }
                }
                command.Parameters.Clear();

                command.CommandText = $"UPDATE [{tableName}] SET [{IdColumnName}] = -([{IdColumnName}])";
                command.ExecuteNonQuery();

                command.CommandText = $"UPDATE [{tableName}] SET [{IdColumnName}] = ? where [{IdColumnName}] = ?";
                command.Parameters.Add(new SQLiteParameter());
                command.Parameters.Add(new SQLiteParameter());

                foreach (KeyValuePair<long, int> pair in updates)
                {
                    command.Parameters[1].Value = pair.Key;
                    command.Parameters[0].Value = pair.Value;
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddColumnWithAlias(string columnName, string alias, string tableName, string defaultValue)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"ALTER TABLE [{tableName}] ADD COLUMN [{columnName}] varchar(255) NOT NULL DEFAULT [{defaultValue}] COLLATE NATURALSORT";
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.CommandText = $"INSERT INTO [{tableName + MetaTableAffix}] (column, alias) values ($column, $alias)";
                command.Parameters.Add(new SQLiteParameter("$column", columnName));
                command.Parameters.Add(new SQLiteParameter("$alias", ImportOperation()(alias)));
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
        internal void AddColumnsWithAdditionalIfExists(IEnumerable<string> columnsNames, string defaultValue, out string[] newColumnNames, string tableName)
        {
            Dictionary<string, string> destinationTableColumnAliasMapping = GetAliasColumnMapping(tableName);
            newColumnNames = new string[columnsNames.Count()];
            int index = 0;
            foreach (string columnName in columnsNames)
            {
                newColumnNames[index] = AddColumnWithAdditionalIfExists(columnName, tableName, defaultValue, destinationTableColumnAliasMapping);
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
        internal string AddColumnWithAdditionalIfExists(string columnName, string tableName, string defaultValue = "", Dictionary<string, string> destinationTableColumnAliasMapping = null)
        {
            destinationTableColumnAliasMapping = destinationTableColumnAliasMapping ?? GetAliasColumnMapping(tableName);
            string newAlias = columnName;
            int counter;

            for (counter = 1; destinationTableColumnAliasMapping.Keys.Any(alias => alias.Equals(newAlias, StringComparison.OrdinalIgnoreCase)); counter++)
            {
                newAlias = columnName + counter;
            }
            return AddColumnFixedAlias(newAlias, tableName, defaultValue);
        }

        /// <summary>
        /// Add column to table. If column name is already taken, a counter is increased but alias is not changed
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="tableName"></param>
        /// <param name="defaultValue"></param>
        /// /// <returns>Column name of created column</returns>
        internal string AddColumnFixedAlias(string alias, string tableName, string defaultValue = "")
        {
            string columnName = GetValidColumnName(alias, tableName);
            AddColumnWithAlias(columnName, alias, tableName, defaultValue);
            return columnName;
        }

        private string GetValidColumnName(string alias, string tableName)
        {
            List<string> columnNames = GetColumnNames(tableName, true);
            string columnName = alias;
            for (int counter = 1; columnNames.Any(col => col.Equals(columnName, StringComparison.OrdinalIgnoreCase)); ++counter)
            {
                columnName = alias + counter;
            }
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
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                try
                {
                    command.CommandText = $"CREATE {(unique ? "UNIQUE" : string.Empty)} index if not exists [{columnName + IndexAffix + tableName}] on [{tableName}]([{columnName}])";
                    command.ExecuteNonQuery();
                }
                catch
                {
                    DialogResult result = MessageHandler.MessagesYesNo(invokeForm, MessageBoxIcon.Warning, "Die angegebene identifizierende Spalte der geladenen Tabelle enthält Duplikate.\nTrotzdem fortfahren?");
                    abort = result != DialogResult.Yes;
                }
            }
            return abort;
        }

        private void DeleteIndexOn(string tableName, string columnName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"DROP Index if exists [{columnName + IndexAffix + tableName}]";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Add columns of import table depending on a given identifier on destination and import table. Only rows that are defined in the imported table are kept
        /// </summary>
        /// <param name="importTable"></param>
        /// <param name="importColumnNames"></param>
        /// <param name="destinationIdentifierColumn"></param>
        /// <param name="importIdentifierColumn"></param>
        /// <param name="destinationTableColumnAliasMapping"></param>
        /// <param name="importTableColumnAliasMapping"></param>
        /// <param name="destinationTable"></param>
        internal bool PVMImport(string importTable, string[] importColumnNames, string destinationIdentifierColumn, string importIdentifierColumn, string destinationTable, Form1 invokeForm, out string sortOrderColumn, out string isNewRowColumn)
        {
            bool abort;
            isNewRowColumn = string.Empty;
            if (!(abort = CreateIndexOn(destinationTable, destinationIdentifierColumn, invokeForm)))
            {
                AddColumnsWithAdditionalIfExists(importColumnNames, string.Empty, out string[] destinationColumnNames, destinationTable);
                sortOrderColumn = AddColumnWithAdditionalIfExists("Importiersortierung", destinationTable);
                isNewRowColumn = AddColumnWithAdditionalIfExists("$$isNewRow", destinationTable);
                //int rowCount = GetRowCount(importTable);
                int sortOrder = 0;
                int offset = 2;
                using (SQLiteCommand destinationCommand = GetConnection(destinationTable).CreateCommand())
                {
                    destinationCommand.CommandText = $"UPDATE [{destinationTable}] SET [{string.Join("]=?,[", new string[] { sortOrderColumn, isNewRowColumn }.Concat(destinationColumnNames))}] =? where [{destinationIdentifierColumn}] = ?";

                    for (int i = 0; i < destinationColumnNames.Length + 3; i++) //+3 because of sortOrder and where statement
                    {
                        destinationCommand.Parameters.Add(new SQLiteParameter());
                    }
                    destinationCommand.Parameters[1].Value = isNewRowIdentifier;

                    using (SQLiteCommand command = GetConnection(importTable).CreateCommand())
                    {
                        command.CommandText = $"SELECT {GetHeaderString(importColumnNames.Concat(new string[] { importIdentifierColumn }))} from [{importTable}]";

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                destinationCommand.Parameters[0].Value = sortOrder;
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    destinationCommand.Parameters[i + offset].Value = reader.GetValue(i).ToString();
                                }
                                destinationCommand.ExecuteNonQuery();
                                sortOrder++;
                            }
                        }
                    }
                }
            }
            else
            {
                sortOrderColumn = null;
            }
            Delete(importTable);
            //remove index on destinationTable? (destinationColumn + IndexAffix)
            return abort;
        }

        private string GetValueString(int count)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < count - 1; ++i)
            {
                builder.Append("?,");
            }
            return builder.Append('?').ToString();
        }

        private string GetHeaderString(IEnumerable<string> headers)
        {

            return "[" + string.Join("],[", headers) + "]";
        }

        private string GetHeaderStringWithAlias(IEnumerable<KeyValuePair<string, string>> columnAliasMapping)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in columnAliasMapping)
            {
                builder.Append('[').Append(pair.Value).Append("] AS [").Append(pair.Key).Append("],");
            }
            return builder.Remove(builder.Length - 1, 1).ToString();
        }

        internal DataTable GetData(string tableName, int offset = 0)
        {
            DataTable dt = new DataTable();
            List<string> headers = GetSortedHeadersIncludeAsAlias(tableName);
            if (headers.Count != 0)
            {
                string headerString = string.Join(",", headers);

                using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
                {
                    command.CommandText = $"SELECT {headerString} FROM [{tableName}] LIMIT {Properties.Settings.Default.MaxRows} OFFSET {offset}";
                    SQLiteDataAdapter sqlda = new SQLiteDataAdapter(command);
                    sqlda.Fill(dt);
                }
            }
            return dt;
        }

        internal DataTable GetData(string order, OrderType orderType, int offset, string tableName)
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
                if (orderType == OrderType.Reverse && order != string.Empty)
                {
                    dt.Columns.Remove("rnumber");
                }
            }

            return dt;
        }

        private string GetSortedSelectString(string headerString, string order, OrderType orderType, int limit, int offset, bool includeId, string tableName, string whereStatement = "", string idAlias = null)
        {
            StringBuilder selectString = new StringBuilder("SELECT ");
            if (includeId)
            {
                selectString.Append($"{IdColumnName} AS [{idAlias ?? IdColumnName}] {(headerString == string.Empty ? "" : ",")}");
            }

            if (orderType == OrderType.Reverse && order != string.Empty)
            {
                int rowCount = GetRowCount(tableName);
                if (rowCount % 2 == 1)
                {
                    Match match = new Regex(@"\[(.*?)\]").Matches(order).Cast<Match>().FirstOrDefault();
                    string column = match?.Value.Substring(1, match.Value.Length - 2);
                    Dictionary<string, string> row = string.IsNullOrEmpty(column) ? null : new Dictionary<string, string>() { { column, 0.ToString() } };
                    InsertRow(tableName, row);
                    rowCount++;
                }
                int half = rowCount / 2;
                selectString.Append($"{headerString}{(headerString == string.Empty && !includeId ? "" : ",")} ROW_NUMBER() OVER(ORDER BY {order}) as rnumber from [{tableName}] {whereStatement} ORDER BY case when rnumber > {half}  then(rnumber - {half}-0.5) when rnumber <= {half} then rnumber end, rnumber COLLATE NATURALSORT"); //append ASC or DESC
            }
            else if (orderType == OrderType.Windows && order != string.Empty)
            {
                selectString.Append($"{headerString} FROM [{tableName}] {whereStatement} ORDER BY {order}");
            }
            else
            {
                selectString.Append($"{headerString} FROM [{tableName}] {whereStatement} ORDER BY {IdColumnName}");
            }
            if (limit != -1)
            {
                selectString.Append($" LIMIT {limit} OFFSET {offset}");
            }
            return selectString.ToString();
        }

        private string GetSelectString(string headerString, bool includeId, string tableName)
        {
            return $"SELECT {(includeId ? $"{IdColumnName}," : string.Empty)}{headerString} FROM [{tableName}]";
        }

        private List<string> GetSortedHeadersIncludeAsAlias(string tableName)
        {
            List<string> headers = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column, alias from [{tableName + MetaTableAffix}] where alias is not null order by sortorder asc";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        headers.Add($"[{reader.GetValue(0).ToString()}] AS [{reader.GetValue(1).ToString()}]");
                    }
                }
            }
            return headers;
        }

        private bool AddColumnIfNotExists(string tableName, ref string column, string defaultValue = "")
        {
            bool status;
            if (!(status = ContainsAlias(tableName, column)))
            {
                column = AddColumn(tableName, column, defaultValue);
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
        internal Dictionary<string, string> GetAliasColumnMapping(string tableName)
        {
            Dictionary<string, string> headerMapping = new Dictionary<string, string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias, column from [{tableName + MetaTableAffix}] where alias is not null order by sortorder";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        headerMapping.Add(reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    }
                }
            }
            return headerMapping;
        }

        internal void ConcatTable(string destinationTable, string newTable, string fileNameBefore, string filename)
        {
            string fileNameColumn = FileNameColumn;
            AddColumnIfNotExists(destinationTable, ref fileNameColumn, fileNameBefore);
            AddColumnIfNotExists(newTable, ref fileNameColumn, filename);
            string[] headersAliases = GetSortedColumnsAsAlias(newTable).ToArray();
            List<string> headersColumns = GetSortedColumns(newTable);
            SQLiteConnection destinationConnection = GetConnection(destinationTable);

            for (int i = 0; i < headersAliases.Length; ++i)
            {
                AddColumnIfNotExists(destinationTable, ref headersAliases[i]);
            }

            Dictionary<string, string> destinationHeaders = GetAliasColumnMapping(destinationTable);
            List<string> headerMapping = new List<string>();

            foreach (string header in headersAliases)
            {
                string value = destinationHeaders.First(pair => pair.Key.Equals(header, StringComparison.OrdinalIgnoreCase)).Value; //should be case insensitive
                headerMapping.Add(value);
            }
            //map headers of newTable to original column name of originalTable

            using (SQLiteCommand destinationCommand = destinationConnection.CreateCommand())
            {
                string newTableHeaderString = GetHeaderString(headersColumns);
                string headerString = GetHeaderString(headerMapping);
                destinationCommand.CommandText = $"INSERT into [{destinationTable}] ({headerString}) values ({GetValueString(headerMapping.Count)})";
                for (int i = 0; i < headerMapping.Count; ++i)
                {
                    destinationCommand.Parameters.Add(new SQLiteParameter());
                }

                using (SQLiteCommand command = GetConnection(newTable).CreateCommand())
                {
                    command.CommandText = $"SELECT {newTableHeaderString} from [{newTable}]";
                    SQLiteDataReader reader = command.ExecuteReader();

                    while(reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            destinationCommand.Parameters[i].Value = reader.GetValue(i).ToString();
                        }
                        destinationCommand.ExecuteNonQuery();
                    }
                }
            }
            MoveColumnToIndex(GetColumnCountWithNull(destinationTable) - 1, fileNameColumn, destinationTable);
            Delete(newTable);
        }

        private int GetColumnCountWithNull(string tableName)
        {
            int columnCount = 0;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT count(*) from [{tableName + MetaTableAffix}]";
                columnCount = Convert.ToInt32(command.ExecuteScalar());
            }
            return columnCount;
        }

        internal int GetRowCount(string tableName)
        {
            int rowCount = 0;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
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

        /// <summary>
        /// Renames the columns to the ones specified in the importTable. Additional 
        /// </summary>
        /// <param name="importTable"></param>
        /// <param name="destinationTable"></param>
        internal void RenameColumns(string importTable, string destinationTable)
        {
            string[] headers = GetSortedColumnsAsAlias(importTable).ToArray();
            Dictionary<long, string> updates = new Dictionary<long, string>();
            int index;
            using (SQLiteCommand command = GetConnection(destinationTable).CreateCommand())
            {
                command.CommandText = $"SELECT [{IdColumnName}] from [{destinationTable + MetaTableAffix}] where alias is not null order by sortorder";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    for (index = 0; reader.Read() && index < headers.Length; ++index)
                    {
                        updates.Add(reader.GetInt64(0), headers[index]);
                    }
                }
            }

            for (; index < headers.Length; ++index)
            {
                AddColumn(destinationTable, headers[index]);
            }
            SQLiteCommand updateCommand = null;
            foreach (KeyValuePair<long, string> pair in updates)
            {
                updateCommand = UpdateCell(pair.Value, "alias", pair.Key, destinationTable + MetaTableAffix, true, updateCommand, GetConnection(destinationTable));
            }
        }

        internal void SetSavepoint(bool ignoreMax = false)
        {
            if (UpdateHandler == null)
            {
                UpdateHandler = Update;
                Connection.Trace += UpdateHandler;
            }
            CreateSavePoint(ignoreMax);
        }

        private void CreateSavePoint(bool ignoreMax)
        {
            int savepoint;
            ++Pointer;
            if (!ignoreMax)
            {
                ++SavePoints;
                savepoint = SavePoints;
            }
            else
            {
                savepoint = Pointer;
            }

            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"SAVEPOINT \"{savepoint}\"";
                command.ExecuteNonQuery();
            }
            if (!ignoreMax)
            {
                DatabaseHistory.CreateSavePoint(SavePoints);
            }
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
            return command.Substring(0, index == -1 ? command.Length - 1 : index).ToUpper();
        }

        internal int PVMSplit(string sourceFilePath, Form1 invokeForm, int encoding, string invalidColumnName, string tableName, string isNewRowColumn, string orderColumnName)
        {
            string directory = Path.GetDirectoryName(sourceFilePath);
            string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

            SplitAndSavePVM(tableName, invalidColumnName, directory, fileName + Properties.Settings.Default.FailAddressText, encoding, invokeForm, false, isNewRowColumn, orderColumnName);
            return SplitAndSavePVM(tableName, invalidColumnName, directory, fileName + Properties.Settings.Default.RightAddressText, encoding, invokeForm, true, isNewRowColumn, orderColumnName); //return count of rows
        }

        private int SplitAndSavePVM(string tableName, string invalidColumnName, string directory, string fileName, int encoding, Form1 invokeForm, bool saveValidRows, string isNewRowColumn, string orderColumnName)
        {
            int rowCount = 0;
            HashSet<string> ignoreColumns = new HashSet<string>();
            ignoreColumns.Add(orderColumnName);
            ignoreColumns.Add(isNewRowColumn);
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {

                IEnumerable<KeyValuePair<string, string>> aliasColumnMapping = GetAliasColumnMapping(tableName).Where(pair => !ignoreColumns.Contains(pair.Value));
                command.CommandText = $"SELECT {GetHeaderStringWithAlias(aliasColumnMapping)} from [{tableName}] where [{invalidColumnName}] {(saveValidRows ? "!=" : "=")} ? AND [{isNewRowColumn}]=? ORDER BY [{orderColumnName}] ASC";
                command.Parameters.Add(new SQLiteParameter() { Value = Properties.Settings.Default.FailAddressValue });
                command.Parameters.Add(new SQLiteParameter() { Value = isNewRowIdentifier });
                rowCount = ExportHelper.Save(directory, fileName, null, encoding, (SaveFormat)Properties.Settings.Default.PVMSaveFormat, string.Empty, OrderType.Windows, invokeForm, tableName, command);
            }
            return rowCount;
        }

        /// <summary>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="idAlias"></param>
        /// <returns>Everything of a table if orderAlias is not null, else everything without the id</returns>
        internal SQLiteCommand GetDataCommand(string tableName, string idAlias = null)
        {
            Dictionary<string, string> columnAliasMapping = GetAliasColumnMapping(tableName);
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            command.CommandText = $"SELECT "
                                  + (idAlias != null ? $"[{IdColumnName}] AS [{idAlias}]," : string.Empty)
                                  + $"{GetHeaderStringWithAlias(columnAliasMapping)} from [{tableName}]";
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
            command.CommandText = $"SELECT [{IdColumnName}], {GetHeaderString(GetColumnNames(aliases, tableName))} from [{tableName}]";
            return command;
        }

        /// <summary>
        /// returns all rows and columns (AS ALIAS NAME) of a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="order"></param>
        /// <param name="orderType"></param>
        /// <param name="orderAlias"></param>
        /// <returns></returns>
        internal SQLiteCommand GetDataCommand(string tableName, string order, OrderType orderType)
        {
            Dictionary<string, string> columnAliasMapping = GetAliasColumnMapping(tableName);
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            command.CommandText = GetSortedSelectString(GetHeaderStringWithAlias(columnAliasMapping), order, orderType, -1, -1, false, tableName, string.Empty);
            return command;
        }

        internal SQLiteCommand GetDataCommandWithOrder(string tableName, string order, OrderType orderType)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, -1, true, tableName, string.Empty, IdColumnName);
            return command;
        }

        internal void DeleteInvalidRows(string tableName, string invalidAliasName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string columnName = GetColumnName(invalidAliasName, tableName);
                command.CommandText = $"DELETE FROM [{tableName}] where [{columnName}] = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = Properties.Settings.Default.FailAddressValue });
                command.ExecuteNonQuery();
            }
        }

        internal List<string> GetAliases(string tableName, bool includeDeleted = false)
        {
            List<string> columnNames = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias from [{tableName + MetaTableAffix}] {(includeDeleted ? string.Empty : "where alias is not null")} order by sortorder";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnNames.Add(reader.GetValue(0).ToString());
                    }
                }
            }
            return columnNames;
        }

        internal string GetAliasName(string columnName, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT alias from [{tableName + MetaTableAffix}] where column = $column COLLATE NO_CASE";
                command.Parameters.Add(new SQLiteParameter("$column", columnName));
                return command.ExecuteScalar()?.ToString();
            }
        }

        internal string GetColumnName(string alias, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] where alias = $alias COLLATE NO_CASE";
                command.Parameters.Add(new SQLiteParameter("$alias", alias));
                return command.ExecuteScalar()?.ToString();
            }
        }

        internal string[] GetColumnNames(string[] aliases, string tableName)
        {
            string[] columnNames = new string[aliases.Length];
            for (int i = 0; i < aliases.Length; ++i)
            {
                columnNames[i] = GetColumnName(aliases[i], tableName);
            }
            return columnNames;
        }

        internal List<string> GetColumnNames(string tableName, bool includeDeleted = false)
        {
            List<string> columnNames = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT column from [{tableName + MetaTableAffix}] {(includeDeleted ? string.Empty : "where alias is not null")} order by sortorder";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnNames.Add(reader.GetValue(0).ToString());
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
            string oldColumnName = GetColumnName(alias, tableName);
            alias = GetAliasName(oldColumnName, tableName); //real alias name (right case)

            string columnName = AddColumnWithAdditionalIfExists(alias + Properties.Settings.Default.OldAffix, tableName);

            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] SET [{columnName}] = [{oldColumnName}]";
                command.ExecuteNonQuery();
            }

            return oldColumnName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aliases"></param>
        /// <param name="tableName"></param>
        /// <returns>Colum names of new columns</returns>
        internal string[] CopyColumns(string[] aliases, string tableName)
        {
            for (int i = 0; i < aliases.Length; ++i)
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
                if (!inserted)
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
        internal void AddColumnsWithDialog(string alias, string[] originalAlias, Form invokeForm, string tableName, out string[] columnNames)
        {
            bool result = true;
            columnNames = new string[originalAlias.Length];
            for (int i = 0; i < originalAlias.Length; ++i)
            {
                result &= AddColumnWithDialog(originalAlias.Length == 1 ? alias : $"{originalAlias[i]}_{alias}", invokeForm, tableName, out string columnName);
                columnNames[i] = columnName;
            }
            if (!result)
            {
                columnNames = null;
            }
        }

        internal void InsertDataPerColumnValue(string columnName, OrderType orderType, int limit, string sourceTable, string destinationTable, Form1 invokeForm)
        {
            CreateIndexOn(sourceTable, columnName, unique: false); // create index on column to speed up the following queries
            List<string> groupColumn = GroupColumn(columnName, orderType, sourceTable);
            invokeForm?.StartLoadingBarCount(groupColumn.Count);

            using (SQLiteCommand command = GetConnection(sourceTable).CreateCommand())
            {
                using (SQLiteCommand insertCommand = GetConnection(destinationTable).CreateCommand())
                {
                    List<string> columnNames = GetColumnNames(sourceTable);
                    string headerString = GetHeaderString(columnNames);

                    insertCommand.CommandText = $"INSERT INTO [{destinationTable}] ({headerString}) values ({GetValueString(columnNames.Count)})";
                    for (int i = 0; i < columnNames.Count; ++i)
                    {
                        insertCommand.Parameters.Add(new SQLiteParameter());
                    }
                    command.CommandText = GetSortedSelectString(headerString, GenerateOrderAsc(columnName), orderType, limit, 0, false, sourceTable, $"where [{columnName}] = ?");
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
                                    insertCommand.Parameters[i].Value = reader.GetValue(i).ToString();
                                }
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                        invokeForm?.UpdateLoadingBar();
                    }
                }
            }

        }

        internal static string GenerateOrderAsc(string columnName)
        {
            return $"[{columnName}] asc";
        }

        internal List<string> GroupColumn(string columnName, OrderType orderType, string tableName)
        {
            List<string> list = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSortedSelectString($"[{columnName}]", GenerateOrderAsc(columnName), orderType, -1, 0, false, tableName, $"group by [{columnName}] COLLATE CASESENSITIVE");
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader.GetValue(0).ToString());
                    }
                }
            }
            return list;
        }

        internal Dictionary<string, long> GroupCountOfColumn(string columnName, string tableName)
        {
            Dictionary<string, long> dict = new Dictionary<string, long>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT [{columnName}], count(*) from [{tableName}] group by [{columnName}] COLLATE CASESENSITIVE order by [{columnName}]";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dict.Add(reader.GetValue(0).ToString(), reader.GetInt64(1));
                    }
                }
            }
            return dict;
        }

        internal void SplitTableOnRowValue(Dictionary<string, string[]> dict, string column, string tableName)
        {
            Dictionary<string, string> aliasColumnMapping = GetAliasColumnMapping(tableName);
            string headerString = GetHeaderString(aliasColumnMapping.Keys);
            string valueString = GetValueString(aliasColumnMapping.Count +1);
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                CreateIndexOn(tableName, column, null, false);
                command.CommandText = $"SELECT {IdColumnName},{headerString} from [{tableName}] where [{column}] = ? COLLATE CASESENSITIVE";
                // ID Column is used to keep original order
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
                            insertCommand.CommandText = $"INSERT INTO [{pair.Value[0]}] ({IdColumnName},{headerString}) values ({valueString})";
                            for (int i = 0; i < aliasColumnMapping.Count+1; i++)
                            {
                                insertCommand.Parameters.Add(new SQLiteParameter());
                            }
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    insertCommand.Parameters[i].Value = reader.GetValue(i).ToString();
                                }
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        internal int[] GetMaxColumnLength(string[] aliases, string tableName, bool minOne = true)
        {
            string[] columnNames = GetColumnNames(aliases, tableName);
            int[] max = new int[columnNames.Length];
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                for (int i = 0; i < columnNames.Length; i++)
                {
                    command.CommandText = $"SELECT max(length([{columnNames[i]}])) from [{tableName}]";
                    int length = Convert.ToInt32(command.ExecuteScalar());
                    max[i] = minOne && length == 0 ? 1 : length;
                }
            }
            return max;
        }

        internal void DictionaryToTable(Dictionary<string, long> dict, string columnName, bool showFromTo, string tableName, int sourceRowCount, Form1 invokeForm)
        {
            SQLiteCommand command = null;
            invokeForm?.StartLoadingBarCount(dict.Count);
            if (showFromTo)
            {
                string[] columns = new string[] { columnName, "Anzahl", "Von", "Bis" };
                CreateTable(columns, tableName);

                long count = 1;
                foreach (KeyValuePair<string, long> item in dict)
                {
                    long newCount = count + item.Value;
                    command = InsertRow(columns, new object[] { item.Key, item.Value.ToString(), count.ToString(), (newCount - 1).ToString() }, tableName, command);
                    invokeForm?.UpdateLoadingBar();
                    count = newCount;
                }
            }
            else
            {
                string[] columns = new string[] { columnName, "Anzahl" };
                CreateTable(columns, tableName);
                foreach (KeyValuePair<string, long> item in dict)
                {
                    command = InsertRow(columns, new object[] { item.Key, item.Value.ToString() }, tableName, command);
                    invokeForm?.UpdateLoadingBar();
                }
            }
            InsertRow(new string[] { columnName, "Anzahl" }, new object[] { "Gesamt", sourceRowCount.ToString() }, tableName, command);
        }

        internal SQLiteCommand DeleteRow(long id, string tableName, SQLiteCommand cmd = null)
        {
            SQLiteCommand command;
            if (cmd != null)
            {
                command = cmd;
                command.Parameters[0].Value = id;
            }
            else
            {
                command = GetConnection(tableName).CreateCommand();
                command.CommandText = $"DELETE from [{tableName}] where [{IdColumnName}] = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = id });
            }
            command.ExecuteNonQuery();
            return command;
        }

        internal void DeleteRowsByIndex(int[] ind, string order, OrderType orderType, string tableName)
        {
            HashSet<int> indizes = new HashSet<int>(ind);
            List<long> deleteRows = new List<long>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, -1, true, tableName);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int index = 0;
                    while (reader.Read() && indizes.Count != 0)
                    {
                        if (indizes.Contains(index))
                        {
                            deleteRows.Add(reader.GetInt64(0));
                            indizes.Remove(index);
                        }
                        index++;
                    }
                }
            }
            SQLiteCommand deleteCommand = null;
            foreach (long id in deleteRows)
            {
                deleteCommand = DeleteRow(id, tableName, deleteCommand);
            }
        }

        internal bool Undo()
        {
            bool status;
            if (status = Pointer > 0)
            {
                Pointer--;
                GoToSavePoint(Pointer);
            }
            return status;
        }

        internal bool Redo()
        {
            bool status;
            if (status = Pointer < SavePoints)
            {
                Connection.Trace -= UpdateHandler;

                DatabaseHistory.Redo(Pointer);
                SetSavepoint(true);

                Connection.Trace += UpdateHandler;
            }
            return status;
        }

        internal void ExecuteCommand(string sql, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
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

        /// <summary>
        /// Updates all row values on the given column
        /// </summary>
        /// <param name="updates"></param>
        /// <param name="column"></param>
        /// <param name="tableName"></param>
        internal void UpdateCells(IEnumerable<KeyValuePair<long, string>> updates, string column, string tableName)
        {
            if (updates.Count() != 0)
            {
                using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
                {
                    command.CommandText = $"UPDATE [{tableName}] set [{column}] = ? where [{IdColumnName}] = ?";
                    command.Parameters.Add(new SQLiteParameter());
                    command.Parameters.Add(new SQLiteParameter());
                    foreach (KeyValuePair<long, string> pair in updates)
                    {
                        command.Parameters[1].Value = pair.Key;
                        command.Parameters[0].Value = pair.Value;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        internal SQLiteCommand UpdateCell(string value, string alias, long id, string tableName, bool aliasIsColumnName = false, SQLiteCommand cmd = null, SQLiteConnection connection = null)
        {
            SQLiteCommand command;
            if (cmd == null)
            {
                command = (connection ?? GetConnection(tableName)).CreateCommand();
                string columnName = aliasIsColumnName ? alias : GetColumnName(alias, tableName);
                command.CommandText = $"UPDATE [{tableName}] set [{columnName}] = ? where [{IdColumnName}] = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                command.Parameters.Add(new SQLiteParameter() { Value = id });
            }
            else
            {
                command = cmd;
                command.Parameters[0].Value = value;
                command.Parameters[1].Value = id;
            }
            command.ExecuteNonQuery();
            return command;
        }

        internal void SetColumnValues(string alias, string newValue, string tableName)
        {
            string columnName = GetColumnName(alias, tableName);
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] set [{columnName}] = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = newValue });
                command.ExecuteNonQuery();
            }
        }

        internal void SetCheckSum(string columnName, Action updateLoadingBar, Form invokeForm, string tableName)
        {
            Dictionary<long, string> updates = new Dictionary<long, string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT [{IdColumnName}], [{columnName}] from [{tableName}]";

                bool askAgain = true;
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int rowCount = 0;
                    while (reader.Read())
                    {
                        rowCount++;
                        string value = reader.GetValue(1).ToString();
                        int checkSum = ChecksumEAN9(value);
                        if (checkSum == -1 && askAgain)
                        {
                            DialogResult result = MessageHandler.MessagesYesNo(invokeForm, MessageBoxIcon.Warning, $"Ungültige Zahl in Zeile {rowCount}. Trotzdem fortfahren?");
                            if (result == DialogResult.No)
                            {
                                break;
                            }
                            else
                            {
                                askAgain = false;
                            }
                        }
                        else if (checkSum != -1)
                        {
                            updates.Add(reader.GetInt64(0), value + checkSum);
                        }
                        updateLoadingBar();
                    }
                }
            }
            UpdateCells(updates, columnName, tableName);
        }

        /// <summary>
        /// Before a new main instance with a table is opened, copy the table into it's own file
        /// </summary>
        /// <param name="tableName"></param>
        internal void CopyToNewDatabaseFile(string tableName)
        {
            string path = Path.Combine(DatabaseDirectory, tableName + ".sqlite");
            CreateDatabase(path);

            SQLiteConnection connection = new SQLiteConnection($"Data Source={path};Version=3;");
            connection.Open();
            SQLiteTransaction transaction = connection.BeginTransaction();
            IEnumerable<string> columns = GetAliasColumnMapping(tableName).Keys;
            CreateTable(columns, DefaultTable, connection);
            using (SQLiteCommand command = GetDataCommand(tableName))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    SQLiteCommand insertCommand = null;
                    while (reader.Read())
                    {
                        insertCommand = InsertRow(columns, reader, DefaultTable, insertCommand, connection);
                    }
                }
            }
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
        internal void MergeRows(string columnName, List<PlusListboxItem> additionalColumns, bool separator, Form1 invokeForm, Action updateLoadingBar, string tableName)
        {
            //additionalColumns[...].Value is the columnName
            Dictionary<string, string> aliasColumnMapping = GetAliasColumnMapping(tableName);
            string separatorText = separator ? "N2" : "F2";
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture("de-DE");
            string[] sumColumns = additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Sum).Select(item => item.Value).ToArray();
            string[] countColumns = additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Count).Select(item => item.Value).ToArray();
            string[] appendArray = additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Nothing).Select(item => item.Value).ToArray();
            string newTableName = Guid.NewGuid().ToString();
            string[] tempColumns = new string[] { "id", "isAlias", "columns", "values" };
            CreateTable(tempColumns, newTableName);
            SQLiteCommand insertCommand = null;
            bool hasAppend = appendArray.Length != 0;
            List<long> deleteRows = new List<long>();
            List<string> newColumns = new List<string>();
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string headerString = GetHeaderString(new string[] { columnName }.Concat(appendArray).Concat(sumColumns).Concat(countColumns));
                int offset = 2; //ofset till the values of "headerString"
                int sumOffset = offset + appendArray.Length;
                int countOffset = sumOffset + sumColumns.Length;
                bool containsSumColumns = sumColumns.Length != 0;
                bool containsCountColumns = countColumns.Length != 0;
                command.CommandText = GetSortedSelectString(headerString, GenerateOrderAsc(columnName), OrderType.Windows, -1, -1, true, tableName);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        #region Init
                        Dictionary<string, string> newRowValues = new Dictionary<string, string>();
                        long id = reader.GetInt64(0);
                        string nameBefore = reader.GetValue(1).ToString();
                        int counter = 1;
                        decimal[] sumResults = new decimal[sumColumns.Length];
                        int[] countResults = new int[countColumns.Length];

                        InitSumResults(sumResults, sumColumns.Length, sumOffset, reader);
                        InitCountResults(countResults, countColumns.Length, countOffset, reader);
                        #endregion
                        
                        while (reader.Read())
                        {
                            string name = reader.GetValue(1).ToString();
                            if (name != nameBefore)
                            {
                                InsertValues();

                                #region InitNew
                                id = reader.GetInt64(0); //newId
                                InitSumResults(sumResults, sumColumns.Length, sumOffset, reader);
                                InitCountResults(countResults, countColumns.Length, countOffset, reader);
                                newRowValues.Clear();
                                counter = 1;
                                #endregion
                            }
                            else
                            {
                                if (hasAppend)
                                {
                                    for (int i = 0; i < appendArray.Length; ++i)
                                    {
                                        string value = reader.GetValue(i + offset).ToString();
                                        if (!string.IsNullOrWhiteSpace(value))
                                        {
                                            string newAlias = appendArray[i] + counter;

                                            if (!aliasColumnMapping.Keys.Any(key => key.Equals(newAlias, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                newColumns.Add(newAlias);
                                                aliasColumnMapping.Add(newAlias, newAlias);
                                            }

                                            newRowValues.Add(newAlias, value);
                                        }
                                    }

                                    counter++;
                                }
                                for (int i = 0; i < sumColumns.Length; ++i)
                                {
                                    if (decimal.TryParse(reader.GetValue(i + sumOffset).ToString(), out decimal result))
                                    {
                                        sumResults[i] += result;
                                    }

                                }
                                for (int i = 0; i < countColumns.Length; ++i)
                                {
                                    if (!string.IsNullOrWhiteSpace(reader.GetValue(i + countOffset).ToString()))
                                    {
                                        countResults[i]++;
                                    }

                                }
                                deleteRows.Add(reader.GetInt64(0));
                            }

                            nameBefore = name;
                        }

                        InsertValues();

                        void InsertValues()
                        {
                            //save values of before
                            //update where [{IdColumnName}] = ?   (id)
                            //Combine values with \t
                            if (newRowValues.Count != 0)
                            {
                                insertCommand = InsertRow(tempColumns, new string[] { id.ToString(), "1", string.Join("\t", newRowValues.Keys), string.Join("\t", newRowValues.Values) }, newTableName, insertCommand);

                            }
                            if (containsSumColumns || containsCountColumns)
                            {
                                //sumResults and count
                                insertCommand = InsertRow(tempColumns, new string[] { id.ToString(), "0", string.Join("\t", sumColumns.Concat(countColumns)), string.Join("\t", sumResults.Select(value => value.ToString(separatorText, culture)).Concat(countResults.Select(value => value.ToString()))) }, newTableName, insertCommand);
                            }
                        }
                    }
                }
            }
            SQLiteCommand deleteCommand = null;
            foreach (long id in deleteRows)
            {
                updateLoadingBar?.Invoke();
                deleteCommand = DeleteRow(id, tableName, deleteCommand);
            }

            foreach (string alias in newColumns)
            {
                AddColumnFixedAlias(alias, tableName);
            }

            aliasColumnMapping = GetAliasColumnMapping(tableName);
            SQLiteCommand updateCommand = null;
            using (SQLiteDataReader reader = GetDataCommand(newTableName).ExecuteReader())
            {
                while (reader.Read())
                {
                    updateLoadingBar?.Invoke();
                    //0 = id of destination table
                    //1 = type (isAlias, 0 or 1) if name is alias or column
                    //2 = column/alias names
                    //3 = values
                    int id = int.Parse(reader.GetValue(0).ToString());
                    bool isAlias = reader.GetValue(1).ToString() == "1";
                    string[] names = reader.GetValue(2).ToString().Split('\t');
                    string[] columnNames = isAlias ? names.Select(name => aliasColumnMapping.First(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value).ToArray() : names;
                    UpdateRow(id, columnNames, reader.GetValue(3).ToString().Split('\t'), tableName);
                }
            }
            Delete(newTableName);
        }

        private void InitSumResults(decimal[] sumResults, int count, int offset, SQLiteDataReader reader)
        {
            for (int i = 0; i < count; ++i)
            {
                decimal.TryParse(reader.GetValue(i + offset).ToString(), out decimal result);
                sumResults[i] = result;
            }
        }

        private void InitCountResults(int[] countResults, int count, int offset, SQLiteDataReader reader)
        {
            for (int i = 0; i < count; ++i)
            {
                countResults[i] = string.IsNullOrWhiteSpace(reader.GetValue(i + offset).ToString()) ? 0 : 1;
            }
        }

        private SQLiteCommand UpdateRow(int id, string[] columnNames, string[] columnValues, string tableName, SQLiteCommand command = null)
        {

            if (command == null)
            {
                command = GetConnection(tableName).CreateCommand();
                command.CommandText = $"UPDATE [{tableName}] SET {GetUpdateCommandText(columnNames)} WHERE [{IdColumnName}] = ?";
                foreach (string value in columnValues)
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
            for (int i = 0; i < columnNames.Length - 1; ++i)
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
            if (long.TryParse(data, out long _))
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

        internal long GetRowWithMaxCharacters(string order, OrderType orderType, out int index, string tableName)
        {
            long id = 0;
            index = 0;

            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT [{IdColumnName}], max({GetLengthCalcString(tableName)}) from [{tableName}]";
                string result = command.ExecuteScalar()?.ToString();

                if (result != null)
                {
                    id = long.Parse(result);
                    command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, 0, true, tableName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        for (; reader.Read() && reader.GetInt64(0) != id; ++index) { }
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
        internal long SearchValue(string value, string alias, bool strictMatch, string order, OrderType orderType, string tableName)
        {
            long index = -1;
            //strictMatch == true: column = value NO_CASE
            //strictMatch == false: column like %value% //nocase not needed; default of like is case insensitive
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, 0, true, tableName, $"where [{GetColumnName(alias, tableName)}] {(strictMatch ? "= ? COLLATE NO_CASE" : "like ?")}");
                command.Parameters.Add(new SQLiteParameter() { Value = strictMatch ? value : $"%{value}%" });
                string id = command.ExecuteScalar()?.ToString();
                if (id != null)
                {
                    index = 0;
                    long parseId = long.Parse(id);
                    command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, 0, true, tableName);
                    command.Parameters.Clear();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        for (; reader.Read() && reader.GetInt64(0) != parseId; ++index) { }
                    }
                }
            }
            return index;
        }

        private string GetLengthCalcString(string tableName)
        {
            List<string> columnNames = GetColumnNames(tableName);
            return "length([" + string.Join("])+length([", columnNames) + "])";
        }

        internal void UpdateRowsWithMinCharacters(string columnName, int minLength, string value, string destinationColumn, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] set [{destinationColumn}] = ? where length([{columnName}]) >= ?";
                command.Parameters.Add(new SQLiteParameter() { Value = value });
                command.Parameters.Add(new SQLiteParameter() { Value = minLength });
                command.ExecuteNonQuery();
            }
        }

        internal void DeleteRowByMatch(string value, string columnName, bool strictMatch, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"DELETE from [{tableName}] where [{columnName}] {(strictMatch ? "=? COLLATE NO_CASE" : "like ?")}";
                command.Parameters.Add(new SQLiteParameter() { Value = strictMatch ? value : $"%{value}%" });
                command.ExecuteNonQuery();
            }
        }

        internal void EmptyColumnByCondition(string sourceColumn, string destinationColumn, string compareAlias, string tableName)
        {
            string compareColumn = GetColumnName(compareAlias, tableName);
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] SET [{destinationColumn}] = CASE WHEN [{sourceColumn}] = [{compareColumn}] THEN '' ELSE [{sourceColumn}] END";
                command.ExecuteNonQuery();
            }
        }

        private int GetMaxOccurenciesOfString(string columnName, string subString, string tableName)
        {
            int max = 0;
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"SELECT max(t1.sum) from (SELECT COUNTSTRING([{columnName}], ?) as sum from [{tableName}]) t1";
                command.Parameters.Add(new SQLiteParameter() { Value = subString });
                max = int.Parse(command.ExecuteScalar().ToString());
            }
            return max;
        }

        internal void SplitColumnByString(string sourceAlias, string newColumn, string splitText, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string sourceColumn = GetColumnName(sourceAlias, tableName);
                int max = GetMaxOccurenciesOfString(sourceColumn, splitText, tableName);
                if (max != 0)
                {
                    StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] set ");
                    for (int i = 0; i <= max; i++)
                    {
                        string createdColumn = AddColumnWithAdditionalIfExists($"{newColumn}{i + 1}", tableName);
                        builder.Append($"[{createdColumn}] = GETSPLIT([{sourceColumn}], $splitText,{i}),");
                    }
                    command.CommandText = builder.Remove(builder.Length - 1, 1).ToString();
                    command.Parameters.AddWithValue("$splitText", splitText);
                    command.ExecuteNonQuery();
                }
            }
        }

        internal void Enumerate(string destinationColumn, int start, int end, bool repeat, string order, OrderType orderType, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = GetSortedSelectString(string.Empty, order, orderType, -1, -1, true, tableName);

                List<KeyValuePair<long, string>> updates = new List<KeyValuePair<long, string>>();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int count = start;
                    bool noEnd = end != 0;
                    while (reader.Read())
                    {
                        updates.Add(new KeyValuePair<long, string>(reader.GetInt64(0), count.ToString()));

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

        internal void ReplaceColumnValues(DataRow[] distinctDataTale, string tableName)
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

        internal void SetCustomUppercase(string[] aliases, int option, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                string[] columns = GetColumnNames(aliases, tableName);
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] set ");
                foreach (string column in columns)
                {
                    builder.Append($"[{column}] = CUSTOMUPPERCASE([{column}], {option}),");
                }
                command.CommandText = builder.Remove(builder.Length - 1, 1).ToString();
                command.ExecuteNonQuery();
            }
        }

        internal void Trim(string characters, string[] columnAlias, bool deleteDouble, ProcTrim.TrimType type, string tableName)
        {
            string[] columns = columnAlias == null ? GetColumnNames(tableName).ToArray() : GetColumnNames(columnAlias, tableName);
            using (SQLiteCommand commmand = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] set ");
                foreach (string column in columns)
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

        internal void SearchAndShortcut(string sourceColumn, string destinationColumn, bool totalSearch, string searchText, string shortcut, int from, int to, string order, OrderType orderType, string tableName)
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

                    List<KeyValuePair<long, string>> updates = new List<KeyValuePair<long, string>>();
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
                                    updates.Add(new KeyValuePair<long, string>(reader.GetInt64(0), counter.ToString()));
                                    counter++;
                                }
                            }
                            else if (search.Invoke(reader.GetValue(1).ToString(), searchText))
                            {
                                updates.Add(new KeyValuePair<long, string>(reader.GetInt64(0), counter.ToString()));
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
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] set [{destinationColumn}] = ? where [{sourceColumn}] {(totalSearch ? "= ?" : "like ?")}";
                command.Parameters.Add(new SQLiteParameter() { Value = shortcut });
                command.Parameters.Add(new SQLiteParameter() { Value = totalSearch ? searchText : $"%{searchText}%" });
                command.ExecuteNonQuery();
            }
        }

        internal void Substring(string[] sourceColumns, string[] destinationColumns, string replaceText, int start, int end, bool replaceChecked, bool reverseCheck, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] SET ");
                for (int i = 0; i < sourceColumns.Length; ++i)
                {
                    builder.Append($"[{destinationColumns[i]}] = CUSTOMSUBSTRING([{sourceColumns[i]}],$replaceText,$start,$end, $replaceChecked, $reverseCheck),");
                }
                command.CommandText = builder.Remove(builder.Length - 1, 1).ToString();
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

                List<KeyValuePair<long, string[]>> updates = new List<KeyValuePair<long, string[]>>();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string[] newValues = new string[sourceColumns.Length];

                        for (int i = 0; i < sourceColumns.Length; i++)
                        {
                            string index = destinationColumns[i];
                            string value = reader.GetValue(i + 1).ToString();
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
                        updates.Add(new KeyValuePair<long, string[]>(reader.GetInt64(0), newValues));
                    }
                }
                UpdateRows(updates, destinationColumns, tableName);
            }
        }

        private void UpdateRows(List<KeyValuePair<long, string[]>> updates, string[] destinationColumns, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder($"UPDATE [{tableName}] SET ");
                foreach (string column in destinationColumns)
                {
                    builder.Append($"[{column}]=?,");
                    command.Parameters.Add(new SQLiteParameter());
                }
                builder.Remove(builder.Length - 1, 1);
                builder.Append($" WHERE [{IdColumnName}]=?");
                command.Parameters.Add(new SQLiteParameter());
                command.CommandText = builder.ToString();
                foreach (KeyValuePair<long, string[]> update in updates)
                {
                    for (int i = 0; i < destinationColumns.Length; ++i)
                    {
                        command.Parameters[i].Value = update.Value[i];
                    }
                    command.Parameters[command.Parameters.Count - 1].Value = update.Key;
                    command.ExecuteNonQuery();
                }
            }
        }

        internal SQLiteCommand SplitTableOnColumnsCommand(string[] sourceColumns, string order, OrderType orderType, string tableName)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            command.CommandText = GetSortedSelectString(GetHeaderString(sourceColumns), order, orderType, -1, -1, false, tableName);
            return command;
        }

        internal SQLiteCommand ExistsValueInColumnCommand(string column, string tableName, string idColumn)
        {
            SQLiteCommand command = GetConnection(tableName).CreateCommand();
            command.CommandText = $"SELECT [{idColumn}] from [{tableName}] where [{column}] = ?";
            command.Parameters.Add(new SQLiteParameter());
            return command;
        }

        internal bool ExistsValueInColumn(string value, out int id, SQLiteCommand command)
        {
            id = 0;
            command.Parameters[0].Value = value;
            bool exists;
            object result = command.ExecuteScalar();
            if (exists = (result != null))
            {
                id = int.Parse(result.ToString());
            }
            return exists;
        }

        internal void InsertRowDuplicate(string id, string value, SQLiteCommand command)
        {
            command.Parameters[0].Value = id;
            command.Parameters[1].Value = value;
            command.ExecuteNonQuery();
        }

        internal void Divide(string[] sourceColumns, string[] destinationColumns, decimal divisor, bool alwaysShowTwoDecimals, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < sourceColumns.Length; ++i)
                {
                    builder.Append("[").Append(destinationColumns[i]).Append("]=DIVIDE([").Append(sourceColumns[i]).Append("],$divisor, $showDecimals), ");
                }
                builder.Remove(builder.Length - 2, 2);
                command.Parameters.AddWithValue("$divisor", divisor);
                command.Parameters.AddWithValue("$showDecimals", alwaysShowTwoDecimals);
                command.CommandText = $"UPDATE [{tableName}] SET {builder}";
                command.ExecuteNonQuery();
            }
        }

        internal void ThousandSeparator(string[] sourceColumns, string[] destinationColumns, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < sourceColumns.Length; ++i)
                {
                    builder.Append("[").Append(destinationColumns[i]).Append("]=THOUSAND_SEPARATOR([").Append(sourceColumns[i]).Append("]), ");
                }
                builder.Remove(builder.Length - 2, 2);
                command.CommandText = $"UPDATE [{tableName}] SET {builder}";
                command.ExecuteNonQuery();
            }
        }

        internal void ReplaceLeadingZero(string column, string text, string tableName)
        {
            using (SQLiteCommand command = GetConnection(tableName).CreateCommand())
            {
                command.CommandText = $"UPDATE [{tableName}] SET [{column}]=REPLACE_LEADING_ZERO([{column}], ?)";
                command.Parameters.Add(new SQLiteParameter { Value = text });
                command.ExecuteNonQuery();
            }
        }

        internal string CreateTableWithCommand(SQLiteCommand command)
        {
            string tablename = Guid.NewGuid().ToString();
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                List<string> columns = new List<string>();
                for(int i = 0; i < reader.FieldCount; ++i)
                {
                    columns.Add(reader.GetName(i));
                }
                CreateTable(columns, tablename);
                SQLiteCommand insertCommand = null;
                while (reader.Read())
                {
                    insertCommand = InsertRow(columns, reader, tablename, insertCommand);
                }
            }
            return tablename;
        }
    }
}
