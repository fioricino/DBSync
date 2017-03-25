namespace DBSyncNew
{
    /// <summary>
    /// defines a synchronization scope types
    /// </summary>
    public enum ScopeType
    {

		/// <summary>
		/// Scope to exclude tables from synchronization
		/// Sync flow:  server --> X 
		/// Sync flow:  client --> X
		/// </summary>
		/// <remarks>already used in new sync implementation</remarks>
		None = 0,

		/// <summary>
		/// Core data scope (Stammdaten). Defines core data (Stammdaten) tables 
		/// Sync flow:  server---> client 
		/// </summary>
		/// <remarks>already used in new sync implementation</remarks>
        Core = 1,

 
        /// <summary>
        /// Login data scope. Currently represents 2 tables with the "user's lock" info & user's password. Contains a data with the blocked users & password 
        ///  
		/// Sync flow:   server---> client 
        /// </summary>
		/// <remarks>already used in new sync implementation</remarks>
        Login = 3,

        /// <summary>
		/// Historical data scope (HFD). Represents a data stored in TUEV_SUED_VEHICLES database . By default it's a non-normalized huge plain table  VEHICLE_HISTORY
        ///   
		/// Sync flow:  server---> client 
		/// </summary>
		/// <remarks>already used in new sync implementation</remarks>
        Historical = 4,

        /// <summary>
		/// Expenses data scope (Aufwand). A part of fluent data (Bewegungsdaten).
		/// Represents list of object related with "working day".   
        /// 
		/// Sync flow:   client  ----> server
        /// </summary>
		/// <remarks>already used in new sync implementation</remarks>
        Expenses = 5,
        
        /// <summary>
		/// Attachments data scope. A part of fluent data (Bewegungsdaten). 
		/// Represents a binary data ([TUEV_SUED].[dbo].[INS_COMMON_BINARY_DATA_VALUE]) attached to order or position objects. 
		/// 
		/// Sync flow:   client  ----> server
		/// </summary>
		/// <remarks>already used in new sync implementation</remarks>
		Attachments = 7,

        /// <summary>
		/// Order data scope (Auftrag & TP & Inspection report) A part of fluent data (Bewegungsdaten). Represents a group of objects based on/(related with) the order entity. 
        /// 
		/// Sync flow:   client  ----> server
        /// </summary>
		/// <remarks>already used in new sync implementation</remarks>
		Order = 8,

		/// <summary>
		/// Cash data scope (Kasse). A part of fluent data (Bewegungsdaten). Represents a group of objects based on/(related with) the cash entity. 
		/// 
		/// Sync flow:   client  ----> server
		/// </summary>
		/// <remarks>already used in new sync implementation</remarks>
		Cash = 9,

        /// <summary>
        /// Scope for client import protocol data
        /// 
        /// Sync flow:   client  ----> server
        /// </summary>
        Protocol = 10,

        /// <summary>
        /// Expenses
        /// </summary>
        ExpensesAll = 11
    }
}