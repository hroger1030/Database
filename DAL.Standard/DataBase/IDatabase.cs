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

namespace DAL.Standard
{
    public interface IDatabase
    {
        #region Sync Methods

        int ExecuteNonQuery(string sqlQuery, IList<SqlParameter> parameters);

        int ExecuteNonQuerySp(string sqlQuery, IList<SqlParameter> parameters);

        DataTable ExecuteQuery(string sqlQuery, IList<SqlParameter> parameters);

        DataTable ExecuteQuerySp(string sqlQuery, IList<SqlParameter> parameters);

        T ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor);

        T ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, T> processor);

        List<T> ExecuteQuery<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new();

        List<T> ExecuteQuerySp<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new();

        T ExecuteScalar<T>(string sqlQuery, IList<SqlParameter> parameters);

        T ExecuteScalarSp<T>(string sqlQuery, IList<SqlParameter> parameters);

        DataTable GetSchema(eCollectionType collection, string[] restrictions = null);

        #endregion

        #region Async Methods

        Task<int> ExecuteNonQueryAsync(string sqlQuery, IList<SqlParameter> parameters);

        Task<int> ExecuteNonQuerySpAsync(string sqlQuery, IList<SqlParameter> parameters);

        Task<DataTable> ExecuteQueryAsync(string sqlQuery, IList<SqlParameter> parameters);

        Task<DataTable> ExecuteQuerySpAsync(string sqlQuery, IList<SqlParameter> parameters);

        Task<T> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, Task<T>> processor);

        Task<T> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters, Func<SqlDataReader, Task<T>> processor);

        Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new();

        Task<List<T>> ExecuteQuerySpAsync<T>(string sqlQuery, IList<SqlParameter> parameters) where T : class, new();

        Task<T> ExecuteScalarAsync<T>(string sqlQuery, IList<SqlParameter> parameters);

        Task<T> ExecuteScalarSpAsync<T>(string sqlQuery, IList<SqlParameter> parameters);

        Task<DataTable> GetSchemaAsync(eCollectionType collection, string[] restrictions = null);

        #endregion
    }
}