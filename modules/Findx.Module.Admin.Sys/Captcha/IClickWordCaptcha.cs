using System;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Captcha
{
	public interface IClickWordCaptcha
	{
		Task<ClickWordCaptchaResult> CheckCode(ClickWordCaptchaRequest input);

		Task<ClickWordCaptchaResult> CreateCaptchaImage(string code, int width, int height, int point = 3);

		string RandomCode(int number);
	}
}

