using System;
using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.Admin.Captcha
{
	/// <summary>
	/// 点击验证码输入参数
	/// </summary>
	public class ClickWordCaptchaRequest: IRequest
	{
        /// <summary>
        /// 验证码类型
        /// </summary>
        [Required(ErrorMessage = "验证码类型")]
        public string CaptchaType { get; set; }

        /// <summary>
        /// 坐标点集合
        /// </summary>
        [Required(ErrorMessage = "坐标点集合不能为空")]
        public string PointJson { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
    }
}

