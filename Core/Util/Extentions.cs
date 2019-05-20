using System;
using System.Collections.Generic;
using Godot;
using Object = Godot.Object;

namespace MusicMachine
{
public static class StructEx
{
    public static TValue GetElsePut<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> ifAbsent)
    {
        if (!dict.TryGetValue(key, out var val))
            val = dict[key] = ifAbsent();
        return val;
    }
    public static bool TryGetAndRemove<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out TValue value)
    {
        var b = dict.TryGetValue(key, out value);
        if (b)
            dict.Remove(key);
        return b;
    }
    public static IList<T> Swap<T>(this IList<T> list, int a, int b)
    {
        var tmp = list[a];
        list[a] = list[b];
        list[b] = tmp;
        return list;
    }
    public static bool TryFind<T>(this IEnumerable<T> list, out T value, Predicate<T> match)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));
        foreach (var i in list)
            if (match(i))
            {
                value = i;
                return true;
            }

        value = default;
        return false;
    }
    public static int BinarySearchIndexOf<T>(this IList<T> list, T value, IComparer<T> comparer = null)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));
        comparer = comparer ?? Comparer<T>.Default;
        var lower = 0;
        var upper = list.Count - 1;

        while (lower <= upper)
        {
            var middle = lower + ((upper - lower) >> 1);
            var comp   = comparer.Compare(value, list[middle]);
            if (comp == 0)
                return middle;
            if (comp < 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }

        return ~lower;
    }

    public static bool IsEmpty<T>(this IReadOnlyCollection<T> collection) => collection.Count == 0;

    public static bool NotEmpty<T>(this IReadOnlyCollection<T> collection) => collection.Count != 0;

    public static IEnumerable<T> ListReverse<T>(this IList<T> list)
    {
        for (var i = list.Count - 1; i >= 0; i--)
            yield return list[i];
    }
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (var item in enumeration)
            action(item);
    }
    public static bool AddIfAbsent<T>(this ICollection<T> collection, T item)
    {
        var absent = !collection.Contains(item);
        if (absent)
            collection.Add(item);
        return absent;
    }
}

public static class VectorEx
{
    public static Vector3 ClampX(
        this Vector3 @this,
        float min,
        float max)
    {
        @this.x = Mathf.Clamp(@this.x, min, max);
        return @this;
    }
    public static Vector3 ClampY(
        this Vector3 @this,
        float min,
        float max)
    {
        @this.y = Mathf.Clamp(@this.y, min, max);
        return @this;
    }
    public static Vector3 ClampZ(
        this Vector3 @this,
        float min,
        float max)
    {
        @this.z = Mathf.Clamp(@this.z, min, max);
        return @this;
    }
    public static Vector2 ClampX(
        this Vector2 @this,
        float min,
        float max)
    {
        @this.x = Mathf.Clamp(@this.x, min, max);
        return @this;
    }
    public static Vector2 ClampY(
        this Vector2 @this,
        float min,
        float max)
    {
        @this.y = Mathf.Clamp(@this.y, min, max);
        return @this;
    }
    public static void RotateBy(this Spatial spatial, Vector3 rot)
    {
        spatial.RotateX(rot.x);
        spatial.RotateY(rot.y);
        spatial.RotateZ(rot.z);
    }
}

public static class GodotEx
{
    public static bool Is(this Object obj, string property) => obj.Get(property) as bool? ?? false;

    public static bool TryCall(
        this Object obj,
        string method,
        params object[] args)
    {
        if (!obj.HasMethod(method))
            return false;
        obj.Call(method, args);
        return true;
    }
    public static bool TryCall(
        this Object obj,
        out object o,
        string method,
        params object[] args)
    {
        if (!obj.HasMethod(method))
        {
            o = null;
            return false;
        }

        o = obj.Call(method, args);
        return true;
    }
    public static bool CallOrNull(
        this Object obj,
        string method,
        out object result,
        params object[] args)
    {
        var o = obj.HasMethod(method);
        result = o ? obj.Call(method, args) : null;
        return o;
    }
    public static Timer CreateAndConnectTimer(
        this Node node,
        string method,
        bool physics = true,
        bool oneShot = true)
    {
        var timer = new Timer
        {
            OneShot = oneShot, ProcessMode = physics ? Timer.TimerProcessMode.Physics : Timer.TimerProcessMode.Idle
        };
        node.AddChild(timer);
        timer.Connect("timeout", node, method);
        return timer;
    }
    public static Shape CreateShape(this Mesh mesh, bool isTrimesh) =>
        isTrimesh ? mesh.CreateTrimeshShape() : mesh.CreateConvexShape();
}
}