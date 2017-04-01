using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Configuration;
using DBSyncNew.Database.Interfaces;

namespace DBSyncNew.SchemaObjects
{
    public class TableObj : ITableOrAlias
    {
        private TableObj(Builder builder)
        {
            Scope = builder.ScopeObj;
            Aliases = builder.TableConfig.Aliases.Select(a => new AliasObj.Builder(this, a).Build()).ToList();
            Name = builder.TableConfig.Name;
            IsRoot = builder.TableConfig.IsRoot;
            IgnoreKey = builder.TableConfig.IgnoreKey;
            KeepRowVersion = builder.TableConfig.KeepRowVersion;
            IsSkippedOnDelete = builder.TableConfig.IsSkippedOnDelete;
            ConflictResolutionPolicy = builder.TableConfig.ConflictResolutionPolicy;
            FilterColumns =
                builder.TableConfig.FilterColumns.Select(f => new FilterColumnObj.Builder(f, this).Build()).ToList();
            Columns = builder.DBTables[builder.TableConfig.Name].Select(c => new ColumnObj.Builder(this, c).Build()).ToList();
        }

        public ScopeObj Scope { get; }

        //TODO remvoe
        public IEnumerable<TableObj> ParentInfos
        {
            get
            {
                return Columns.Where(c => c.ReferencedColumnObj != null).Select(c => c.ReferencedColumnObj.Table);
            }
        }

        public string Name { get; }

        public bool IsRoot { get; }

        //TODO remove
        public bool IgnoreKey { get; }

        //TODO remove
        public bool KeepRowVersion { get; }

        //TODO remove
        public bool IsSkippedOnDelete { get; }

        //TODO remove setter
        public int Level { get; set; }

        public IEnumerable<ColumnObj> PKColumns
        {
            get { return Columns.Where(c => c.IsPk); }
        }

        public SyncConflictResolutionPolicy ConflictResolutionPolicy { get; }

        public string NameForTempTable { get { return String.Format("@{0}_FOR_DELETE", Name); } }

        public List<AliasObj> Aliases { get; }

        public List<FilterColumnObj> FilterColumns { get; } 

        public List<ColumnObj> Columns { get; }

        public bool PossibleWrongKey
        {
            get { return !IgnoreKey && (IsCoreData && IsGuid || IsFluentData && !IsGuid); }
        }

        //TODO contracts
        public bool IsCoreData
        {
            get { return Scope.ScopeType == ScopeType.Core; }
        }

        public bool IsFluentData
        {
            get { return Scope.ScopeType != ScopeType.Core && Scope.ScopeType != ScopeType.None; }
        }

        //TODO remove, add separate validator
        public bool PossibleWrongConflictResolution
        {
            get { return ConflictResolutionPolicy == SyncConflictResolutionPolicy.LastChangeDate && Columns.All(c => c.Name != "CHANGE_DATE"); }
        }

        public bool IsGuid
        {
            get { return Columns.Any(c => c.IsGuid); }
        }

        public IEnumerable<ColumnObj> NotPKColumns
        {
            get { return SyncColumns.Where(c => !c.IsPk); }
        }

        public IEnumerable<ColumnObj> SyncColumns
        {
            get { return Columns.Where(c => !c.IsReadOnly); }
        }

        //TODO move

        public string StoredProcedureName
        {
            get { return SyncMetaDataHelper.GetSPName(Name); }
        }

        public string TVPName
        {
            get { return SyncMetaDataHelper.GetTVPName(Name); }
        }

        public string QuotedName
        {
            get { return String.Format("[{0}]", Name); }
        }

        public string GetTVPStatement()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("CREATE TYPE [dbo].{0} AS TABLE (", TVPName));
            for (int index = 0; index < Columns.Count; index++)
            {
                var column = Columns[index];
                if (column.IsReadOnly && !column.IsPk)
                {
                    continue;
                }
                sb.Append(column.ToSqlString());
                if (index < Columns.Count - 1)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.AppendLine(String.Format("PRIMARY KEY ({0}))", String.Join(",", PKColumns.Select(c => c.QuotedName))));
            return sb.ToString();
        }

        public string GetSPStatement()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("CREATE PROCEDURE {0}", StoredProcedureName));
            sb.AppendLine(String.Format("@source as dbo.{0} READONLY", TVPName));
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SET NOCOUNT ON;");
            sb.AppendLine(String.Format("MERGE dbo.{0} AS TARGET", QuotedName));
            sb.AppendLine("USING @source AS SOURCE");
            sb.AppendLine(String.Format("ON {0}", String.Join(" AND ", PKColumns.Select(c =>
                String.Format("SOURCE.{0} = TARGET.{0}", c.QuotedName)))));
            sb.AppendLine(String.Format("WHEN MATCHED {0} THEN UPDATE SET", GetConflictResolutionPolicyParam()));
            sb.AppendLine(String.Join(", ", NotPKColumns.Select(c => String.Format("TARGET.{0} = SOURCE.{0}", c.QuotedName))));
            sb.AppendLine("WHEN NOT MATCHED BY TARGET THEN INSERT (");
            sb.AppendLine(String.Join(", ", SyncColumns.Select(c => c.QuotedName)));
            sb.AppendLine(") VALUES (");
            sb.AppendLine(String.Join(", ", SyncColumns.Select(c => String.Format("SOURCE.{0}", c.QuotedName))));
            sb.AppendLine(");");
            sb.AppendLine("END");

            return sb.ToString();
        }

        private string GetConflictResolutionPolicyParam()
        {
            var result = String.Empty;
            switch (ConflictResolutionPolicy)
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

        public string GetDropSPStatement()
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                String.Format(
                    "IF EXISTS(select * from sys.procedures where name = '{0}' AND schema_id = SCHEMA_ID('dbo'))",
                    StoredProcedureName));
            sb.AppendLine(String.Format("DROP PROCEDURE {0}", StoredProcedureName));
            return sb.ToString();
        }

        public string GetDropTVPStatement()
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                String.Format(
                    "IF EXISTS(select * from sys.types where name = '{0}' AND schema_id = SCHEMA_ID('dbo'))",
                    TVPName));
            sb.AppendLine(String.Format("DROP TYPE {0}", TVPName));
            return sb.ToString();
        }

        public class Builder
        {
            public Builder(ScopeObj scopeObj, TableConfig tableConfig, IDBTables dbTables)
            {
                this.ScopeObj = scopeObj;
                this.TableConfig = tableConfig;
                DBTables = dbTables;
            }

            public TableConfig TableConfig { get; }

            public ScopeObj ScopeObj { get; }

            public IDBTables DBTables { get; }

            public TableObj Build()
            {
                return new TableObj(this);
            }
        }

        public string NameOrAlias { get {return Name;} }
    }
}
