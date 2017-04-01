using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace DBSyncNew
{
    public class ScopeInfo
    {
        public ScopeInfo()
        {
            Tables = new List<TableInfo>();
            ReversedForeignKeys = new List<FKDescription>();
            FilterColumns = new List<FilterColumnInfo>();
            ArtificialForeignKeys = new List<FKDescription>();
        }

        public ScopeType ScopeType { get; set; }

        public string FilterColumnName { get; set; }

        public string FilterClause { get; set; }

        public MetaDataGenerationPattern MetaDataGenerationPattern { get; set; }

        public List<FilterColumnInfo> FilterColumns { get; set; }

        public List<FKDescription> ReversedForeignKeys { get; set; }

        public List<FKDescription> ArtificialForeignKeys { get; set; }
        
        public List<FKDescription> IgnoredForeignKeys { get; set; }
            
        public List<TableInfo> Tables { get; set; }

        //[XmlIgnore]
        //public IEnumerable<TableInfo> RootTables
        //{
        //    get { return Tables.Where(t => t.IsRoot); }
        //}

        public IEnumerable<AliasInfo> Aliases
        {
            get { return Tables.SelectMany(t => t.Aliases); }
        }

        public IEnumerable<AliasInfo> RootAliases
        {
            get { return Aliases.Where(a => a.IsRoot); }
        }

        public bool HasRoot
        {
            get { return RootAliases.Any(); }
        }

        public IEnumerable<TableInfo> OrderedTables
        {
            get
            {
                return Tables.OrderBy(t => t.Level).ThenBy(t => t.Name);
            }
        }

        public void SetRelations()
        {
            foreach (var table in Tables)
            {
                table.Scope = this;
                table.SetRelations();
            }
        }

        public ForeignKeyDirection GetForeignKeyDirection(string column, string table, string referencedcolumn,
            string referencedTable)
        {
            return FindFK(ReversedForeignKeys, column, table, referencedcolumn, referencedTable) == null
                ? FindFK(IgnoredForeignKeys, column, table, referencedcolumn, referencedTable) == null
                    ? ForeignKeyDirection.Direct
                    : ForeignKeyDirection.Ignored
                : ForeignKeyDirection.Reversed;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return ScopeType.ToString();
        }

        private FKDescription FindFK(IEnumerable<FKDescription> collection, string column, string table,
            string referencedcolumn,
            string referencedTable)
        {
            return
                collection.SingleOrDefault(
                    fk =>
                        fk.Column == column && fk.Table == table && fk.ReferencedColumn == referencedcolumn &&
                        fk.ReferencedTable == referencedTable);
        }
    }
}
