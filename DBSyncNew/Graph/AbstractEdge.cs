using System;
using System.Collections.Generic;

namespace DBSyncNew.Graph
{
    public abstract class AbstractEdge<TVertex> : IEquatable<AbstractEdge<TVertex>>
        where TVertex : IEquatable<TVertex>
    {
        public abstract TVertex Start { get; }

        public abstract TVertex End { get; }

        public bool Equals(AbstractEdge<TVertex> other)
        {
            return EqualityComparer<TVertex>.Default.Equals(Start, End);
        }
    }
}