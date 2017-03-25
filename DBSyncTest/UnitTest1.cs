using System;
using System.Collections.Generic;
using DBSyncOld;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSyncTest
{
    [TestClass]
    public class UnitTest1
    {
        private static DBSyncOld.DBAnalyzer dbAnalyzerOld;
        private static DBSyncNew.DBAnalyzer dbAnalyzerNew;

        [ClassInitialize]
        public static void InitClass(TestContext context)
        {
            dbAnalyzerOld = new DBSyncOld.DBAnalyzer("", "", "", "", "Resources/scopes.xml", "Server=(local); Initial catalog=TUEV_SUED; Integrated Security=true");
            dbAnalyzerNew = new DBSyncNew.DBAnalyzer("", "", "", "", "Resources/scopes.xml", "Server=(local); Initial catalog=TUEV_SUED; Integrated Security=true");
        }

  
        [TestMethod]
        public void TestGenerateDataSetMetaData()
        {
            foreach (DBSyncOld.ScopeType scopeType in Enum.GetValues(typeof (DBSyncOld.ScopeType)))
            {
                if (scopeType == ScopeType.Historical) continue;
                
                string dataSetMetadataOld = dbAnalyzerOld.GenerateDataSetMetaData(scopeType);
                var scopeTypeNew = (DBSyncNew.ScopeType) Enum.Parse(typeof (DBSyncNew.ScopeType), scopeType.ToString());
                string dataSetMetadataNew = dbAnalyzerNew.GenerateDataSetMetaData(scopeTypeNew);

                Assert.AreEqual(dataSetMetadataOld, dataSetMetadataNew);
            }
        }
    }
}
