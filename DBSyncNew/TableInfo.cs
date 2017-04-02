using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DBSyncNew.Graph;

namespace DBSyncNew
{
    public class TableInfo : ITableOrAlias
    {
        public TableInfo()
        {
            Columns = new List<ColumnInfo>();
            ForeignKeys = new List<ForeignKeyInfo>();
            FilterColumns = new List<FilterColumnInfo>();
            Aliases = new List<AliasInfo>();
        }

        public string Name { get; set; }

        public string NameForTempTable { get { return String.Format("@{0}_FOR_DELETE", Name); } }

        public bool IsRoot { get; set; }
        
        public bool IgnoreKey { get; set; }

        public bool KeepRowVersion { get; set; }

        public bool IsSkippedOnDelete { get; set; }
        
        public SyncConflictResolutionPolicy ConflictResolutionPolicy { get; set; }

        public List<FilterColumnInfo> FilterColumns { get; set; }

        public List<AliasInfo> Aliases { get; set; }

        public IEnumerable<TableInfo> ParentInfos
        {
            get
            {
                return Columns.Where(c => c.ReferencedColumn != null).Select(c => c.ReferencedColumn.Table);
            }
        }

        public int Level { get; set; }
        
        public List<ColumnInfo> Columns { get; set; }

        public List<ForeignKeyInfo> ForeignKeys { get; set; }

        public IEnumerable<ColumnInfo> FKColumns
        {
            get { return Columns.Where(c => c.ReferencedColumn != null); }
        }

        public bool IsCoreData
        {
            get { return ScopeType == ScopeType.Core;}
        }

        public bool IsFluentData
        {
            get { return ScopeType != ScopeType.Core && ScopeType != ScopeType.None; }
        }

        public ScopeType ScopeType { get { return Scope.ScopeType; } }

        public bool IsGuid
        {
            get { return Columns.Any(c => c.IsGuid); }
        }

        public IEnumerable<ColumnInfo> PKColumns
        {
            get { return Columns.Where(c => c.IsPk); }
        }

        public IEnumerable<ColumnInfo> NotPKColumns
        {
            get { return SyncColumns.Where(c => !c.IsPk); }
        }

        public IEnumerable<ColumnInfo> SyncColumns
        {
            get { return Columns.Where(c => !c.IsReadOnly); }
        }

        public bool PossibleWrongKey
        {
            get { return !IgnoreKey && (IsCoreData && IsGuid || IsFluentData && !IsGuid); }
        }

        public bool PossibleWrongConflictResolution
        {
            get { return ConflictResolutionPolicy == SyncConflictResolutionPolicy.LastChangeDate && Columns.All(c => c.Name != "CHANGE_DATE"); }
        }

        public bool IsTableMetadata
        {
            get { return IsCoreData  && !Name.StartsWith("DRL_"); }
        }

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

        public ScopeInfo Scope { get; set; }

        public string NameOrAlias { get { return Name; } }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name;
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

        public void SetRelations()
        {
            foreach (var filterColumnInfo in FilterColumns)
            {
                filterColumnInfo.Table = this;
            }

            //self
            Aliases.Add(new AliasInfo() {IsRoot = IsRoot, FilterColumns = FilterColumns});

            foreach (var aliasInfo in Aliases)
            {
                aliasInfo.Table = this;
                aliasInfo.SetRelations();
            }
        }

   
    }
}
