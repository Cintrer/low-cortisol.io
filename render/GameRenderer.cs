using Raylib_cs;
using System.Numerics;

namespace LowCortisolIO;

public static class GameRenderer
{
    public static void DrawLevel(LevelData level, Player player, int levelIndex, Camera2D camera)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(level.BackgroundColor);

        Raylib.BeginMode2D(camera);

        DrawRoom(level, levelIndex);
        DrawPlatforms(level);
        DrawTraps(level);
        DrawDoor(level.ExitDoor);
        DrawPlayer(player);

        Raylib.EndMode2D();
        Raylib.EndDrawing();
    }

    private static void DrawRoom(LevelData level, int levelIndex)
    {
        Raylib.DrawRectangle(36, 36, level.Width - 72, (int)level.FloorY - 36, level.RoomColor);
        Raylib.DrawRectangle(36, (int)level.FloorY + 56, level.Width - 72, level.Height - ((int)level.FloorY + 56), level.VoidColor);
        Raylib.DrawRectangle(36, 36, level.Width - 72, 10, level.BorderColor);

        if (levelIndex == 2)
        {
            for (int x = 36; x < level.Width - 36; x += 40)
            {
                Raylib.DrawTriangle(
                    new Vector2(x, 36),
                    new Vector2(x + 20, 8),
                    new Vector2(x + 40, 36),
                    new Color(76, 69, 170, 255)
                );
                Raylib.DrawTriangle(
                    new Vector2(x, (int)level.FloorY + 56),
                    new Vector2(x + 20, (int)level.FloorY + 84),
                    new Vector2(x + 40, (int)level.FloorY + 56),
                    new Color(76, 69, 170, 255)
                );
            }
        }
    }

    private static void DrawPlatforms(LevelData level)
    {
        foreach (var platform in level.Platforms)
        {
            if (!platform.Active) continue;

            Color fill = level.PlatformColor;
            Color edge = level.PlatformEdgeColor;

            if (platform.Type == PlatformType.Moving)
                fill = new Color((byte)Math.Min(fill.R + 18, 255), (byte)Math.Min(fill.G + 18, 255), (byte)Math.Min(fill.B + 18, 255), 255);

            if (platform.Type == PlatformType.Falling)
                fill = new Color((byte)Math.Max(fill.R - 8, 0), (byte)Math.Max(fill.G - 8, 0), (byte)Math.Max(fill.B - 8, 0), 255);

            if (platform.Type == PlatformType.Fake)
                fill = new Color((byte)Math.Min(edge.R + 12, 255), edge.G, edge.B, 255);

            Raylib.DrawRectangleRec(platform.Rect, fill);
            Raylib.DrawRectangle((int)platform.Rect.X, (int)platform.Rect.Y, (int)platform.Rect.Width, 4, edge);

            if (platform.Type == PlatformType.Fake)
            {
                Raylib.DrawLineEx(new Vector2(platform.Rect.X + 4, platform.Rect.Y + 4), new Vector2(platform.Rect.X + platform.Rect.Width - 4, platform.Rect.Y + platform.Rect.Height - 4), 2f, new Color(90, 25, 18, 255));
                Raylib.DrawLineEx(new Vector2(platform.Rect.X + platform.Rect.Width - 4, platform.Rect.Y + 4), new Vector2(platform.Rect.X + 4, platform.Rect.Y + platform.Rect.Height - 4), 2f, new Color(90, 25, 18, 255));
            }
        }
    }

    private static void DrawTraps(LevelData level)
    {
        foreach (var trap in level.Traps)
        {
            if (!trap.Active) continue;
            if (trap.Type == TrapType.PopupSpike && !trap.Visible) continue;

            if (trap.Type == TrapType.Saw)
            {
                Vector2 center = new(trap.Rect.X + trap.Rect.Width / 2f, trap.Rect.Y + trap.Rect.Height / 2f);
                float radius = trap.Rect.Width / 2f;

                Raylib.DrawCircleV(center, radius, new Color(180, 180, 180, 255));
                Raylib.DrawCircleLines((int)center.X, (int)center.Y, radius, Color.Black);

                for (int i = 0; i < 8; i++)
                {
                    float a = i * MathF.PI / 4f;
                    Vector2 p1 = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
                    Vector2 p2 = center + new Vector2(MathF.Cos(a + 0.16f), MathF.Sin(a + 0.16f)) * (radius + 7f);
                    Vector2 p3 = center + new Vector2(MathF.Cos(a - 0.16f), MathF.Sin(a - 0.16f)) * (radius + 7f);
                    Raylib.DrawTriangle(p1, p2, p3, new Color(210, 40, 28, 255));
                }
            }
            else
            {
                DrawSpikeStrip(trap.Rect);
            }
        }
    }

    private static void DrawSpikeStrip(Rectangle rect)
    {
        int count = Math.Max(1, (int)(rect.Width / 8f));
        float step = rect.Width / count;

        for (int i = 0; i < count; i++)
        {
            float x = rect.X + i * step;
            Raylib.DrawTriangle(
                new Vector2(x, rect.Y + rect.Height),
                new Vector2(x + step / 2f, rect.Y),
                new Vector2(x + step, rect.Y + rect.Height),
                new Color(192, 34, 30, 255)
            );
        }
    }

    private static void DrawDoor(Door door)
    {
        Rectangle d = door.Rect;
        Raylib.DrawRectangleRec(d, new Color(220, 220, 224, 255));
        Raylib.DrawRectangleLinesEx(d, 2f, new Color(90, 80, 75, 255));
        Raylib.DrawCircle((int)(d.X + d.Width - 7), (int)(d.Y + d.Height / 2f), 2f, new Color(90, 80, 75, 255));
    }

    private static void DrawPlayer(Player player)
    {
        Color c = player.Dead ? new Color(70, 30, 30, 255) : Color.Black;
        Raylib.DrawRectangleRec(player.Rect, c);
    }
}
