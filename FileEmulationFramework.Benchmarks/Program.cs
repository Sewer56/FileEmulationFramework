// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using FileEmulationFramework.Benchmarks;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<MemoryManagerReadBenchmarks>();