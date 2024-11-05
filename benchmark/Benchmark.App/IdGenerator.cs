using BenchmarkDotNet.Attributes;
using DotNetCore.CAP.Internal;
using Findx.Utilities;
using NewLife.Data;
using Yitter.IdGenerator;
namespace Benchmark.App;

public class IdGenerator
{
    private const long CyclesCount = 1_000_000;
    private static SnowflakeId _capSnowflakeId;
    private static Snowflake _newLifeSnowflakeId;
    private static SnowflakeIdUtility _findxSnowflakeId;
    
    static IdGenerator()
    {
        var options = new IdGeneratorOptions { WorkerIdBitLength = 10, SeqBitLength = 12 };
        YitIdHelper.SetIdGenerator(options);
        _capSnowflakeId = new SnowflakeId();
        _newLifeSnowflakeId = new Snowflake();
        MassTransit.NewId.NextSequentialGuid();
        _findxSnowflakeId = new SnowflakeIdUtility(46);
        
        Console.WriteLine(Guid.NewGuid());
        Console.WriteLine(SequentialGuidUtility.Next(SequentialGuidType.AsString));
        Console.WriteLine($"Findx:" + SnowflakeIdUtility.Default().NextId());
        Console.WriteLine($"YitIdHelper:" + YitIdHelper.NextId());
        Console.WriteLine($"NewLife.Snowflake:" + _newLifeSnowflakeId.NewId());
    }
    
    [Benchmark]
    public void MassTransit_NewId()
    {
        MassTransit.NewId.NextSequentialGuid();
    }
    
    [Benchmark]
    public void CAP_SnowflakeId()
    {
        _capSnowflakeId.NextId();
    }
    
    [Benchmark]
    public void NewLife_Snowflake()
    {
        _newLifeSnowflakeId.NewId();
    }
    
    [Benchmark]
    public void Findx_SnowflakeId()
    {
        _findxSnowflakeId.NextId();
    }
    
    [Benchmark]
    public void YitId_IdGenerator()
    {
        YitIdHelper.NextId();
    }
}