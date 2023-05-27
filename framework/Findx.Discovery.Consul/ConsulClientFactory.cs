﻿using System;
using Consul;
using Findx.Utils;

namespace Findx.Discovery.Consul
{
    public static class ConsulClientFactory
    {
        public static IConsulClient CreateClient(ConsulOptions options)
        {
            Check.NotNull(options, nameof(options));

            var client = new ConsulClient(s =>
            {
                s.Address = new Uri($"{options.Scheme}://{options.Host}:{options.Port}");
                s.Token = options.Token;
                s.Datacenter = options.Datacenter;
                if (!string.IsNullOrEmpty(options.WaitTime)) s.WaitTime = Time.ToTimeSpan(options.WaitTime);
            });

            return client;
        }
    }
}