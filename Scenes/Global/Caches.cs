using System;
using Godot;
using Godot.Collections;

namespace MusicMachine.Scenes.Global
{
public class Caches : Node
{
    //Singleton, not Static: unique to each game instance.
    private readonly Dictionary<Mesh, Shape> _convexShapeCaches =
        new Dictionary<Mesh, Shape>();

    private readonly Dictionary<Mesh, Shape> _trimeshShapeCache =
        new Dictionary<Mesh, Shape>();

    private static Caches _instance;

    public static Caches Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("No Caches Singleton Instantiated!");
            return _instance;
        }
    }

    public override void _EnterTree()
    {
        if (_instance != null)
            throw new InvalidOperationException("Caches Singleton Created Twice!");
        _instance = this;
    }

    public override void _ExitTree()
    {
        _convexShapeCaches.Clear();
        _trimeshShapeCache.Clear();
        _instance = null;
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