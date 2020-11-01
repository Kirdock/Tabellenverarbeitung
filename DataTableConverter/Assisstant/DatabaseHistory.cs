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
        private static readonly string HistoryPath = "History.sqlite";
        private static SQLiteConnection Connection;
        private static SQLiteTransaction Transaction;

        internal static void CreateDatabase()
        {
            SQLiteConnection.CreateFile(HistoryPath);
            File.SetAttributes(HistoryPath, FileAttributes.Hidden);
            Connect();
        }

        private static void Connect()
        {
            Connection = new SQLiteConnection($"Data Source={HistoryPath};Version=3;");
            Connection.Open();
            
            
            Transaction = Connection.BeginTransaction();
            Init();
        }

        private static void Init()
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

        internal static void Close()
        {
            Transaction.Dispose();
            Connection.Close();
        }

        internal static void Reset()
        {
            try
            {
                Transaction.Rollback();
            }
            catch { }
        }

        private static void DeleteSavePoint(int savePoint)
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"DELETE FROM history where spoint = {savePoint}";
                command.ExecuteNonQuery();
            }
        }

        internal static void Log(int pointer, ref int savePoints, string cmd)
        {
            while(pointer < savePoints)
            {
                DeleteSavePoint(savePoints);
                savePoints--;
            }
            
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"insert into log (spoint, command) values ($spoint, $command)";
                command.Parameters.Add(new SQLiteParameter("$point", savePoints));
                command.Parameters.Add(new SQLiteParameter("$command", cmd));
                command.ExecuteNonQuery();
            }

        }

        internal static void CreateSavePoint(int savePoints)
        {
            using (SQLiteCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"insert into history ({savePoints})";
                command.ExecuteNonQuery();
            }
        }
    }
}
