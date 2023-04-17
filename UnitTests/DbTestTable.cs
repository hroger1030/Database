using Microsoft.SqlServer.Types;
using System;

namespace UnitTests
{
    /// <summary>
    /// This is a POCO container that maps to the SQL test table.
    /// Note that several 
    /// </summary>
    public class DbTestTable
    {
        public int Id { get; set; }

        public long bigintTest { get; set; }

        public long? bigintTestNull { get; set; }

        public byte[] binaryTest { get; set; }

        public byte[] binaryTestNull { get; set; }

        public bool bitTest { get; set; }

        public bool? bitTestNull { get; set; }

        public string charTest { get; set; }

        public string charTestNull { get; set; }

        public DateTime dateTest { get; set; }

        public DateTime? dateTestNull { get; set; }

        public DateTime datetimeTest { get; set; }

        public DateTime? datetimeTestNull { get; set; }

        public DateTime datetime2Test { get; set; }

        public DateTime? datetime2TestNull { get; set; }

        public DateTimeOffset datetimeoffsetTest { get; set; }

        public DateTimeOffset? datetimeoffsetTestNull { get; set; }

        public decimal decimalTest { get; set; }

        public decimal? decimalTestNull { get; set; }

        public double floatTest { get; set; }

        public double? floatTestNull { get; set; }

        public byte[] imageTest { get; set; }

        public byte[] imageTestNull { get; set; }

        public int intTest { get; set; }

        public int? intTestNull { get; set; }

        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public SqlGeography geographyTest { get; set; }

        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public SqlGeography geographyTestNull { get; set; }

        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public SqlGeometry geometryTest { get; set; }

        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public SqlGeometry geometryTestNull { get; set; }

        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public SqlHierarchyId heiarchyIdTest { get; set; }

        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public SqlHierarchyId heiarchyIdTestNull { get; set; }

        public decimal moneyTest { get; set; }

        public decimal? moneyTestNull { get; set; }

        public string ncharTest { get; set; }

        public string ncharTestNull { get; set; }

        public string ntextTest { get; set; }

        public string ntextTestNull { get; set; }

        public decimal numericTest { get; set; }

        public decimal? numericTestNull { get; set; }

        public string nvarcharTest { get; set; }

        public string nvarcharTestNull { get; set; }

        public string nvarcharMAXTest { get; set; }

        public string nvarcharMAXTestNull { get; set; }

        public float realTest { get; set; }

        public float? realTestNull { get; set; }

        public DateTime smalldatetimeTest { get; set; }

        public DateTime? smalldatetimeTestNull { get; set; }

        public short smallintTest { get; set; }

        public short? smallintTestNull { get; set; }

        public decimal smallmoneyTest { get; set; }

        public decimal? smallmoneyTestNull { get; set; }

        public object sql_variantTest { get; set; }

        public object sql_variantTestNull { get; set; }

        public string textTest { get; set; }

        public string textTestNull { get; set; }

        public TimeSpan timeTest { get; set; }

        public TimeSpan? timeTestNull { get; set; }

        public byte tinyintTest { get; set; }

        public byte? tinyintTestNull { get; set; }

        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public byte[] uniqueidentifierTest { get; set; }
        /// <summary>
        /// NYI - ADO does not support this type
        /// </summary>
        public byte[] uniqueidentifierTestNull { get; set; }

        public byte[] varbinaryTest { get; set; }

        public byte[] varbinaryTestNull { get; set; }

        public byte[] varbinaryMAXTest { get; set; }

        public byte[] varbinaryMAXTestNull { get; set; }

        public string varcharTest { get; set; }

        public string varcharTestNull { get; set; }

        public string varcharMAXTest { get; set; }

        public string varcharMAXTestNull { get; set; }

        public string xmlTest { get; set; }

        public string xmlTestNull { get; set; }

        public byte[] timestampTest { get; set; }
    }
}
