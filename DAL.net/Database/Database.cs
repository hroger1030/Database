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

using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL.Net
{
    public partial class Database : IDatabase
    {
        private readonly string _Connection;
        private readonly bool _Debug;
        private readonly bool _LogConnection;
        private readonly bool _LogParameters;
        private readonly bool _ThrowUnmappedFieldsError;

        public Database() : this(DEFAULT_CONNECTION_STRING) { }

        /// <summary>
        /// CTOR for Database object.
        /// </summary>
        /// <param name="connection">A sql connection string.</param>
        /// <param name="debug">Allow console logging to occur. Defaults to false.</param>
        /// <param name="logConnection">Allow connection string to be included in thrown exceptions. Defaults to false.</param>
        /// <param name="logParameters">Allow query parameters to be included in thrown exceptions. Defaults to false.</param>
        public Database(string connection, bool debug = false, bool logConnection = false, bool logParameters = false, bool throwUnmappedFieldsError = true)
        {
            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(nameof(connection));

            _Connection = connection;
            _Debug = debug;
            _LogConnection = logConnection;
            _LogParameters = logParameters;
            _ThrowUnmappedFieldsError = throwUnmappedFieldsError;
        }

        #region Sync Methods

        public DataTable ExecuteQuery(string sqlQuery, IList<SqlParameter> parameters)
        {
            return ExecuteQuery(sqlQuery, parameters, _Connection, false);
        }

        public DataTable ExecuteQuerySp(string sqlQuery, IList<SqlParameter> parameters)
        {
            return ExecuteQuery(sqlQuery, parameters, _Connection, true);
        }

        /// <summary>
        /// This method retuyrns the result of a collection of queries in the form of a dataset.
        /// </summary>
        public DataSet ExecuteMultipleQueries(List<QueryData> queryList)
        {
            return ExecuteMultipleQueries(queryList, _Connection);
        }

        public List<T> ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return ExecuteQuery<T>(sqlQuery, parameters, _Connection, false);
        }

        public List<T> ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return ExecuteQuery<T>(sqlQuery, parameters, _Connection, true);
        }

        public T ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            return ExecuteQuery<T>(sqlQuery, parameters, _Connection, false, processor);
        }

        public T ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            return ExecuteQuery<T>(sqlQuery, parameters, _Connection, true, processor);
        }

        public int ExecuteNonQuery(string sqlQuery, IList<SqlParameter> parameters)
        {
            return ExecuteNonQuery(sqlQuery, parameters, _Connection, false);
        }

        public int ExecuteNonQuerySp(string sqlQuery, IList<SqlParameter> parameters)
        {
            return ExecuteNonQuery(sqlQuery, parameters, _Connection, true);
        }

        public T ExecuteScalar<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            return ExecuteScalar<T>(sqlQuery, parameters, _Connection, false);
        }

        public T ExecuteScalarSp<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            return ExecuteScalar<T>(sqlQuery, parameters, _Connection, true);
        }

        public DataTable GetSchema(eCollectionType collection, string[] restrictions = null)
        {
            var buffer = collection.ToString().Replace('_', ' ');

            using var conn = new SqlConnection(_Connection);

            conn.Open();
            var dt = conn.GetSchema(buffer, restrictions);
            conn.Close();

            return dt;
        }

        private DataTable ExecuteQuery(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };
                using var adapter = new SqlDataAdapter() { SelectCommand = cmd, };

                ReadInParameters(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                var dt = new DataTable();

                conn.Open();
                adapter.Fill(dt);
                PersistOutputParameters(parameters, cmd);
                conn.Close();

                return dt;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private DataSet ExecuteMultipleQueries(List<QueryData> queryList, string connection)
        {
            if (queryList == null || queryList.Count < 1)
                throw new ArgumentNullException(nameof(queryList));

            int index = 0;

            try
            {
                var ds = new DataSet();
                using var conn = new SqlConnection(connection);
                conn.Open();

                for (index = 0; index < queryList.Count; index++)
                {
                    using var cmd = new SqlCommand(queryList[index].Query, conn) { CommandType = (queryList[index].StoredProcedure) ? CommandType.StoredProcedure : CommandType.Text, };
                    using var adapter = new SqlDataAdapter() { SelectCommand = cmd, };

                    ReadInParameters(queryList[index].Parameters, cmd);

                    if (_Debug)
                    {
                        var sqlDebugString = GenerateSqlDebugString(queryList[index].Query, queryList[index].Parameters);
                        Console.WriteLine($"Query{index}:{sqlDebugString}");
                    }

                    var dt = new DataTable();
                    adapter.Fill(dt);
                    ds.Tables.Add(dt);
                    PersistOutputParameters(queryList[index].Parameters, cmd);
                }

                conn.Close();
                return ds;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, queryList[index].Query);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && queryList[index].Parameters != null)
                {
                    for (int i = 0; i < queryList[i].Parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{queryList[index].Parameters[i].ParameterName} = {queryList[index].Parameters[i].Value}");
                }

                throw;
            }
        }

        private List<T> ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                ReadInParameters(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                conn.Open();
                using SqlDataReader dataReader = cmd.ExecuteReader();
                var output = ParseDatareaderResult<T>(dataReader, _ThrowUnmappedFieldsError);
                PersistOutputParameters(parameters, cmd);
                dataReader.Close();
                conn.Close();

                return output;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private T ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure, Func<SqlDataReader, T> processor)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                ReadInParameters(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                conn.Open();
                using SqlDataReader dataReader = cmd.ExecuteReader();
                var output = processor.Invoke(dataReader);
                PersistOutputParameters(parameters, cmd);
                dataReader.Close();
                conn.Close();

                return output;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private int ExecuteNonQuery(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(nameof(connection));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                ReadInParameters(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                conn.Open();
                int results = cmd.ExecuteNonQuery();
                PersistOutputParameters(parameters, cmd);
                conn.Close();

                return results;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private T ExecuteScalar<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                ReadInParameters(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                conn.Open();
                object buffer = cmd.ExecuteScalar();
                PersistOutputParameters(parameters, cmd);
                conn.Close();

                if (buffer == null)
                    return default;

                if (buffer.GetType() == typeof(DBNull))
                    return default;

                if (buffer is T t)
                    return t;

                return (T)Convert.ChangeType(buffer, typeof(T));
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        #endregion

        #region Async Methods

        public async Task<DataTable> ExecuteQueryAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await ExecuteQueryAsync(sqlQuery, parameters, _Connection, false);
        }

        public async Task<DataTable> ExecuteQuerySpAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await ExecuteQueryAsync(sqlQuery, parameters, _Connection, true);
        }

        /// <summary>
        /// This method retuyrns the result of a collection of queries in the form of a dataset.
        /// </summary>
        public async Task<DataSet> ExecuteMultipleQueriesAsync(List<QueryData> queryList)
        {
            return await ExecuteMultipleQueriesAsync(queryList, _Connection);
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters, _Connection, false);
        }

        public async Task<List<T>> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters, _Connection, true);
        }

        public async Task<T> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, Task<T>> processor)
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters, _Connection, false, processor);
        }

        public async Task<T> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, Task<T>> processor)
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters, _Connection, true, processor);
        }

        public async Task<int> ExecuteNonQueryAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await ExecuteNonQueryAsync(sqlQuery, parameters, _Connection, false);
        }

        public async Task<int> ExecuteNonQuerySpAsync(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await ExecuteNonQueryAsync(sqlQuery, parameters, _Connection, true);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await ExecuteScalarAsync<T>(sqlQuery, parameters, _Connection, false);
        }

        public async Task<T> ExecuteScalarSpAsync<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            return await ExecuteScalarAsync<T>(sqlQuery, parameters, _Connection, true);
        }

        public async Task<DataTable> GetSchemaAsync(eCollectionType collection, string[] restrictions = null)
        {
            var buffer = collection.ToString().Replace('_', ' ');

            using var conn = new SqlConnection(_Connection);

            await conn.OpenAsync();
            DataTable dt = await Task.Run(() => conn.GetSchema(buffer, restrictions));
            await conn.CloseAsync();

            return dt;
        }

        private async Task<DataTable> ExecuteQueryAsync(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };
                using var adapter = new SqlDataAdapter() { SelectCommand = cmd, };

                await ReadInParametersAsync(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                var dt = new DataTable();

                await conn.OpenAsync();
                await Task.Run(() => adapter.Fill(dt));
                await PersistOutputParametersAsync(parameters, cmd);
                await conn.CloseAsync();

                return dt;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private async Task<DataSet> ExecuteMultipleQueriesAsync(List<QueryData> queryList, string connection)
        {
            if (queryList == null || queryList.Count < 1)
                throw new ArgumentNullException(nameof(queryList));

            int index = 0;

            try
            {
                var ds = new DataSet();
                using var conn = new SqlConnection(connection);
                await conn.OpenAsync();

                for (index = 0; index < queryList.Count; index++)
                {
                    using var cmd = new SqlCommand(queryList[index].Query, conn) { CommandType = (queryList[index].StoredProcedure) ? CommandType.StoredProcedure : CommandType.Text, };
                    using var adapter = new SqlDataAdapter() { SelectCommand = cmd, };

                    await ReadInParametersAsync(queryList[index].Parameters, cmd);

                    if (_Debug)
                    {
                        var sqlDebugString = await GenerateSqlDebugStringAsync(queryList[index].Query, queryList[index].Parameters);
                        Console.WriteLine($"Query{index}:{sqlDebugString}");
                    }

                    var dt = new DataTable();
                    adapter.Fill(dt);
                    ds.Tables.Add(dt);
                    await PersistOutputParametersAsync(queryList[index].Parameters, cmd);
                }

                await conn.CloseAsync();
                return ds;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, queryList[index].Query);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && queryList[index].Parameters != null)
                {
                    for (int i = 0; i < queryList[i].Parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{queryList[index].Parameters[i].ParameterName} = {queryList[index].Parameters[i].Value}");
                }

                throw;
            }
        }

        private async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                await ReadInParametersAsync(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                await conn.OpenAsync();
                using SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
                var output = await ParseDatareaderResultAsync<T>(dataReader, _ThrowUnmappedFieldsError);
                await PersistOutputParametersAsync(parameters, cmd);
                await dataReader.CloseAsync();
                await conn.CloseAsync();

                return output;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private async Task<T> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure, Func<SqlDataReader, Task<T>> processor)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                await ReadInParametersAsync(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                await conn.OpenAsync();
                using SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
                var output = await processor.Invoke(dataReader);
                await PersistOutputParametersAsync(parameters, cmd);
                await dataReader.CloseAsync();
                await conn.CloseAsync();

                return output;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private async Task<int> ExecuteNonQueryAsync(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(nameof(connection));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                await ReadInParametersAsync(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                await conn.OpenAsync();
                int results = await cmd.ExecuteNonQueryAsync();
                await PersistOutputParametersAsync(parameters, cmd);
                await conn.CloseAsync();

                return results;
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        private async Task<T> ExecuteScalarAsync<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                await ReadInParametersAsync(parameters, cmd);

                if (_Debug)
                {
                    var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters);
                    Console.WriteLine(sqlDebugString);
                }

                await conn.OpenAsync();
                object buffer = await cmd.ExecuteScalarAsync();
                await PersistOutputParametersAsync(parameters, cmd);
                await conn.CloseAsync();

                if (buffer == null)
                    return default;

                if (buffer.GetType() == typeof(DBNull))
                    return default;

                if (buffer is T t)
                    return t;

                return (T)Convert.ChangeType(buffer, typeof(T));
            }
            catch (Exception ex)
            {
                ex.Data.Add(EXCEPTION_KEY_QUERY, sqlQuery);

                if (_LogConnection)
                    ex.Data.Add(EXCEPTION_KEY_CONNECTION, connection);

                if (_LogParameters && parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        ex.Data.Add($"{EXCEPTION_SQL_PREFIX}{i + 1}", $"{parameters[i].ParameterName} = {parameters[i].Value}");
                }

                throw;
            }
        }

        #endregion
    }
}