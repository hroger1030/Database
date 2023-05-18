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

using System.Data;

namespace DAL.Net.SqlMetadata
{
    public class SqlColumn
    {
        public SqlTable Table { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPk { get; set; }
        public bool IsIdentity { get; set; }
        public int ColumnOrdinal { get; set; }
        public string DefaultValue { get; set; }

        public SqlDbType SqlDataType
        {
            get
            {
                // HACK - SqlDbType has no entry for 'numeric'. Until the twerps in Redmond add this value:
                if (DataType == "numeric")
                    return SqlDbType.Decimal;

                // HACK - SqlDbType has no entry for 'sql_variant'. Until the twerps in Redmond add this value:
                if (DataType == "sql_variant")
                    return SqlDbType.Variant;

                return (SqlDbType)Enum.Parse(typeof(SqlDbType), DataType!, true);
            }
        }
        public eSqlBaseType BaseType
        {
            get { return MapBaseType(); }
        }

        public SqlColumn() { }

        public SqlColumn(SqlTable sqlTable, string columnName) : this(sqlTable, columnName, string.Empty, 0, 0, 0, false, false, false, 0, string.Empty) { }

        public SqlColumn(SqlTable sqlTable, string columnName, string datatype, int length, int precision, int scale, bool isNullable, bool isPk, bool isIdentity, int columnOrdinal, string defaultValue)
        {
            Table = sqlTable;

            Name = columnName;
            DataType = datatype;
            Length = length;
            Precision = precision;
            Scale = scale;
            IsNullable = isNullable;
            IsPk = isPk;
            IsIdentity = isIdentity;
            ColumnOrdinal = columnOrdinal;
            DefaultValue = defaultValue;
        }

        protected eSqlBaseType MapBaseType()
        {
            SqlDbType sqlType = (SqlDbType)Enum.Parse(typeof(SqlDbType), DataType!, true);

            return sqlType switch
            {
                SqlDbType.BigInt => eSqlBaseType.Integer,
                SqlDbType.Binary => eSqlBaseType.BinaryData,
                SqlDbType.Bit => eSqlBaseType.Bool,
                SqlDbType.Char => eSqlBaseType.String,
                SqlDbType.Date => eSqlBaseType.Time,
                SqlDbType.DateTime => eSqlBaseType.Time,
                SqlDbType.DateTime2 => eSqlBaseType.Time,
                SqlDbType.DateTimeOffset => eSqlBaseType.Time,
                SqlDbType.Decimal => eSqlBaseType.Float,
                SqlDbType.Float => eSqlBaseType.Float,
                //case SqlDbType.Geography: 
                //case SqlDbType.Geometry: 
                SqlDbType.Image => eSqlBaseType.BinaryData,
                SqlDbType.Int => eSqlBaseType.Integer,
                SqlDbType.Money => eSqlBaseType.Float,
                SqlDbType.NChar => eSqlBaseType.String,
                SqlDbType.NText => eSqlBaseType.String,
                SqlDbType.NVarChar => eSqlBaseType.String,
                SqlDbType.Real => eSqlBaseType.Float,
                SqlDbType.SmallDateTime => eSqlBaseType.Time,
                SqlDbType.SmallInt => eSqlBaseType.Integer,
                SqlDbType.SmallMoney => eSqlBaseType.Float,
                SqlDbType.Structured => eSqlBaseType.String,
                SqlDbType.Text => eSqlBaseType.String,
                SqlDbType.Time => eSqlBaseType.Time,
                SqlDbType.Timestamp => eSqlBaseType.BinaryData,
                SqlDbType.TinyInt => eSqlBaseType.Integer,
                SqlDbType.Udt => eSqlBaseType.String,
                SqlDbType.UniqueIdentifier => eSqlBaseType.Guid,
                SqlDbType.VarBinary => eSqlBaseType.BinaryData,
                SqlDbType.VarChar => eSqlBaseType.String,
                SqlDbType.Variant => eSqlBaseType.String,
                SqlDbType.Xml => eSqlBaseType.String,
                _ => eSqlBaseType.Unknown,
            };
        }
    }
}
