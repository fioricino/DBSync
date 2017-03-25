using System;
using DBSyncNew.Graph;

namespace DBSyncNew
{
    public class ForeignKeyAliasInfo : EdgeSource<AliasInfo>
    {
        public ForeignKeyAliasInfo()
        {
            
        }

        public string Column { get; set; }

        public string ReferencedColumn { get; set; }

        public AliasInfo Alias { get; set; }

        public AliasInfo ReferencedAlias { get; set; }

        public ForeignKeyDirection Direction { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0}->{1}", Column, ReferencedColumn);
        }


        public override AliasInfo Start { get { return Alias; } }
        public override AliasInfo End { get { return ReferencedAlias; } }
    }
}
