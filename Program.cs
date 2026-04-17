using Raylib_cs;
using System.Numerics;

namespace LowCortisolIO;

public enum PlayerAnimState
{
    Idle,
    Walk,
    Jump,
    Death
}

public enum TrapType
{
    StaticSpike,
    MoverSpike,
    SawMover,
    PopupSpike,
    TeleportSpike
}

public enum PlatformType
{
    Solid,
    Falling,
    Moving,
    Fake
}

public class FrameAnimation
{
    private readonly List<Texture2D> _frames = new();
    private readonly float _fps;
    private float _timer;
    private int _currentFrame;

    public FrameAnimation(string folder, float fps)
    {
        _fps = fps;

        if (!Directory.Exists(folder))
            return;

        var files = Directory.GetFiles(folder)
            .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToArray();

        foreach (var file in files)
            _frames.Add(Raylib.LoadTexture(file));
    }

    public void Update(float dt, bool loop = true)
    {
        if (_frames.Count <= 1) return;

        _timer += dt;
        float frameDuration = 1f / _fps;

        while (_timer >= frameDuration)
        {
            _timer -= frameDuration;

            if (loop) _currentFrame = (_currentFrame + 1) % _frames.Count;
            else if (_currentFrame < _frames.Count - 1) _currentFrame++;
        }
    }

    public void Reset()
    {
        _timer = 0f;
        _currentFrame = 0;
    }

    public Texture2D? Current()
    {
        if (_frames.Count == 0) return null;
        return _frames[_currentFrame];
    }

    public void Unload()
    {
        foreach (var frame in _frames)
            Raylib.UnloadTexture(frame);
    }
}

public class PlayerVisuals
{
    public FrameAnimation Idle { get; }
    public FrameAnimation Walk { get; }
    public FrameAnimation Death { get; }

    public PlayerVisuals()
    {
        Idle = new FrameAnimation("assets/player/idle", 8f);
        Walk = new FrameAnimation("assets/player/walk", 11f);
        Death = new FrameAnimation("assets/player/death", 9f);
    }

    public void Unload()
    {
        Idle.Unload();
        Walk.Unload();
        Death.Unload();
    }
}

public class Platform
{
    public Rectangle Rect;
    public Rectangle StartRect;
    public Rectangle EndRect;
    public PlatformType Type;
    public bool Active = true;
    public bool Triggered = false;
    public float Speed = 0f;
    public bool Forward = true;
    public float FallSpeed = 0f;
    public float ResetTimer = 0f;

    public Platform(float x, float y, float w, float h, PlatformType type)
    {
        Rect = new Rectangle(x, y, w, h);
        StartRect = Rect;
        EndRect = Rect;
        Type = type;
    }
}

public class Trap
{
    public Rectangle Rect;
    public Rectangle StartRect;
    public Rectangle EndRect;
    public Rectangle HiddenRect;
    public Rectangle VisibleRect;
    public TrapType Type;
    public bool Active = true;
    public bool Forward = true;
    public float Speed = 0f;
    public float Timer = 0f;
    public float Interval = 0f;
    public bool Visible = true;

    public Trap(float x, float y, float w, float h, TrapType type)
    {
        Rect = new Rectangle(x, y, w, h);
        StartRect = Rect;
        EndRect = Rect;
        HiddenRect = Rect;
        VisibleRect = Rect;
        Type = type;
    }
}

public class Door
{
    public Rectangle Rect;

    public Door(float x, float y, float w, float h)
    {
        Rect = new Rectangle(x, y, w, h);
    }
}

public class Level
{
    public List<Platform> Platforms = new();
    public List<Trap> Traps = new();
    public Door ExitDoor = new Door(0, 0, 22, 34);
    public Vector2 Spawn = new Vector2(90, 470);
    public int Width = 3200;
    public int Height = 900;
    public float DeathLineY = 820f;
    public float FloorY = 760f;
    public Color Background = new Color(165, 72, 36, 255);
    public Color RoomColor = new Color(235, 190, 50, 255);
}

