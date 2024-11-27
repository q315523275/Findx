using BenchmarkDotNet.Attributes;
using DotNetCore.CAP.Internal;
using Findx.Utilities;
using NewLife.Data;
using Yitter.IdGenerator;
namespace BenchmarkDotNetExercise;

[MemoryDiagnoser]//记录内存分配情况
public class GenerateSequentialId
{
    private static readonly SnowflakeId CapSnowflakeId;
    private static readonly Snowflake NewLifeSnowflakeId;
    private static readonly SnowflakeIdUtility FindxSnowflakeId;
    
    static GenerateSequentialId()
    {
        var options = new IdGeneratorOptions { WorkerIdBitLength = 10, SeqBitLength = 12 };
        YitIdHelper.SetIdGenerator(options);
        CapSnowflakeId = new SnowflakeId();
        NewLifeSnowflakeId = new Snowflake();
        MassTransit.NewId.NextSequentialGuid();
        FindxSnowflakeId = new SnowflakeIdUtility(46);
        
        Console.WriteLine(Guid.NewGuid());
        Console.WriteLine(SequentialGuidUtility.Next(SequentialGuidType.AsString));
        Console.WriteLine($"Findx:" + SnowflakeIdUtility.Default().NextId());
        Console.WriteLine($"YitIdHelper:" + YitIdHelper.NextId());
        Console.WriteLine($"NewLife.Snowflake:" + NewLifeSnowflakeId.NewId());
    }
    
    [Benchmark]
    public void MassTransit_NewId()
    {
        MassTransit.NewId.NextSequentialGuid();
    }
    
    [Benchmark]
    public void CAP_SnowflakeId()
    {
        CapSnowflakeId.NextId();
    }
    
    [Benchmark]
    public void NewLife_Snowflake()
    {
        NewLifeSnowflakeId.NewId();
    }
    
    [Benchmark]
    public void Findx_SnowflakeId()
    {
        FindxSnowflakeId.NextId();
    }
    
    [Benchmark]
    public void YitId_IdGenerator()
    {
        YitIdHelper.NextId();
    }
    
    [Benchmark]
    public void Guid_CreateVersion7()
    {
        Guid.CreateVersion7();
    }
}