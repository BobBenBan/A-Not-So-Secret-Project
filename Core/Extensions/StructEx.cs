using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MusicMachine.Extensions
{
public static class StructEx
{
    public static TValue GetElsePut<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> ifAbsent)
    {
        if(!dict.TryGetValue(key, out var val)) val = dict[key] = ifAbsent();
        return val;
    }


    public static TValue GetAndRemove<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
    {
        var o = dict[key];
        dict.Remove(key);
        return o;
    }

    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) => dict.ContainsKey(key) ? dict[key] : default;
}
}