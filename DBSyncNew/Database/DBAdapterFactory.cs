using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Interfaces;
using DBSyncNew.Database.MsSql;

namespace DBSyncNew.Database
{
    public class DBAdapterFactory
    {
        private static Dictionary<string, Func<string, IDBAdapter>> adapters = new Dictionary<string, Func<string, IDBAdapter>>();

        public static void RegisterAdapter(string name, Func<string, IDBAdapter> constructor)
        {
            if (adapters.ContainsKey(name))
            {
                throw new InvalidOperationException($"Adapter {name} is already registered.");
            }
            adapters.Add(name, constructor);
        }

        public static IDBAdapter GetDBAdapter(string name, string connectionString)
        {
            if (!adapters.ContainsKey(name))
            {
                throw new InvalidOperationException($"Adapter {name} is not registered.");
            }
            return adapters[name].Invoke(connectionString);
        }
    }
}
