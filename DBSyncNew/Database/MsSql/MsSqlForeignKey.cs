using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Interfaces;

namespace DBSyncNew.Database.MsSql
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
