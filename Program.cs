using Raylib_cs;

Raylib.InitWindow(800, 600, "Test");

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Raylib_cs.Color.RayWhite);

    Raylib.DrawText("Hello", 300, 280, 20, Raylib_cs.Color.Black);

    Raylib.EndDrawing();
}

Raylib.CloseWindow();