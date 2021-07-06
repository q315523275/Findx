namespace Findx.RabbitMQ
{
    public interface IRabbitConsumerBuilder
    {
        /// <summary>
        /// 初始化消费者数据
        /// </summary>
        void Initialize();
        /// <summary>
        /// 构建消费者
        /// </summary>
        void Build();
    }
}
