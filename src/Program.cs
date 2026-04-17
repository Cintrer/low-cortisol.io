using Raylib_cs;
using System.Numerics;

namespace LevelDevilBase;

public static class Program
{
    public static void Main()
    {
        const int screenWidth = 1280;
        const int screenHeight = 720;

        Raylib.InitWindow(screenWidth, screenHeight, "Level Devil Base - C#");
        Raylib.SetTargetFPS(60);

        var level = new Level();
        var player = new Player(new Vector2(120, 520));

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            player.Update(dt, level.Platforms);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(18, 18, 24, 255));

            level.Draw();
            player.Draw();

            Raylib.DrawText("A / D ou fleches = bouger", 20, 20, 24, Color.White);
            Raylib.DrawText("SPACE = sauter", 20, 50, 24, Color.White);
            Raylib.DrawText("SHIFT = courir", 20, 80, 24, Color.White);
            Raylib.DrawText("Remplace les images dans assets/player/run", 20, 120, 20, Color.Yellow);

            Raylib.EndDrawing();
        }

        player.Unload();
        Raylib.CloseWindow();
    }
}
