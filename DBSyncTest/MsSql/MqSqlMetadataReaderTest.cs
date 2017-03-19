using System;
using DBSync.MsSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSyncTest.MsSql
{
    [TestClass]
    public class MqSqlMetadataReaderTest
    {
        [TestMethod]
        public void TestReadTables()
        {
            var reader = new MsSqlMetadataReader();
            var msSqlTables = reader.ReadMetadata("Server=localhost;Database=TUEV_SUED;Integrated Security=true");
        }
    }
}
