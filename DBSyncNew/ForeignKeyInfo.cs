using System;

namespace DBSyncNew
{
    public class ForeignKeyInfo
    {
        public ColumnInfo Column { get; set; }

        public ColumnInfo ReferencedColumn { get; set; }

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


    }
}
