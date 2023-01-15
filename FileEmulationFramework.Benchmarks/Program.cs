// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using FileEmulationFramework.Benchmarks;

//BenchmarkRunner.Run<RouteMergingBenchmarks>();
//BenchmarkRunner.Run<MemoryManagerReadBenchmarks>();
//BenchmarkRunner.Run<MemoryManagerWriteBenchmarks>();
//BenchmarkRunner.Run<OffsetSelectionBenchmarks>();
//BenchmarkRunner.Run<DelegateCallVsDoubleDictLookup>();
BenchmarkRunner.Run<SearchDirectoryBenchmark>();