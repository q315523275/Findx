using System.Collections.Generic;

namespace Findx.Redis
{
    public class RedisConnections : Dictionary<string, string>
    {
        public const string DefaultConnectionName = "Default";

        public RedisConnections()
        {
            Default = string.Empty;
        }
        
        public string Default
        {
            get => this[DefaultConnectionName];
            set => this[DefaultConnectionName] = Check.NotNull(value, nameof(value));
        }

        public string GetOrDefault(string connectionName)
        {
            if (TryGetValue(connectionName, out var connectionFactory)) return connectionFactory;

            return Default;
        }
    }
}