using Findx.Threading;

namespace Findx.Data
{
	/// <summary>
	/// 租户管理
	/// </summary>
	public static class TenantManager
	{
		private static readonly IValueAccessor<Guid> ValueAccessor = new ValueAccessor<Guid>();
		
		/// <summary>
		/// 当前租户编号
		/// </summary>
		public static Guid Current
		{
			get => ValueAccessor.Value;
			set => ValueAccessor.Value = value;    
		}
	}
}

