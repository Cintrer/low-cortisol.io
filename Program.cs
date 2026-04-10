using Raylib_cs;
using System.Numerics;

class Program
{
    static void Main()
    {
        int screenWidth = 800;
        int screenHeight = 600;

        Raylib.InitWindow(screenWidth, screenHeight, "Test Level");

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();

            // fond blanc
            Raylib.ClearBackground(Color.White);

            // couleurs
            Color rose = new Color(255, 105, 180, 255);
            Color roseFonce = new Color(200, 50, 130, 255);

            // SOL
            Raylib.DrawRectangle(0, 500, 800, 100, rose);
            Raylib.DrawRectangleLines(0, 500, 800, 100, roseFonce);

            // PLAFOND
            Raylib.DrawRectangle(0, 0, 800, 20, rose);
            Raylib.DrawRectangleLines(0, 0, 800, 20, roseFonce);

            // MUR GAUCHE
            Raylib.DrawRectangle(0, 0, 20, 600, rose);
            Raylib.DrawRectangleLines(0, 0, 20, 600, roseFonce);

            // MUR DROIT
            Raylib.DrawRectangle(780, 0, 20, 600, rose);
            Raylib.DrawRectangleLines(780, 0, 20, 600, roseFonce);

            // TEXTE TEST
            Raylib.DrawText("ZONE DE TEST", 300, 250, 20, Color.Black);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}