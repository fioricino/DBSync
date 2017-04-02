using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Configuration;
using DBSyncNew.Database.Read.Interfaces;

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
            //TODO check if table from config not present
            Columns = builder.DBTables[builder.TableConfig.Name].Select(c => new ColumnObj.Builder(this, c).Build()).ToList();
            Aliases.Add(new AliasObj.TableBuilder(this).Build());
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
