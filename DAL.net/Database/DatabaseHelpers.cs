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
using Microsoft.SqlServer.Types;
using System.Data;
using System.Reflection;
using System.Text;

namespace DAL.Net
{
    public partial class Database
    {
        public const string DEFAULT_CONNECTION_STRING = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Pooling=true;";
        public const string EXCEPTION_SQL_PREFIX = "Sql.Parameter";
        public const string EXCEPTION_KEY_QUERY = "Sql.Query";
        public const string EXCEPTION_KEY_CONNECTION = "Sql.ConnectionString";

        #region Sync Methods

        /// <summary>
        /// Converts a list of IEnumerable objects to a string of comma delimited items. If a quote_character
        /// is defined, this will wrap each item with the character(s) passed.
        /// </summary>
        public static string GenericListToStringList<T>(IEnumerable<T> list, string quoteCharacter = null, string quoteEscapeCharacter = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var sb = new StringBuilder();
            bool firstFlag = true;

            foreach (T item in list)
            {
                if (firstFlag)
                    firstFlag = false;
                else
                    sb.Append(',');

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
        public static string GenerateSqlDebugString(string sqlQuery, IList<SqlParameter> parameterList)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            if (parameterList == null || parameterList.Count == 0)
                return sqlQuery;

            // set up a structured parameter string builder
            var sb = new StringBuilder();
            var valueList = new List<string>();

            for (int i = 0; i < parameterList.Count; i++)
            {
                if (parameterList[i].Direction == ParameterDirection.ReturnValue)
                    continue;

                if (parameterList[i].Value == DBNull.Value || parameterList[i].Value == null)
                {
                    valueList.Add($"{parameterList[i].ParameterName} = null");
                }
                else
                {
                    switch (parameterList[i].SqlDbType)
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
                            valueList.Add($"{parameterList[i].ParameterName} = '{parameterList[i].Value?.ToString().Replace("'", "''")}'");
                            break;

                        // structured data is partially supported, very tricky to render in a debug string.
                        case SqlDbType.Structured:
                            if (parameterList[i].Value is DataTable dt)
                            {
                                sb.AppendLine($"DECLARE @StructuredParam{i} [{parameterList[i].TypeName}]");

                                foreach (DataRow row in dt.Rows)
                                {
                                    var columnNames = dt.Columns
                                        .Cast<DataColumn>()
                                        .Select(col => $"[{col.ColumnName}]");

                                    var columnValues = dt.Columns
                                        .Cast<DataColumn>()
                                        .Select(col =>
                                        {
                                            var value = row[col];
                                            if (value == DBNull.Value)
                                                return "NULL";

                                            if (col.DataType == typeof(string) || col.DataType == typeof(DateTime) || col.DataType == typeof(Guid))
                                                return $"'{value.ToString().Replace("'", "''")}'"; // escape single quotes

                                            if (col.DataType == typeof(bool))
                                                return ((bool)value) ? "1" : "0";

                                            return value.ToString();
                                        });

                                    sb.AppendLine($"INSERT @StructuredParam{i} ({string.Join(",", columnNames)}) VALUES ({string.Join(", ", columnValues)})");
                                    sb.AppendLine();
                                }

                                valueList.Add($"@{parameterList[i].ParameterName} = @StructuredParam{i}");
                            }
                            else
                            {
                                valueList.Add($"{parameterList[i].ParameterName} = [Structured] (unknown structure or null)");
                            }
                            break;

                        default:
                            valueList.Add($"{parameterList[i].ParameterName} = {parameterList[i].Value}");
                            break;
                    }
                }
            }

            return $"{sb.ToString()} {sqlQuery} {GenericListToStringList(valueList, null, null)}";
        }

        /// <summary>
        /// This method performs automatic mapping between a data reader and a POCO object, mapping any values that 
        /// have properties names that match column names. It can be configured to throw exceptions if there isn't a 1:1 mapping.
        /// </summary>
        /// <returns></returns>
        public static List<T> ParseDataReaderResult<T>(SqlDataReader reader, bool throwUnmappedFieldsError) where T : class, new()
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

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
                int ordinal;

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

                        ordinal = reader.GetOrdinal(columnName);

                        switch (propertyName)
                        {
                            case "System.Int32":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<int>(ordinal);
                                break;

                            case "System.String":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<string>(ordinal);
                                break;

                            case "System.Double":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<double>(ordinal);
                                break;

                            case "System.Float":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<float>(ordinal);
                                break;

                            case "System.Boolean":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<bool>(ordinal);
                                break;

                            case "System.DateTime":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<DateTime>(ordinal);
                                break;

                            case "System.DateTimeOffset":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<DateTimeOffset>(ordinal);
                                break;

                            case "System.TimeSpan":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<TimeSpan>(ordinal);
                                break;

                            case "System.Guid":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<Guid>(ordinal);
                                break;

                            case "System.Single":
                                fieldValue = reader.IsDBNull(ordinal) ? null : Convert.ToSingle(reader.GetValue(ordinal));
                                break;

                            case "System.Decimal":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<decimal>(ordinal);
                                break;

                            case "System.Byte":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<byte>(ordinal);
                                break;

                            case "System.Byte[]":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<byte[]>(ordinal);
                                break;

                            case "System.SByte":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<sbyte>(ordinal);
                                break;

                            case "System.Char":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<char>(ordinal);
                                break;

                            case "System.UInt32":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<uint>(ordinal);
                                break;

                            case "System.Int64":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<long>(ordinal);
                                break;

                            case "System.UInt64":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<ulong>(ordinal);
                                break;

                            case "System.Object":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
                                break;

                            case "System.Int16":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<short>(ordinal);
                                break;

                            case "System.UInt16":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<ushort>(ordinal);
                                break;

                            case "System.Udt":
                                // generated a Microsoft.SqlServer.Server.InvalidUdtException. Udts will have unique datatypes.
                                throw new NotImplementedException("System.Udt is an unsupported datatype");

                            case "Microsoft.SqlServer.Types.SqlGeometry":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<SqlGeometry>(ordinal);
                                break;

                            case "Microsoft.SqlServer.Types.SqlGeography":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<SqlGeography>(ordinal);
                                break;

                            case "Microsoft.SqlServer.Types.SqlHierarchyId":
                                fieldValue = reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<SqlHierarchyId>(ordinal);
                                break;

                            default:
                                if (propertyType.IsEnum)
                                {
                                    // Read as the *actual* underlying integer type of the enum
                                    Type underlying = Enum.GetUnderlyingType(propertyType);
                                    object raw = reader.GetFieldValue<object>(ordinal);

                                    // Convert.ChangeType can handle int16/int32/int64, etc.
                                    object converted = Convert.ChangeType(raw, underlying);

                                    fieldValue = Enum.ToObject(propertyType, converted);
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
                            throw new Exception($"Cannot map data reader field '{columnName}' to object property on object '{outputType}'");
                        }
                    }
                }

