using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace AppdateChecker
{
    public static class SQLHelper
    {
        private static int RetryCount = 5;
        public static string DbTableApp = "apps";
        public static string DbTableAppPath = "app_path";
        public static string DbColId = "Uid";
        public static string DbColName = "appName";
        public static string DbColRepoOwner = "repoOwner";
        public static string DbColRepoName = "repoName";
        public static string DbColCurVer = "currentVer";
        public static string DbColLatestVer = "latestVer";
        public static string DbColFilepath = "filepath";

        private static void Log(string log, string caller)
        {
            Logs.Log(Logs.FileDblog, $"SQLHelper-{caller}", log);
        }
        private static void Log(string log)
        {
            Log(log, "");
        }
        public static void Initiate()
        {
            var res = ExecNonQuery($"CREATE TABLE IF NOT EXISTS '{DbTableApp}' (" +
                "'Id'	INTEGER PRIMARY KEY AUTOINCREMENT, " +
                $"'{DbColId}'  INTEGER DEFAULT 0, " +
                $"'{DbColName}' TEXT, " +
                $"'{DbColRepoOwner}' TEXT, " +
                $"'{DbColRepoName}' TEXT, " +
                $"'{DbColCurVer}' TEXT, " +
                $"'{DbColLatestVer}' TEXT" +
                ");");
            ExecNonQuery($"CREATE TABLE IF NOT EXISTS '{DbTableAppPath}' (" +
                "`Id`	INTEGER PRIMARY KEY AUTOINCREMENT, " +
                $"'{DbColId}'	INTEGER DEFAULT 0, " +
                $"'{DbColFilepath}' TEXT" +
                ");");
        }
        public static SQLiteConnection OpenDb()
        {
            try
            {
                string PATH_START = AppContext.BaseDirectory;
                string DB_PATH = PATH_START + "AppdateCheckerApps.db";
                SQLiteConnection conn = new SQLiteConnection("URI=file:" + DB_PATH);
                conn.Open();
                return conn;
            }
            catch { return null; }
        }
        public static string GenerateId()
        {
            long Uid = 1;
            var dt = Query($"SELECT MAX({DbColId}) AS 'maxid' FROM {DbTableApp};", "DbGenerateId");
            if (dt?.Rows.Count > 0)
            {
                long.TryParse(dt.Rows[0]["maxid"].ToString(), out Uid);
            }
            dt.Dispose();
            Uid += 1;
            return Uid.ToString();
        }
        public static bool ExecNonQuery(string qry, string caller)
        {
            return (ExecNonQuery(qry, -1, caller) > 0);
        }
        public static int ExecNonQuery(string qry)
        {
            return ExecNonQuery(qry, -1);
        }
        public static int ExecNonQuery(string qry, int defaultValue, string caller = "")
        {
            int resultCode = defaultValue;
            int retry = RetryCount;
            while (retry > 0)
            {
                using (var conn = OpenDb())
                {
                    if (conn == null)
                    {
                        Log(caller, "Cannot establish connection to database (connection is null)!");
                        retry -= 1;
                        continue;
                    }
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = qry;
                        try
                        {
                            Log($"Executing query..Retry left: ({retry}), Query: {qry}");
                            resultCode = cmd.ExecuteNonQuery();
                            Log($"Query result: ({resultCode})");
                            retry = -1;
                        }
                        catch (SQLiteException ex)
                        {
                            GlobalFunc.ShowError($"SQLHelper-ExecNonQuery (SQL Error)({resultCode})", ex, false);
                            retry -= 1;
                        }
                        catch (Exception ex)
                        {
                            GlobalFunc.ShowError($"SQLHelper-ExecNonQuery (Error)({resultCode})", ex, false);
                            retry -= 1;
                        }
                    }
                }
            }
            return resultCode;
        }
        public static DataTable Query(string qry, string calledFrom = "")
        {
            string errFrom = "SQLHelper-Query";
            int retry = RetryCount;
            while (retry > 0)
            {
                try
                {
                    // Create Connection to database
                    using (var conn = OpenDb())
                    {
                        using (var cmd = new SQLiteCommand(conn))
                        {
                            cmd.CommandText = qry;

                            // Create DataTable for results
                            var dt = new DataTable();

                            // Execute query
                            Log($"{errFrom} (START) [Called by: {calledFrom}] Query: {qry}");

                            SQLiteDataAdapter sqlda = new SQLiteDataAdapter(qry, conn);
                            sqlda.Fill(dt);

                            // Log actions to textfile
                            Log($"{errFrom} (Finished executing Query) Number of Rows returned by query: {Convert.ToString(dt.Rows.Count)}");

                            conn.Close();
                            retry = -1;
                            return dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"{errFrom} (Called: {calledFrom}) {ex.Message}");
                    --retry;
                }
            }
            return null;
        }
        public static long InsertNewApp(Dictionary<string, string> dtInfo, Dictionary<string, string> dtFilepath, string callFrom)
        {
            // Setups
            int retry = RetryCount;
            string errFrom = $"SQLHelper-InsertNewApp [calledFrom: {callFrom}]";
            long LastID = 0; // Last ID Inserted succesfully
            string infoCols = "", infoVals = ""; // apps info table
            string fileCols = "", fileVals = ""; // filepath table
            int successCode; // code after execute query
            string value = ""; // variable to hold values
            string newId = GenerateId();

            // Exit if no data to process
            if (dtInfo==null || dtFilepath==null || dtInfo?.Count < 1) { return 0; }

            // Create pairing of colname and colvals
            foreach (var item in dtInfo)
            {
                if (item.Key != DbColId)
                {
                    value = item.Value.Replace("'", "''").Replace("\"", String.Empty);
                    infoCols += $"`{item.Key}`,";
                    infoVals += $"'{value}',";
                }
            }
            infoCols = infoCols.TrimEnd(',');
            infoVals = infoVals.TrimEnd(',');
            // Filepath table
            foreach (var item in dtFilepath)
            {
                if (item.Key != DbColId)
                {
                    fileCols += $"`{item.Key}`,";
                    fileVals += $"'{item.Value}',";
                }
            }
            fileCols = fileCols.TrimEnd(',');
            fileVals = fileVals.TrimEnd(',');

            while (retry > 0)
            {
                // Create Connection to database
                using (var conn = OpenDb())
                {
                    if (conn == null)
                    {
                        Log($"{errFrom} Cannot establish connection to database (connection is null)!");
                        --retry;
                        continue;
                    }
                    // Make Command and Transaction
                    var cmd = new SQLiteCommand(conn);
                    var transaction = conn.BeginTransaction();

                    // Insert entry
                    cmd.CommandText = $"INSERT INTO {DbTableApp} (`{DbColId}`,{infoCols}) VALUES({newId},{infoVals});";
                    Log($"{errFrom} (Insert query) {cmd.CommandText}");
                    successCode = cmd.ExecuteNonQuery();

                    if (successCode > 0)
                    {
                        cmd.CommandText = $"INSERT INTO {DbTableAppPath} (`{DbColId}`,{fileCols}) VALUES({newId},{fileVals});";
                        successCode = cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        Log($"{errFrom} [Insert failed code: {successCode}], Query: {cmd.CommandText}");
                    }

                    // Commit transaction
                    if (successCode > 0)
                    {
                        long.TryParse(newId, out LastID);
                        transaction.Commit();
                        Log($"{errFrom} (FINISHED INSERT) Last ID inserted: ({LastID})");
                        retry = -1;
                    }
                    else
                    {
                        LastID = 0;
                        transaction.Rollback();
                        --retry;
                    }
                    transaction.Dispose();

                    // Close Connection to DB
                    cmd.Dispose();
                    conn.Close();
                }
            }
            return LastID; // Return LastID inserted.
        }
        /// <summary>
        /// Update specified table wih Dictionary keypair values.
        /// </summary>
        /// <param name="TableName">Table name to update</param>
        /// <param name="dt">Dictionary that contains key-pair of column-value.</param>
        /// <param name="from">Method calling.</param>
        /// <returns>True if succesful. Otherwise, false.</returns>
        public static bool UpdateTable(string TableName, Dictionary<string, string> dt, string callFrom)
        {
            // Set values
            string valpair = "";
            string Id = "";
            string caller = $"UpdateTable ({callFrom})";
            try
            {
                dt.TryGetValue(DbColId, out Id);
            }
            catch { Id = ""; }
            if (String.IsNullOrWhiteSpace(Id))
            {
                Log("Cannot Update entry with empty Id!");
                return false;
            }

            foreach (var item in dt)
            {
                if (item.Key != DbColId)
                {
                    valpair += $"`{item.Key}`='{item.Value}',";
                }
            }
            valpair = valpair.TrimEnd(',');

            // Query to db
            if (!String.IsNullOrWhiteSpace(valpair))
            {
                string qry = $"UPDATE {TableName} " +
                             $"SET {valpair} " +
                             $"WHERE `{DbColId}`={Id}";
                if (ExecNonQuery(qry, caller))
                {
                    Log($"Entry with Id({Id}) is updated Succesfully!");
                    return true;
                }
            }
            return false;
        }
        public static bool DeleteItem(string uid)
        {
            string caller = "SQLHelper-DeleteItem";
            bool result = false;
            result = ExecNonQuery($"DELETE FROM {DbTableApp} WHERE `{DbColId}`={uid}", caller);
            if (result)
            {
                result = ExecNonQuery($"DELETE FROM {DbTableAppPath} WHERE `{DbColId}`={uid}", caller);
            }
            return result;
        }
        //############################################# End of Class, place new methods above this line
    }
}
