using System.Data;
using DBSyncNew.Database.Read.Interfaces;

namespace DBSyncNew.Database.Read.MsSql
{
    public class MsSqlForeignKey : IDBForeignKey
    {
        private DataRow dataRow;

        public MsSqlForeignKey(DataRow dataRow)
        {
            this.dataRow = dataRow;
        }

        public string Table => dataRow.Field<string>("TABLE_NAME");
        public string Column => dataRow.Field<string>("COLUMN_NAME");
        public string ReferencedTable => dataRow.Field<string>("REFERENCED_TABLE");
        public string ReferencedColumn => dataRow.Field<string>("REFERENCED_COLUMN");
    }
}
