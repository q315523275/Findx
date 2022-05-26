using System;
using System.Threading;

namespace Findx.Data
{
	public class Tenant
	{
		public static AsyncLocal<int> TenantId { get; set; } = new AsyncLocal<int>();
	}
}