public struct Player
{
    public Rectangle Rect;
    public Vector2 Vel;
    public Vector2 Spawn;
    public bool OnGround;
    public bool FacingRight;
    public bool Dead;
    public int Deaths;
    public float DeathTimer;
    public PlayerAnimState State;
}

public static class Program
{
    const int SCREEN_WIDTH = 1280;
    const int SCREEN_HEIGHT = 720;

    const float GRAVITY = 1200f;
    const float MOVE_SPEED = 175f;
    const float JUMP_SPEED = 455f;
    const float MAX_FALL_SPEED = 800f;

    static readonly Color SolidColor = new(215, 78, 38, 255);
    static readonly Color SolidEdge = new(192, 62, 26, 255);
    static readonly Color FakeColor = new(175, 58, 26, 255);
    static readonly Color FallingColor = new(200, 70, 30, 255);
    static readonly Color MovingColor = new(190, 85, 45, 255);
    static readonly Color RoomShadow = new(226, 179, 45, 255);
    static readonly Color VoidColor = new(70, 16, 18, 255);
    static readonly Color DoorColor = new(215, 215, 220, 255);

    static Player player;
    static Camera2D camera;
    static readonly PlayerVisuals visuals = new();
    static List<Level> levels = new();
    static int currentLevelIndex = 0;
    static bool finishedGame = false;

    public static void Main()
    {
        Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "low-cortisol.io");
        Raylib.SetTargetFPS(60);

        camera = new Camera2D
        {
            Offset = new Vector2(SCREEN_WIDTH / 2f, SCREEN_HEIGHT / 2f),
            Target = Vector2.Zero,
            Rotation = 0f,
            Zoom = 1f
        };

        levels = BuildLevels();
        LoadLevel(0, true);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            if (Raylib.IsKeyPressed(KeyboardKey.R))
                LoadLevel(currentLevelIndex, false);

            if (!finishedGame)
            {
                UpdateLevel(dt);
                UpdatePlayer(dt);
                UpdateAnimations(dt);
                UpdateCamera();
                CheckDoor();
            }
            else
            {
                UpdateCamera();
            }

