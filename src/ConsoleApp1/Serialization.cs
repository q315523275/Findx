using BenchmarkDotNet.Attributes;

namespace ConsoleApp1
{
    public class Serialization
    {
        public class user
        {
            public string name { set; get; }
            public int age { set; get; }
        }
        user testUser = new user { age = 99, name = "haha" };

        [Benchmark]
        public string Utf8JsonSerializer()
        {
            return Utf8Json.JsonSerializer.ToJsonString(testUser);
        }
        [Benchmark]
        public string SystemTextJsonSerializer()
        {
            return System.Text.Json.JsonSerializer.Serialize(testUser);
        }
    }
}
