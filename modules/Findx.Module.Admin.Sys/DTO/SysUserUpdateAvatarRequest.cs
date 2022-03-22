using System;
using System.ComponentModel.DataAnnotations;

namespace Findx.Module.Admin.Sys.DTO
{
	public class SysUserUpdateAvatarRequest
	{
		/// <summary>
		/// 头像信息
		/// </summary>
		[Required(ErrorMessage = "头像信息不能为空")]
		public SysFileInfoDTO Avatar { set; get; }
	}


}

