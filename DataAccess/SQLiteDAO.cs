using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace DataAccess
{
    public class SQLiteDAO
    {
        /// <summary>Connection string for the database.</summary>
        public string ConnectionString { get; }

        /// <summary>Initializes an instance of the <see cref="SQLiteDAO"/> with a connection string.</summary>
        /// <param name="connectionString"></param>
        public SQLiteDAO(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>Creates a SQLite database file.</summary>
        /// <remarks>Do not include the file extension in the <paramref name="databaseName"/>.</remarks>
        /// <param name="databaseName">The name of the database.</param>
        public void CreateDatabase(string databaseName)
        {
            SQLiteConnection.CreateFile(databaseName);
        }

        /// <summary>Checks if a SQlite database exists.</summary>
        /// <remarks>Do not include the file extension in the <paramref name="databaseName"/>.</remarks>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns></returns>
        public bool DatabaseExists(string databaseName)
        {
            return File.Exists(databaseName);
        }

        /// <summary>Creates a table.</summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnDefinitions">The column definitions. Include the name of column, data type, and any constraints.</param>
        public void CreateTable(string tableName, string columnDefinitions)
        {
            ExecuteCommand($"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefinitions})", (command) =>
            {
                return command.ExecuteNonQuery();
            });
        }

        public bool TableExists(string tableName)
        {
            return ExecuteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';", (command) =>
            {
                return command.ExecuteScalar() != null;
            });
        }

        /// <summary>Inserts data into a table.</summary>
        /// <param name="tableName">Name of the table to insert data into.</param>
        /// <param name="parameters">The columns and values that will be inserted.</param>
        /// <returns>The number of rows that were inserted.</returns>
        public int InsertSingle(string tableName, SQLiteParameter[] parameters)
        {
            CreateQueryFromParameters(parameters, out string columnsQuery, out string valuesQuery);

            return ExecuteCommand($"INSERT INTO {tableName} ({columnsQuery}) VALUES ({valuesQuery})", (command) =>
            {
                return command.ExecuteNonQuery();
            }, parameters);
        }

        public int InsertMultiple(string tableName, SQLiteParameter[][] parameters)
        {
            int count = 0;

            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    conn.Open();

                    SQLiteTransaction transaction = conn.BeginTransaction();
                    command.Transaction = transaction;
                    command.Connection = conn;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        CreateQueryFromParameters(parameters[i], out string columnsQuery, out string valuesQuery);

                        command.CommandText = $"INSERT INTO {tableName} ({columnsQuery}) VALUES ({valuesQuery})";

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters[i]);
                        }

                        count += command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            return count;
        }

        /// <summary>Reads data from a table.</summary>
        /// <typeparam name="TData">The type of data that's being read.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columns">The columns that will be read.</param>
        /// <param name="dataConverter">Callback for converting the data from the <see cref="SQLiteDataReader"/>.</param>
        /// <param name="where">The where clause that specifies which rows will be read.</param>
        /// <returns></returns>
        public TData Read<TData>(string tableName, string columns, Func<SQLiteDataReader, TData> dataConverter, string where = null)
        {
            return ExecuteCommand($"SELECT {columns} FROM {tableName}{(where != null ? $" WHERE {where}" : "")}", (command) =>
            {
                SQLiteDataReader reader = command.ExecuteReader();

                TData data = dataConverter(reader); // Call the function to convert the data

                reader.Close();

                return data;
            });
        }

        /// <summary>Updates data in a table.</summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnValuePairs">The columns and values that will be updated.</param>
        /// <param name="where">The where clause that specifies which rows will be updated.</param>
        /// <returns>The number of rows that were updated.</returns>
        public int UpdateSingle(string tableName, SQLiteParameter[] parameters, string where = null)
        {
            string set = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                set += $"{parameters[i].ParameterName.Replace("@", "")} = {parameters[i].ParameterName}";

                if (i != parameters.Length - 1)
                {
                    set += ",";
                }
            }

            return ExecuteCommand($"UPDATE {tableName} SET {set}{(where != null ? $" WHERE {where}" : "")}", (command) =>
            {
                return command.ExecuteNonQuery();
            }, parameters);
        }

        /// <summary>Deletes rows from a table.</summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="where">The where clause that specifies which rows will be deleted.</param>
        /// <returns>The number of rows that were deleted.</returns>
        public int Delete(string tableName, string where)
        {
            return ExecuteCommand($"DELETE FROM {tableName}{(where != null ? $" WHERE {where}" : "")}", (command) =>
            {
                return command.ExecuteNonQuery();
            });
        }

        /// <summary>Opens a connection, creates a command using that connection, and then passes the command to a callback.</summary>
        /// <remarks>The callback function should execute the command.</remarks>
        /// <typeparam name="TReturn">The type of data returned by the <paramref name="invoker"/>.</typeparam>
        /// <param name="commandText">The SQL statement that will be executed.</param>
        /// <param name="invoker">The function that will invoke the <see cref="DbCommand"/>.</param>
        /// <param name="parameters">The parameters that will be provided to the <see cref="DbCommand"/>.</param>
        /// <returns>The data that is returned by executing the command (e.g. the data returned by the SQL query).</returns>
        private TReturn ExecuteCommand<TReturn>(string commandText, Func<SQLiteCommand, TReturn> invoker, SQLiteParameter[] parameters = null)
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    conn.Open();

                    command.Connection = conn;
                    command.CommandText = commandText;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                    }

                    return (TReturn)invoker.DynamicInvoke(command);
                }
            }
        }

        private void CreateQueryFromParameters(SQLiteParameter[] parameters, out string columnsQuery, out string valuesQuery)
        {
            valuesQuery = "";
            columnsQuery = "";

            // Parameterize all columns and values
            for (int i = 0; i < parameters.Length; i++)
            {
                valuesQuery += parameters[i].ParameterName;
                columnsQuery += parameters[i].ParameterName.Replace("@", "");
                if (i != parameters.Length - 1)
                {
                    valuesQuery += ",";
                    columnsQuery += ",";
                }
            }
        }
    }
}
