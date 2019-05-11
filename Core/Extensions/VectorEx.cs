using Godot;

namespace MusicMachine
{
public static class VectorEx
{
    public static Vector3 ClampX(this Vector3 @this, float min,
        float max)
    {
        @this.x = Mathf.Clamp(@this.x, min, max);
        return @this;
    }

    public static Vector3 ClampY(this Vector3 @this, float min,
        float max)
    {
        @this.y = Mathf.Clamp(@this.y, min, max);
        return @this;
    }

    public static Vector3 ClampZ(this Vector3 @this, float min,
        float max)
    {
        @this.z = Mathf.Clamp(@this.z, min, max);
        return @this;
    }

    public static Vector2 ClampX(this Vector2 @this, float min,
        float max)
    {
        @this.x = Mathf.Clamp(@this.x, min, max);
        return @this;
    }

    public static Vector2 ClampY(this Vector2 @this, float min,
        float max)
    {
        @this.y = Mathf.Clamp(@this.y, min, max);
        return @this;
    }
}
}