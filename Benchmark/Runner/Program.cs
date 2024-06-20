using BenchmarkDotNet.Running;
using Runner;

var summary = BenchmarkRunner.Run<Benchmarks>();
Console.ReadLine();
