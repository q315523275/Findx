using Findx.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisDataBaseProvider : IStackExchangeRedisDataBaseProvider, IDisposable
    {
        private readonly ILogger<StackExchangeRedisDataBaseProvider> _logger;

        private readonly SemaphoreSlim _connectionLock;

        private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connectionMultiplexerCache;

        private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connectionMultiplexerAsyncCache;

        public StackExchangeRedisDataBaseProvider(ILogger<StackExchangeRedisDataBaseProvider> logger)
        {
            _logger = logger;
            _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
            _connectionMultiplexerCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();
            _connectionMultiplexerAsyncCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();
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

        public ConnectionMultiplexer GetConnection(RedisOptions options)
        {
            Check.NotNull(options, nameof(options));

            _connectionMultiplexerCache.TryGetValue(options.Name, out var _connectionMultiplexer);
            if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
            {
                return _connectionMultiplexer;
            }

            _connectionLock.Wait();
            try
            {
                _connectionMultiplexerCache.TryGetValue(options.Name, out _connectionMultiplexer);
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    return _connectionMultiplexer;
                }

                _connectionMultiplexer?.Dispose();
                _connectionMultiplexer = null;

                _connectionMultiplexer = ConnectionMultiplexer.Connect(options.Configuration);
                _connectionMultiplexer.ConnectionFailed += Conn_ConnectionFailed;
                _connectionMultiplexer.ConnectionRestored += Conn_ConnectionRestored;
                _connectionMultiplexer.ErrorMessage += Conn_ErrorMessage;

                _connectionMultiplexerCache[options.Name] = _connectionMultiplexer;

                return _connectionMultiplexer;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task<ConnectionMultiplexer> GetConnectionAsync(RedisOptions options, CancellationToken cancellationToken = default)
        {
            Check.NotNull(options, nameof(options));

            _connectionMultiplexerAsyncCache.TryGetValue(options.Name, out var _connectionMultiplexer);
            if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
            {
                return _connectionMultiplexer;
            }

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                _connectionMultiplexerAsyncCache.TryGetValue(options.Name, out _connectionMultiplexer);
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    return _connectionMultiplexer;
                }

                _connectionMultiplexer?.Dispose();
                _connectionMultiplexer = null;
                _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(options.Configuration);
                _connectionMultiplexer.ConnectionFailed += Conn_ConnectionFailed;
                _connectionMultiplexer.ConnectionRestored += Conn_ConnectionRestored;
                _connectionMultiplexer.ErrorMessage += Conn_ErrorMessage;

                _connectionMultiplexerAsyncCache[options.Name] = _connectionMultiplexer;

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

        /// <summary>
        /// 获取 服务器列表
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public IEnumerable<IServer> GetServerList(RedisOptions options)
        {
            var endpoints = GetMastersServersEndpoints(options);
            foreach (var endPoint in endpoints)
            {
                yield return GetConnection(options).GetServer(endPoint, options);
            }
        }

        /// <summary>
        /// 获取当前Redis服务器
        /// </summary>
        /// <param name="hostAndPort"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public IServer GetServer(string hostAndPort, RedisOptions options)
        {
            return GetConnection(options).GetServer(hostAndPort);
        }

        /// <summary>
        /// 获取主服务器端点列表
        /// </summary>
        /// <returns></returns>
        private List<EndPoint> GetMastersServersEndpoints(RedisOptions options)
        {
            var masters = new List<EndPoint>();
            foreach (var endPoint in GetConnection(options).GetEndPoints())
            {
                var server = GetConnection(options).GetServer(endPoint);
                if (server.IsConnected)
                {
                    // 集群
                    if (server.ServerType == ServerType.Cluster)
                    {
                        // n.IsSlave => n.IsReplica
                        masters.AddRange(server.ClusterConfiguration.Nodes.Where(n => !n.IsReplica).Select(n => n.EndPoint));
                        break;
                    }
                    // 单节点，主-从
                    if (server.ServerType == ServerType.Standalone & !server.IsReplica)
                    {
                        masters.Add(endPoint);
                        break;
                    }
                }
            }

            return masters;
        }
    }
}
