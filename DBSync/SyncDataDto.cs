using System.Runtime.Serialization;

namespace DBSync
{
    /// <summary>
    ///Represents a "synchronization chunk" (data which synchronized in one isolated transaction) 
    /// contains serialized data and an "indicator" how many rows inside data   
    /// </summary>
    [DataContract]
    public sealed class SyncDataDto
    {
        /// <summary>
        /// ctor 
        /// </summary>
        /// <param name="data">serialized data</param>
        /// <param name="recordNumber">how many rows inside data </param>
        public SyncDataDto(byte[] data, int recordNumber)
        {
            Data = data;
            RecordNumber = recordNumber;
        }

        /// <summary>
        /// Gets a serialized sync data
        /// </summary>
        [DataMember]
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets a value indicates how many rows inside data 
        /// </summary>
        [DataMember]
        public int RecordNumber { get; private set; }
    }
}
