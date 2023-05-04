﻿using System.Text;
using RabbitMQ.Client;

namespace Findx.RabbitMQ
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IConnectionPool _connectionPool;
        private readonly IRabbitMqSerializer _serializer;

        public RabbitMqPublisher(IConnectionPool connectionPool, IRabbitMqSerializer serializer)
        {
            _connectionPool = connectionPool;
            _serializer = serializer;
        }

        public void Publish(object obj, string exchangeName, string exchangeType, string routingKey)
        {
            var message = _serializer.Serialize(obj);
            var body = Encoding.UTF8.GetBytes(message);

            using (var channel = _connectionPool.Get().CreateModel())
            {
                // 创建并配置交换器
                channel.ExchangeDeclare(exchangeName, exchangeType);
                // 创建队列属性
                var properties = channel.CreateBasicProperties();
                // 决定发送数据类型
                properties.ContentType = "application/json";
                // 是否持久化  1 no  2 yes
                properties.DeliveryMode = 2;
                // 发送数据
                channel.BasicPublish(exchangeName, routingKey, true, properties, body);
            }
        }
    }
}