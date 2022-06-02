using System;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	public class QueryRoleRequest: PageBase
	{
		public string Name { set; get; }

		public string Code { set; get; }

		public string Comments { set; get; }
	}
}

