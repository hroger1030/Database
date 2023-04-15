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
using System.Threading.Tasks;

namespace DAL.Core
{
    public partial class Database : IDatabase
    {
        private readonly string _Connection;
        private readonly bool _LogConnection;
        private readonly bool _LogParameters;
        private readonly bool _ThrowUnmappedFieldsError;

        public Database() : this(DEFAULT_CONNECTION_STRING) { }

        /// <summary>
        /// CTOR for Database object.
        /// </summary>
        /// <param name="connection">A sql connection string.</param>
        /// <param name="logConnection">Allow connection string to be included in thrown exceptions. Defaults to false.</param>
        /// <param name="logParameters">Allow query parameters to be included in thrown exceptions. Defaults to false.</param>
        public Database(string connection, bool logConnection = false, bool logParameters = false, bool throwUnmappedFieldsError = true)
        {
            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(nameof(connection));

            _Connection = connection;
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

        public List<T> ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return ExecuteQuery<T>(sqlQuery, parameters, _Connection, false);
        }

        public T ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            return ExecuteQuery<T>(sqlQuery, parameters, _Connection, false, processor);
        }

        public List<T> ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return ExecuteQuery<T>(sqlQuery, parameters, _Connection, true);
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

        public DataTable GetSchema()
        {
            using var conn = new SqlConnection(_Connection);

            conn.Open();
            var dt = conn.GetSchema("Databases");
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

#if (DEBUG)
                var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

                var dt = new DataTable();

                conn.OpenAsync();
                adapter.Fill(dt);
                PersistOutputParameters(parameters, cmd);
                conn.CloseAsync();

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

                throw ex;
            }
        }

        private List<T> ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn);
                cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                        cmd.Parameters.Add(parameter);
                }

#if (DEBUG)
                var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

                conn.Open();

                using SqlDataReader data_reader = cmd.ExecuteReader();
                var output = ParseDatareaderResult<T>(data_reader, _ThrowUnmappedFieldsError);

                if (parameters != null)
                {
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                        parameters[i].Value = cmd.Parameters[i].Value;
                }

                data_reader.Close();
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

                throw ex;
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
                using var cmd = new SqlCommand(sqlQuery, conn);
                cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                        cmd.Parameters.Add(parameter);
                }

#if (DEBUG)
                var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

                conn.Open();

                using SqlDataReader data_reader = cmd.ExecuteReader();
                var output = processor.Invoke(data_reader);

                if (parameters != null)
                {
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                        parameters[i].Value = cmd.Parameters[i].Value;
                }

                data_reader.Close();
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

                throw ex;
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
                using var cmd = new SqlCommand(sqlQuery, conn);
                cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                        cmd.Parameters.Add(parameter);
                }

#if (DEBUG)
                var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

                conn.Open();
                int results = cmd.ExecuteNonQuery();
                conn.Close();

                if (parameters != null)
                {
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                        parameters[i].Value = cmd.Parameters[i].Value;
                }

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

                throw ex;
            }
        }

        private T ExecuteScalar<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn);
                T results = default;

                cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                        cmd.Parameters.Add(parameter);
                }

#if (DEBUG)
                var sqlDebugString = GenerateSqlDebugString(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

                conn.Open();

                object buffer = cmd.ExecuteScalar();

                if (buffer == null)
                {
                    results = default;
                }
                else
                {
                    if (buffer.GetType() == typeof(DBNull))
                        results = default;
                    else if (buffer is T t)
                        return t;
                    else
                        return (T)Convert.ChangeType(buffer, typeof(T));
                }

                conn.Close();

                if (parameters != null)
                {
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                        parameters[i].Value = cmd.Parameters[i].Value;
                }

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

                throw ex;
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

        public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters, _Connection, false);
        }

        public async Task<T> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters, _Connection, false, processor);
        }

        public async Task<List<T>> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters, _Connection, true);
        }

        public async Task<T> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
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

        public async Task<DataTable> GetSchemaAsync()
        {
            using var conn = new SqlConnection(_Connection);

            await conn.OpenAsync();
            DataTable dt = await Task.Run(() => conn.GetSchema("Databases"));
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

#if (DEBUG)
                var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

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

        private async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var conn = new SqlConnection(connection);
                using var cmd = new SqlCommand(sqlQuery, conn) { CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text, };

                await ReadInParametersAsync(parameters, cmd);

#if (DEBUG)
                var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

                await conn.OpenAsync();
                using SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
                var output = ParseDatareaderResult<T>(dataReader, _ThrowUnmappedFieldsError);
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

        private async Task<T> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure, Func<SqlDataReader, T> processor)
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

#if (DEBUG)
                var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

                await conn.OpenAsync();
                using SqlDataReader data_reader = await cmd.ExecuteReaderAsync();
                var output = processor.Invoke(data_reader);
                await PersistOutputParametersAsync(parameters, cmd);
                await data_reader.CloseAsync();
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

#if (DEBUG)
                var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

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

#if (DEBUG)
                var sqlDebugString = await GenerateSqlDebugStringAsync(sqlQuery, parameters!);
                Console.WriteLine(sqlDebugString);
#endif

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
