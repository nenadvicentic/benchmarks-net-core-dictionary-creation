# Benchmark different ways to create dictionary in .NET Core #

## Original Stackoverflow Question from Oct 2011 ##

Idea for this test stems from Stackoverflow question I asked 8 years ago (Oct 2011):

[How to convert IEnumerable of KeyValuePair<x, y> to Dictionary?](https://stackoverflow.com/q/7850334/186822)

Since I unsuccesful attempted to use `.ToDictionary()` extension method from `System.Linq` namespace,correct answer was to add lambda selectors for `key` and `value`:

```
.ToDictionary(kvp=>kvp.Key,kvp=>kvp.Value);
```

My assumption was that, since `Dictionary` is implementation of  `ICollection<KeyValuePair<TKey, TValue>>`, there has to be "direct", most efficient way to convert `IEnumerable<KeyValuePair<TKey, TValue>>` to `Dictionary`.


## Benchmark creation of dictionary in .NET Core 3.0 using BenchmarkNet Nuget package ##

Few days ago (Nov 2019) there were few upvotes/downvotes on the original question, so I have taken a look at the my quesiton and answers again. With the hindsight, I wondered why I simply did not use `Dictionary<TKey, TValue>` constructor which accepts `IEnumerable<KeyValuePair<TKey, TValue>>`.

There was also one answer proposing usage of `IDictionary<TKey, TValue>` interface, claming 20% improvement over `System.Linq`'s `ToDictionary(..)` extension method. I started to wonder if `Dictionary<TKey, TVAlue>` constructor overload accepting `IEnumerable<KeyValuePair<TKey, TValue>>` is more efficient than both extension method and `IDictionary` casting.

Several months ago,  while reading [Stephen Toub](https://github.com/stephentoub)'s blog post [Performance Improvements in .NET Core 3.0](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core-3-0/), I played with [Benchmark.NET](https://github.com/dotnet/benchmarkdotnet) library, so I decided to use same methodology to measure performance of 3 methods above.

Benchmarks can be run using following command from the repository root folder:

```> dotnet run -c Release```

## (Surprising) Benchmark Results ##

|                                           Method |     Mean |     Error |    StdDev |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|------------------------------------------------- |---------:|----------:|----------:|----------:|---------:|---------:|----------:|
|                 CreateUsingDictionaryConstructor | 3.396 ms | 0.0300 ms | 0.0281 ms |  257.8125 | 257.8125 | 257.8125 |    2.9 MB |
|                         CreateUsingLinqExtension | 2.891 ms | 0.0271 ms | 0.0254 ms |  253.9063 | 253.9063 | 253.9063 |    2.9 MB |
| CreateUsingCodeCopiedFromLinqExtensionSourceCode | 5.565 ms | 0.0806 ms | 0.0714 ms |  601.5625 | 554.6875 | 554.6875 |   8.06 MB |
|                       CreateUsingForEachAndApend | 3.917 ms | 0.0204 ms | 0.0171 ms | 1906.2500 |        - |        - |   7.63 MB |
|            CreateUsingForEachApendAndInitialSize | 5.230 ms | 0.1346 ms | 0.1124 ms | 2296.8750 | 390.6250 | 390.6250 |  10.53 MB |
|                  CreateUsingIDictionaryInterface | 5.823 ms | 0.0451 ms | 0.0422 ms |  578.1250 | 531.2500 | 531.2500 |   8.06 MB |