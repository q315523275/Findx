using Microsoft.Extensions.Options;

namespace Findx.Email
{
    public class EmailSenderOptions : IOptions<EmailSenderOptions>
    {
        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string FromDisplayName { get; set; }

        /// <summary>
        /// 发送地址
        /// </summary>
        public string FromAddress { get; set; }

        public EmailSenderOptions Value => this;
    }
}
