using System;
using System.Threading;

namespace Findx.Data
{
	public class Tenant
	{
		public static AsyncLocal<Guid> TenantId { get; set; } = new AsyncLocal<Guid>();
	}
}

