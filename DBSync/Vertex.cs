using System.Collections.Generic;
using System.Linq;

namespace DBSync
{
    public sealed class Vertex<TV, TE>
    {
        #region Constructor
        public Vertex(TV value)
        {
            Value = value;
            Edges = new List<Edge<TV, TE>>();
        }
        #endregion
        #region Public properties
        public TV Value { get; private set; }
        public List<Edge<TV, TE>> Edges { get; private set; }
        #endregion
        #region Public methods
        public override string ToString()
        {
            return string.Format("{0};Edges: {1}, o->:{2}, o<-:{3}",
                    Value,
                    Edges.Count,
                    Edges.Count(edge => edge.Start.Equals(this)),
                    Edges.Count(edge => edge.End.Equals(this)));
        }
        #endregion
    }
}
