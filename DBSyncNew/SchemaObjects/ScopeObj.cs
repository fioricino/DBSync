using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Configuration;
using DBSyncNew.Database.Interfaces;

namespace DBSyncNew.SchemaObjects
{
    public class ScopeObj
    {
        private ScopeObj(Builder builder)
        {
            MetaDataGenerationPattern = builder.ScopeConfig.MetaDataGenerationPattern;
            FilterColumns =
                builder.ScopeConfig.FilterColumns.Select(f => new FilterColumnObj.Builder(f, null).Build()).ToList();
            Tables = builder.ScopeConfig.Tables.Select(t => new TableObj.Builder(this, t, builder.DBTables).Build()).ToList();
            ScopeType = builder.ScopeConfig.ScopeType;
            ReversedForeignKeys = builder.ScopeConfig.ReversedForeignKeys;
            IgnoredForeignKeys = builder.ScopeConfig.IgnoredForeignKeys;
            //TODO calculate level
            //TODO check circular dependencies
            //TODO init foregn keys
            foreach (var foreignKeyConfig in builder.ScopeConfig.ArtificialForeignKeys)
            {
                AddForeignKey(foreignKeyConfig, ForeignKeyDirection.Direct);
            }
        }

        private void AddForeignKey(ForeignKeyConfig foreignKeyConfig, ForeignKeyDirection direction)
        {
            foreach (var alias in Aliases.Where(a => a.NameOrAlias == foreignKeyConfig.Table))
            {
                foreach (var refAlias in Aliases.Where(a => a.NameOrAlias == foreignKeyConfig.ReferencedTable))
                {
                    var fk =
                        new ForeignKeyObj.ConfigBuilder(foreignKeyConfig, direction, alias, refAlias).Build();
                    alias.ForeignKeys.Add(fk);
                    refAlias.ForeignKeys.Add(fk);
                }
            }
        }

        //TODO change
        public ScopeType ScopeType { get; }

        //TODO remove
        public List<ForeignKeyConfig> ReversedForeignKeys { get; }
        //TODO remove

        public List<ForeignKeyConfig> IgnoredForeignKeys { get; }


        public MetaDataGenerationPattern MetaDataGenerationPattern { get; }

        //TODO not used now
        public List<FilterColumnObj> FilterColumns { get; }

        public List<TableObj> Tables { get; }

        public IEnumerable<AliasObj> Aliases
        {
            get { return Tables.SelectMany(t => t.Aliases); }
        }

        public IEnumerable<AliasObj> RootAliases
        {
            get { return Aliases.Where(a => a.IsRoot); }
        }

        public bool HasRoot
        {
            get { return RootAliases.Any(); }
        }

        public IEnumerable<TableObj> OrderedTables
        {
            get
            {
                return Tables.OrderBy(t => t.Level).ThenBy(t => t.Name);
            }
        }


        public class Builder
        {
            public Builder(ScopeConfig scopeConfig, IDBTables dbTables)
            {
                ScopeConfig = scopeConfig;
                DBTables = dbTables;
            }

            public ScopeConfig ScopeConfig { get; }

            public IDBTables DBTables { get; }

            public ScopeObj Build()
            {
                return new ScopeObj(this);
            }
        }
    }
}
