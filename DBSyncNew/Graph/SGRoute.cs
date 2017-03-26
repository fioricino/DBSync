using System;
using System.Collections.Generic;
using System.Linq;
using DBSyncNew.Graph;

namespace DBSyncNew
{
    public sealed class SGRoute<TVertex, TEdge> where TVertex : IEquatable<TVertex> where TEdge : IReversable<TEdge>
    {
        public static SGRoute<TVertex, TEdge> Create(Vertex<TVertex, TEdge> rootTable, List<Edge<TVertex, TEdge>> list)
        {
            return new SGRoute<TVertex, TEdge>(list, GetDirected(rootTable, list), rootTable);
        }

        private static List<Edge<TVertex, TEdge>> GetDirected(Vertex<TVertex, TEdge> rootTable, List<Edge<TVertex, TEdge>> original)
        {
            if (original.Count==0)
            {
                return original;
            }

            var reverse = EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(original[0].Start, rootTable)
                || EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(original[0].End, rootTable);
            var result = new List<Edge<TVertex, TEdge>>();
            if (original.Count == 1)
            {
                var currKey = original[0];
                if (reverse)
                {
                    if (!EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(currKey.End, rootTable))
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
                var start = EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(first.Start, second.Start) 
                    || EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(first.Start, second.End) ? first.End : first.Start;
                foreach (var key in original)
                {
                    var currKey = key;
                    if (!EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(start, key.Start))
                    {
                        currKey = Reverse(key);
                    }
                    result.Add(currKey);
                    start = currKey.End;
                }    
            }
            return result;
        }
        private static Edge<TVertex, TEdge> Reverse(Edge<TVertex, TEdge> key)
        {
            var result = new Edge<TVertex, TEdge>(key.End, key.Start, key.Value.Reversed(), ForeignKeyDirection.Reversed);
             
            return result;
        }

        private readonly List<Edge<TVertex, TEdge>> directed;
        public SGRoute(List<Edge<TVertex, TEdge>> original, Vertex<TVertex, TEdge> rootTable)
            : this(original, original, rootTable)
        {
        }
        public SGRoute(List<Edge<TVertex, TEdge>> original, List<Edge<TVertex, TEdge>> directed, Vertex<TVertex, TEdge> rootTable)
        {
            Original = original;
            this.directed = directed;
            RootTable = rootTable;
        }


        public Vertex<TVertex, TEdge> RootTable { get; private set; }

        public Vertex<TVertex, TEdge> Start
        {
            get { return Directed.First().Start; }
        }

        public Vertex<TVertex, TEdge> End
        {
            get { return Directed.Last().End; }
        }

        public IEnumerable<Vertex<TVertex, TEdge>> AllVertices
        {
            get
            {
                return Directed.Select(d => d.Start).Union(Directed.Select(d => d.End), EqualityComparer<Vertex<TVertex, TEdge>>.Default);
            }
        }

        public List<Edge<TVertex, TEdge>> Original { get; private set; }
        public List<Edge<TVertex, TEdge>> Directed
        {
            get { return directed; }
        }
        public override string ToString()
        {
            var points = new List<string>(Directed.Count + 1) { Start.Value.ToString() };
            points.AddRange(Directed.Select(key => key.End.Value.ToString()));
            return string.Join(" -> ", points);
        }
    }
}