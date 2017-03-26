using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Interfaces;

namespace DBSyncNew.Database.MsSql
{
    public class MsSqlDBAdapter : IDBAdapter
    {
        private string connectionString;

        public MsSqlDBAdapter(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDBMetadata GetDbMetadata()
        {
            DataTable sqlColumns;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                sqlColumns = ReadColumnDataFromDB(sqlConnection);
            }

            return new MsSqlMetadataAdapter(sqlColumns);
        }

        private static DataTable ReadColumnDataFromDB(SqlConnection sqlConnection)
        {
            DataTable mdc;
            //black magic
            var columnCommand = new SqlCommand(@"SELECT DISTINCT 
c.name COLUMN_NAME, 
tp.name COLUMN_TYPE,
COLUMNPROPERTY(c.object_id, c.name, 'charmaxlen') MAX_LENGTH,
c.precision PRECISION,
c.scale SCALE,
c.is_nullable IS_NULLABLE,
CONVERT(BIT, CASE WHEN (c.is_identity = 1 OR tp.name = 'timestamp') THEN 1 ELSE 0 END) IS_READONLY,
CONVERT(BIT, COALESCE(ix.is_primary_key, 0)) IS_PK,
t.name TABLE_NAME,
tt.name REFERENCED_TABLE,
cc.NAME REFERENCED_COLUMN,
c.column_id ORDINAL_NUMBER

FROM sys.columns c
LEFT JOIN sys.types tp
ON c.system_type_id = tp.system_type_id
AND c.user_type_id = tp.user_type_id
LEFT JOIN
(SELECT ic.column_id, ic.object_id, is_primary_key 
FROM sys.index_columns ic
JOIN sys.indexes i
ON i.index_id = ic.index_id
AND i.object_id = ic.object_id
AND i.is_primary_key = 1) ix
ON c.column_id = ix.column_id
AND c.object_id = ix.object_id

LEFT JOIN sys.tables t
ON t.object_id = c.object_id
LEFT JOIN sys.foreign_key_columns f
ON f.parent_object_id = c.object_id
AND f.parent_column_id = c.column_id
LEFT JOIN sys.tables tt
ON f.referenced_object_id = tt.object_id
LEFT JOIN sys.columns cc
ON f.referenced_object_id = cc.object_id
AND f.referenced_column_id = cc.column_id
WHERE t.schema_id = 1
AND t.type = 'U'
ORDER BY TABLE_NAME, ORDINAL_NUMBER
                                                                ", sqlConnection);


            var columnReader = columnCommand.ExecuteReader();
            mdc = new DataTable();
            mdc.Load(columnReader);
            return mdc;
        }
    }
}
