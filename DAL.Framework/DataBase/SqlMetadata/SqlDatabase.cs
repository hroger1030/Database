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
using System.Text;

namespace DAL.Framework.SqlMetadata
{
    public class SqlDatabase
    {
        public const string DEFAULT_CONNECTION_STRING = "Data Source=Localhost;Initial Catalog=Master;Integrated Security=SSPI;Connect Timeout=1;";

        public string Name { get; set; }
        public Dictionary<string, SqlTable> Tables { get; set; }
        public Dictionary<string, SqlScript> StoredProcedures { get; set; }
        public Dictionary<string, SqlScript> Functions { get; set; }
        public Dictionary<string, SqlConstraint> Constraints { get; set; }
        public string ConnectionString { get; set; }

        public string FormattedDatabaseName
        {
            get { return $"[{Name}]"; }
        }

        public SqlDatabase()
        {
            Reset();
        }

        private void Reset()
        {
            Name = string.Empty;
            Tables = new Dictionary<string, SqlTable>();
            StoredProcedures = new Dictionary<string, SqlScript>();
            Functions = new Dictionary<string, SqlScript>();
            Constraints = new Dictionary<string, SqlConstraint>();
            ConnectionString = string.Empty;
        }

        public void LoadDatabaseMetadata(string databaseName, string connectionString)
        {
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentException("Database name is null or empty");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string name is null or empty");

            Reset();

            Name = databaseName;
            ConnectionString = connectionString;

            // load and parse out table data
            string sqlQuery = GetTableData();

            var db = new Database(ConnectionString);
            DataTable dt = db.ExecuteQuery(sqlQuery, null);

            if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string tableName = (string)dr["TableName"];
                    string columnName = (string)dr["ColumnName"];
                    string schemaName = (string)dr["SchemaName"];

                    // because tables are tied to the schema they are in, we need to make sure that
                    // the schema is included with the table name.
                    string fullTableName = $"{schemaName}.{tableName}";

                    if (!Tables.ContainsKey(fullTableName))
                    {
                        SqlTable sqlTable = new SqlTable(this, schemaName, tableName);
                        Tables.Add(fullTableName, sqlTable);
                    }

                    var sql_column = new SqlColumn
                    {
                        Schema = (string)dr["SchemaName"],
                        Table = Tables[fullTableName],
                        Name = (string)dr["ColumnName"],
                        DataType = (string)dr["DataType"],
                        Length = Convert.ToInt32(dr["Length"]),
                        Precision = Convert.ToInt32(dr["Precision"]),
                        IsNullable = Convert.ToBoolean(dr["IsNullable"]),
                        IsPk = Convert.ToBoolean(dr["IsPK"]),
                        IsIdentity = Convert.ToBoolean(dr["IsIdentity"]),
                        ColumnOrdinal = Convert.ToInt32(dr["ColumnOrdinal"]),
                        DefaultValue = (dr["DefaultValue"] == DBNull.Value) ? string.Empty : RemoveWrappingCharacters((string)dr["DefaultValue"])
                    };

                    if (Tables[fullTableName].Columns.ContainsKey(columnName))
                        throw new Exception($"Column {columnName} already exists in table {Tables[tableName]}");
                    else
                        Tables[fullTableName].Columns.Add(columnName, sql_column);
                }
            }

            // get SP
            sqlQuery = GetStoredProcedures();
            db = new Database(ConnectionString);
            dt = db.ExecuteQuery(sqlQuery, null);

            if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    SqlScript sql_script = new SqlScript
                    {
                        Name = (string)dr["Name"],
                        Body = (string)dr["Body"]
                    };

