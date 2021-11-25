using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 中国行政地区表
    /// </summary>
    [Table(Name = "sys_area")]
    public class SysAreaInfo : EntityBase<long>, IResponse, IRequest
	{
		/// <summary>
		/// 编号
		/// </summary>
		[Column(Name = "id", IsPrimary = true, IsIdentity = true)]
		public override long Id { get; set; }

		/// <summary>
		/// 行政代码
		/// </summary>
		[Column(Name = "area_code", DbType = "varchar(20)")]
		public string AreaCode { get; set; }

		/// <summary>
		/// 区号
		/// </summary>
		[Column(Name = "city_code", DbType = "varchar(6)")]
		public string CityCode { get; set; }

		/// <summary>
		/// 纬度
		/// </summary>
		[Column(Name = "lat", DbType = "decimal(10,6)")]
		public decimal? Lat { get; set; }

		/// <summary>
		/// 层级
		/// </summary>
		[Column(Name = "level_code")]
		public byte? LevelCode { get; set; }

		/// <summary>
		/// 经度
		/// </summary>
		[Column(Name = "lng", DbType = "decimal(10,6)")]
		public decimal? Lng { get; set; }

		/// <summary>
		/// 组合名
		/// </summary>
		[Column(Name = "merger_name", DbType = "varchar(50)")]
		public string MergerName { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[Column(Name = "name", DbType = "varchar(50)")]
		public string Name { get; set; }

		/// <summary>
		/// 父级行政代码
		/// </summary>
		[Column(Name = "parent_code", DbType = "varchar(20)")]
		public string ParentCode { get; set; }

		/// <summary>
		/// 拼音
		/// </summary>
		[Column(Name = "pinyin", DbType = "varchar(30)")]
		public string Pinyin { get; set; }

		/// <summary>
		/// 简称
		/// </summary>
		[Column(Name = "short_name", DbType = "varchar(50)")]
		public string ShortName { get; set; }

		/// <summary>
		/// 邮政编码
		/// </summary>
		[Column(Name = "zip_code", DbType = "varchar(6)")]
		public string ZipCode { get; set; }
	}
}
