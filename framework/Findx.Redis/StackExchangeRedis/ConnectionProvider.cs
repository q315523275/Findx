using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Findx.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Findx.Redis.StackExchangeRedis
{
    public class ConnectionProvider : IConnectionProvider, IDisposable
    {
        private readonly ILogger<ConnectionProvider> _logger;

        private bool _isDisposed;


        public ConnectionProvider(IOptions<RedisOptions> options, ILogger<ConnectionProvider> logger)
        {
            _logger = logger;
            Options = options.Value;
            Connections = new ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>>();
        }

        private RedisOptions Options { get; }

        private ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>> Connections { get; }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            foreach (var connection in Connections.Values)
                try
                {
                    connection?.Value?.Dispose();
                }
                catch
                {
                    // ignored
                }

            Connections.Clear();
        }


        public ConnectionMultiplexer GetConnection(string connectionName = null)
        {
            connectionName ??= RedisConnections.DefaultConnectionName;

            try
            {
                var lazyConnection = Connections.GetOrAdd(
                    connectionName, () => new Lazy<ConnectionMultiplexer>(() =>
                    {
                        var configuration = Options.Connections.GetOrDefault(connectionName);
                        var connection = ConnectionMultiplexer.Connect(configuration);
                        connection.ConnectionFailed += Conn_ConnectionFailed;
                        connection.ConnectionRestored += Conn_ConnectionRestored;
                        connection.ErrorMessage += Conn_ErrorMessage;
                        return connection;
                    })
                );

                return lazyConnection.Value;
            }
            catch (Exception ex)
            {
                Connections.TryRemove(connectionName, out _);
                ex.ReThrow();
                return default;
            }
        }

        public IServer GetServer(string hostAndPort, string connectionName = null)
        {
            connectionName ??= RedisConnections.DefaultConnectionName;

            return GetConnection(connectionName).GetServer(hostAndPort);
        }

        public IEnumerable<IServer> GetServerList(string connectionName = null)
        {
            connectionName ??= RedisConnections.DefaultConnectionName;

            var endpoints = GetMastersServersEndpoints(connectionName);
            foreach (var endPoint in endpoints)
                yield return GetConnection(connectionName).GetServer(endPoint, connectionName);
        }


        /// <summary>
        ///     获取主服务器端点列表
        /// </summary>
        /// <returns></returns>
        private List<EndPoint> GetMastersServersEndpoints(string connectionName)
        {
            var masters = new List<EndPoint>();
            foreach (var endPoint in GetConnection(connectionName).GetEndPoints())
            {
                var server = GetConnection(connectionName).GetServer(endPoint);
                if (server.IsConnected)
                {
                    // 集群
                    if (server.ServerType == ServerType.Cluster)
                    {
                        // n.IsSlave => n.IsReplica
                        if (server.ClusterConfiguration != null)
                            masters.AddRange(server.ClusterConfiguration.Nodes.Where(n => !n.IsReplica)
                                .Select(n => n.EndPoint));
                        break;
                    }

                    // 单节点，主-从
                    if ((server.ServerType == ServerType.Standalone) & !server.IsReplica)
                    {
                        masters.Add(endPoint);
                        break;
                    }
                }
            }

            return masters;
        }


        private void Conn_ErrorMessage(object sender, RedisErrorEventArgs e)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogError($"redis内部发生错误,{e.EndPoint}:{e.Message}");
        }

        private void Conn_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogError($"redis重新连接时发生错误,{e.EndPoint}{e.Exception.FormatMessage()}");
        }

        private void Conn_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogError($"redis连接时发生错误,{e.EndPoint}{e.Exception.FormatMessage()}");
        }
    }
}