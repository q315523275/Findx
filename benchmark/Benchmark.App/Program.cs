// See https://aka.ms/new-console-template for more information

using Benchmark.App;
using BenchmarkDotNet.Running;

Console.WriteLine("Hello, World!");

var summary = BenchmarkRunner.Run<IdGenerator>();
