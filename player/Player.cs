using Raylib_cs;
using System.Numerics;

namespace LowCortisolIO;

public sealed class Player
{
    public Rectangle Rect;
    public Vector2 Velocity;
    public Vector2 Spawn;
    public bool OnGround;
    public bool FacingRight;
    public bool Dead;
    public float RespawnTimer;

    public Player(Vector2 spawn)
    {
        Spawn = spawn;
        Rect = new Rectangle(spawn.X, spawn.Y, GameConstants.PlayerWidth, GameConstants.PlayerHeight);
        Velocity = Vector2.Zero;
        OnGround = false;
        FacingRight = true;
        Dead = false;
        RespawnTimer = 0f;
    }

    public void ResetToSpawn()
    {
        Rect.X = Spawn.X;
        Rect.Y = Spawn.Y;
        Velocity = Vector2.Zero;
        OnGround = false;
        FacingRight = true;
        Dead = false;
        RespawnTimer = 0f;
    }

    public void Kill()
    {
        if (Dead) return;
        Dead = true;
        RespawnTimer = 0.45f;
        Velocity = Vector2.Zero;
    }
}
