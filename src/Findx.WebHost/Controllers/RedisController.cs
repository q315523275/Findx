using Findx.Redis;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.Locks;
namespace Findx.WebHost.Controllers
{
    public class RedisController: Controller
    {
        [HttpGet("/redis/geo")]
        public async Task<string> Geo([FromServices] IRedisClientProvider redisClientProvider)
        {
            var redisClient = redisClientProvider.CreateClient();

            await redisClient.GeoAddAsync($"cin.oms_geo", new List<(double longitude, double latitude, string member)> { (118.763709, 32.106839, "1") });

            await redisClient.SortedSetRemoveAsync($"cin.oms_geo", new List<string> { "1" });

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

        [HttpGet("/redis/autoLiveLock")]
        public async Task<string> AutoLiveLock([FromServices] IDistributedLock _lock)
        {
            var getlock = _lock.GetLock("autoLiveLock", 10);

            try
            {
                if (await getlock.TryLockAsync())
                {
                    await Task.Delay(20 * 1000);
                }
            }
            finally
            {
                await getlock.UnLockAsync();
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
}
