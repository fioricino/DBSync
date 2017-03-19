using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DBSync.SqlInfo
{
    [Serializable]
    public class AliasInfo : ITableOrAlias
    {
        [XmlAttribute]
        public string Name { get; set; }

        public List<FilterColumnInfo> FilterColumns { get; set; } = new List<FilterColumnInfo>();

        [XmlAttribute]
        public bool IsRoot { get; set; }

        [XmlIgnore]
        public TableInfo Table { get; set; }

        /// <summary>
        /// Gets table name with alias
        /// </summary>
        [XmlIgnore]
        public string NameOrAlias => String.IsNullOrEmpty(Name)
            ? Table.Name
            : Name;

        [XmlIgnore]
        public List<ForeignKeyAliasInfo> ForeignKeys { get; set; } = new List<ForeignKeyAliasInfo>();

        /// <summary>
        /// Gets table name with alias
        /// </summary>
        [XmlIgnore]
        public string NameWithAlias => String.IsNullOrEmpty(Name)
            ? Table.Name
            : $"{Table.Name} AS {Name}";

        [XmlIgnore]
        public List<SyncScript> Scripts { get; private set; } = new List<SyncScript>();

        public void SetRelations()
        {
            foreach (var filterColumnInfo in FilterColumns)
            {
                filterColumnInfo.Table = this;
            }
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

    }
}
