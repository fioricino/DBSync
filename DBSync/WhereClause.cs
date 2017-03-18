namespace DBSync
{
    /// <summary>
    /// WhereClause
    /// </summary>
    public class WhereClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WhereClause(string clause, bool isSkippedOnDelete)
        {
            Clause = clause;
            IsSkippedOnDelete = isSkippedOnDelete;
        }

        /// <summary>
        /// Clause
        /// </summary>
        public string Clause { get; private set; }

        /// <summary>
        /// IsSkippedOnDelete
        /// </summary>
        public bool IsSkippedOnDelete { get; private set; }
    }
}
