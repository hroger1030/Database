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
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace UnitTests
{
    public static class Constants
    {
        /// <summary>
        /// We are assuming that we are working off a local SQL Server instance by default.
        /// If you change the db that you are running the tests against, you will need to update the catalog
        /// </summary>
        public const string SQL_CONN = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ToolsDb;Integrated Security=True;Pooling=true;";

        /// <summary>
        /// Name of test database
        /// </summary>
        public const string DB_NAME = "toolsDb";

        /// <summary>
        /// Schema name
        /// </summary>
        public const string SCHEMA_NAME = "dbo";

        /// <summary>
        /// Name of test table and schema
        /// </summary>
        public const string TABLE_NAME = "TestTable";

        /// <summary>
        /// Name of test table and schema
        /// </summary>
        public const string PROCEDURE_NAME = "SelectDataById";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        // Basic crud operations. Note that the order they are executed in might affect the results of the tests.
        // the operations should be produce the same results when executed in order no matter how many times the test is run

        public readonly static string QUERY_BASIC_SELECT_WITH_PARAMETERS = $"select * from [{DB_NAME}].[{SCHEMA_NAME}].[{TABLE_NAME}] where Id = @Id order by Id";

        public readonly static string QUERY_BASIC_INSERT_WITH_PARAMETERS = $"insert [{DB_NAME}].[{SCHEMA_NAME}].[{TABLE_NAME}] ([Id]) values (@Id)";

        public readonly static string QUERY_BASIC_UPDATE_WITH_PARAMETERS = $"update [{DB_NAME}].[{SCHEMA_NAME}].[{TABLE_NAME}] set [Id] = 99 where Id = @Id";

        public readonly static string QUERY_BASIC_DELETE_WITH_PARAMETERS = $"delete [{DB_NAME}].[{SCHEMA_NAME}].[{TABLE_NAME}] where Id = @Id";

        public readonly static string STOREDPROC_SELECT_WITH_PARAMETERS = $"[{DB_NAME}].[{SCHEMA_NAME}].[{PROCEDURE_NAME}]";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// We are seeding the database with known values, so we can test the results.
        /// If any of the constants are altered, this function will need to be updated to match.
        /// </summary>
        public static bool IsDbTestTableCorrect(DbTestTable input, int expectedId)
        {
            if (input.Id != expectedId)
                return false;

            if (input.bigintTest != long.MaxValue)
                return false;

            if (input.bigintTestNull != null)
                return false;

            if (input.binaryTest.Length == 1 && input.binaryTest[0] != 0xFF)
                return false;

            if (input.binaryTestNull != null)
                return false;

            if (!input.bitTest)
                return false;

            if (input.bitTestNull != null)
                return false;

            if (input.charTest != "abcdefghijklmnopqrstuvwxyz")
                return false;

            if (input.charTestNull != null)
                return false;

            if (input.dateTest != new DateTime(9999, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc))
                return false;

            if (input.datetimeTest != new DateTime(9999, 12, 31, 23, 59, 59, 997, DateTimeKind.Unspecified))
                return false;

            if (input.datetimeTestNull != null)
                return false;

            if (input.datetime2Test != new DateTime(9999, 12, 31, 23, 59, 59, 997, DateTimeKind.Unspecified))
                return false;

            if (input.datetime2TestNull != null)
                return false;

            if (input.datetimeoffsetTest != new DateTimeOffset(9999, 12, 31, 23, 59, 59, 997, TimeSpan.FromHours(0)))
                return false;

            if (input.datetimeoffsetTestNull != null)
                return false;

            if (input.decimalTest != 1234567890)
                return false;

            if (input.decimalTestNull != null)
                return false;

            if (input.floatTest != 1234567890.12345)
                return false;

            if (input.floatTestNull != null)
                return false;

            if (input.imageTest.Length == 1 && input.imageTest[0] != 0xFF)
                return false;

            if (input.imageTestNull != null)
                return false;

            if (input.intTest != int.MaxValue)
                return false;

            if (input.intTestNull != null)
                return false;

            //NYI
            //geographyTest
            //geographyTestNull
            //geometryTest
            //geometryTestNull
            //heiarchyIdTest
            //heiarchyIdTestNull

            if (input.moneyTest != 922337203685477.5807m)
                return false;

            if (input.moneyTestNull != null)
                return false;

            if (input.ncharTest != "你好")
                return false;

            if (input.moneyTestNull != null)
                return false;

            if (input.ntextTest != "你好")
                return false;

            if (input.ntextTestNull != null)
                return false;

            if (input.numericTest != 999999999999999999)
                return false;

            if (input.numericTestNull != null)
                return false;

            if (input.nvarcharTest != "你好")
                return false;

            if (input.nvarcharTestNull != null)
                return false;

            if (input.nvarcharMAXTest != "你好")
                return false;

            if (input.nvarcharMAXTestNull != null)
                return false;

            if (input.realTest != 1234567890.123456789f)
                return false;

            if (input.realTestNull != null)
                return false;

            if (input.smalldatetimeTest != new DateTime(2079, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified))
                return false;

            if (input.smalldatetimeTestNull != null)
                return false;

            if (input.smallintTest != short.MaxValue)
                return false;

            if (input.smallintTestNull != null)
                return false;

            if (input.smallmoneyTest != 214748.3647m)
                return false;

            if (input.smallmoneyTestNull != null)
                return false;

            if (input.sqlvariantTest.Length == 1 && input.sqlvariantTest[0] != 0xFF)
                return false;

            if (input.sqlvariantTestNull != null)
                return false;

            if (input.textTest != "The quick brown fox Jumped over the lazy dog")
                return false;

            if (input.textTestNull != null)
                return false;

            if (input.timeTest != new TimeSpan(0, 23, 59, 59, 0))
                return false;

            if (input.timeTestNull != null)
                return false;

            if (input.tinyintTest != byte.MaxValue)
                return false;

            if (input.tinyintTestNull != null)
                return false;

            //NYI
            //uniqueidentifierTest
            //uniqueidentifierTestNull

            if (input.varbinaryTest.Length == 1 && input.varbinaryTest[0] != 0xFF)
                return false;

            if (input.varbinaryTestNull != null)
                return false;

            if (input.varbinaryMAXTest.Length == 1 && input.varbinaryMAXTest[0] != 0xFF)
                return false;

            if (input.varbinaryMAXTestNull != null)
                return false;

            if (input.varcharTest != "abcdefghijklmnopqrstuvwxyz")
                return false;

            if (input.varcharTestNull != null)
                return false;

            if (input.varcharMAXTest != "abcdefghijklmnopqrstuvwxyz")
                return false;

            if (input.varcharMAXTestNull != null)
                return false;

            if (input.xmlTest != "<Root><ProductDescription ProductModelID=\"5\"><Summary>Some Text</Summary></ProductDescription></Root>")
                return false;

            if (input.xmlTestNull != null)
                return false;

            // cannot say a lot about this since we cannot set a specific value
            if (input.timestampTest == null && input.timestampTest.Length == 26)
                return false;

            return true;
        }

        public static List<DbTestTable> ParseDatareader(SqlDataReader reader)
        {
            var output = new List<DbTestTable>();

            while (reader.Read())
            {
                var buffer = new DbTestTable
                {
                    Id = (int)reader["Id"],

                    bigintTest = (long)reader["bigintTest"],
                    bigintTestNull = (reader["bigintTestNull"] == DBNull.Value) ? null : (long?)reader["bigintTestNull"],

                    binaryTest = (byte[])reader["binaryTest"],
                    binaryTestNull = (reader["binaryTestNull"] == DBNull.Value) ? null : (byte[])reader["binaryTestNull"],

                    bitTest = (bool)reader["bitTest"],
                    bitTestNull = (reader["bitTestNull"] == DBNull.Value) ? null : (bool?)reader["bitTestNull"],

                    charTest = (string)reader["charTest"],
                    charTestNull = (reader["charTestNull"] == DBNull.Value) ? null : (string)reader["charTestNull"],

                    dateTest = (DateTime)reader["dateTest"],
                    dateTestNull = (reader["dateTestNull"] == DBNull.Value) ? null : (DateTime?)reader["dateTestNull"],

                    datetimeTest = (DateTime)reader["datetimeTest"],
                    datetimeTestNull = (reader["datetimeTestNull"] == DBNull.Value) ? null : (DateTime?)reader["datetimeTestNull"],

                    datetime2Test = (DateTime)reader["datetime2Test"],
                    datetime2TestNull = (reader["datetime2TestNull"] == DBNull.Value) ? null : (DateTime?)reader["datetime2Test"],

                    datetimeoffsetTest = (DateTimeOffset)reader["datetimeoffsetTest"],
                    datetimeoffsetTestNull = (reader["datetimeoffsetTestNull"] == DBNull.Value) ? null : (DateTimeOffset?)reader["datetimeoffsetTestNull"],

                    decimalTest = (decimal)reader["decimalTest"],
                    decimalTestNull = (reader["decimalTestNull"] == DBNull.Value) ? null : (decimal?)reader["decimalTestNull"],

                    floatTest = (double)reader["floatTest"],
                    floatTestNull = (reader["floatTestNull"] == DBNull.Value) ? null : (double?)reader["floatTestNull"],

                    imageTest = (byte[])reader["imageTest"],
                    imageTestNull = (reader["imageTestNull"] == DBNull.Value) ? null : (byte[])reader["imageTestNull"],

                    intTest = (int)reader["intTest"],
                    intTestNull = (reader["intTestNull"] == DBNull.Value) ? null : (int?)reader["intTestNull"],

                    //NYI
                    //geographyTest
                    //geographyTestNull
                    //geometryTest
                    //geometryTestNull
                    //heiarchyIdTest
                    //heiarchyIdTestNull

                    moneyTest = (decimal)reader["moneyTest"],
                    moneyTestNull = (reader["moneyTestNull"] == DBNull.Value) ? null : (decimal?)reader["moneyTestNull"],

                    ncharTest = (string)reader["ncharTest"],
                    ncharTestNull = (reader["ncharTestNull"] == DBNull.Value) ? null : (string)reader["ncharTestNull"],

                    ntextTest = (string)reader["ntextTest"],
                    ntextTestNull = (reader["ntextTestNull"] == DBNull.Value) ? null : (string)reader["ntextTestNull"],

                    numericTest = (decimal)reader["numericTest"],
                    numericTestNull = (reader["numericTestNull"] == DBNull.Value) ? null : (decimal?)reader["numericTestNull"],

                    nvarcharTest = (string)reader["nvarcharTest"],
                    nvarcharTestNull = (reader["nvarcharTestNull"] == DBNull.Value) ? null : (string)reader["nvarcharTestNull"],

                    nvarcharMAXTest = (string)reader["nvarcharMAXTest"],
                    nvarcharMAXTestNull = (reader["nvarcharMAXTestNull"] == DBNull.Value) ? null : (string)reader["nvarcharMAXTestNull"],

                    realTest = (float)reader["realTest"],
                    realTestNull = (reader["realTestNull"] == DBNull.Value) ? null : (float?)reader["realTestNull"],

                    smalldatetimeTest = (DateTime)reader["smalldatetimeTest"],
                    smalldatetimeTestNull = (reader["smalldatetimeTestNull"] == DBNull.Value) ? null : (DateTime?)reader["smalldatetimeTestNull"],

                    smallintTest = (short)reader["smallintTest"],
                    smallintTestNull = (reader["smallintTestNull"] == DBNull.Value) ? null : (short?)reader["smallintTestNull"],

                    smallmoneyTest = (decimal)reader["smallmoneyTest"],
                    smallmoneyTestNull = (reader["smallmoneyTestNull"] == DBNull.Value) ? null : (decimal?)reader["smallmoneyTestNull"],

                    sqlvariantTest = (byte[])reader["sqlvariantTest"],
                    sqlvariantTestNull = (reader["sqlvariantTestNull"] == DBNull.Value) ? null : (byte[])reader["sqlvariantTestNull"],

                    textTest = (string)reader["textTest"],
                    textTestNull = (reader["textTestNull"] == DBNull.Value) ? null : (string)reader["textTestNull"],

                    timeTest = (TimeSpan)reader["timeTest"],
                    timeTestNull = (reader["timeTestNull"] == DBNull.Value) ? null : (TimeSpan?)reader["timeTestNull"],

                    tinyintTest = (byte)reader["tinyintTest"],
                    tinyintTestNull = (reader["tinyintTestNull"] == DBNull.Value) ? null : (byte)reader["tinyintTestNull"],

                    //NYI
                    //uniqueidentifierTest
                    //uniqueidentifierTestNull

                    varbinaryTest = (byte[])reader["varbinaryTest"],
                    varbinaryTestNull = (reader["varbinaryTestNull"] == DBNull.Value) ? null : (byte[])reader["varbinaryTestNull"],

                    varbinaryMAXTest = (byte[])reader["varbinaryMAXTest"],
                    varbinaryMAXTestNull = (reader["varbinaryMAXTestNull"] == DBNull.Value) ? null : (byte[])reader["varbinaryMAXTestNull"],

                    varcharTest = (string)reader["varcharTest"],
                    varcharTestNull = (reader["varcharTestNull"] == DBNull.Value) ? null : (string)reader["varcharTestNull"],

                    varcharMAXTest = (string)reader["varcharMAXTest"],
                    varcharMAXTestNull = (reader["varcharMAXTestNull"] == DBNull.Value) ? null : (string)reader["varcharMAXTestNull"],

                    xmlTest = (string)reader["xmlTest"],
                    xmlTestNull = (reader["xmlTestNull"] == DBNull.Value) ? null : (string)reader["xmlTestNull"],

                    timestampTest = (byte[])reader["timestampTest"],
                };

                output.Add(buffer);
            }

            return output;
        }

        public static async Task<List<DbTestTable>> ParseDatareaderAsync(SqlDataReader reader)
        {
            var result = ParseDatareader(reader);
            return await Task.FromResult(result);
        }
    }
}