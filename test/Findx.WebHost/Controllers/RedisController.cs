using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Locks;
using Findx.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     Redis服务
/// </summary>
[Route("api/redis")]
[Description("Redis服务"), Tags("Redis服务")]
public class RedisController : ApiControllerBase
{
    /// <summary>
    ///     Geo
    /// </summary>
    /// <param name="redisClientProvider"></param>
    /// <returns></returns>
    [HttpGet("geo")]
    public async Task<string> Geo([FromServices] IRedisClientProvider redisClientProvider)
    {
        var redisClient = redisClientProvider.CreateClient();

        await redisClient.GeoAddAsync("cin.oms_geo", new List<(double longitude, double latitude, string member)> { (118.763709, 32.106839, "1") });

        await redisClient.SortedSetRemoveAsync("cin.oms_geo", new List<string> { "1" });

        return DateTime.Now.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     锁
    /// </summary>
    /// <param name="redisClientProvider"></param>
    /// <returns></returns>
    [HttpGet("lock_verify_value")]
    public async Task<string> LockVerifyValue([FromServices] IRedisClientProvider redisClientProvider)
    {
        var redisClient = redisClientProvider.CreateClient();

        var lockResult = await redisClient.LockTakeAsync("lock_verify_value", 1, TimeSpan.FromSeconds(30));
        var unlockResult = await redisClient.LockReleaseAsync("lock_verify_value", 2);

        return $"{lockResult}|{unlockResult}";
    }

    /// <summary>
    ///     续期锁
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="lockType"></param>
    /// <returns></returns>
    [HttpGet("renewLock")]
    public async Task<string> AutoLiveLock([FromServices] ILockFactory provider, [Required] string lockType)
    {
        var @lock = provider.Create(lockType);

        var fetlock = await @lock.AcquireAsync("test_renew_lock", renew: true);

        if (!fetlock.IsLocked())
            return "未拿到锁";

        try
        {
            await Task.Delay(26 * 1000);
        }
        finally
        {
            await fetlock.ReleaseAsync();
        }

        return DateTime.Now.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Key是否存在
    /// </summary>
    /// <param name="redisClientProvider"></param>
    /// <returns></returns>
    [HttpGet("exist")]
    public async Task<string> KeyExist([FromServices] IRedisClientProvider redisClientProvider)
    {
        var redisClient = redisClientProvider.CreateClient();

        await redisClient.ExpireAsync("exist", TimeSpan.FromSeconds(60));

        return (await redisClient.ExistsAsync("exist")).ToString();
    }
}