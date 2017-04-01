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
    public class MsSqlDatabase : IDatabase
    {
        //TODO add schema
        //TODO move to resources
        private const string SelectColumnsCommand = @"SELECT  
c.name COLUMN_NAME, 
tp.name COLUMN_TYPE,
COLUMNPROPERTY(c.object_id, c.name, 'charmaxlen') MAX_LENGTH,
c.precision PRECISION,
c.scale SCALE,
c.is_nullable IS_NULLABLE,
CONVERT(BIT, CASE WHEN (c.is_identity = 1 OR tp.name = 'timestamp') THEN 1 ELSE 0 END) IS_READONLY,
CONVERT(BIT, COALESCE(ix.is_primary_key, 0)) IS_PK,
t.name TABLE_NAME

FROM sys.columns c
JOIN sys.types tp
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

JOIN sys.tables t
ON t.object_id = c.object_id
WHERE t.schema_id = 1
AND t.type = 'U'
ORDER BY TABLE_NAME, c.column_id";

        private const string SelectFkCommand = @"SELECT DISTINCT 
c.name COLUMN_NAME, 
t.name TABLE_NAME,
tt.name REFERENCED_TABLE,
cc.NAME REFERENCED_COLUMN
FROM sys.columns c
JOIN sys.tables t
ON t.object_id = c.object_id
JOIN sys.foreign_key_columns f
ON f.parent_object_id = c.object_id
AND f.parent_column_id = c.column_id
JOIN sys.tables tt
ON f.referenced_object_id = tt.object_id
JOIN sys.columns cc
ON f.referenced_object_id = cc.object_id
AND f.referenced_column_id = cc.column_id
WHERE t.schema_id = 1
AND t.type = 'U'
ORDER BY TABLE_NAME";

        private string connectionString;

        //TODO
        private Lazy<Tuple<IDBTables, List<IDBForeignKey>>> data;

        private Tuple<IDBTables, List<IDBForeignKey>> Init()
        {
            DataTable sqlColumns;
            DataTable sqlFks;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                sqlColumns = ReadColumnDataFromDB(sqlConnection);
                sqlFks = ReadFkDataFromDB(sqlConnection);
            }

            return new Tuple<IDBTables, List<IDBForeignKey>>(new MsSqlTables(sqlColumns), sqlFks.Rows.Cast<DataRow>()
                .Select(f => new MsSqlForeignKey(f)).Cast<IDBForeignKey>().ToList());
        }

        public MsSqlDatabase(string connectionString)
        {
            this.connectionString = connectionString;
            data = new Lazy<Tuple<IDBTables, List<IDBForeignKey>>>(Init);
        }



        private static DataTable ReadColumnDataFromDB(SqlConnection sqlConnection)
        {
            return LoadDataTable(sqlConnection, SelectColumnsCommand);
        }

        private static DataTable ReadFkDataFromDB(SqlConnection sqlConnection)
        {
            return LoadDataTable(sqlConnection, SelectFkCommand);
        }

        private static DataTable LoadDataTable(SqlConnection sqlConnection, string commandText)
        {
            var columnCommand = new SqlCommand(commandText, sqlConnection);
            var columnReader = columnCommand.ExecuteReader();
            var columnData = new DataTable();
            columnData.Load(columnReader);
            return columnData;
        }

        public IDBTables Tables => data.Value.Item1;
        public List<IDBForeignKey> ForeignKeys => data.Value.Item2;
    }
}