                    if (StoredProcedures.ContainsKey(sql_script.Name))
                        StoredProcedures[sql_script.Name].Body += sql_script.Body;
                    else
                        StoredProcedures.Add(sql_script.Name, sql_script);
                }
            }

            // get functions
            sqlQuery = GetFunctions();
            db = new Database(ConnectionString);
            dt = db.ExecuteQuery(sqlQuery, null);

            if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    SqlScript sql_script = new SqlScript
                    {
                        Name = (string)dr["Name"],
                        Body = (string)dr["Body"]
                    };

                    if (Functions.ContainsKey(sql_script.Name))
                        Functions[sql_script.Name].Body += sql_script.Body;
                    else
                        Functions.Add(sql_script.Name, sql_script);
                }
            }

            // get constraints
            sqlQuery = GetConstraints();
            db = new Database(ConnectionString);
            dt = db.ExecuteQuery(sqlQuery, null);

            if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    SqlConstraint sql_constraint = new SqlConstraint
                    {
                        ConstraintName = (string)dr["ConstraintName"],
                        FKTable = (string)dr["FKTable"],
                        FKColumn = (string)dr["FKColumn"],
                        PKTable = (string)dr["PKTable"],
                        PKColumn = (string)dr["PKColumn"]
                    };

                    if (Constraints.ContainsKey(sql_constraint.ConstraintName))
                        throw new Exception($"Constraint {sql_constraint.ConstraintName} already exists");
                    else
                        Constraints.Add(sql_constraint.ConstraintName, sql_constraint);
                }
            }
            return;
        }

        /// <summary>
        /// Generates SQL procedure to pull db info out of a table.
        /// Updated for SQL 2008, but several types are not supported:
        /// sysname, timestamp, hierarchyid, geometry, geography
        /// </summary>
        protected string GetTableData()
        {
            /*
            USE [<db>] 

            SELECT	ss.[Name]							AS [SchemaName],
		            so.[Name]							AS [TableName],
                    sc.[Name]				            AS [ColumnName],
                    st.[name]							AS [DataType],
                    sc.[max_length]						AS [Length],
                    sc.[precision]			            AS [Precision],
                    sc.[scale]				            AS [Scale],	 
                    sc.[is_nullable]		            AS [IsNullable],
					ISNULL(si.[is_primary_key],0)		AS [IsPK],
					sc.[is_identity]		            AS [IsIdentity],
                    sc.[column_id]			            AS [ColumnOrdinal],
					OBJECT_DEFINITION(sc.default_object_id) AS [Default Value]

            FROM	sys.objects so
                    INNER JOIN sys.columns sc ON so.object_id = sc.object_id
                    INNER JOIN sys.types st ON sc.system_type_id = st.system_type_id
		            INNER JOIN sys.schemas ss on so.schema_id = ss.schema_id
					LEFT JOIN sys.index_columns sic  ON sic.object_id = sc.object_id AND sic.column_id = sc.column_id
					LEFT JOIN sys.indexes si ON so.object_id = si.object_id AND sic.index_id = si.index_id

            WHERE	so.type = 'U'
            AND     st.[name] NOT IN ('sysname','timestamp','hierarchyid','geometry','geography')
            AND     st.is_user_defined = 0

            ORDER	BY ss.[Name], so.[name], sc.[column_id]
            */

            var sb = new StringBuilder();

            sb.AppendLine("USE [" + Name + "]");
            sb.AppendLine("SELECT ss.[Name] AS [SchemaName],");
            sb.AppendLine("so.[Name] AS [TableName],");
            sb.AppendLine("sc.[Name] AS [ColumnName],");
            sb.AppendLine("st.[name] AS [DataType],");
            sb.AppendLine("sc.[max_length] AS [Length],");
            sb.AppendLine("sc.[precision] AS [Precision],");
            sb.AppendLine("sc.[scale] AS [Scale],");
            sb.AppendLine("sc.[is_nullable] AS [IsNullable],");
            sb.AppendLine("ISNULL(si.[is_primary_key],0) AS [IsPK],");
            sb.AppendLine("sc.[is_identity] AS [IsIdentity],");
            sb.AppendLine("sc.[column_id] AS [ColumnOrdinal],");
            sb.AppendLine("OBJECT_DEFINITION(sc.default_object_id) AS [DefaultValue]");

            sb.AppendLine("FROM sys.objects so");
            sb.AppendLine("INNER JOIN sys.columns sc ON so.object_id = sc.object_id");
            sb.AppendLine("INNER JOIN sys.types st ON sc.system_type_id = st.system_type_id");
            sb.AppendLine("INNER JOIN sys.schemas ss on so.schema_id = ss.schema_id");
            sb.AppendLine("LEFT JOIN sys.index_columns sic  ON sic.object_id = sc.object_id AND sic.column_id = sc.column_id");
            sb.AppendLine("LEFT JOIN sys.indexes si ON so.object_id = si.object_id AND sic.index_id = si.index_id");

            sb.AppendLine("WHERE so.type = 'U'");
            sb.AppendLine("AND st.[name] NOT IN ('sysname','timestamp','hierarchyid','geometry','geography')");
            sb.AppendLine("AND st.is_user_defined = 0");
            sb.AppendLine("ORDER BY ss.[Name], so.[name], sc.[column_id]");

            return sb.ToString();
        }

        protected string GetStoredProcedures()
        {
            /*
            USE [<db>] 

            SELECT	sys.objects.name	AS [Name],
                    syscomments.text	AS [Body] 
            FROM	sys.objects
                    INNER JOIN syscomments ON sys.objects.object_id = syscomments.id
            WHERE	sys.objects.type = 'p'
            AND		sys.objects.is_ms_shipped = 0
            ORDER	BY sys.objects.name
             */

            var sb = new StringBuilder();

            sb.AppendLine(" USE [" + Name + "]");
            sb.AppendLine(" SELECT sys.objects.name	AS [Name],");
            sb.AppendLine(" syscomments.text AS [Body]");
            sb.AppendLine(" FROM sys.objects");
            sb.AppendLine(" INNER JOIN syscomments ON sys.objects.object_id = syscomments.id");
            sb.AppendLine(" WHERE sys.objects.type = 'p'");
            sb.AppendLine(" AND sys.objects.is_ms_shipped = 0");
            sb.AppendLine(" ORDER BY sys.objects.name, syscomments.colid");

            return sb.ToString();
        }

        protected string GetFunctions()
        {
            /*
            USE [<db>] 

            SELECT	sys.objects.name	AS [Name],
                    syscomments.text	AS [Body] 
            FROM	sys.objects
                    INNER JOIN syscomments ON sys.objects.object_id = syscomments.id
            WHERE	sys.objects.type = 'fn'
            AND		sys.objects.is_ms_shipped = 0
            ORDER	BY sys.objects.name
            */

            var sb = new StringBuilder();

            sb.AppendLine(" USE [" + Name + "]");
            sb.AppendLine(" SELECT sys.objects.name AS [Name],");
            sb.AppendLine(" syscomments.text AS [Body]");
            sb.AppendLine(" FROM sys.objects");
            sb.AppendLine(" INNER JOIN syscomments ON sys.objects.object_id = syscomments.id");
            sb.AppendLine(" WHERE sys.objects.type = 'fn'");
            sb.AppendLine(" AND sys.objects.is_ms_shipped = 0");
            sb.AppendLine(" ORDER BY sys.objects.name, syscomments.colid");

            return sb.ToString();
        }

        protected string GetConstraints()
        {
            /*
            USE [<db>] 

            SELECT	C.CONSTRAINT_NAME	AS [ConstraintName],
                    FK.TABLE_NAME		AS FKTable,
                    CU.COLUMN_NAME		AS FKColumn,
                    PK.TABLE_NAME		AS PKTable,
                    PT.COLUMN_NAME		AS PKColumn

            FROM	INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
                    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
                    INNER JOIN 
                    (
                        SELECT	i1.TABLE_NAME, 
                                i2.COLUMN_NAME
                        FROM	INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                        WHERE	i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    ) PT ON PT.TABLE_NAME = PK.TABLE_NAME

            ORDER BY
            C.CONSTRAINT_NAME
            */

            var sb = new StringBuilder();

            sb.AppendLine(" USE [" + Name + "]");
            sb.AppendLine(" SELECT C.CONSTRAINT_NAME AS [ConstraintName],");
            sb.AppendLine(" FK.TABLE_NAME AS FKTable,");
            sb.AppendLine(" CU.COLUMN_NAME AS FKColumn,");
            sb.AppendLine(" PK.TABLE_NAME AS PKTable,");
            sb.AppendLine(" PT.COLUMN_NAME AS PKColumn");

            sb.AppendLine(" FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C");
            sb.AppendLine(" INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME");
            sb.AppendLine(" INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME");
            sb.AppendLine(" INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME");
            sb.AppendLine(" INNER JOIN (");
            sb.AppendLine(" SELECT i1.TABLE_NAME,");
            sb.AppendLine(" i2.COLUMN_NAME");
            sb.AppendLine(" FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1");
            sb.AppendLine(" INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME");
            sb.AppendLine(" WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'");
            sb.AppendLine(" ) PT ON PT.TABLE_NAME = PK.TABLE_NAME");
            sb.AppendLine(" ORDER BY");
            sb.AppendLine(" C.CONSTRAINT_NAME");

            return sb.ToString();
        }

        /// <summary>
        /// gets rid of characters that wrap a sql default value
        /// Ex: ('Something') -> Something
        /// </summary>
        protected string RemoveWrappingCharacters(string input)
        {
            if (input.Length > 1 && (input[0] == '(' || input[0] == '\''))
                input = input.Substring(1, input.Length - 2);

            if (input.Length > 1 && (input[0] == '(' || input[0] == '\''))
                input = input.Substring(1, input.Length - 2);

            return input;
        }
    }
}
