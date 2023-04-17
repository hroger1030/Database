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

using DAL.Net;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace UnitTests.Tests
{
    [TestFixture]
    public class SynchronousTests
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

        ///////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [TestCase(eCollectionType.Databases)]
        [TestCase(eCollectionType.Tables)]
        [TestCase(eCollectionType.Procedures)]
        public void Database_GetSchema_Datatable(eCollectionType collection)
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

            Assert.IsNotNull(dt);
            Assert.IsTrue(dt.Rows.Count != 0);
            Assert.IsTrue(dt.Columns.Count != 0);

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

            Assert.IsTrue(exists);
        }

        [Test]
        public void Database_UnconditionalSelect_DataTable()
        {
            var buffer = _Db.ExecuteQuery(Constants.QUERY_BASIC_SELECT, null);

            Assert.IsNotNull(buffer);
        }

        [Test]
        public void Database_UnconditionalSelect_Poco()
        {
            var buffer = _Db.ExecuteQuery<DbTestTable>(Constants.QUERY_BASIC_SELECT, null);

            Assert.IsNotNull(buffer);
            Assert.IsTrue(buffer.Count == 3);

            foreach (var item in buffer)
                Assert.IsTrue(Constants.IsDbTestTableCorrect(item));
        }

        [Test]
        public void Database_UnconditionalSelect_ManualParse()
        {
            var buffer = _Db.ExecuteQuery(Constants.QUERY_BASIC_SELECT, null, Constants.ParseDatareader);

            Assert.IsNotNull(buffer);
            Assert.IsTrue(buffer.Count == 3);

            foreach (var item in buffer)
                Assert.IsTrue(Constants.IsDbTestTableCorrect(item));
        }





        //// run integration tests against the testDb
        //IDatabase test = new Database(SQL_CONN, true, true);

        //    var parameters = new SqlParameter[]
        //    {
        //        new SqlParameter() { Value = 1, ParameterName = "@Id", DbType = DbType.Int32 },
        //    };

        Func<SqlDataReader, Dictionary<int, string>> processor = delegate (SqlDataReader reader)
        {
            var output = new Dictionary<int, string>();

            while (reader.Read())
            {
                int id = (int)reader["Id"];
                string name = (string)reader["Location"];

                output.Add(id, name);
            }

            return output;
        };

        //    var result = Task.Run(() => test.ExecuteQuerySpAsync<DbTableTest>("[toolsdb].[dbo].[SelectAllData]", parameters)).Result;


        //    Assert.IsTrue(c.Center.Y == 0f, "Failed center Y check");
        //}

    }
}
