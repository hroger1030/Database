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
using System.Diagnostics;

namespace DAL.Standard
{
    public class DatabaseFake : IDatabase
    {
        private const string EMPTY_QUERY_STRING = "Query string is null or empty";
        private const string NULL_PROCESSOR_METHOD = "Processor method is null";
        private const string DEFAULT_CONNECTION_STRING = "local host connection string";

        private void WriteArguments(string sqlQuery, IList<SqlParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentException(nameof(sqlQuery));

            Debug.WriteLine($"Sql query: {sqlQuery}");

            if (parameters == null)
                return;

            foreach (var item in parameters)
                Debug.WriteLine($"Parameter: {item}");

            Debug.WriteLine(string.Empty);
        }

        public DatabaseFake()
        {
            Debug.WriteLine("DatabaseFake Initialized");
        }

        public DataTable ExecuteQuery(string sqlQuery, IList<SqlParameter> parameters)
        {
            Debug.WriteLine("Called ExecuteQuery()");
            WriteArguments(sqlQuery, parameters);

            return new DataTable();
        }

        public DataTable ExecuteQuerySp(string sqlQuery, IList<SqlParameter> parameters)
        {
            Debug.WriteLine("Called ExecuteQuerySp()");
            WriteArguments(sqlQuery, parameters);

            return new DataTable();
        }

        public List<T> ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            Debug.WriteLine("Called ExecuteQuery<T>()");
            WriteArguments(sqlQuery, parameters);

            return new List<T>();
        }

        public T ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            if (processor == null)
                throw new ArgumentException(nameof(processor));

            Debug.WriteLine("Called ExecuteQuery<T>()");
            Debug.WriteLine($"Processor: '{processor}'");
            WriteArguments(sqlQuery, parameters);

            return default(T);
        }

        public List<T> ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new()
        {
            Debug.WriteLine("Called ExecuteQuerySp<T>()");
            WriteArguments(sqlQuery, parameters);

            return new List<T>();
        }

        public T ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor)
        {
            if (processor == null)
                throw new ArgumentException(nameof(processor));

            Debug.WriteLine("Called ExecuteQuerySp<T>()");
            Debug.WriteLine($"Processor: '{processor}'");
            WriteArguments(sqlQuery, parameters);

            return default(T);
        }

        public int ExecuteNonQuery(string sqlQuery, IList<SqlParameter> parameters)
        {
            Debug.WriteLine("Called ExecuteNonQuery()");
            WriteArguments(sqlQuery, parameters);

            return 0;
        }

        public int ExecuteNonQuerySp(string sqlQuery, IList<SqlParameter> parameters)
        {
            Debug.WriteLine("Called ExecuteNonQuerySp()");
            WriteArguments(sqlQuery, parameters);

            return 0;
        }

        public T ExecuteScalar<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            Debug.WriteLine("Called ExecuteScalar()");
            WriteArguments(sqlQuery, parameters);

            return default(T);
        }

        public T ExecuteScalarSp<T>(string sqlQuery, IList<SqlParameter> parameters)
        {
            Debug.WriteLine("Called ExecuteScalarSp()");
            WriteArguments(sqlQuery, parameters);

            return default(T);
        }

        public DataTable GetSchema()
        {
            Debug.WriteLine("Called GetSchema()");

            return new DataTable();
        }
    }
}
