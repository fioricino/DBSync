using System.Collections.Generic;
using System.Linq;

namespace DBSyncNew
{
    public sealed class SGRoute
    {
        #region Static
        public static SGRoute Create(AliasInfo rootTable, List<ForeignKeyAliasInfo> list)
        {
            return new SGRoute(list, GetDirected(rootTable, list), rootTable);
        }

        private static List<ForeignKeyAliasInfo> GetDirected(AliasInfo rootTable, List<ForeignKeyAliasInfo> original)
        {
            if (original.Count==0)
            {
                return original;
            }

            var reverse = original[0].Alias == rootTable || original[0].ReferencedAlias == rootTable;
            var result = new List<ForeignKeyAliasInfo>();
            if (original.Count == 1)
            {
                var currKey = original[0];
                if (reverse)
                {
                    if (currKey.ReferencedAlias != rootTable)
                    {
                        currKey = Reverse(currKey);
                    }
                }
                
                result.Add(currKey);
                
            }
            else
            {
                if (reverse)
                {
                    original = original.ToList();
                    original.Reverse();
                }

                var first = original[0];
                var second = original[1];
                var start = first.Alias == second.Alias || first.Alias == second.ReferencedAlias ? first.ReferencedAlias : first.Alias;
                foreach (var key in original)
                {
                    var currKey = key;
                    if (start != key.Alias)
                    {
                        currKey = Reverse(key);
                    }
                    result.Add(currKey);
                    start = currKey.ReferencedAlias;
                }    
            }
            return result;
        }
        private static ForeignKeyAliasInfo Reverse(ForeignKeyAliasInfo key)
        {
            var result = new ForeignKeyAliasInfo
                         {
                             Column = key.ReferencedColumn,
                             ReferencedColumn = key.Column,
                             Alias = key.ReferencedAlias,
                             ReferencedAlias = key.Alias
                         };
             
            return result;
        }

        #endregion
        #region Private fields
        private readonly List<ForeignKeyAliasInfo> directed;
        #endregion
        #region Constructor
        public SGRoute(List<ForeignKeyAliasInfo> original, AliasInfo rootTable)
            : this(original, original, rootTable)
        {
        }
        public SGRoute(List<ForeignKeyAliasInfo> original, List<ForeignKeyAliasInfo> directed, AliasInfo rootTable)
        {
            Original = original;
            this.directed = directed;
            RootTable = rootTable;
        }


        #endregion
        #region Public properties
        public AliasInfo RootTable { get; private set; }

        public AliasInfo Start
        {
            get { return Directed.First().Alias; }
        }

        public AliasInfo End
        {
            get { return Directed.Last().ReferencedAlias; }
        }

        public IEnumerable<AliasInfo> AllTables
        {
            get
            {
                return Directed.Select(d => d.Alias).Union(Directed.Select(d => d.ReferencedAlias));
            }
        }

        public List<ForeignKeyAliasInfo> Original { get; private set; }
        public List<ForeignKeyAliasInfo> Directed
        {
            get { return directed; }
        }
        #endregion
        #region Public methods
        public override string ToString()
        {
            var points = new List<string>(Directed.Count + 1) { Start.NameOrAlias };
            points.AddRange(Directed.Select(key => key.ReferencedAlias.NameOrAlias));
            return string.Join(" -> ", points);
        }
        #endregion
        #region Private methods
        
        #endregion
    }
}