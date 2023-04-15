/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Core
{
    public class DatabaseFake : IDatabase
    {
        /// <summary>
        /// Contains a list of commands that have been executed against the database.
        /// </summary>
        public List<string> CommandHistory { get; set; } = new List<string>();

        public DatabaseFake()
        {
            CommandHistory.Add("DatabaseFake Initialized");
        }

        #region Sync Methods

        public DataTable ExecuteQuery(string sqlQuery, IList<SqlParameter> parameters)
        {
            CommandHistory.Add("Called ExecuteQuery()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return new DataTable();
        }

        public DataTable ExecuteQuerySp(string sqlQuery, IList<SqlParameter> parameters)
        {
            CommandHistory.Add("Called ExecuteQuerySp()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return new DataTable();
        }

        public List<T> ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            CommandHistory.Add("Called ExecuteQuery<T>()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return new List<T>();
        }

        public List<T> ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            CommandHistory.Add("Called ExecuteQuerySp<T>()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return new List<T>();
        }

        public T ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            if (processor == null)
                throw new ArgumentException(nameof(processor));

            CommandHistory.Add("Called ExecuteQuery<T>()");
            CommandHistory.Add($"Processor: '{processor}'");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return default;
        }

        public T ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            if (processor == null)
                throw new ArgumentException(nameof(processor));

            CommandHistory.Add("Called ExecuteQuerySp<T>()");
            CommandHistory.Add($"Processor: '{processor}'");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return default;
        }

        public int ExecuteNonQuery(string sqlQuery, IList<SqlParameter> parameters)
        {
            CommandHistory.Add("Called ExecuteNonQuery()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return 0;
        }

        public int ExecuteNonQuerySp(string sqlQuery, IList<SqlParameter> parameters)
        {
            CommandHistory.Add("Called ExecuteNonQuerySp()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return 0;
        }

        public T ExecuteScalar<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            CommandHistory.Add("Called ExecuteScalar()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return default;
        }

        public T ExecuteScalarSp<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            CommandHistory.Add("Called ExecuteScalarSp()");
            CommandHistory.Add(WriteArguments(sqlQuery, parameters));

            return default;
        }

        public DataTable GetSchema()
        {
            CommandHistory.Add("Called GetSchema()");

            return new DataTable();
        }

        #endregion

        #region Async Methods

        public async Task<int> ExecuteNonQueryAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await Task.Run(() => ExecuteNonQuery(sqlQuery, parameters));
        }

        public async Task<int> ExecuteNonQuerySpAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await Task.Run(() => ExecuteNonQuerySp(sqlQuery, parameters));
        }

        public async Task<DataTable> ExecuteQueryAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await Task.Run(() => ExecuteQuery(sqlQuery, parameters));
        }

        public async Task<DataTable> ExecuteQuerySpAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await Task.Run(() => ExecuteQuerySp(sqlQuery, parameters));
        }

        public async Task<T> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            return await Task.Run(() => ExecuteQuery<T>(sqlQuery, parameters, processor));
        }

        public async Task<T> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            return await Task.Run(() => ExecuteQuerySp<T>(sqlQuery, parameters, processor));
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return await Task.Run(() => ExecuteQuery<T>(sqlQuery, parameters));
        }

        public async Task<List<T>> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return await Task.Run(() => ExecuteQuerySp<T>(sqlQuery, parameters));
        }

        public async Task<T> ExecuteScalarAsync<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await Task.Run(() => ExecuteScalar<T>(sqlQuery, parameters));
        }

        public async Task<T> ExecuteScalarSpAsync<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await Task.Run(() => ExecuteScalarSp<T>(sqlQuery, parameters));
        }

        public async Task<DataTable> GetSchemaAsync()
        {
            return await Task.Run(() => GetSchema());
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Writes out a sql query and parameters to a string.
        /// </summary>
        public static string WriteArguments(string sqlQuery, IList<SqlParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentException(nameof(sqlQuery));

            var sb = new StringBuilder();

            sb.AppendLine($"Sql query: {sqlQuery}");

            if (parameters != null)
            {
                foreach (var item in parameters)
                    sb.AppendLine($"Parameter: {item}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes out a sql query and parameters to a string.
        /// </summary>
        public static async Task<string> WriteArgumentsAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            var output = await Task.Run(() => WriteArguments(sqlQuery, parameters));
            return output;
        }

        #endregion
    }
}
