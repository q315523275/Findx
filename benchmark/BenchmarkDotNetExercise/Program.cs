// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using BenchmarkDotNetExercise;

Console.WriteLine("Hello, World!");

var summary = BenchmarkRunner.Run<GenerateSequentialId>();
