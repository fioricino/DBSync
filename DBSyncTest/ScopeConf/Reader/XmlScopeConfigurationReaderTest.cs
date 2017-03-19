using System;
using DBSync.ScopeConf.Reader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSyncTest.ScopeConfiguration.Reader
{
    [TestClass]
    public class XmlScopeConfigurationReaderTest
    {
        private XmlScopeConfigurationReader reader;

        [TestInitialize]
        public void Init()
        {
            reader = new XmlScopeConfigurationReader();
        }

        [TestMethod]
        public void TestReadXmlConfig()
        {
            var config = reader.ReadConfiguration("scopes.xml");
            Assert.IsNotNull(config);
            Assert.IsTrue(config.Scopes.Count > 0);
        }
    }
}
