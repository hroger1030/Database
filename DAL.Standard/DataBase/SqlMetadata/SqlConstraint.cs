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

using System.Text;

namespace DAL.Standard.SqlMetadata
{
    public class SqlConstraint
    {
        public string ConstraintName { get; set; } = string.Empty;
        public string FKTable { get; set; } = string.Empty;
        public string FKColumn { get; set; } = string.Empty;
        public string PKTable { get; set; } = string.Empty;
        public string PKColumn { get; set; } = string.Empty;

        public SqlConstraint() { }

        public SqlConstraint(string constraintName, string fkTable, string fkColumn, string pkTable, string pkColumn)
        {
            ConstraintName = constraintName;
            FKTable = fkTable;
            FKColumn = fkColumn;
            PKTable = pkTable;
            PKColumn = pkColumn;
        }

        public string GenerateSQLScript()
        {
            var sb = new StringBuilder();

            sb.AppendLine(" ALTER TABLE " + FKTable);
            sb.AppendLine(" ADD CONSTRAINT " + ConstraintName);
            sb.AppendLine(" FOREIGN KEY(" + FKColumn + ")");
            sb.AppendLine(" REFERENCES " + PKTable + "(" + PKColumn + ");");

            return sb.ToString();
        }

        public void GenerateConstraintName()
        {
            ConstraintName = $"FK_{FKTable}_{PKTable}_{GetHashCode()}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            SqlConstraint other = (SqlConstraint)obj;

            if (FKTable != other.FKTable)
                return false;

            if (FKColumn != other.FKColumn)
                return false;

            if (PKTable != other.PKTable)
                return false;

            if (PKColumn != other.PKColumn)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return FKTable.GetHashCode() ^ (FKColumn.GetHashCode() * 1997) ^ (PKTable.GetHashCode() * 83) ^ (PKColumn.GetHashCode() * 389);
            }
        }

        public override string ToString()
        {
            return $"[{PKTable}].[{PKColumn}] = [{FKTable}].[{FKColumn}]";
        }
    }
}
