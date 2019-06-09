using System;
using Godot;
using Godot.Collections;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Global
{
public sealed class Caches : Node
{
    private static Caches _instance;

    //Singleton, not Static: unique to each game instance.
    private readonly Dictionary<Mesh, Shape> _convexShapeCaches = new Dictionary<Mesh, Shape>();
    private readonly Dictionary<Mesh, Shape> _trimeshShapeCache = new Dictionary<Mesh, Shape>();

    public static Caches Instance
    {
        get => _instance ?? throw new InvalidOperationException($"No {nameof(Caches)} node instantiated");
        private set
        {
            if (value != null && _instance != null)
                throw new InvalidOperationException($"Two {nameof(Caches)} nodes instantiated!");
            _instance = value;
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        Instance = null;
        _convexShapeCaches.Clear();
        _trimeshShapeCache.Clear();
    }

    public static Shape GetOrCreateShape(Mesh mesh, bool isTrimesh)
    {
        var shapeCache = isTrimesh ? Instance._trimeshShapeCache : Instance._convexShapeCaches;
        if (!shapeCache.TryGetValue(mesh, out var o))
            o = shapeCache[mesh] = mesh.CreateShape(isTrimesh);
        return o;
    }
}

public static class CachesEx
{
    public static Shape GetCachedShape(this Mesh mesh, bool isTrimesh) => Caches.GetOrCreateShape(mesh, isTrimesh);
}
}