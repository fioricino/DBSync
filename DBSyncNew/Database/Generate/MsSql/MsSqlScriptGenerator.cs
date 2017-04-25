using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBSyncNew.SchemaObjects;

namespace DBSyncNew.Database.Generate.MsSql
{
    public class MsSqlScriptGenerator
    {
        //TODO accept all objects
        //TODO accept other name in destination
        public string GetQuotedName(string name)
        {
            return $"[{name}]"; 
        }

        public string GetNameWithAlias(AliasObj alias)
        {
                return String.IsNullOrEmpty(alias.Name)
                    ? alias.Table.Name
                    : String.Format("{0} AS {1}", alias.Table.Name, alias.Name);
        }

        public string GetColumnSqlString(ColumnObj column)
        {
            return String.Format("[{0}] {1} {2} {3}", column.Name, column.DataType, GetDataTypeModifiers(column),
                column.IsNullable ? "NULL" : "NOT NULL");
        }

        public string GetFullColumnName(string tableName, string columnName)
        {
            return String.Format("[{0}].[{1}]", tableName, columnName);
        }

        private string GetDataTypeModifiers(ColumnObj column)
        {
            var result = new List<string>();

            switch (column.DataType)
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
                    result.Add(column.Scale.ToString());
                    break;
                case "decimal":
                    result.Add(column.Precision.ToString());
                    result.Add(column.Scale.ToString());
                    break;
                case "varbinary":
                case "varchar":
                case "char":
                case "nvarchar":
                    result.Add(column.MaxLength.ToString().Replace("-1", "max"));
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Data type {0} not implemented.", column.DataType));
            }

            return result.Count == 0 ? String.Empty : String.Format("({0})", String.Join(",", result));
        }


        //TODO quoted?
        public string GetNameForTempTable(string tableName)
        {
            return String.Format("@{0}_FOR_DELETE", tableName);
        }

        public string GetSPName(string tableName)
        {
            return String.Format("SYNC_MERGE_{0}", tableName);
        }

        public static string GetTVPName(string tableName)
        {
            return String.Format("TVP_{0}", tableName);
        }

        public string GetTVPStatement(TableObj table)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("CREATE TYPE [dbo].{0} AS TABLE (", GetTVPName(table.Name)));
            for (int index = 0; index < table.Columns.Count; index++)
            {
                var column = table.Columns[index];
                if (column.IsReadOnly && !column.IsPk)
                {
                    continue;
                }
                sb.Append(GetColumnSqlString(column));
                if (index < table.Columns.Count - 1)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.AppendLine(String.Format("PRIMARY KEY ({0}))", String.Join(",", table.PKColumns.Select(c => GetQuotedName(c.Name)))));
            return sb.ToString();
        }

        public string GetSPStatement(TableObj table)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("CREATE PROCEDURE {0}", GetSPName(table.Name)));
            sb.AppendLine(String.Format("@source as dbo.{0} READONLY", GetTVPName(table.Name)));
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SET NOCOUNT ON;");
            sb.AppendLine(String.Format("MERGE dbo.{0} AS TARGET", GetQuotedName(table.Name)));
            sb.AppendLine("USING @source AS SOURCE");
            sb.AppendLine(String.Format("ON {0}", String.Join(" AND ", table.PKColumns.Select(c =>
                String.Format("SOURCE.{0} = TARGET.{0}", GetQuotedName(c.Name))))));
            sb.AppendLine(String.Format("WHEN MATCHED {0} THEN UPDATE SET", GetConflictResolutionPolicyParam(table.ConflictResolutionPolicy)));
            sb.AppendLine(String.Join(", ", table.NotPKColumns.Select(c => String.Format("TARGET.{0} = SOURCE.{0}", GetQuotedName(c.Name)))));
            sb.AppendLine("WHEN NOT MATCHED BY TARGET THEN INSERT (");
            sb.AppendLine(String.Join(", ", table.SyncColumns.Select(c => GetQuotedName(c.Name))));
            sb.AppendLine(") VALUES (");
            sb.AppendLine(String.Join(", ", table.SyncColumns.Select(c => String.Format("SOURCE.{0}", GetQuotedName(c.Name)))));
            sb.AppendLine(");");
            sb.AppendLine("END");

            return sb.ToString();
        }

        private string GetConflictResolutionPolicyParam(SyncConflictResolutionPolicy policy)
        {
            var result = String.Empty;
            switch (policy)
            {
                case SyncConflictResolutionPolicy.None:
                    break;
                case SyncConflictResolutionPolicy.LastChangeDate:
                    result = "AND SOURCE.CHANGE_DATE > TARGET.CHANGE_DATE";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;
        }

        public string GetDropSPStatement(string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                String.Format(
                    "IF EXISTS(select * from sys.procedures where name = '{0}' AND schema_id = SCHEMA_ID('dbo'))",
                    GetSPName(tableName)));
            sb.AppendLine(String.Format("DROP PROCEDURE {0}", GetSPName(tableName)));
            return sb.ToString();
        }

        //TODO generate delete metadata script
        public string GetDropTVPStatement(string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                String.Format(
                    "IF EXISTS(select * from sys.types where name = '{0}' AND schema_id = SCHEMA_ID('dbo'))",
                    GetTVPName(tableName)));
            sb.AppendLine(String.Format("DROP TYPE {0}", GetTVPName(tableName)));
            return sb.ToString();
        }

        public string GenerateTableInsertMetaData(TableObj table)
        {
            var result = new StringBuilder();

            result.AppendLine(GetDropSPStatement(table.Name));
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine(GetDropTVPStatement(table.Name));
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine(GetTVPStatement(table));
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine(GetSPStatement(table));
            result.AppendLine("GO");
            result.AppendLine();

            return result.ToString();
        }

        public void AppendJoin(StringBuilder sbuilder, ForeignKeyObj key)
        {
            sbuilder.Append(" JOIN ");
            sbuilder.Append(GetNameWithAlias(key.ReferencedAlias));
            sbuilder.Append(" ON ");
            sbuilder.Append(string.Format("{0}.{1} = {2}.{3}",
                key.Alias.NameOrAlias,
                key.Column,
                key.ReferencedAlias.NameOrAlias,
                key.ReferencedColumn));
            sbuilder.AppendLine();
        }

        public string GenerateInsertMetadataTableScript(IList<TableObj> levelInfo)
        {
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < levelInfo.Count; index++)
            {
                TableObj tableLevelInfo = levelInfo[index];
                string insertCommandText = string.Format(
                    @"INSERT INTO [dbo].[SYS_TUEV_TABLES] ([ID], [TABLE_NAME], [SCOPE_TYPE], [LEVEL], [IS_DATE_FILTERED]) VALUES({0},'{1}',{2},{3},{4});",
                    index, tableLevelInfo.Name, (int) tableLevelInfo.Scope.ScopeType, tableLevelInfo.Level, 0);
                sb.AppendLine(insertCommandText);
            }
            return sb.ToString();
        }
    }
}
