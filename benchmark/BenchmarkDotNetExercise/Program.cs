// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using BenchmarkDotNetExercise;

Console.WriteLine("Hello, World!");

// BenchmarkRunner.Run<GenerateSequentialId>();

BenchmarkRunner.Run<PropertyDynamicGet>();
