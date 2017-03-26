using System;
using System.Collections.Generic;

namespace DBSyncNew.Graph
{
    public abstract class EdgeSource<TVertex> : IEquatable<EdgeSource<TVertex>>
        where TVertex : IEquatable<TVertex>
    {
        public abstract TVertex Start { get; }

        public abstract TVertex End { get; }

        public abstract ForeignKeyDirection Direction { get; set; }

        public bool Equals(EdgeSource<TVertex> other)
        {
            return EqualityComparer<TVertex>.Default.Equals(Start, End);
        }
    }
}