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
using System.Reflection;
using System.Text;

namespace DAL.Framework
{
    public sealed class Database : IDatabase
    {
        private const string EMPTY_QUERY_STRING = "Query string is null or empty";
        private const string EMPTY_CONNECTION_STRING = "Connection string is null or empty";
        private const string NULL_PROCESSOR_METHOD = "Processor method is null";
        private const string DEFAULT_CONNECTION_STRING = "Data Source=Localhost;Initial Catalog=Master;Integrated Security=SSPI;Connect Timeout=1;";

        private const string EXCEPTION_SQL_PREFIX = "Sql.Parameter";
        private const string EXCEPTION_KEY_QUERY = "Sql.Query";
        private const string EXCEPTION_KEY_CONNECTION = "Sql.ConnectionString";

        private readonly string _Connection;
        private readonly bool _LogConnection;
        private readonly bool _LogParameters;
        private readonly bool _ThrowUnmappedFieldsError;

        public Database() : this(DEFAULT_CONNECTION_STRING) { }

        /// <summary>
        /// CTOR for Database object
        /// </summary>
        /// <param name="connection">A sql connection string.</param>
        /// <param name="logConnection">Allow connection string to be included in thrown exceptions. Defaults to false.</param>
        /// <param name="logParameters">Allow query parameters to be included in thrown exceptions. Defaults to false.</param>
        public Database(string connection, bool logConnection = false, bool logParameters = false, bool throwUnmappedFieldsError = true)
        {
            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(EMPTY_CONNECTION_STRING);

            _Connection = connection;
            _LogConnection = logConnection;
            _LogParameters = logParameters;
            _ThrowUnmappedFieldsError = throwUnmappedFieldsError;
        }

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
            using (SqlConnection conn = new SqlConnection(_Connection))
            {
                DataTable dt = null;

                conn.Open();
                dt = conn.GetSchema("Databases");
                conn.Close();

                return dt;
            }
        }

        private DataTable ExecuteQuery(string sqlQuery, IList<SqlParameter> parameters, string connection, bool storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(EMPTY_QUERY_STRING);

            try
            {
                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand(sqlQuery, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                            if (parameters != null)
                            {
                                foreach (SqlParameter parameter in parameters)
                                    cmd.Parameters.Add(parameter);
                            }

#if (DEBUG)
                            string SqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
#endif

                            var dt = new DataTable();

                            conn.Open();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(dt);
                            conn.Close();

                            if (parameters != null)
                            {
                                for (int i = 0; i < cmd.Parameters.Count; i++)
                                    parameters[i].Value = cmd.Parameters[i].Value;
                            }

                            return dt;
                        }
                    }
                }
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
                throw new ArgumentNullException(EMPTY_QUERY_STRING);

            try
            {
                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                                cmd.Parameters.Add(parameter);
                        }

#if (DEBUG)
                        string SqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
#endif

                        conn.Open();

                        using (SqlDataReader data_reader = cmd.ExecuteReader())
                        {
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
                    }
                }
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
                throw new ArgumentNullException(EMPTY_QUERY_STRING);

            if (processor == null)
                throw new ArgumentNullException(NULL_PROCESSOR_METHOD);

            try
            {
                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                                cmd.Parameters.Add(parameter);
                        }

#if (DEBUG)
                        string SqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
#endif

                        conn.Open();

                        using (SqlDataReader data_reader = cmd.ExecuteReader())
                        {
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
                    }
                }
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
                throw new ArgumentNullException(EMPTY_QUERY_STRING);

            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(EMPTY_CONNECTION_STRING);

            try
            {
                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                                cmd.Parameters.Add(parameter);
                        }

#if (DEBUG)
                        string SqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
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
                }
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
                throw new ArgumentNullException(EMPTY_QUERY_STRING);

            try
            {
                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand(sqlQuery, conn))
                    {
                        T results = default;

                        cmd.CommandType = (storedProcedure) ? CommandType.StoredProcedure : CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                                cmd.Parameters.Add(parameter);
                        }

