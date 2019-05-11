using System;
using System.Collections.Generic;

namespace MusicMachine
{
public static class StructEx
{
    public static TValue GetElsePut<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> ifAbsent)
    {
        if (!dict.TryGetValue(key, out var val)) val = dict[key] = ifAbsent();
        return val;
    }


    public static TValue GetAndRemove<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
    {
        var o = dict[key];
        dict.Remove(key);
        return o;
    }

    public static IList<T> Swap<T>(this IList<T> list, int a, int b)
    {
        var tmp = list[a];
        list[a] = list[b];
        list[b] = tmp;
        return list;
    }
}
}