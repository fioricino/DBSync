using System;
using System.Collections.Generic;

namespace DBSync
{
    public class ColumnInfo
    {
        public ColumnInfo(TableInfo parent)
        {
            Table = parent;
            Precisions = new List<string>();
        }

        public string Name { get; set; }

        public bool IsPk { get; set; }

        public bool IsFk { get { return ReferencedColumn != null; } }

        public bool IsGuid
        {
            get { return DataType == "uniqueidentifier"; }
        }

        public string DataType { get; set; }

        public byte Precision { get; set; }

        public byte Scale { get; set; }

        public List<string> Precisions { get; set; } 

        public int? MaxLength { get; set; }

        public bool IsNullable { get; set; }

        public TableInfo Table { get; private set; }

        public ColumnInfo ReferencedColumn { get; set; }

        public bool IsReadOnly { get; set; }

        public string QuotedName
        {
            get { return String.Format("[{0}]", Name); }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0}.{1}", Table, Name);
        }

        public string ToSqlString()
        {
            return String.Format("[{0}] {1} {2} {3}", Name, DataType, GetDataTypeModifiers(),
                IsNullable ? "NULL" : "NOT NULL");
        }

        private string GetDataTypeModifiers()
        {
            var result = new List<string>();

            switch (DataType)
            {
                case "int":
                case "smallint":
                case "uniqueidentifier":
                case "tinyint":
                case "date":
                case "bigint":
                case "bit":
                case "datetime":
                case "xml":
                case "timestamp":
                case "money":
                    break;
                case "datetime2":
                case "time":
                    result.Add(Scale.ToString());
                    break;
                case "decimal":
                    result.Add(Precision.ToString());
                    result.Add(Scale.ToString());
                    break;
                case "varbinary":
                case "varchar":
                case "char":
                case "nvarchar":
                    result.Add(MaxLength.ToString().Replace("-1", "max"));
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Data type {0} not implemented.", DataType));
            }

            return result.Count == 0 ? String.Empty : String.Format("({0})", String.Join(",", result));
        }
    }
}
