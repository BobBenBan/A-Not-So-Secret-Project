using Godot;

namespace MusicMachine.Extensions
{
public static class ObjectEx
{
    public static bool Is(this Object obj, string property) =>
        obj.Get(property) as bool? ?? false;

    public static bool TryCall(this Object obj, string method,
        params object[] args)
    {
        if (!obj.HasMethod(method)) return false;
        obj.Call(method, args);
        return true;
    }
    public static bool CallOrNull(this Object obj, string method, out object result,
        params object[] args)
    {
        var o = obj.HasMethod(method);
        result = o ? obj.Call(method, args) : null;
        return o;
    }
}
}