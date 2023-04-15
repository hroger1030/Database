﻿/*
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
    public class SqlTable
    {
        public SqlDatabase Database { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public Dictionary<string, SqlColumn> Columns { get; set; }
        public Dictionary<string, SqlConstraint> TableConsraints
        {
            get
            {
                var output = new Dictionary<string, SqlConstraint>();

                if (Database != null)
                {
                    foreach (KeyValuePair<string, SqlConstraint> kvp in Database.Constraints)
                    {
                        if (kvp.Value.PKTable == Name || kvp.Value.FKTable == Name)
                        {
                            if (!output.ContainsKey(kvp.Key))
                                output.Add(kvp.Key, kvp.Value);
                        }
                    }
                }

                return output;
            }
        }
        public List<SqlColumn> PkList
        {
            get { return Columns.Values.Where(c => c.IsPk).ToList(); }
        }
        public string[] PkNames
        {
            get { return Columns.Values.Where(c => c.IsPk).Select(c => c.Name!).ToArray(); }
        }
        public string[] ColumnNames
        {
            get { return PkList.Select(c => c.Name!).ToArray(); }
        }
        public string FullName
        {
            get { return $"{Schema}.{Name}"; }
        }

        public SqlTable()
        {
            Database = null!;
            Schema = string.Empty;
            Name = null!;
            Columns = new Dictionary<string, SqlColumn>();
        }

        public SqlTable(SqlDatabase sqlDatabase, string schemaName, string tableName)
        {
            Database = sqlDatabase;
            Schema = schemaName;
            Name = tableName;
            Columns = new Dictionary<string, SqlColumn>();
        }

        protected void GetColumnMetaData(DataTable dt)
        {
            Columns.Clear();

            if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
            {
                SqlColumn obj;

                foreach (DataRow dr in dt.Rows)
                {
                    // For some strange reason, if a column's type is nvarchar SQL2K
                    // will add an additional entry to the syscolumns table with the 
                    // type listed as a sysname. Since we don't want duplicate entries, omit.

                    //if ((string)dr["DataType"] == "sysname")
                    //    continue;

                    obj = new SqlColumn
                    (
                        this,
                        (string)dr["ColumnName"],
                        (string)dr["DataType"],
                        (int)dr["Length"],
                        (int)dr["Precision"],
                        (int)dr["Scale"],
                        (bool)dr["IsNullable"],
                        (bool)dr["IsPK"],
                        (bool)dr["IsIdentity"],
                        (int)dr["ColumnOrdinal"],
                        (string)dr["DefaultValue"]
                    );

                    Columns.Add(obj.Name!, obj);
                }
            }
            else
            {
                throw new Exception("Cannot retrieve metadata for table " + Name + ".");
            }
        }

        /// <summary>
        /// There are a number of cases where we need to get some sort of single Id column
        /// for searches or hash collections. Uses identity first if avaialble, otherwise first pk.
        /// This will return null if there are no pks or identity columns set, or if there is a 
        /// composite key on th table
        /// </summary>
        public SqlColumn GetIdColumn()
        {
            foreach (var column in Columns)
            {
                if (column.Value.IsIdentity)
                    return column.Value;
            }

            var pk_list = PkList;

            if (pk_list.Count == 0 || pk_list.Count > 1)
                return null!;
            else
                return PkList.FirstOrDefault()!;
        }
    }
}
