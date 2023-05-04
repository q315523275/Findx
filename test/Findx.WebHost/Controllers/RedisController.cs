using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Findx.Locks;
using Findx.Redis;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

public class RedisController : Controller
{
    [HttpGet("/redis/geo")]
    public async Task<string> Geo([FromServices] IRedisClientProvider redisClientProvider)
    {
        var redisClient = redisClientProvider.CreateClient();

        await redisClient.GeoAddAsync("cin.oms_geo",
            new List<(double longitude, double latitude, string member)> { (118.763709, 32.106839, "1") });

        await redisClient.SortedSetRemoveAsync("cin.oms_geo", new List<string> { "1" });

        return DateTime.Now.ToString();
    }

    [HttpGet("/redis/lock_verify_value")]
    public async Task<string> LockVerifyValue([FromServices] IRedisClientProvider redisClientProvider)
    {
        var redisClient = redisClientProvider.CreateClient();

        var lock_result = await redisClient.LockTakeAsync("lock_verify_value", 1, TimeSpan.FromSeconds(30));
        var unlock_result = await redisClient.LockReleaseAsync("lock_verify_value", 2);

        return $"{lock_result}|{unlock_result}";
    }

    [HttpGet("/redis/renewLock")]
    public async Task<string> AutoLiveLock([FromServices] ILockProvider provider, [Required] LockType lockType)
    {
        var @lock = provider.Get(lockType);

        var getlock = await @lock.AcquireAsync("test_renew_lock", renew: true);

        if (!getlock.IsLocked())
            return "未拿到锁";

        try
        {
            await Task.Delay(26 * 1000);
        }
        finally
        {
            await getlock.ReleaseAsync();
        }

        return DateTime.Now.ToString();
    }

    [HttpGet("/redis/exist")]
    public async Task<string> KeyExist([FromServices] IRedisClientProvider redisClientProvider)
    {
        var redisClient = redisClientProvider.CreateClient();

        await redisClient.ExpireAsync("noexist", TimeSpan.FromSeconds(60));

        return (await redisClient.ExistsAsync("noexist")).ToString();
    }
}