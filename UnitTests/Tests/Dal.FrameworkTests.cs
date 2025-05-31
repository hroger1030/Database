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

using DAL.Framework;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace UnitTests
{
    /// <summary>
    /// This class presumes that you have run the GenerateTestTable.sql
    /// </summary>
    [TestFixture]
    public class DALFrameworkTests
    {
        private IDatabase _Db;

        /// <summary>
        /// Run once before any tests are executed.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _Db = new Database(Constants.SQL_CONN, true, true);
        }

        /// <summary>
        /// Run before each test is executed.
        /// </summary>
        [SetUp]
        public void Setup() { }

        /// <summary>
        /// Run after each test is completed.
        /// </summary>
        [TearDown]
        public void TearDown() { }

        /// <summary>
        /// Run once all tests are completed.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _Db = null;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Category("GetSchema")]
        [Category("SynchronousTests")]
        [TestCase(eCollectionType.Databases)]
        [TestCase(eCollectionType.Tables)]
        [TestCase(eCollectionType.Procedures)]
        public void GetSchema_SelectCollection_Datatable(eCollectionType collection)
        {
            string[] restrictions;

            // the GetSchema can take varing numbers of parameters depending on the schema type that is being retrieved. Oh, Microsoft...
            switch (collection)
            {
                case eCollectionType.Databases:
                    restrictions = null;
                    break;

                case eCollectionType.Tables:
                    restrictions = new string[] { Constants.DB_NAME, Constants.SCHEMA_NAME, Constants.TABLE_NAME, null };
                    break;

                case eCollectionType.Procedures:
                    restrictions = null;
                    break;

                default:
                    throw new NotImplementedException($" Collection type '{collection}' is not yet supported");
            }

            var dt = _Db.GetSchema(collection, restrictions);

            Assert.That(dt, Is.Not.Null);
            Assert.That(dt.Rows.Count != 0, Is.True);
            Assert.That(dt.Columns.Count != 0, Is.True);
            bool exists = false;

            foreach (DataRow dr in dt.Rows)
            {
                switch (collection)
                {
                    case eCollectionType.Databases:
                        if (dr["database_name"].ToString().ToLower() == Constants.DB_NAME.ToLower())
                            exists = true;
                        break;

                    case eCollectionType.Tables:
                        if (dr["TABLE_NAME"].ToString().ToLower() == Constants.TABLE_NAME.ToLower())
                            exists = true;
                        break;

                    case eCollectionType.Procedures:
                        if (dr["SPECIFIC_NAME"].ToString().ToLower() == Constants.PROCEDURE_NAME.ToLower())
                            exists = true;
                        break;

                    default:
                        throw new NotImplementedException($" Collection type '{collection}' is not yet supported");
                }
            }

            Assert.That(exists, Is.True);
        }

        [Test]
        [Category("GetSchema")]
        [Category("SynchronousTests")]
        [TestCase(eCollectionType.Databases)]
        [TestCase(eCollectionType.Tables)]
        [TestCase(eCollectionType.Procedures)]
        public async Task GetSchema_SelectCollectionAsync_Datatable(eCollectionType collection)
        {
            string[] restrictions;

            // the GetSchema can take varing numbers of parameters depending on the schema type that is being retrieved. Oh, Microsoft...
            switch (collection)
            {
                case eCollectionType.Databases:
                    restrictions = null;
                    break;

                case eCollectionType.Tables:
                    restrictions = new string[] { Constants.DB_NAME, Constants.SCHEMA_NAME, Constants.TABLE_NAME, null };
                    break;

                case eCollectionType.Procedures:
                    restrictions = null;
                    break;

                default:
                    throw new NotImplementedException($" Collection type '{collection}' is not yet supported");
            }

            var dt = await _Db.GetSchemaAsync(collection, restrictions);

            Assert.That(dt, Is.Not.Null);
            Assert.That(dt.Rows.Count > 0, Is.True);
            Assert.That(dt.Columns.Count > 0, Is.True);

            bool exists = false;

            foreach (DataRow dr in dt.Rows)
            {
                switch (collection)
                {
                    case eCollectionType.Databases:
                        if (dr["database_name"].ToString().ToLower() == Constants.DB_NAME.ToLower())
                            exists = true;
                        break;

                    case eCollectionType.Tables:
                        if (dr["TABLE_NAME"].ToString().ToLower() == Constants.TABLE_NAME.ToLower())
                            exists = true;
                        break;

                    case eCollectionType.Procedures:
                        if (dr["SPECIFIC_NAME"].ToString().ToLower() == Constants.PROCEDURE_NAME.ToLower())
                            exists = true;
                        break;

                    default:
                        throw new NotImplementedException($" Collection type '{collection}' is not yet supported");
                }
            }

            Assert.That(exists, Is.True);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        // test datatable select

        [Test]
        [Category("InlineSql")]
        [Category("SynchronousTests")]
        public void InlineSql_SelectAll_DataTable()
        {
            int count;

            // insert a new value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value
            var buffer = _Db.ExecuteQuery(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters());

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Rows.Count == 1, Is.True);
            Assert.That(buffer.Columns.Count == 1, Is.True);

            // delete value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);
        }

        [Test]
        [Category("InlineSql")]
        [Category("AsynchronousTests")]
        public async Task InlineSql_SelectAllAsync_DataTable()
        {
            int count;

            // insert a new value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value
            var buffer = await _Db.ExecuteQueryAsync(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters());

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Rows.Count == 1, Is.True);
            Assert.That(buffer.Columns.Count != 0, Is.True);

            // delete value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);
        }

        [Test]
        [Category("InlineSql")]
        [Category("SynchronousTests")]
        public void InlineSql_SelectAll_DataSet()
        {
            int count;

            // insert a new value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // delete value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);
        }

        [Test]
        [Category("InlineSql")]
        [Category("AsynchronousTests")]
        public async Task InlineSql_SelectAllAsync_DataSet()
        {
            int count;

            // insert a new value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // delete value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        // test inline sql select all as POCO

        [Test]
        [Category("InlineSql")]
        [Category("Synchronous_Tests")]
        public void InlineSql_FullCRUD_POCOs()
        {
            int count;
            List<DbTestTable> buffer;

            // insert a new value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = _Db.ExecuteQuery<DbTestTable>(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters());

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 1), Is.True);

            // update value 
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_UPDATE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = _Db.ExecuteQuery<DbTestTable>(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters(99));

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 99), Is.True);

            // delete value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters(99));
            Assert.That(count == 1, Is.True);
        }

        [Test]
        [Category("InlineSql")]
        [Category("Asynchronous_Tests")]
        public async Task InlineSql_FullCRUDAsync_POCOs()
        {
            int count;
            List<DbTestTable> buffer;

            // insert a new value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = await _Db.ExecuteQueryAsync<DbTestTable>(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters());

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 1), Is.True);

            // update value 
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_UPDATE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = await _Db.ExecuteQueryAsync<DbTestTable>(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters(99));

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 99), Is.True);

            // delete value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters(99));
            Assert.That(count == 1, Is.True);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Category("StoredProcs")]
        [Category("Synchronous_Tests")]
        public void StoredProc_SelectWithParams_ManualParse()
        {
            int count;
            List<DbTestTable> buffer;

            // insert a new value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = _Db.ExecuteQuery(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters(), Constants.ParseDatareader);

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 1), Is.True);

            // update value 
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_UPDATE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = _Db.ExecuteQuery(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters(99), Constants.ParseDatareader);

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 99), Is.True);

            // delete value
            count = _Db.ExecuteNonQuery(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters(99));
            Assert.That(count == 1, Is.True);
        }

        [Test]
        [Category("StoredProcs")]
        [Category("Asynchronous_Tests")]
        public async Task StoredProc_SelectWithParamsAsync_ManualParse()
        {
            int count;
            List<DbTestTable> buffer;

            // insert a new value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_INSERT_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = await _Db.ExecuteQueryAsync(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters(), Constants.ParseDatareaderAsync);

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 1), Is.True);

            // update value 
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_UPDATE_WITH_PARAMETERS, CreateIdParameters());
            Assert.That(count == 1, Is.True);

            // load value and check it
            buffer = await _Db.ExecuteQueryAsync(Constants.QUERY_BASIC_SELECT_WITH_PARAMETERS, CreateIdParameters(99), Constants.ParseDatareaderAsync);

            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Count == 1, Is.True);
            Assert.That(Constants.IsDbTestTableCorrect(buffer[0], 99), Is.True);

            // delete value
            count = await _Db.ExecuteNonQueryAsync(Constants.QUERY_BASIC_DELETE_WITH_PARAMETERS, CreateIdParameters(99));
            Assert.That(count == 1, Is.True);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        public static SqlParameter[] CreateIdParameters(int id = 1)
        {
            return new SqlParameter[] { new SqlParameter() { Value = id, ParameterName = "@Id", DbType = DbType.Int32 } };
        }
    }
}
