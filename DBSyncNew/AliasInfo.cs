using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DBSyncNew.Graph;

namespace DBSyncNew
{
    public class AliasInfo : VertexSource<AliasInfo, ForeignKeyAliasInfo>, ITableOrAlias
    {
        public AliasInfo()
        {
            Scripts = new List<SyncScript>();      
            ForeignKeys = new List<ForeignKeyAliasInfo>();
            FilterColumns = new List<FilterColumnInfo>();
        }

        public string Name { get; set; }

        public List<FilterColumnInfo> FilterColumns { get; set; }
        
        public bool IsRoot { get; set; }

        public TableInfo Table { get; set; }

        /// <summary>
        /// Gets table name with alias
        /// </summary>
        public string NameOrAlias
        {
            get
            {
                return String.IsNullOrEmpty(Name)
                    ? Table.Name
                    : Name;
            }
        }

        public List<ForeignKeyAliasInfo> ForeignKeys { get; set; }

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

        public List<SyncScript> Scripts { get; private set; }

        public void SetRelations()
        {
            foreach (var filterColumnInfo in FilterColumns)
            {
                filterColumnInfo.Table = this;
            }
        }

        public override bool Equals(AliasInfo other)
        {
            return other != null && other.NameOrAlias == NameOrAlias;
        }

        public override bool IsReferenced { get { return FilterColumns.Any(f => f.IsReferenced); } }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return NameWithAlias;
        }

        public override IEnumerable<ForeignKeyAliasInfo> OutgoingEdges { get { return ForeignKeys; } }

    }
}
