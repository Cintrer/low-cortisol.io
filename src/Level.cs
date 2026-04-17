using Raylib_cs;

namespace LevelDevilBase;

public class Level
{
    public List<Rectangle> Platforms { get; } = new()
    {
        new Rectangle(0, 650, 1280, 70),
        new Rectangle(260, 560, 180, 24),
        new Rectangle(540, 500, 150, 24),
        new Rectangle(790, 430, 170, 24),
        new Rectangle(1030, 360, 130, 24),
    };

    public void Draw()
    {
        foreach (var platform in Platforms)
        {
            Raylib.DrawRectangleRec(platform, new Color(120, 80, 50, 255));
        }
    }
}