#if (DEBUG)
                        string SqlDebugString = GenerateSqlDebugString(sqlQuery, parameters);
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
                            else if (buffer is T)
                                return (T)buffer;
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
                }
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

        /// <summary>
        /// Converts a list of IEnumerable objects to a string of comma delimited items. If a quote_character
        /// is defined, this will wrap each item with the character(s) passed.
        /// </summary>
        public static string GenericListToStringList<T>(IEnumerable<T> list, string quoteCharacter = null, string quoteEscapeCharacter = null)
        {
            if (list == null)
                throw new ArgumentNullException("Cannot convert a null IEnumerable object");

            var sb = new StringBuilder();
            bool firstFlag = true;

            foreach (T item in list)
            {
                if (firstFlag)
                    firstFlag = false;
                else
                    sb.Append(",");

                if (item == null)
                {
                    sb.Append("null");
                }
                else
                {
                    string buffer = item.ToString();

                    if (!string.IsNullOrWhiteSpace(quoteEscapeCharacter))
                        buffer = buffer.Replace(quoteCharacter, quoteEscapeCharacter);

                    if (!string.IsNullOrWhiteSpace(quoteCharacter))
                        sb.Append(quoteCharacter + buffer + quoteCharacter);
                    else
                        sb.Append(buffer);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Method creates sql debugging strings with parameterized argument lists
        /// </summary>
        private string GenerateSqlDebugString(string sqlQuery, IList<SqlParameter> parameterList)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(EMPTY_QUERY_STRING);

            if (parameterList == null || parameterList.Count == 0)
                return sqlQuery;

            var value_list = new List<string>();

            foreach (var item in parameterList)
            {
                if (item.Direction == ParameterDirection.ReturnValue)
                    continue;

                if (item.IsNullable)
                {
                    value_list.Add($"{item.ParameterName} = null");
                }
                else
                {
                    switch (item.SqlDbType)
                    {
                        case SqlDbType.Char:
                        case SqlDbType.NChar:
                        case SqlDbType.Text:
                        case SqlDbType.NText:
                        case SqlDbType.NVarChar:
                        case SqlDbType.VarChar:
                        case SqlDbType.UniqueIdentifier:
                        case SqlDbType.DateTime:
                        case SqlDbType.Date:
                        case SqlDbType.Time:
                        case SqlDbType.DateTime2:
                            value_list.Add($"@{item.ParameterName} = '{item.Value}'");
                            break;

                        default:
                            value_list.Add($"@{item.ParameterName} = {item.Value}");
                            break;
                    }
                }
            }

            return $"{sqlQuery} {GenericListToStringList(value_list, null, null)}";
        }

        /// <summary>
        /// This method performs automatic mapping between a data reader and a POCO object, mapping any values that 
        /// have properties names that match column names. It can be configured to throw exceptions if there isn't a 1:1 mapping.
        /// </summary>
        /// <returns></returns>
        private List<T> ParseDatareaderResult<T>(SqlDataReader reader, bool throwUnmappedFieldsError = false) where T : class, new()
        {
            var outputType = typeof(T);
            var results = new List<T>();
            var propertyLookup = new Dictionary<string, PropertyInfo>();

            foreach (var propertyInfo in outputType.GetProperties())
                propertyLookup.Add(propertyInfo.Name, propertyInfo);

            T new_object;
            object fieldValue;

            while (reader.Read())
            {
                new_object = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);

                    if (propertyLookup.TryGetValue(columnName, out PropertyInfo propertyInfo))
                    {
                        Type propertyType = propertyInfo.PropertyType;
                        string propertyName = propertyInfo.PropertyType.FullName;

                        // in the event that we are looking at a nullable type, we need to look at the underlying type.
                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            propertyName = Nullable.GetUnderlyingType(propertyType).ToString();
                            propertyType = Nullable.GetUnderlyingType(propertyType);
                        }

                        switch (propertyName)
                        {
                            case "System.Int32":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as int? ?? null;
                                else
                                    fieldValue = (int)reader[columnName];
                                break;

                            case "System.String":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = null;
                                else
                                    fieldValue = (string)reader[columnName];
                                break;

                            case "System.Double":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as double? ?? null;
                                else
                                    fieldValue = (double)reader[columnName];
                                break;

                            case "System.Float":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as float? ?? null;
                                else
                                    fieldValue = (float)reader[columnName];
                                break;

                            case "System.Boolean":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as bool? ?? null;
                                else
                                    fieldValue = (bool)reader[columnName];
                                break;

                            case "System.Boolean[]":
                                if (reader[i] == DBNull.Value)
                                {
                                    fieldValue = null;
                                }
                                else
                                {
                                    // inline conversion, blech. improve later.
                                    var byteArray = (byte[])reader[i];
                                    var boolArray = new bool[byteArray.Length];

                                    for (int index = 0; index < byteArray.Length; index++)
                                        boolArray[index] = Convert.ToBoolean(byteArray[index]);

                                    fieldValue = boolArray;
                                }
                                break;

                            case "System.DateTime":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as DateTime? ?? null;
                                else
                                    fieldValue = DateTime.Parse(reader[columnName].ToString());
                                break;

                            case "System.Guid":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as Guid? ?? null;
                                else
                                    fieldValue = (Guid)reader[columnName];
                                break;

                            case "System.Single":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as float? ?? null;
                                else
                                    fieldValue = float.Parse(reader[columnName].ToString());
                                break;

                            case "System.Decimal":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as decimal? ?? null;
                                else
                                    fieldValue = (decimal)reader[columnName];
                                break;

                            case "System.Byte":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = null;
                                else
                                    fieldValue = (byte)reader[columnName];
                                break;

                            case "System.Byte[]":
                                if (reader[i] == DBNull.Value)
                                {
                                    fieldValue = null;
                                }
                                else
                                {
                                    string byteArray = reader[columnName].ToString();

                                    byte[] bytes = new byte[byteArray.Length * sizeof(char)];
                                    Buffer.BlockCopy(byteArray.ToCharArray(), 0, bytes, 0, bytes.Length);
                                    fieldValue = bytes;
                                }
                                break;

                            case "System.SByte":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as sbyte? ?? null;
                                else
                                    fieldValue = (sbyte)reader[columnName];
                                break;

                            case "System.Char":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as char? ?? null;
                                else
                                    fieldValue = (char)reader[columnName];
                                break;

                            case "System.UInt32":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as uint? ?? null;
                                else
                                    fieldValue = (uint)reader[columnName];
                                break;

                            case "System.Int64":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as long? ?? null;
                                else
                                    fieldValue = (long)reader[columnName];
                                break;

                            case "System.UInt64":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as ulong? ?? null;
                                else
                                    fieldValue = (ulong)reader[columnName];
                                break;

                            case "System.Object":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = null;
                                else
                                    fieldValue = reader[columnName];
                                break;

                            case "System.Int16":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as short? ?? null;
                                else
                                    fieldValue = (short)reader[columnName];
                                break;

                            case "System.UInt16":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as ushort? ?? null;
                                else
                                    fieldValue = (ushort)reader[columnName];
                                break;

                            case "System.Udt":
                                // no idea how to handle a custom type
                                throw new NotImplementedException("System.Udt is an unsupported datatype");

                            case "Microsoft.SqlServer.Types.SqlGeometry":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as Microsoft.SqlServer.Types.SqlGeometry ?? null;
                                else
                                    fieldValue = (Microsoft.SqlServer.Types.SqlGeometry)reader[columnName];
                                break;

                            case "Microsoft.SqlServer.Types.SqlGeography":
                                if (reader[i] == DBNull.Value)
                                    fieldValue = reader[columnName] as Microsoft.SqlServer.Types.SqlGeography ?? null;
                                else
                                    fieldValue = (Microsoft.SqlServer.Types.SqlGeography)reader[columnName];
                                break;

                            default:
                                if (propertyType.IsEnum)
                                {
                                    // enums are common, but don't fit into the above buckets. 
                                    if (reader[i] == DBNull.Value)
                                        fieldValue = null;
                                    else
                                        fieldValue = Enum.ToObject(propertyType, reader[columnName]);
                                    break;
                                }
                                else
                                {
                                    throw new Exception($"Column '{propertyLookup[columnName]}' has an unknown data type: '{propertyLookup[columnName].PropertyType.FullName}'.");
                                }
                        }

                        propertyLookup[columnName].SetValue(new_object, fieldValue, null);
                    }
                    else
                    {
                        // found a row in data reader that cannot be mapped to a property in object.
                        // might be an error, but it is dependent on the specific use case.
                        if (throwUnmappedFieldsError)
                        {
                            throw new Exception($"Cannot map datareader field '{columnName}' to object property on object '{outputType}'");
                        }
                    }
                }

                results.Add(new_object);
            }

            return results;
        }

        private DataTable ConvertObjectToDataTable<T>(IEnumerable<T> input)
        {
            var dt = new DataTable();
            var outputType = typeof(T);
            var object_properties = outputType.GetProperties();

            foreach (var propertyInfo in object_properties)
                dt.Columns.Add(propertyInfo.Name);

            foreach (var item in input)
            {
                var dr = dt.NewRow();

                foreach (var property in object_properties)
                    dr[property.Name] = outputType.GetProperty(property.Name).GetValue(item, null);

                dt.Rows.Add(dr);
            }

            return dt;
        }

        /// <summary>
        /// Generates a SqlParameter object from a generic object list. This allows you to pass in a list of N 
        /// objects into a stored procedure as a single argument. The sqlTypeName type needs to exist in the db
        /// however, and be of the correct type.
        /// 
        /// Sample: ConvertObjectCollectionToParameter("@Foo", "dbo.SomeUserType", a_generic_object_collection);
        /// </summary>
        public SqlParameter ConvertObjectCollectionToParameter<T>(string parameterName, string sqlTypeName, IEnumerable<T> input)
        {
            DataTable dt = ConvertObjectToDataTable(input);

            var sql_parameter = new SqlParameter(parameterName, dt)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = sqlTypeName
            };

            return sql_parameter;
        }
    }
}