using System.Data;

namespace DBSyncOld
{
    public sealed class SyncDataContainer
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="syncSession"></param>
        /// <param name="syncDataSet"></param>
        /// <param name="commandToFillDataSet"></param>
        public SyncDataContainer(DataSet syncDataSet, string commandToFillDataSet)
        {
            CommandToFillDataSet = commandToFillDataSet;
            SyncDataSet = syncDataSet;
           
        }


        public DataSet SyncDataSet { get; private set; }

        public string CommandToFillDataSet { get; private set; }
    }
}