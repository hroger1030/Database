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

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DALFramework
{
    public partial class Database : IDatabase
    {
        internal static DataTable ExecuteQuery(string sqlQuery, SqlParameter[] parameters, string connection)
        {
            return ExecuteQuery(sqlQuery, parameters, connection, false);
        }

        internal static DataTable ExecuteQuerySp(string sqlQuery, SqlParameter[] parameters, string connection)
        {
            return ExecuteQuery(sqlQuery, parameters, connection, true);
        }

        internal static List<T> ExecuteQuery<T>(string sqlQuery, SqlParameter[] parameters, string connection) where T : class, new()
        {
            return ExecuteQuery<T>(sqlQuery, parameters, connection, false);
        }

        internal static List<T> ExecuteQuerySp<T>(string sqlQuery, SqlParameter[] parameters, string connection) where T : class, new()
        {
            return ExecuteQuery<T>(sqlQuery, parameters, connection, true);
        }

        internal static int ExecuteNonQuery(string sqlQuery, SqlParameter[] parameters, string connection)
        {
            return ExecuteNonQuery(sqlQuery, parameters, connection, false);
        }

        internal static int ExecuteNonQuerySp(string sqlQuery, SqlParameter[] parameters, string connection)
        {
            return ExecuteNonQuery(sqlQuery, parameters, connection, true);
        }

        internal static T ExecuteScalar<T>(string sqlQuery, SqlParameter[] parameters, string connection)
        {
            return ExecuteScalar<T>(sqlQuery, parameters, connection, false);
        }

        internal static T ExecuteScalarSp<T>(string sqlQuery, SqlParameter[] parameters, string connection)
        {
            return ExecuteScalar<T>(sqlQuery, parameters, connection, true);
        }
    }
}
