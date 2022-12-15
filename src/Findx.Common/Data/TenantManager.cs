namespace Findx.Data
{
	/// <summary>
	/// 租户管理
	/// </summary>
	public static class TenantManager
	{
		/// <summary>
		/// 
		/// </summary>
		private static AsyncLocal<Guid> AsyncLocal { get; set; } = new();
		
		/// <summary>
		/// 当前租户编号
		/// </summary>
		public static Guid Current
		{
			get => AsyncLocal.Value;
			set => AsyncLocal.Value = value;    
		}
	}
}

