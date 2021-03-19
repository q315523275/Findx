using Findx.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
namespace Findx.Redis.StackExchangeRedis
{
    public class ConnectionPool : IConnectionPool, IDisposable
    {

        private readonly IOptionsMonitor<RedisCacheOptions> _options;

        private readonly ILogger<ConnectionPool> _logger;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        private readonly ConcurrentDictionary<int, ConnectionMultiplexer> _connectionMultiplexerCache = new ConcurrentDictionary<int, ConnectionMultiplexer>();

        private readonly ConcurrentDictionary<int, ConnectionMultiplexer> _connectionMultiplexerAsyncCache = new ConcurrentDictionary<int, ConnectionMultiplexer>();

        public ConnectionPool(IOptionsMonitor<RedisCacheOptions> options, ILogger<ConnectionPool> logger)
        {
            _logger = logger;
            _options = options;
        }

        private RedisCacheOptions Options
        {
            get
            {
                if (_options != null)
                {
                    return _options.CurrentValue;
                }
                return default;
            }
        }

        public void Dispose()
        {
            foreach (var conn in _connectionMultiplexerCache.Values)
            {
                conn?.Dispose();
            }
            _connectionMultiplexerCache.Clear();

            foreach (var conn in _connectionMultiplexerAsyncCache.Values)
            {
                conn?.Dispose();
            }
            _connectionMultiplexerAsyncCache.Clear();
        }

        public ConnectionMultiplexer Acquire(string connection = null)
        {
            connection = connection ?? Options?.Configuration;

            Check.NotNullOrWhiteSpace(connection, nameof(connection));

            var hashCode = connection.GetHashCode();

            _connectionMultiplexerCache.TryGetValue(hashCode, out var _connectionMultiplexer);
            if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
            {
                return _connectionMultiplexer;
            }

            _connectionLock.Wait();
            try
            {
                _connectionMultiplexerCache.TryGetValue(hashCode, out _connectionMultiplexer);
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    return _connectionMultiplexer;
                }

                _connectionMultiplexer?.Dispose();
                _connectionMultiplexer = null;

                _connectionMultiplexer = ConnectionMultiplexer.Connect(connection);
                _connectionMultiplexer.ConnectionFailed += Conn_ConnectionFailed;
                _connectionMultiplexer.ConnectionRestored += Conn_ConnectionRestored;
                _connectionMultiplexer.ErrorMessage += Conn_ErrorMessage;

                _connectionMultiplexerCache[hashCode] = _connectionMultiplexer;

                return _connectionMultiplexer;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task<ConnectionMultiplexer> AcquireAsync(string connection = null, CancellationToken cancellationToken = default)
        {
            connection = connection ?? Options?.Configuration;

            Check.NotNullOrWhiteSpace(connection, nameof(connection));

            var hashCode = connection.GetHashCode();

            _connectionMultiplexerAsyncCache.TryGetValue(hashCode, out var _connectionMultiplexer);
            if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
            {
                return _connectionMultiplexer;
            }

            await _connectionLock.WaitAsync();
            try
            {
                _connectionMultiplexerAsyncCache.TryGetValue(hashCode, out _connectionMultiplexer);
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    return _connectionMultiplexer;
                }

                _connectionMultiplexer?.Dispose();
                _connectionMultiplexer = null;
                _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connection);
                _connectionMultiplexer.ConnectionFailed += Conn_ConnectionFailed;
                _connectionMultiplexer.ConnectionRestored += Conn_ConnectionRestored;
                _connectionMultiplexer.ErrorMessage += Conn_ErrorMessage;

                _connectionMultiplexerAsyncCache[hashCode] = _connectionMultiplexer;

                return _connectionMultiplexer;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private void Conn_ErrorMessage(object sender, RedisErrorEventArgs e)
        {
            _logger.LogError($"redis内部发生错误,{e.EndPoint}:{e.Message}");
        }

        private void Conn_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            _logger.LogError($"redis重新连接时发生错误,{e.EndPoint}{e.Exception.FormatMessage()}");
        }

        private void Conn_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            _logger.LogError($"redis连接时发生错误,{e.EndPoint}{e.Exception.FormatMessage()}");
        }
    }
}
