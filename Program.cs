using Raylib_cs;
using System.Numerics;

class Program
{
    static void Main()
    {
        Raylib.InitWindow(800, 600, "Background Infinite");


        Texture2D back = Raylib.LoadTexture("back.jpg");

        float bgX = 0;

        while (!Raylib.WindowShouldClose())
        {
        
            bgX -= 0.01f; // vitesse 

            if (bgX <= -back.Width)
            {
                bgX = 0;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            // Dessine le fond 2 fois pour faire la boucle
            Raylib.DrawTexture(back, (int)bgX, 0, Color.White);
            Raylib.DrawTexture(back, (int)bgX + back.Width, 0, Color.White);

            Raylib.EndDrawing();
        }

        Raylib.UnloadTexture(back);
        Raylib.CloseWindow();
    }
}