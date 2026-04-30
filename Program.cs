using LowCortisolIO;
using Raylib_cs;

Raylib.InitWindow(GameConstants.ScreenWidth, GameConstants.ScreenHeight, "low-cortisol.io");
Raylib.SetTargetFPS(60);

Game game = new();
game.Run();

Raylib.CloseWindow();
