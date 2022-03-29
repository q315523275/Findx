using System;
namespace Findx.Redis
{
	public class FindxRedisOptions
	{
		public RedisConnections Connections { get; }

		public FindxRedisOptions()
		{
			Connections = new RedisConnections();
		}
	}
}

