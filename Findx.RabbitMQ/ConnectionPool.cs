using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Threading;

namespace Findx.RabbitMQ
{
    public class ConnectionPool : IConnectionPool, IDisposable
    {
        private RabbitMQOptions _options;
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private bool _isDisposed;

        private readonly ILogger<ConnectionPool> _logger;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        public ConnectionPool(IOptionsMonitor<RabbitMQOptions> options, ILogger<ConnectionPool> logger)
        {
            _logger = logger;
            _options = options.CurrentValue;
            options.OnChange(ConfigurationOnChange);
            CreateConnectionFactory();
        }

        private void ConfigurationOnChange(RabbitMQOptions changeOptions)
        {
            if (changeOptions.ToString() != _options.ToString())
            {
                _options = changeOptions;

                Dispose();

                CreateConnectionFactory();

                _logger.LogInformation("RabbitMQ client configuration update");
            }
        }

        private void CreateConnectionFactory()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                ClientProvidedName = _options.ClientProvidedName ?? $"{Environment.MachineName}-{Process.GetCurrentProcess()?.Id}"
            };
        }

        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_isDisposed;
            }
        }

        public IConnection Acquire()
        {
            if (IsConnected)
                return _connection;

            _connectionLock.Wait();
            try
            {
                if (!IsConnected)
                {
                    _connection = _connectionFactory.CreateConnection();

                    if (IsConnected)
                    {
                        _connection.ConnectionShutdown += OnConnectionShutdown;
                        _connection.CallbackException += OnCallbackException;
                        _connection.ConnectionBlocked += OnConnectionBlocked;
                    }
                }
            }
            finally
            {
                _connectionLock.Release();
            }
            return _connection;
        }

        public void Dispose()
        {
            if (IsConnected)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                try
                {
                    _connection.Dispose();
                }
                catch
                {

                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_isDisposed) return;

            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            Acquire();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_isDisposed) return;

            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            Acquire();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_isDisposed) return;

            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            Acquire();
        }
    }
}
