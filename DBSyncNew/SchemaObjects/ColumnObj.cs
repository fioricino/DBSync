using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Read.Interfaces;

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

        //TODO extract object
        public string DataType { get; }

        public byte Precision { get; }

        public byte Scale { get; }

        public int? MaxLength { get; }

        public bool IsNullable { get; }

        //TODO remove
        public ColumnObj ReferencedColumnObj { get; set; }

        public bool IsReadOnly { get; }

      
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
