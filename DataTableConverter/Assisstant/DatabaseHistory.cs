using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class DatabaseHistory
    {
        private readonly string HistoryPath;
        private SQLiteConnection Connection;
        private SQLiteTransaction Transaction;
        private readonly DatabaseHelper DatabaseHelper;

        internal DatabaseHistory(DatabaseHelper databaseHelper, string directory, string databaseName)
        {
            DatabaseHelper = databaseHelper;
            HistoryPath = Path.Combine(directory, databaseName + "_history.sqlite");
            CreateDatabase();
        }

        internal void CreateDatabase()
        {
            if (File.Exists(HistoryPath))
            {
                File.Delete(HistoryPath);
            }
            SQLiteConnection.CreateFile(HistoryPath);
            File.SetAttributes(HistoryPath, FileAttributes.Hidden);
            Connect();
        }

        private void Connect()
        {
            Connection = new SQLiteConnection($"Data Source={HistoryPath};Version=3;");
            Connection.Open();
            
            
            Transaction = Connection.BeginTransaction();
            Init();
        }

        private void Init()
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON"; //foreign keys are disabled by default in sqlite
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE history (spoint INTEGER PRIMARY KEY)";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE log (ID INTEGER PRIMARY KEY AUTOINCREMENT, spoint INTEGER, command VARCHAR(255), FOREIGN KEY(spoint) REFERENCES history(spoint) ON DELETE CASCADE)";
                command.ExecuteNonQuery();
            }
        }

        internal void Close()
        {
            Transaction.Dispose();
            Connection.Close();
            DeleteDatabase();
        }

        private void DeleteDatabase()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(HistoryPath);
        }

        internal void Reset()
        {
            try
            {
                Transaction.Rollback();
            }
            catch { }
        }

        private void DeleteSavePoint(int savePoint)
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM history where spoint = ?";
                command.Parameters.Add(new SQLiteParameter() { Value = savePoint });
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM log where spoint = ?";
                command.ExecuteNonQuery();
            }
        }

        internal void Log(int pointer, ref int savePoints, string cmd)
        {
            bool status = savePoints > pointer;
            while(savePoints > pointer)
            {
                DeleteSavePoint(savePoints);
                savePoints--;
            }

            if (status)
            {
                DeleteSavePoint(savePoints);
                CreateSavePoint(savePoints);
            }

            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"insert into log (spoint, command) values (?, ?)";
                command.Parameters.Add(new SQLiteParameter() { Value = savePoints });
                command.Parameters.Add(new SQLiteParameter() { Value = cmd });
                command.ExecuteNonQuery();
            }
        }

        internal void CreateSavePoint(int savePoint)
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"insert into history values ({savePoint})";
                command.ExecuteNonQuery();
            }
        }

        internal void Redo(int savepoint)
        {
            using(SQLiteCommand command = Connection.CreateCommand())
            {
                int offset = 0;
                command.CommandText = $"SELECT command from log where spoint = ? order by rowid LIMIT {Properties.Settings.Default.MaxRows} OFFSET ?";
                command.Parameters.Add(new SQLiteParameter() { Value = savepoint });
                command.Parameters.Add(new SQLiteParameter());

                bool running = true;
                while (running)
                {
                    command.Parameters[1].Value = offset;
                    SQLiteDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        int readRows = 0;
                        while (reader.Read())
                        {
                            DatabaseHelper.ExecuteCommand(reader.GetString(0), DatabaseHelper.DefaultTable);
                            ++offset;
                            ++readRows;
                        }
                        if(readRows < Properties.Settings.Default.MaxRows)
                        {
                            running = false;
                        }
                    }
                    else
                    {
                        running = false;
                    }
                }
            }
        }
    }
}
