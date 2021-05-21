namespace Findx.RabbitMQ
{
    public class RabbitMQOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { set; get; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ClientProvidedName { get; set; }
    }
}
