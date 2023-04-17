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

            if (input.binaryTestNull == null)
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
                };


                //public bool bitTest { get; set; }
                //public bool? bitTestNull { get; set; }

                //public string charTest { get; set; }
                //public string charTestNull { get; set; }

                //public DateTime dateTest { get; set; }
                //public DateTime? dateTestNull { get; set; }

                //public DateTime datetimeTest { get; set; }
                //public DateTime? datetimeTestNull { get; set; }

                //public DateTime datetime2Test { get; set; }
                //public DateTime datetime2TestNull { get; set; }

                //public DateTime datetimeoffsetTest { get; set; }
                //public DateTime? datetimeoffsetTestNull { get; set; }

                //public decimal decimalTest { get; set; }
                //public decimal? decimalTestNull { get; set; }

                //public double floatTest { get; set; }
                //public double? floatTestNull { get; set; }

                //public byte[] imageTest { get; set; }
                //public byte[] imageTestNull { get; set; }

                //public int intTest { get; set; }
                //public int? intTestNull { get; set; }

                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public SqlGeography geographyTest { get; set; }

                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public SqlGeography geographyTestNull { get; set; }

                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public SqlGeometry geometryTest { get; set; }

                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public SqlGeometry geometryTestNull { get; set; }

                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public SqlHierarchyId heiarchyIdTest { get; set; }

                ///// <summary>
                ///// NYI - ADO does not support this type
                ///// </summary>
                //public SqlHierarchyId heiarchyIdTestNull { get; set; }

                //public decimal moneyTest { get; set; }
                //public decimal? moneyTestNull { get; set; }
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