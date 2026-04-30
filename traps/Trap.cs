using Raylib_cs;

namespace LowCortisolIO;

public enum TrapType
{
    Spike,
    PopupSpike,
    Saw,
    TeleportSpike,
    MovingSpike
}

public sealed class Trap
{
    public Rectangle Rect;
    public Rectangle StartRect;
    public Rectangle EndRect;
    public Rectangle HiddenRect;
    public Rectangle VisibleRect;
    public TrapType Type;

    public bool Active = true;
    public bool Visible = true;
    public bool Forward = true;
    public float Speed = 0f;
    public float Timer = 0f;
    public float Interval = 0f;

    public Trap(float x, float y, float w, float h, TrapType type)
    {
        Rect = new Rectangle(x, y, w, h);
        StartRect = Rect;
        EndRect = Rect;
        HiddenRect = Rect;
        VisibleRect = Rect;
        Type = type;
    }

    public void Reset()
    {
        Rect = StartRect;
        Active = true;
        Visible = Type != TrapType.PopupSpike;
        Forward = true;
        Timer = 0f;

        if (Type == TrapType.PopupSpike)
            Rect = HiddenRect;
    }
}
