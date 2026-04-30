using Raylib_cs;

namespace LowCortisolIO;

public enum PlatformType
{
    Solid,
    Moving,
    Falling,
    Fake
}

public sealed class PlatformBlock
{
    public Rectangle Rect;
    public Rectangle StartRect;
    public Rectangle EndRect;
    public PlatformType Type;

    public bool Active = true;
    public bool Triggered = false;
    public bool Forward = true;
    public float Speed = 0f;
    public float FallSpeed = 0f;
    public float ResetTimer = 0f;

    public PlatformBlock(float x, float y, float w, float h, PlatformType type)
    {
        Rect = new Rectangle(x, y, w, h);
        StartRect = Rect;
        EndRect = Rect;
        Type = type;
    }
}
