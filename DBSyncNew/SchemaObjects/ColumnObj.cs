using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Interfaces;

namespace DBSyncNew.SchemaObjects
{
    public class ColumnObj
    {
        private ColumnObj(Builder builder)
        {
            Table = builder.Table;
            Name = builder.DBColumn.Name;
            IsPk = builder.DBColumn.IsPrimaryKey;
            DataType = builder.DBColumn.DataType;
            Precision = builder.DBColumn.Precision;
            Scale = builder.DBColumn.Scale;
            MaxLength = builder.DBColumn.MaxLength;
            IsNullable = builder.DBColumn.IsNullable;
            IsReadOnly = builder.DBColumn.IsReadOnly;
        }

        public TableObj Table { get; }

        public string Name { get; }

        public bool IsPk { get; }

        public bool IsGuid
        {
            get { return DataType == "uniqueidentifier"; }
        }

        public string DataType { get; }

        public byte Precision { get; }

        public byte Scale { get; }

        public int? MaxLength { get; }

        public bool IsNullable { get; }

        //TODO remove
        public ColumnObj ReferencedColumnObj { get; set; }

        public bool IsReadOnly { get; }

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

        public class Builder
        {
            public Builder(TableObj table, IDBColumn dbColumn)
            {
                Table = table;
                DBColumn = dbColumn;
            }

            public TableObj Table { get; }

            public IDBColumn DBColumn { get; }

            public ColumnObj Build()
            {
                return new ColumnObj(this);
            }
        }
    }
}
