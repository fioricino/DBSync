using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DBSyncNew.Graph;

namespace DBSyncNew
{
    [Serializable]
    public class AliasInfo : VertexSource<AliasInfo, ForeignKeyAliasInfo>, ITableOrAlias
    {
        public AliasInfo()
        {
            Scripts = new List<SyncScript>();      
            ForeignKeys = new List<ForeignKeyAliasInfo>();
            FilterColumns = new List<FilterColumnInfo>();
        }

        [XmlAttribute]
        public string Name { get; set; }

        public List<FilterColumnInfo> FilterColumns { get; set; }
        
        [XmlAttribute]
        public bool IsRoot { get; set; }

        [XmlIgnore]
        public TableInfo Table { get; set; }

        /// <summary>
        /// Gets table name with alias
        /// </summary>
        [XmlIgnore]
        public string NameOrAlias
        {
            get
            {
                return String.IsNullOrEmpty(Name)
                    ? Table.Name
                    : Name;
            }
        }

        [XmlIgnore]
        public List<ForeignKeyAliasInfo> ForeignKeys { get; set; }

        /// <summary>
        /// Gets table name with alias
        /// </summary>
        [XmlIgnore]
        public string NameWithAlias
        {
            get
            {
                return String.IsNullOrEmpty(Name)
                    ? Table.Name
                    : String.Format("{0} AS {1}", Table.Name, Name);
            }
        }

        [XmlIgnore]
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
