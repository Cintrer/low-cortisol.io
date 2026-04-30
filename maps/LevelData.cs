using Raylib_cs;
using System.Numerics;

namespace LowCortisolIO;

public sealed class Door
{
    public Rectangle Rect;

    public Door(float x, float y, float w, float h)
    {
        Rect = new Rectangle(x, y, w, h);
    }
}

public sealed class LevelData
{
    public int Width;
    public int Height;
    public float FloorY;
    public Vector2 Spawn;
    public Door ExitDoor = new Door(0, 0, 26, 38);

    public Color BackgroundColor;
    public Color RoomColor;
    public Color BorderColor;
    public Color VoidColor;
    public Color PlatformColor;
    public Color PlatformEdgeColor;

    public List<PlatformBlock> Platforms = new();
    public List<Trap> Traps = new();

    public LevelData(
        int width,
        int height,
        float floorY,
        Vector2 spawn,
        Color backgroundColor,
        Color roomColor,
        Color borderColor,
        Color voidColor,
        Color platformColor,
        Color platformEdgeColor)
    {
        Width = width;
        Height = height;
        FloorY = floorY;
        Spawn = spawn;
        BackgroundColor = backgroundColor;
        RoomColor = roomColor;
        BorderColor = borderColor;
        VoidColor = voidColor;
        PlatformColor = platformColor;
        PlatformEdgeColor = platformEdgeColor;
    }
}
