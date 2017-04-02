using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Configuration;
using DBSyncNew.Graph;

namespace DBSyncNew.SchemaObjects
{
    public class AliasObj : VertexSource<AliasObj, ForeignKeyObj>, ITableOrAlias
    {
        private AliasObj(Builder builder)
        {
            Name = builder.AliasConfig.Name;
            IsRoot = builder.AliasConfig.IsRoot;
            FilterColumns =
                builder.AliasConfig.FilterColumns.Select(f => new FilterColumnObj.Builder(f, this).Build()).ToList();
            Table = builder.TableObj;
        }

        private AliasObj(TableBuilder builder)
        {
            IsRoot = builder.TableObj.IsRoot;
            FilterColumns = builder.TableObj.FilterColumns;
            Table = builder.TableObj;
        }

        public string Name { get; }

        public string NameOrAlias
        {
            get
            {
                return String.IsNullOrEmpty(Name)
                    ? Table.Name
                    : Name;
            }


        }

        public int Level { get { return Table.Level; } }

        /// <summary>
        /// Gets table name with alias
        /// </summary>
        public string NameWithAlias
        {
            get
            {
                return String.IsNullOrEmpty(Name)
                    ? Table.Name
                    : String.Format("{0} AS {1}", Table.Name, Name);
            }
        }

        public override string ToString()
        {
            return NameWithAlias;
        }

        //TODO remove
        public List<SyncScript> Scripts { get; } = new List<SyncScript>();


        public bool IsRoot { get; }

        //TODO make unmodifiable
        public List<FilterColumnObj> FilterColumns { get; }
        
        public TableObj Table { get; } 

        //TODO remove setter
        public List<ForeignKeyObj> ForeignKeys { get; } = new List<ForeignKeyObj>();

        public class TableBuilder
        {
            public TableBuilder(TableObj tableObj)
            {
                TableObj = tableObj;
            }

            public TableObj TableObj { get; }

            public AliasObj Build()
            {
                return new AliasObj(this);
            }
        }

        public class Builder
        {
            public Builder(TableObj tableObj, AliasConfig aliasConfig)
            {
                TableObj = tableObj;
                AliasConfig = aliasConfig;
            }

            public AliasObj Build()
            {
                return new AliasObj(this);
            }

            public TableObj TableObj { get; }

            public AliasConfig AliasConfig { get; }
        }

        public override IEnumerable<ForeignKeyObj> OutgoingEdges => ForeignKeys;
        public override bool Equals(AliasObj other)
        {
            return other != null && other.NameOrAlias == NameOrAlias;
        }

        //TODO WTF?
        public override bool IsReferenced { get { return FilterColumns.Any(f => f.IsReferenced); } }
    }
}
