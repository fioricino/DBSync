using System.Data;
using DBSyncNew.Database.Read.Interfaces;

namespace DBSyncNew.Database.Read.MsSql
{
    public class MsSqlColumn : IDBColumn
    {
        private DataRow dataRow;

        public MsSqlColumn(DataRow dataRow)
        {
            this.dataRow = dataRow;
        }

        public string Name => dataRow.Field<string>("COLUMN_NAME");

        public bool IsPrimaryKey => dataRow.Field<bool>("IS_PK");

        public bool IsNullable => dataRow.Field<bool>("IS_NULLABLE");

        public string DataType => dataRow.Field<string>("COLUMN_TYPE");

        public bool IsReadOnly => dataRow.Field<bool>("IS_READONLY");

        public byte Precision => dataRow.Field<byte>("PRECISION");

        public byte Scale => dataRow.Field<byte>("SCALE");

        public int? MaxLength => dataRow.Field<int?>("MAX_LENGTH");

        public string TableName => dataRow.Field<string>("TABLE_NAME");

        public string ReferencedColumn => dataRow.Field<string>("REFERENCED_COLUMN");

        public string ReferencedTable => dataRow.Field<string>("REFERENCED_TABLE");
    }
}
