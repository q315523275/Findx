using BenchmarkDotNet.Running;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Reflection>();

            Console.WriteLine("Hello World!");

            Console.ReadLine();
        }
    }
}
