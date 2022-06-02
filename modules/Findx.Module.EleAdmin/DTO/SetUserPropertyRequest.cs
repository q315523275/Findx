using System;
namespace Findx.Module.EleAdmin.DTO
{
	public class SetUserPropertyRequest
	{
		public Guid Id { get; set; }

		public int Status { get; set; }

		public string Password { get; set; }
	}
}

