using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSync.MsSql.Objects;

namespace DBSync.MsSql
{
    public class MsSqlMetadataReader
    {
        private const string DefaultSchema = "dbo";
     
        private const string SelectColumnsCommand = @"SELECT DISTINCT 
c.name COLUMN_NAME, 
tp.name COLUMN_TYPE,
COLUMNPROPERTY(c.object_id, c.name, 'charmaxlen') MAX_LENGTH,
c.precision COLUMN_PRECISION,
c.scale COLUMN_SCALE,
c.is_nullable IS_NULLABLE,
CONVERT(BIT, CASE WHEN (c.is_identity = 1 OR c.is_computed = 1 OR  tp.name = 'timestamp') THEN 1 ELSE 0 END) IS_READONLY,
CONVERT(BIT, COALESCE(ix.is_primary_key, 0)) IS_PK,
t.name TABLE_NAME,
tt.name REFERENCED_TABLE,
cc.NAME REFERENCED_COLUMN,
c.column_id ORDINAL_NUMBER

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
JOIN sys.schemas s
ON t.schema_id = s.schema_id
WHERE s.name = @schema_name
AND t.type = 'U'
ORDER BY TABLE_NAME, ORDINAL_NUMBER";

        public List<MsSqlTable> ReadMetadata(string connectionString, string schema = DefaultSchema)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var tables = ReadTables(sqlConnection, schema);

                List<MsSqlTable> result = tables.Rows.Cast<DataRow>().GroupBy(r => r.Field<string>("TABLE_NAME"))
                    .Select(table => new MsSqlTable()
                {
                    Name = table.Key,
                    Columns = table.Select(r => new MsSqlColumn()
                    {
                        Name = r.Field<string>("COLUMN_NAME"),
                        IsPk = r.Field<bool>("IS_PK"),
                        IsNullable = r.Field<bool>("IS_NULLABLE"),
                        DataType = r.Field<string>("COLUMN_TYPE"),
                        IsReadOnly = r.Field<bool>("IS_READONLY"),
                        Precision = r.Field<byte>("COLUMN_PRECISION"),
                        Scale = r.Field<byte>("COLUMN_SCALE"),
                        MaxLength = r.Field<int?>("MAX_LENGTH"),
                        ReferencedTable = r.Field<string>("REFERENCED_TABLE"),
                        ReferencedColumn = r.Field<string>("REFERENCED_COLUMN"),
                        OrdinalNumber = r.Field<int>("ORDINAL_NUMBER")
                    }).ToList()
                }).ToList();
                return result;
            }
        }

        private static DataTable ReadTables(SqlConnection sqlConnection, string schema)
        {
            var tableCommand =
                new SqlCommand(SelectColumnsCommand, sqlConnection);
            var schemaParam = tableCommand.CreateParameter();
            schemaParam.ParameterName = "schema_name";
            schemaParam.Value = schema;
            tableCommand.Parameters.Add(schemaParam);

            var tableReader = tableCommand.ExecuteReader();
            var result = new DataTable();
            result.Load(tableReader);
            return result;
        }

    }
}
