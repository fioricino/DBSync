using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Read.Interfaces;

namespace DBSyncNew.Database
{
    //TODO split source and destination
    public class DBAdapterFactory
    {
        private static Dictionary<string, Func<string, IDatabase>> adapters = new Dictionary<string, Func<string, IDatabase>>();

        public static void RegisterAdapter(string name, Func<string, IDatabase> constructor)
        {
            if (adapters.ContainsKey(name))
            {
                throw new InvalidOperationException($"Adapter {name} is already registered.");
            }
            adapters.Add(name, constructor);
        }

        public static IDatabase GetDBAdapter(string name, string connectionString)
        {
            if (!adapters.ContainsKey(name))
            {
                throw new InvalidOperationException($"Adapter {name} is not registered.");
            }
            return adapters[name].Invoke(connectionString);
        }
    }
}
