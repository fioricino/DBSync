using System.IO;
using System.Xml.Serialization;
using DBSync.ScopeConf.Objects;

namespace DBSync.ScopeConf.Reader
{
    public class XmlScopeConfigurationReader
    {
        public AllScopesConfig ReadConfiguration(string xmlFilePath)
        {
            var serializer = new XmlSerializer(typeof(AllScopesConfig));

            using (var file = File.OpenRead(xmlFilePath))
            {
                return (AllScopesConfig)serializer.Deserialize(file);
            }
        }
    }
}