                results.Add(new_object);
            }

            return results;
        }

        /// <summary>
        /// Reads in a collection of SqlParameters and adds them to a SqlCommand object
        /// </summary>
        /// <param name="parameters">List of sql parameters supplied to method</param>
        /// <param name="cmd">command object for query</param>
        public static void ReadInParameters(IList<SqlParameter> parameters, SqlCommand cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException(nameof(cmd));

            if (parameters == null)
                return;

            foreach (SqlParameter parameter in parameters)
                cmd.Parameters.Add(parameter);
        }

        /// <summary>
        /// Helper method to write sql output parameters back to parameter collection.
        /// </summary>
        /// <param name="parameters">List of sql parameters supplied to method</param>
        /// <param name="cmd">command object to pull output from</param>
        public static void PersistOutputParameters(IList<SqlParameter> parameters, SqlCommand cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException(nameof(cmd));

            // This covers the case where no parameters were passed in, but we have output parameters being returned.
            if (parameters == null)
                parameters = new List<SqlParameter>();

            if (cmd.Parameters.Count != parameters.Count)
                throw new Exception($"Sql command parameter count ({cmd.Parameters.Count}) does not match input parameter count ({parameters.Count})");

            for (int i = 0; i < cmd.Parameters.Count; i++)
                parameters[i].Value = cmd.Parameters[i].Value;
        }

        /// <summary>
        /// Generates a SqlParameter object from a generic poco list. This allows you to pass in a list of N 
        /// objects into a stored procedure as a single argument. The sqlTypeName type needs to exist in the db
        /// however, and be of the correct type.
        /// 
        /// Sample: ConvertObjectCollectionToParameter("@Foo", "dbo.SomeUserType", a_generic_object_collection);
        /// </summary>
        public static SqlParameter ConvertPocoCollectionToParameter<T>(string parameterName, string sqlTypeName, IEnumerable<T> input) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentNullException(nameof(parameterName));

            if (string.IsNullOrWhiteSpace(sqlTypeName))
                throw new ArgumentNullException(nameof(sqlTypeName));

            if (input == null)
                throw new ArgumentException("Collection cannot be null", nameof(input));

            var dt = new DataTable();
            var genericType = typeof(T);

            var objectProperties = genericType.GetProperties();

            foreach (var propertyInfo in objectProperties)
                dt.Columns.Add(propertyInfo.Name);

            foreach (var item in input)
            {
                var dr = dt.NewRow();

                foreach (var property in objectProperties)
                    dr[property.Name] = genericType.GetProperty(property.Name).GetValue(item, null);

                dt.Rows.Add(dr);
            }

            var sqlParameter = new SqlParameter()
            {
                ParameterName = parameterName,
                SqlDbType = SqlDbType.Structured,
                TypeName = sqlTypeName,
                Value = dt,
            };

            return sqlParameter;
        }

        /// <summary>
        /// Generates a SqlParameter object from a generic object list. This allows you to pass in a list of N 
        /// objects into a stored procedure as a single argument. The sqlTypeName type needs to exist in the db
        /// however, and be of the correct type.
        /// 
        /// Sample: ConvertObjectCollectionToParameter("@Foo", "dbo.SomeUserType", a_generic_object_collection, "value");
        /// </summary>
        public static SqlParameter ConvertObjectCollectionToParameter<T>(string parameterName, string sqlTypeName, IEnumerable<T> input, string columnName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentNullException(nameof(parameterName));

            if (string.IsNullOrWhiteSpace(sqlTypeName))
                throw new ArgumentNullException(nameof(sqlTypeName));

            if (input == null)
                throw new ArgumentException("Collection cannot be null", nameof(input));

            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentNullException(nameof(columnName));

            var dt = new DataTable();
            dt.Columns.Add(columnName);

            // only certain types are allowed as input
            var atomicTypes = new HashSet<string>()
            {
                typeof(int).FullName, typeof(string).FullName, typeof(double).FullName, typeof(float).FullName,
                typeof(bool).FullName, typeof(DateTime).FullName, typeof(DateTimeOffset).FullName, typeof(TimeSpan).FullName,
                typeof(Guid).FullName, typeof(float).FullName, typeof(decimal).FullName, typeof(byte).FullName,
                typeof(byte[]).FullName, typeof(sbyte).FullName, typeof(char).FullName, typeof(uint).FullName,
                typeof(long).FullName, typeof(ulong).FullName, typeof(object).FullName, typeof(short).FullName,
            };

            var genericType = typeof(T);

            if (!atomicTypes.Contains(genericType.FullName))
                throw new Exception($"'{genericType.FullName}' is not a known type that can be mapped to a SQL type");

            foreach (var item in input)
            {
                var dr = dt.NewRow();
                dr[columnName] = item;
                dt.Rows.Add(dr);
            }

            var sqlParameter = new SqlParameter()
            {
                ParameterName = parameterName,
                SqlDbType = SqlDbType.Structured,
                TypeName = sqlTypeName,
                Value = dt,
            };

            return sqlParameter;
        }

        /// <summary>
        /// Generates a SqlParameter object from a KVP collection. This allows you to pass in a dictionary of N 
        /// objects into a stored procedure as a single argument. The sqlTypeName type needs to exist in the db
        /// however, and be of the correct type.
        /// 
        /// Sample: ConvertObjectCollectionToParameter("@Foo", "dbo.SomeUserType", a_dictionary, "key", "value");
        /// </summary>
        public static SqlParameter ConvertKvpCollectionToParameter<K, V>(string parameterName, string sqlTypeName, IEnumerable<KeyValuePair<K, V>> input, string keyName, string valueName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentNullException(nameof(parameterName));

            if (string.IsNullOrWhiteSpace(sqlTypeName))
                throw new ArgumentNullException(nameof(sqlTypeName));

            if (input == null)
                throw new ArgumentException("Collection cannot be null", nameof(input));

            if (string.IsNullOrWhiteSpace(keyName))
                throw new ArgumentNullException(nameof(keyName));

            if (string.IsNullOrWhiteSpace(valueName))
                throw new ArgumentNullException(nameof(valueName));

            var dt = new DataTable();
            dt.Columns.Add(keyName);
            dt.Columns.Add(valueName);

            // only certain types are allowed as input
            var atomicTypes = new HashSet<string>()
            {
                typeof(int).FullName, typeof(string).FullName, typeof(double).FullName, typeof(float).FullName,
                typeof(bool).FullName, typeof(DateTime).FullName, typeof(DateTimeOffset).FullName, typeof(TimeSpan).FullName,
                typeof(Guid).FullName, typeof(float).FullName, typeof(decimal).FullName, typeof(byte).FullName,
                typeof(byte[]).FullName, typeof(sbyte).FullName, typeof(char).FullName, typeof(uint).FullName,
                typeof(long).FullName, typeof(ulong).FullName, typeof(object).FullName, typeof(short).FullName,
            };

            var genericKey = typeof(K);
            var genericValue = typeof(V);

            if (!atomicTypes.Contains(genericKey.FullName))
                throw new Exception($"The key, '{genericKey.FullName}' is not a known type that can be mapped to a SQL type");

            if (!atomicTypes.Contains(genericValue.FullName))
                throw new Exception($"The value, '{genericValue.FullName}' is not a known type that can be mapped to a SQL type");

            foreach (var item in input)
            {
                var dr = dt.NewRow();
                dr[keyName] = item.Key;
                dr[valueName] = item.Value;
                dt.Rows.Add(dr);
            }

            var sqlParameter = new SqlParameter()
            {
                ParameterName = parameterName,
                SqlDbType = SqlDbType.Structured,
                TypeName = sqlTypeName,
                Value = dt,
            };

            return sqlParameter;
        }

        #endregion

        #region Async Methods

        public static async Task<string> GenericListToStringListAsync<T>(IEnumerable<T> list, string quoteCharacter = null, string quoteEscapeCharacter = null)
        {
            return await Task.Run(() => GenericListToStringList(list, quoteCharacter, quoteEscapeCharacter));
        }

        /// <summary>
        /// Method creates sql debugging strings with parameterized argument lists
        /// </summary>
        public static async Task<string> GenerateSqlDebugStringAsync(string sqlQuery, IList<SqlParameter> parameterList)
        {
            return await Task.Run(() => GenerateSqlDebugString(sqlQuery, parameterList));
        }

        /// <summary>
        /// This method performs automatic mapping between a data reader and a POCO object, mapping any values that 
        /// have properties names that match column names. It can be configured to throw exceptions if there isn't a 1:1 mapping.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<T>> ParseDatareaderResultAsync<T>(SqlDataReader reader, bool throwUnmappedFieldsError) where T : class, new()
        {
            return await Task.Run(() => ParseDataReaderResult<T>(reader, throwUnmappedFieldsError));
        }

        /// <summary>
        /// Reads in a collection of SqlParameters and adds them to a SqlCommand object
        /// </summary>
        /// <param name="parameters">List of sql parameters supplied to method</param>
        /// <param name="cmd">command object for query</param>
        public static async Task ReadInParametersAsync(IList<SqlParameter> parameters, SqlCommand cmd)
        {
            await Task.Run(() => ReadInParameters(parameters, cmd));
        }

        /// <summary>
        /// Helper method to write sql output parameters back to parameter collection.
        /// </summary>
        /// <param name="parameters">List of sql parameters supplied to method</param>
        /// <param name="cmd">command object to pull output from</param>
        public static async Task PersistOutputParametersAsync(IList<SqlParameter> parameters, SqlCommand cmd)
        {
            await Task.Run(() => PersistOutputParameters(parameters, cmd));
        }

        /// <summary>
        /// Generates a SqlParameter object from a generic poco list. This allows you to pass in a list of N 
        /// objects into a stored procedure as a single argument. The sqlTypeName type needs to exist in the db
        /// however, and be of the correct type.
        /// 
        /// Sample: ConvertObjectCollectionToParameter("@Foo", "dbo.SomeUserType", a_generic_object_collection);
        /// </summary>
        public static async Task<SqlParameter> ConvertPocoCollectionToParameterAsync<T>(string parameterName, string sqlTypeName, IEnumerable<T> input) where T : class, new()
        {
            return await Task.Run(() => ConvertPocoCollectionToParameter<T>(parameterName, sqlTypeName, input));
        }

        /// <summary>
        /// Generates a SqlParameter object from a generic object list. This allows you to pass in a list of N 
        /// objects into a stored procedure as a single argument. The sqlTypeName type needs to exist in the db
        /// however, and be of the correct type.
        /// 
        /// Sample: ConvertObjectCollectionToParameter("@Foo", "dbo.SomeUserType", a_generic_object_collection, "value");
        /// </summary>
        public static async Task<SqlParameter> ConvertObjectCollectionToParameterAsync<T>(string parameterName, string sqlTypeName, IEnumerable<T> input, string columnName)
        {
            return await Task.Run(() => ConvertObjectCollectionToParameter<T>(parameterName, sqlTypeName, input, columnName));
        }

        #endregion
    }
}
