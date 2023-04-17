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
using System;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

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

        public readonly static string QUERY_BASIC_SELECT = $"select * from [{DB_NAME}].[{SCHEMA_NAME}].[{TABLE_NAME}] order by Id";

        public readonly static string QUERY_BASIC_SELECT_WITH_PARAMETERS = $"select * from [{DB_NAME}].[{SCHEMA_NAME}].[{TABLE_NAME}] where Id = @Id order by Id";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        public readonly static string STOREDPROC_SELECT_WITH_PARAMETERS = $"[{DB_NAME}].[{SCHEMA_NAME}].[{PROCEDURE_NAME}]";


        /// <summary>
        /// We are seeding the database with known values, so we can test the results.
        /// If any of the constants are altered, this function will need to be updated to match.
        /// </summary>
        public static bool IsDbTestTableCorrect(DbTestTable input)
        {
            if (input.Id < 1 || input.Id > 3)
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

            if (input.dateTest != new DateTime(9999,12,31,0,0,0,0,DateTimeKind.Utc))
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
                };



                //public string ncharTest { get; set; }
                //public string ncharTestNull { get; set; }
                //public string ntextTest { get; set; }
                //public string ntextTestNull { get; set; }
                //public decimal numericTest { get; set; }
                //public decimal? numericTestNull { get; set; }
                //public string nvarcharTest { get; set; }
                //public string nvarcharTestNull { get; set; }
                //public string nvarcharMAXTest { get; set; }
                //public string nvarcharMAXTestNull { get; set; }
                //public float realTest { get; set; }
                //public float? realTestNull { get; set; }
                //public DateTime smalldatetimeTest { get; set; }
                //public DateTime? smalldatetimeTestNull { get; set; }
                //public short smallintTest { get; set; }
                //public short? smallintTestNull { get; set; }
                //public decimal smallmoneyTest { get; set; }
                //public decimal? smallmoneyTestNull { get; set; }
                //public object sql_variantTest { get; set; }
                //public object sql_variantTestNull { get; set; }
                //public string textTest { get; set; }
                //public string textTestNull { get; set; }
                //public TimeSpan timeTest { get; set; }
                //public TimeSpan? timeTestNull { get; set; }
                //public byte tinyintTest { get; set; }
                //public byte? tinyintTestNull { get; set; }

                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public byte[] uniqueidentifierTest { get; set; }
                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public byte[] uniqueidentifierTestNull { get; set; }

                //public byte[] varbinaryTest { get; set; }
                //public byte[] varbinaryTestNull { get; set; }
                //public byte[] varbinaryMAXTest { get; set; }
                //public byte[] varbinaryMAXTestNull { get; set; }
                //public string varcharTest { get; set; }
                //public string varcharTestNull { get; set; }
                //public string varcharMAXTest { get; set; }
                //public string varcharMAXTestNull { get; set; }
                //public string xmlTest { get; set; }
                //public string xmlTestNull { get; set; }
                //public byte[] timestampTest { get; set; }

                output.Add(buffer);
            }

            return output;
        }
    }
}