using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class Program
{
    private const int NUMBER_OF_ITEMS = 100_000;
    static void Main(string[] args) => BenchmarkSwitcher.FromTypes(new[] { typeof(Program) }).Run(args);

    private IEnumerable<KeyValuePair<int, string>> _collection;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var myList = new List<KeyValuePair<int, string>>(NUMBER_OF_ITEMS);
        for (int i = 1; i <= NUMBER_OF_ITEMS; i++)
        {
            myList.Add(new KeyValuePair<int, string>(i, i.ToString(CultureInfo.InvariantCulture)));
        }
        _collection = myList;
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _collection = null;
    }

    [Benchmark]
    public Dictionary<int, string> CreateUsingDictionaryConstructor()
    {
        return new Dictionary<int, string>(_collection);
    }

    [Benchmark]
    public Dictionary<int, string> CreateUsingLinqExtension()
    {
        return _collection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    [Benchmark]
    public Dictionary<int, string> CreateUsingCodeCopiedFromLinqExtensionSourceCode()
    {
        var dict = new Dictionary<int, string>();
        foreach(var kvp in _collection)
            dict.Add(kvp.Key, kvp.Value);
        return dict;
    }

    [Benchmark]
    public Dictionary<int, string> CreateUsingForEachAndApend()
    {
        var dict = new Dictionary<int, string>();
        
        foreach(var kvp in _collection)
            dict.Append(kvp);
        return dict;
    }

    [Benchmark]
    public Dictionary<int, string> CreateUsingForEachApendAndInitialSize()
    {
        var dict = new Dictionary<int, string>(NUMBER_OF_ITEMS);
        foreach(var kvp in _collection)
            dict.Append(kvp);
        return dict;
    }


    [Benchmark]
    public IDictionary<int, string> CreateUsingIDictionaryInterface()
    {
        return ToDictionary(_collection);
    }

    ///<summary>
    /// Solution proposed in Stackoverflow answer:
    /// https://stackoverflow.com/a/37663781/186822
    ///</summary>
    public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
    {
        var dict = new Dictionary<TKey, TValue>();
        var dictAsIDictionary = (IDictionary<TKey, TValue>) dict;
        foreach (var property in keyValuePairs)
        {
            (dictAsIDictionary).Add(property);
        }
        return dict;
    }
}