            Draw();
        }

        visuals.Unload();
        Raylib.CloseWindow();
    }

    static List<Level> BuildLevels()
    {
        return new List<Level> { BuildLevel1(), BuildLevel2(), BuildLevel3() };
    }

    static Level BuildLevel1()
    {
        Level l = CreateRoomLevel(3200, 900, 760, 980, new Vector2(125, 620));

        AddP(l, 0, 760, 420, 140, PlatformType.Solid);
        AddP(l, 500, 720, 70, 16, PlatformType.Solid);
        AddP(l, 645, 690, 70, 16, PlatformType.Solid);
        AddP(l, 790, 660, 72, 16, PlatformType.Fake);
        AddP(l, 940, 625, 76, 16, PlatformType.Solid);
        AddP(l, 1090, 590, 78, 16, PlatformType.Moving, 1180, 590, 60f);
        AddP(l, 1265, 555, 78, 16, PlatformType.Solid);
        AddP(l, 1430, 520, 76, 16, PlatformType.Falling);
        AddP(l, 1600, 485, 80, 16, PlatformType.Solid);
        AddP(l, 1780, 450, 80, 16, PlatformType.Solid);
        AddP(l, 1960, 415, 82, 16, PlatformType.Moving, 1960, 350, 62f);
        AddP(l, 2145, 380, 78, 16, PlatformType.Solid);
        AddP(l, 2320, 345, 76, 16, PlatformType.Fake);
        AddP(l, 2490, 310, 78, 16, PlatformType.Solid);
        AddP(l, 2665, 275, 78, 16, PlatformType.Solid);
        AddP(l, 2845, 240, 126, 16, PlatformType.Solid);

        AddSpike(l, 450, 742, 28, 18);
        AddPopup(l, 595, 760, 20, 0, 595, 708, 20, 52);
        AddMovingSpike(l, 1040, 742, 22, 18, 1040, 610, 72f);
        AddSaw(l, 1525, 500, 26, 26, 1700, 500, 70f);
        AddTeleportSpike(l, 2055, 742, 24, 18, 2055, 365, 1.35f);
        AddPopup(l, 2420, 345, 20, 0, 2420, 293, 20, 52);

        l.ExitDoor = new Door(2928, 206, 24, 34);
        return l;
    }

    static Level BuildLevel2()
    {
        Level l = CreateRoomLevel(3600, 950, 800, 890, new Vector2(120, 655));

        AddP(l, 0, 800, 300, 150, PlatformType.Solid);
        AddP(l, 390, 760, 92, 16, PlatformType.Moving, 520, 760, 72f);
        AddP(l, 590, 730, 82, 16, PlatformType.Solid);
        AddP(l, 770, 700, 84, 16, PlatformType.Falling);
        AddP(l, 940, 670, 86, 16, PlatformType.Solid);
        AddP(l, 1115, 635, 86, 16, PlatformType.Fake);
        AddP(l, 1290, 600, 86, 16, PlatformType.Solid);
        AddP(l, 1475, 565, 90, 16, PlatformType.Moving, 1475, 485, 74f);
        AddP(l, 1675, 530, 84, 16, PlatformType.Solid);
        AddP(l, 1860, 495, 84, 16, PlatformType.Falling);
        AddP(l, 2035, 460, 84, 16, PlatformType.Solid);
        AddP(l, 2225, 425, 88, 16, PlatformType.Solid);
        AddP(l, 2410, 390, 84, 16, PlatformType.Moving, 2525, 390, 70f);
        AddP(l, 2615, 355, 82, 16, PlatformType.Solid);
        AddP(l, 2795, 320, 82, 16, PlatformType.Fake);
        AddP(l, 2970, 285, 86, 16, PlatformType.Solid);
        AddP(l, 3160, 250, 132, 16, PlatformType.Solid);

        AddSpike(l, 325, 782, 34, 18);
        AddPopup(l, 708, 800, 22, 0, 708, 745, 22, 55);
        AddMovingSpike(l, 1250, 782, 26, 18, 1250, 582, 98f);
        AddSaw(l, 1768, 512, 30, 30, 1960, 512, 100f);
        AddTeleportSpike(l, 2350, 782, 30, 18, 2460, 372, 0.95f);
        AddPopup(l, 2855, 320, 22, 0, 2855, 265, 22, 55);
        AddSaw(l, 3025, 220, 30, 30, 3225, 220, 104f);

        l.ExitDoor = new Door(3238, 216, 24, 34);
        return l;
    }

    static Level BuildLevel3()
    {
        Level l = CreateRoomLevel(4100, 1000, 840, 940, new Vector2(125, 690));

        AddP(l, 0, 840, 320, 160, PlatformType.Solid);
        AddP(l, 420, 800, 86, 16, PlatformType.Fake);
        AddP(l, 600, 765, 82, 16, PlatformType.Solid);
        AddP(l, 775, 730, 84, 16, PlatformType.Falling);
        AddP(l, 950, 695, 88, 16, PlatformType.Solid);
        AddP(l, 1130, 660, 88, 16, PlatformType.Moving, 1260, 660, 70f);
        AddP(l, 1335, 625, 82, 16, PlatformType.Solid);
        AddP(l, 1510, 590, 84, 16, PlatformType.Fake);
        AddP(l, 1690, 555, 84, 16, PlatformType.Solid);
        AddP(l, 1875, 520, 88, 16, PlatformType.Moving, 1875, 450, 72f);
        AddP(l, 2080, 485, 84, 16, PlatformType.Solid);
        AddP(l, 2265, 450, 84, 16, PlatformType.Falling);
        AddP(l, 2445, 415, 84, 16, PlatformType.Solid);
        AddP(l, 2625, 380, 88, 16, PlatformType.Solid);
        AddP(l, 2810, 345, 88, 16, PlatformType.Moving, 2930, 345, 72f);
        AddP(l, 3025, 310, 82, 16, PlatformType.Solid);
        AddP(l, 3200, 275, 82, 16, PlatformType.Fake);
        AddP(l, 3380, 240, 82, 16, PlatformType.Solid);
        AddP(l, 3560, 205, 140, 16, PlatformType.Solid);

        AddSpike(l, 345, 822, 34, 18);
        AddPopup(l, 520, 840, 22, 0, 520, 785, 22, 55);
        AddMovingSpike(l, 1210, 822, 26, 18, 1210, 642, 98f);
        AddSaw(l, 1782, 538, 30, 30, 1962, 538, 96f);
        AddTeleportSpike(l, 2328, 822, 30, 18, 2485, 397, 1.0f);
        AddPopup(l, 3265, 275, 22, 0, 3265, 220, 22, 55);
        AddSaw(l, 3438, 175, 30, 30, 3620, 175, 105f);

        l.ExitDoor = new Door(3645, 171, 24, 34);
        return l;
    }

    static Level CreateRoomLevel(int width, int height, float floorY, float deathLineY, Vector2 spawn)
    {
        Level l = new();
        l.Width = width;
        l.Height = height;
        l.FloorY = floorY;
        l.DeathLineY = deathLineY;
        l.Spawn = spawn;
        l.Background = new Color(162, 76, 39, 255);
        l.RoomColor = new Color(241, 194, 54, 255);

        // room borders / closed map
        l.Platforms.Add(new Platform(-80, 0, 80, height, PlatformType.Solid));
        l.Platforms.Add(new Platform(width, 0, 80, height, PlatformType.Solid));
        l.Platforms.Add(new Platform(0, -80, width, 80, PlatformType.Solid));
        l.Platforms.Add(new Platform(0, floorY, width, 220, PlatformType.Solid));
        return l;
    }

    static void AddP(Level l, float x, float y, float w, float h, PlatformType type)
    {
        l.Platforms.Add(new Platform(x, y, w, h, type));
    }

    static void AddP(Level l, float x, float y, float w, float h, PlatformType type, float endX, float endY, float speed)
    {
        var p = new Platform(x, y, w, h, type)
        {
            EndRect = new Rectangle(endX, endY, w, h),
            Speed = speed
        };
        l.Platforms.Add(p);
    }

    static void AddSpike(Level l, float x, float y, float w, float h)
    {
        l.Traps.Add(new Trap(x, y, w, h, TrapType.StaticSpike));
    }

    static void AddMovingSpike(Level l, float x, float y, float w, float h, float endX, float endY, float speed)
    {
        var t = new Trap(x, y, w, h, TrapType.MoverSpike)
        {
            EndRect = new Rectangle(endX, endY, w, h),
            Speed = speed
        };
        l.Traps.Add(t);
    }

    static void AddSaw(Level l, float x, float y, float w, float h, float endX, float endY, float speed)
    {
        var t = new Trap(x, y, w, h, TrapType.SawMover)
        {
            EndRect = new Rectangle(endX, endY, w, h),
            Speed = speed
        };
        l.Traps.Add(t);
    }

    static void AddPopup(Level l, float hiddenX, float hiddenY, float hiddenW, float hiddenH, float showX, float showY, float showW, float showH)
    {
        var t = new Trap(hiddenX, hiddenY, hiddenW, hiddenH, TrapType.PopupSpike)
        {
            HiddenRect = new Rectangle(hiddenX, hiddenY, hiddenW, hiddenH),
            VisibleRect = new Rectangle(showX, showY, showW, showH),
            Visible = false
        };
        l.Traps.Add(t);
    }

    static void AddTeleportSpike(Level l, float startX, float startY, float w, float h, float endX, float endY, float interval)
    {
        var t = new Trap(startX, startY, w, h, TrapType.TeleportSpike)
        {
            StartRect = new Rectangle(startX, startY, w, h),
            EndRect = new Rectangle(endX, endY, w, h),
            Interval = interval
        };
        l.Traps.Add(t);
    }

    static Level CurrentLevel() => levels[currentLevelIndex];

    static void LoadLevel(int index, bool resetDeaths)
    {
        int savedDeaths = resetDeaths ? 0 : player.Deaths;

        currentLevelIndex = index;
        finishedGame = false;

        Level l = CurrentLevel();

        player = new Player
        {
            Rect = new Rectangle(l.Spawn.X, l.Spawn.Y, 22, 32),
            Vel = Vector2.Zero,
            Spawn = l.Spawn,
            OnGround = false,
            FacingRight = true,
            Dead = false,
            Deaths = savedDeaths,
            DeathTimer = 0f,
            State = PlayerAnimState.Idle
        };

        foreach (var p in l.Platforms)
        {
            p.Rect = p.StartRect;
            p.Active = true;
            p.Triggered = false;
            p.Forward = true;
            p.FallSpeed = 0f;
            p.ResetTimer = 0f;
        }

        foreach (var t in l.Traps)
        {
            t.Rect = t.StartRect;
            t.Active = true;
            t.Forward = true;
            t.Timer = 0f;
            t.Visible = t.Type != TrapType.PopupSpike;
            if (t.Type == TrapType.PopupSpike) t.Rect = t.HiddenRect;
        }

        visuals.Idle.Reset();
        visuals.Walk.Reset();
        visuals.Death.Reset();
    }

    static void KillPlayer()
    {
        if (player.Dead) return;

        player.Dead = true;
        player.DeathTimer = 0.55f;
        player.Vel = Vector2.Zero;
        player.State = PlayerAnimState.Death;
        visuals.Death.Reset();
    }

    static void RespawnPlayer()
    {
        player.Rect.X = player.Spawn.X;
        player.Rect.Y = player.Spawn.Y;
        player.Vel = Vector2.Zero;
        player.OnGround = false;
        player.FacingRight = true;
        player.Dead = false;
        player.Deaths++;
        player.DeathTimer = 0f;
        player.State = PlayerAnimState.Idle;

        foreach (var p in CurrentLevel().Platforms)
        {
            p.Rect = p.StartRect;
            p.Active = true;
            p.Triggered = false;
            p.Forward = true;
            p.FallSpeed = 0f;
            p.ResetTimer = 0f;
        }

        foreach (var t in CurrentLevel().Traps)
        {
            t.Rect = t.StartRect;
            t.Active = true;
            t.Forward = true;
            t.Timer = 0f;
            t.Visible = t.Type != TrapType.PopupSpike;
            if (t.Type == TrapType.PopupSpike) t.Rect = t.HiddenRect;
        }

        visuals.Idle.Reset();
        visuals.Walk.Reset();
        visuals.Death.Reset();
    }

    static void UpdateLevel(float dt)
    {
        Level l = CurrentLevel();

        foreach (var p in l.Platforms)
        {
            if (p.Type == PlatformType.Moving && p.Active)
            {
                Vector2 target = p.Forward ? new Vector2(p.EndRect.X, p.EndRect.Y) : new Vector2(p.StartRect.X, p.StartRect.Y);
                Vector2 pos = new(p.Rect.X, p.Rect.Y);
                Vector2 dir = target - pos;

                if (dir.Length() < 3f)
                {
                    p.Forward = !p.Forward;
                    target = p.Forward ? new Vector2(p.EndRect.X, p.EndRect.Y) : new Vector2(p.StartRect.X, p.StartRect.Y);
                    dir = target - pos;
                }

                if (dir != Vector2.Zero) dir = Vector2.Normalize(dir);
                p.Rect.X += dir.X * p.Speed * dt;
                p.Rect.Y += dir.Y * p.Speed * dt;
            }

            if (p.Type == PlatformType.Falling)
            {
                if (!p.Active)
                {
                    p.ResetTimer += dt;
                    if (p.ResetTimer >= 2.2f)
                    {
                        p.Active = true;
                        p.Triggered = false;
                        p.FallSpeed = 0f;
                        p.Rect = p.StartRect;
                        p.ResetTimer = 0f;
                    }
                }
                else if (p.Triggered)
                {
                    p.FallSpeed += GRAVITY * 0.55f * dt;
                    p.Rect.Y += p.FallSpeed * dt;

                    if (p.Rect.Y > l.DeathLineY + 120)
                    {
                        p.Active = false;
                        p.ResetTimer = 0f;
                    }
                }
            }

            if (p.Type == PlatformType.Fake && p.Triggered)
            {
                p.Active = false;
                p.ResetTimer += dt;
                if (p.ResetTimer >= 2.0f)
                {
                    p.Active = true;
                    p.Triggered = false;
                    p.ResetTimer = 0f;
                }
            }
        }

        foreach (var t in l.Traps)
        {
            if (t.Type == TrapType.MoverSpike || t.Type == TrapType.SawMover)
            {
                Vector2 target = t.Forward ? new Vector2(t.EndRect.X, t.EndRect.Y) : new Vector2(t.StartRect.X, t.StartRect.Y);
                Vector2 pos = new(t.Rect.X, t.Rect.Y);
                Vector2 dir = target - pos;

                if (dir.Length() < 3f)
                {
                    t.Forward = !t.Forward;
                    target = t.Forward ? new Vector2(t.EndRect.X, t.EndRect.Y) : new Vector2(t.StartRect.X, t.StartRect.Y);
                    dir = target - pos;
                }

                if (dir != Vector2.Zero) dir = Vector2.Normalize(dir);
                t.Rect.X += dir.X * t.Speed * dt;
                t.Rect.Y += dir.Y * t.Speed * dt;
            }

            if (t.Type == TrapType.TeleportSpike)
            {
                t.Timer += dt;
                if (t.Timer >= t.Interval)
                {
                    t.Timer = 0f;
                    bool atStart = MathF.Abs(t.Rect.X - t.StartRect.X) < 1f && MathF.Abs(t.Rect.Y - t.StartRect.Y) < 1f;
                    t.Rect = atStart ? t.EndRect : t.StartRect;
                }
            }

            if (t.Type == TrapType.PopupSpike)
            {
                if (!t.Visible && player.Rect.X + player.Rect.Width > t.VisibleRect.X - 24f)
                {
                    t.Visible = true;
                    t.Rect = t.VisibleRect;
                }
            }
        }
    }

    static void UpdatePlayer(float dt)
    {
        if (player.Dead)
        {
            player.DeathTimer -= dt;
            if (player.DeathTimer <= 0f)
                RespawnPlayer();
            return;
        }

        float move = 0f;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) move -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move += 1f;

        if (move > 0) player.FacingRight = true;
        if (move < 0) player.FacingRight = false;

        player.Vel.X = move * MOVE_SPEED;

        if ((Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up)) && player.OnGround)
        {
            player.Vel.Y = -JUMP_SPEED;
            player.OnGround = false;
        }

        player.Vel.Y += GRAVITY * dt;
        if (player.Vel.Y > MAX_FALL_SPEED) player.Vel.Y = MAX_FALL_SPEED;

        player.Rect.X += player.Vel.X * dt;
        ResolveHorizontal();

        player.Rect.Y += player.Vel.Y * dt;
        player.OnGround = false;
        ResolveVertical(dt);

        UpdatePlayerState(move);

        // kill only when player reaches the very bottom line
        if (player.Rect.Y >= CurrentLevel().DeathLineY)
            KillPlayer();

        if (HitTrap(player.Rect))
            KillPlayer();
    }

    static void UpdatePlayerState(float move)
    {
        if (player.Dead)
        {
            player.State = PlayerAnimState.Death;
            return;
        }

        if (!player.OnGround)
        {
            player.State = PlayerAnimState.Jump;
            return;
        }

        if (MathF.Abs(move) > 0.01f)
        {
            player.State = PlayerAnimState.Walk;
            return;
        }

        player.State = PlayerAnimState.Idle;
    }

    static void ResolveHorizontal()
    {
        foreach (var p in CurrentLevel().Platforms)
        {
            if (!p.Active) continue;

            if (Raylib.CheckCollisionRecs(player.Rect, p.Rect))
            {
                if (player.Vel.X > 0) player.Rect.X = p.Rect.X - player.Rect.Width;
                if (player.Vel.X < 0) player.Rect.X = p.Rect.X + p.Rect.Width;
            }
        }
    }

    static void ResolveVertical(float dt)
    {
        foreach (var p in CurrentLevel().Platforms)
        {
            if (!p.Active) continue;

            if (Raylib.CheckCollisionRecs(player.Rect, p.Rect))
            {
                if (player.Vel.Y > 0)
                {
                    player.Rect.Y = p.Rect.Y - player.Rect.Height;
                    player.Vel.Y = 0f;
                    player.OnGround = true;

                    if (p.Type == PlatformType.Falling && !p.Triggered)
                    {
                        p.Triggered = true;
                        p.FallSpeed = 0f;
                    }

                    if (p.Type == PlatformType.Fake && !p.Triggered)
                    {
                        p.Triggered = true;
                        p.ResetTimer = 0f;
                    }

                    if (p.Type == PlatformType.Moving)
                    {
                        Vector2 delta = GetMovingDelta(p, dt);
                        player.Rect.X += delta.X;
                        player.Rect.Y += delta.Y;
                    }
                }
                else if (player.Vel.Y < 0)
                {
                    player.Rect.Y = p.Rect.Y + p.Rect.Height;
                    player.Vel.Y = 0f;
                }
            }
        }
    }

    static Vector2 GetMovingDelta(Platform p, float dt)
    {
        Vector2 target = p.Forward ? new Vector2(p.EndRect.X, p.EndRect.Y) : new Vector2(p.StartRect.X, p.StartRect.Y);
        Vector2 current = new(p.Rect.X, p.Rect.Y);
        Vector2 dir = target - current;
        if (dir != Vector2.Zero) dir = Vector2.Normalize(dir);
        return dir * p.Speed * dt;
    }

    static bool HitTrap(Rectangle rect)
    {
        foreach (var t in CurrentLevel().Traps)
        {
            if (!t.Active) continue;
            if (t.Type == TrapType.PopupSpike && !t.Visible) continue;

            if (Raylib.CheckCollisionRecs(rect, t.Rect))
                return true;
        }
        return false;
    }

    static void CheckDoor()
    {
        if (player.Dead) return;

        if (Raylib.CheckCollisionRecs(player.Rect, CurrentLevel().ExitDoor.Rect))
        {
            if (currentLevelIndex < levels.Count - 1)
            {
                int deaths = player.Deaths;
                LoadLevel(currentLevelIndex + 1, false);
                player.Deaths = deaths;
            }
            else
            {
                finishedGame = true;
            }
        }
    }

    static void UpdateAnimations(float dt)
    {
        switch (player.State)
        {
            case PlayerAnimState.Idle:
                visuals.Idle.Update(dt);
                break;
            case PlayerAnimState.Walk:
                visuals.Walk.Update(dt);
                break;
            case PlayerAnimState.Jump:
                if (visuals.Walk.Current().HasValue) visuals.Walk.Update(dt);
                else visuals.Idle.Update(dt);
                break;
            case PlayerAnimState.Death:
                visuals.Death.Update(dt, false);
                break;
        }
    }

    static void UpdateCamera()
    {
        float halfW = SCREEN_WIDTH / 2f;
        float halfH = SCREEN_HEIGHT / 2f;

        float tx = Math.Clamp(player.Rect.X + player.Rect.Width / 2f, halfW, CurrentLevel().Width - halfW);
        float ty = Math.Clamp(player.Rect.Y + player.Rect.Height / 2f, halfH, CurrentLevel().Height - halfH);

        camera.Target = new Vector2(tx, ty);
    }

    static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(CurrentLevel().Background);

        Raylib.BeginMode2D(camera);

        DrawWorldBackground();
        DrawPlatforms();
        DrawTraps();
        DrawDoor();
        DrawPlayer();

        Raylib.EndMode2D();

        if (finishedGame)
        {
            Raylib.DrawRectangle(360, 250, 560, 150, Raylib.Fade(new Color(30, 20, 20, 255), 0.86f));
            Raylib.DrawRectangleLines(360, 250, 560, 150, new Color(230, 210, 170, 255));
            Raylib.DrawText("FIN DU PROTOTYPE", 447, 292, 34, new Color(240, 220, 170, 255));
        }

        Raylib.EndDrawing();
    }

    static void DrawWorldBackground()
    {
        Level l = CurrentLevel();

        Raylib.DrawRectangle(40, 40, l.Width - 80, (int)l.FloorY - 40, l.RoomColor);
        Raylib.DrawRectangle(40, (int)l.FloorY + 95, l.Width - 80, l.Height - ((int)l.FloorY + 95), VoidColor);

        // top shadow strip inside room
        Raylib.DrawRectangle(40, 40, l.Width - 80, 10, RoomShadow);
    }

    static void DrawPlatforms()
    {
        foreach (var p in CurrentLevel().Platforms)
        {
            if (!p.Active) continue;

            Color fill = SolidColor;
            if (p.Type == PlatformType.Fake) fill = FakeColor;
            if (p.Type == PlatformType.Falling) fill = FallingColor;
            if (p.Type == PlatformType.Moving) fill = MovingColor;

            Raylib.DrawRectangleRec(p.Rect, fill);
            Raylib.DrawRectangle((int)p.Rect.X, (int)p.Rect.Y, (int)p.Rect.Width, 4, SolidEdge);

            if (p.Type == PlatformType.Fake)
            {
                Raylib.DrawLineEx(new Vector2(p.Rect.X + 4, p.Rect.Y + 5), new Vector2(p.Rect.X + p.Rect.Width - 4, p.Rect.Y + p.Rect.Height - 5), 2f, new Color(110, 22, 16, 255));
                Raylib.DrawLineEx(new Vector2(p.Rect.X + p.Rect.Width - 4, p.Rect.Y + 5), new Vector2(p.Rect.X + 4, p.Rect.Y + p.Rect.Height - 5), 2f, new Color(110, 22, 16, 255));
            }
        }
    }

    static void DrawTraps()
    {
        foreach (var t in CurrentLevel().Traps)
        {
            if (!t.Active) continue;
            if (t.Type == TrapType.PopupSpike && !t.Visible) continue;

            if (t.Type == TrapType.SawMover)
            {
                Vector2 center = new(t.Rect.X + t.Rect.Width / 2f, t.Rect.Y + t.Rect.Height / 2f);
                float radius = t.Rect.Width / 2f;

                Raylib.DrawCircleV(center, radius, new Color(180, 180, 180, 255));
                Raylib.DrawCircleLines((int)center.X, (int)center.Y, radius, Color.Black);

                for (int i = 0; i < 8; i++)
                {
                    float a = i * MathF.PI / 4f;
                    Vector2 p1 = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
                    Vector2 p2 = center + new Vector2(MathF.Cos(a + 0.16f), MathF.Sin(a + 0.16f)) * (radius + 7f);
                    Vector2 p3 = center + new Vector2(MathF.Cos(a - 0.16f), MathF.Sin(a - 0.16f)) * (radius + 7f);
                    Raylib.DrawTriangle(p1, p2, p3, new Color(195, 38, 26, 255));
                }
            }
            else
            {
                DrawSpikeStrip(t.Rect);
            }
        }
    }

    static void DrawSpikeStrip(Rectangle rect)
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
                new Color(186, 34, 30, 255)
            );
        }
    }

    static void DrawDoor()
    {
        Rectangle d = CurrentLevel().ExitDoor.Rect;
        Raylib.DrawRectangleRec(d, DoorColor);
        Raylib.DrawRectangleLinesEx(d, 2f, new Color(80, 70, 65, 255));
        Raylib.DrawCircle((int)(d.X + d.Width - 6), (int)(d.Y + d.Height / 2f), 1.8f, new Color(90, 80, 70, 255));
    }

    static void DrawPlayer()
    {
        Texture2D? tex = player.State switch
        {
            PlayerAnimState.Idle => visuals.Idle.Current(),
            PlayerAnimState.Walk => visuals.Walk.Current(),
            PlayerAnimState.Jump => visuals.Walk.Current() ?? visuals.Idle.Current(),
            PlayerAnimState.Death => visuals.Death.Current(),
            _ => null
        };

        if (tex.HasValue)
        {
            Texture2D t = tex.Value;
            Rectangle src = new Rectangle(0, 0, t.Width, t.Height);

            if (!player.FacingRight)
                src = new Rectangle(t.Width, 0, -t.Width, t.Height);

            Raylib.DrawTexturePro(t, src, player.Rect, Vector2.Zero, 0f, Color.White);
        }
        else
        {
            Raylib.DrawRectangleRec(player.Rect, Color.Black);
        }
    }
}
