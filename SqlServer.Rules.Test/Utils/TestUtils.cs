﻿//------------------------------------------------------------------------------
//<copyright company="Microsoft">
//
//    The MIT License (MIT)
//    
//    Copyright (c) 2015 Microsoft
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.
//</copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlServer.Rules.Tests.Utils
{
    /// <summary>
    /// Utility class for test code. Useful for supporting dropping of databases after tests have completed,
    /// verification a particular database exists, etc.
    /// </summary>
    public static class TestUtils
    {
        public const string DefaultDataSourceName = "(localdb)\\MSSQLLocalDB";

        public const string MasterDatabaseName = "master";

        const string _setLockTimeoutDefault = "SET LOCK_TIMEOUT {0}"; //value configurable

        const string _queryDatabaseIfExist = @"SELECT COUNT(*) FROM [sys].[databases] WHERE [name] = '{0}'";

        const string _dropDatabaseIfExist = @"
IF EXISTS (SELECT 1 FROM [sys].[databases] WHERE [name] = '{0}') 
BEGIN
    ALTER DATABASE [{0}]
    SET READ_WRITE;
    ALTER DATABASE [{0}]
    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{0}];
END
";

        private const string _dropDatabaseIfExistAzure = @"DROP DATABASE [{0}];";

        private static Regex _batch = new Regex(@"GO\s*$", RegexOptions.Multiline);
        private static InstanceInfo _defaultInstanceInfo;

        /// <summary>
        /// Default connection string to LocalDB. Consider extending in the future to allow 
        /// specification of multiple server versions and paths.
        /// </summary>
        public static string ServerConnectionString
        {
            get { return "Data Source=" + DefaultDataSourceName + ";Integrated Security=True"; }
        }

        public static InstanceInfo DefaultInstanceInfo
        {
            get
            {
                if (_defaultInstanceInfo == null)
                {
                    _defaultInstanceInfo = new InstanceInfo(DefaultDataSourceName);
                }
                return _defaultInstanceInfo;
            }
        }

        public static void DropDbAndDeleteFiles(string dbName, string mdfFilePath = null, string ldfFilePath = null)
        {
            DropDbAndDeleteFiles(TestUtils.ServerConnectionString, dbName, mdfFilePath, ldfFilePath);
        }

        public static void DropDbAndDeleteFiles(string serverName, string dbName, string mdfFilePath = null, string ldfFilePath = null)
        {
            DropDatabase(serverName, dbName);
            DeleteIfExists(mdfFilePath);
            DeleteIfExists(ldfFilePath);
        }

        public static void DeleteIfExists(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath)
                && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void DropDatabase(InstanceInfo instance, string databaseName, bool displayException = true)
        {
            DropDatabase(instance.BuildConnectionString(CommonConstants.MasterDatabaseName), databaseName, displayException);
        }

        public static bool DropDatabase(
            string connString,
            string databaseName,
            bool displayException = true,
            bool isAzureDb = false)
        {
            bool rc = false;
            int retryCount = 1;

            for (int i = 0; i < retryCount && rc == false; i++)
            {
                SqlConnection conn = null;
                try
                {
                    SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(connString);
                    scsb.InitialCatalog = "master";
                    scsb.Pooling = false;
                    conn = new SqlConnection(scsb.ConnectionString);
                    conn.Open();

                    if (DoesDatabaseExist(conn, databaseName) == true)
                    {
                        string dropStatement;

                        if (isAzureDb)
                        {
                            dropStatement = string.Format(CultureInfo.InvariantCulture,
                                _dropDatabaseIfExistAzure,
                                databaseName);

                            // Attempt a retry due to azure instability
                            retryCount = 2;
                        }
                        else
                        {
                            conn.ChangeDatabase(MasterDatabaseName);
                            dropStatement = string.Format(CultureInfo.InvariantCulture,
                                _dropDatabaseIfExist,
                                databaseName);
                        }

                        Execute(conn, dropStatement);
                    }

                    rc = true;
                }
                catch (SqlException exception)
                {
                    if (displayException)
                    {
                        // Capture exception information, but don't fail test.
                        Console.WriteLine("Exception while dropping database {0}", databaseName);
                        Console.WriteLine(exception);
                    }
                }
                finally
                {
                    if (conn != null)
                    {
                        conn.Close();
                    }
                }
            }

            return rc;
        }

        public static bool DoesDatabaseExist(SqlConnection connection, string databaseName)
        {
            string query = string.Format(CultureInfo.InvariantCulture, _queryDatabaseIfExist, databaseName);

            int result = (int)ExecuteScalar(connection, query);

            return (result == 1);
        }

        public static SqlTestDB CreateTestDatabase(InstanceInfo instance, string dbName)
        {
            // Cleanup the database if it already exists
            TestUtils.DropDatabase(instance, dbName);

            // Create the test database
            string createDB = string.Format(CultureInfo.InvariantCulture, "create database [{0}]", dbName);
            ExecuteNonQuery(instance, "master", CommonConstants.DefaultCommandTimeout, createDB);
            SqlTestDB db = new SqlTestDB(instance, dbName, true);
            return db;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the 
        /// result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public static object ExecuteScalar(SqlConnection connection, string sqlCommandText, int commandTimeOut = 30)
        {
            ArgumentValidation.CheckForEmptyString(sqlCommandText, "sqlCommandText");

            using (SqlCommand cmd = GetCommandObject(connection, sqlCommandText, commandTimeOut))
            {
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes commands such as Transact-SQL INSERT, DELETE, UPDATE, and SET statements.
        /// </summary>
        public static void Execute(SqlConnection connection, string sqlCommandText, int? commandTimeOut = null)
        {
            ArgumentValidation.CheckForEmptyString(sqlCommandText, "sqlCommandText");

            if (commandTimeOut == null)
            {
                // Assume infinite timeout in this case for now
                commandTimeOut = 0;
            }

            using (SqlCommand cmd = GetCommandObject(connection, sqlCommandText, commandTimeOut.Value))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static SqlCommand GetCommandObject(SqlConnection conn, string sqlCommandText, int commandTimeOut)
        {
            SqlCommand cmd = conn.CreateCommand();
            // reasonable hard code to prevent hang client.  
            cmd.CommandTimeout = commandTimeOut;
            cmd.CommandText = String.Format(CultureInfo.InvariantCulture, _setLockTimeoutDefault, GetLockTimeoutMS());
            cmd.ExecuteNonQuery();
            cmd.CommandText = sqlCommandText;
            return cmd;
        }

        public static void ExecuteNonQuery(SqlTestDB db, int commandTimeout, params string[] scripts)
        {
            ExecuteNonQuery(db, (IList<string>)scripts, commandTimeout);
        }

        public static void ExecuteNonQuery(SqlTestDB db, params string[] scripts)
        {
            ExecuteNonQuery(db, (IList<string>)scripts);
        }

        public static void ExecuteNonQuery(SqlTestDB db, IList<string> scripts, int commandTimeout = CommonConstants.DefaultCommandTimeout)
        {
            ExecuteNonQuery(db.Instance, db.DatabaseName, scripts, commandTimeout);
        }

        public static void ExecuteNonQuery(InstanceInfo instance, string dbName, int commandTimeout, params string[] scripts)
        {
            ExecuteNonQuery(instance, dbName, (IList<string>)scripts, commandTimeout);
        }

        public static void ExecuteNonQuery(InstanceInfo instance, string dbName, params string[] scripts)
        {
            ExecuteNonQuery(instance, dbName, (IList<string>)scripts);
        }

        public static void ExecuteNonQuery(InstanceInfo instance, string dbName, IList<string> scripts, int commandTimeout = CommonConstants.DefaultCommandTimeout)
        {
            using (SqlConnection conn = new SqlConnection(instance.BuildConnectionString(dbName)))
            {
                conn.Open();

                foreach (string script in scripts)
                {
                    // Replace SqlCmd variables with actual values
                    string exeScript = script.Replace("$(DatabaseName)", dbName);
                    ExecuteNonQuery(conn, exeScript, commandTimeout);
                }
            }
        }

        public static void ExecuteNonQuery(SqlConnection conn, string sql, int commandTimeout = CommonConstants.DefaultCommandTimeout)
        {

            SqlCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = commandTimeout;

                // Set seven-sets
                cmd.CommandText = "SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SET NUMERIC_ROUNDABORT OFF;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Exception{0}{1}{0}While executing TSQL:{0}{2}",
                    Environment.NewLine,
                    ex,
                    sql);
                throw ex;
            }
        }


        /// <summary>
        /// Retrieves the default lock timeout in Milliseconds.  This value should
        /// be used to set the lock timeout on a connection.
        /// </summary>
        /// <returns></returns>
        private static int GetLockTimeoutMS()
        {
            // For now defaulting timout to 90 sec. This could be replaced with a better method for calculating a smart timeout
            // To have no timeout, use 0
            int timeoutMS = 90 * 1000;

            return timeoutMS;
        }

        public static IList<string> GetBatches(string script)
        {
            return _batch.Split(script).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }

    }

    public class ArgumentValidation
    {
        public static void CheckForEmptyString(string arg, string argName)
        {
            if (string.IsNullOrEmpty(arg))
            {
                throw new ArgumentException(argName);
            }
        }
    }
}