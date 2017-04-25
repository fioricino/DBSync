using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBSyncNew.Configuration
{
    public class ConfigurationReader
    {
        public ScopesConfig ReadXmlConfiguration(string pathToXml)
        {
            ScopesConfig result;
            var serializer = new XmlSerializer(typeof(ScopesConfig));

            using (var file = File.OpenRead(pathToXml))
            {
                result = (ScopesConfig)serializer.Deserialize(file);
            }
            if (result.Scopes.All(s => s.ScopeType != ScopeType.None))
            {
                result.Scopes.Add(new ScopeConfig() { ScopeType = ScopeType.None });
            }

            return result;
        }
    
    }
}
