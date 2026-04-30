using Raylib_cs;
using System.Numerics;

namespace LowCortisolIO;

public enum PlayerAnimState
{
    Idle,
    Walk,
    Jumping,
    DoubleJump,
    JumpWithStrike,
    PlatformJump,
    Landing,
    LandingWithImpact,
    Sprint,
    StopRunning,
    Attack1,
    Attack2,
    ChargedAttack,
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
            .OrderBy(ExtractFrameNumber)
            .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var file in files)
            _frames.Add(Raylib.LoadTexture(file));
    }

    public FrameAnimation(string spriteSheetPath, float fps, int frameCount, int startFrameIndex = 0)
    {
        _fps = fps;

        if (!File.Exists(spriteSheetPath) || frameCount <= 0)
            return;

        Image sheet = Raylib.LoadImage(spriteSheetPath);
        if (sheet.Width <= 0 || sheet.Height <= 0)
            return;

        int totalFrames = Math.Max(1, sheet.Width / Math.Max(1, sheet.Height));
        if (totalFrames <= 0)
        {
            Raylib.UnloadImage(sheet);
            return;
        }

        int clampedStart = Math.Clamp(startFrameIndex, 0, totalFrames - 1);
        int available = totalFrames - clampedStart;
        int takeCount = Math.Min(frameCount, available);
        int frameWidth = Math.Max(1, sheet.Width / totalFrames);

        for (int i = 0; i < takeCount; i++)
        {
            int frameIndex = clampedStart + i;
            Rectangle src = new Rectangle(frameIndex * frameWidth, 0, frameWidth, sheet.Height);
            Image frame = Raylib.ImageFromImage(sheet, src);
            _frames.Add(Raylib.LoadTextureFromImage(frame));
            Raylib.UnloadImage(frame);
        }

        Raylib.UnloadImage(sheet);
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

    public bool IsPlaying => _frames.Count > 1 && _currentFrame < _frames.Count - 1;

    public void Unload()
    {
        foreach (var frame in _frames)
            Raylib.UnloadTexture(frame);
    }

    static int ExtractFrameNumber(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        int i = name.Length - 1;

        while (i >= 0 && char.IsDigit(name[i])) i--;

        if (i == name.Length - 1)
            return int.MaxValue;

        string number = name[(i + 1)..];
        return int.TryParse(number, out int parsed) ? parsed : int.MaxValue;
    }
}

public class PlayerVisuals
{
    public FrameAnimation Idle { get; }
    public FrameAnimation Walk { get; }
    public FrameAnimation Sprint { get; }
    public FrameAnimation StopRunning { get; }
    public FrameAnimation Attack1 { get; }
    public FrameAnimation Attack2 { get; }
    public FrameAnimation ChargedAttack { get; }
    public FrameAnimation SkillCharging { get; }
    public FrameAnimation Jumping { get; }
    public FrameAnimation DoubleJump { get; }
    public FrameAnimation JumpWithStrike { get; }
    public FrameAnimation PlatformJump { get; }
    public FrameAnimation Landing { get; }
    public FrameAnimation LandingWithImpact { get; }
    public FrameAnimation Death { get; }

    public PlayerVisuals()
    {
        string walkSheet = "assets/Animations/Pack Animations 1/Walking.png";
        string sprintSheet = "assets/Animations/Pack Animations 1/Running.png";
        string stopRunningSheet = "assets/Animations/Pack Animations 1/Stop_Running.png";
        string jumpingSheet = "assets/Animations/Pack Animations 1/Jumping.png";
        string doubleJumpSheet = "assets/Animations/Pack Animations 1/Double_Jump.png";
        string jumpWithStrikeSheet = "assets/Animations/Pack Animations 1/Jump witn Strike.png";
        string platformJumpSheet = "assets/Animations/Pack Animations 1/Platform Jump.png";
        string landingSheet = "assets/Animations/Pack Animations 1/Landing.png";
        string landingWithImpactSheet = "assets/Animations/Pack Animations 1/Landing with Impact.png";

        if (File.Exists(walkSheet))
        {
            Walk = new FrameAnimation(walkSheet, 12f, 12);
            // Use one frame from the same character for idle until a dedicated idle sheet is available.
            Idle = new FrameAnimation(walkSheet, 1f, 1, 0);
        }
        else
        {
            Walk = new FrameAnimation("assets/player/walk", 11f);
            Idle = new FrameAnimation("assets/player/idle", 8f);
        }

        Sprint = File.Exists(sprintSheet)
            ? new FrameAnimation(sprintSheet, 14f, 13)
            : Walk;

        StopRunning = File.Exists(stopRunningSheet)
            ? new FrameAnimation(stopRunningSheet, 8f, 5)
            : Walk;

        Jumping = File.Exists(jumpingSheet)
            ? new FrameAnimation(jumpingSheet, 10f, 10)
            : Walk;

        DoubleJump = File.Exists(doubleJumpSheet)
            ? new FrameAnimation(doubleJumpSheet, 10f, 12)
            : Jumping;

        JumpWithStrike = File.Exists(jumpWithStrikeSheet)
            ? new FrameAnimation(jumpWithStrikeSheet, 8f, 3)
            : Jumping;

        string punch1Sheet = "assets/Animations/Animations 2/Punch_1.png";
        string punch2Sheet = "assets/Animations/Animations 2/Punch_2.png";

        Attack1 = File.Exists(punch1Sheet)
            ? new FrameAnimation(punch1Sheet, 12f, 5)
            : Walk;

        Attack2 = File.Exists(punch2Sheet)
            ? new FrameAnimation(punch2Sheet, 12f, 3)
            : Walk;

        string chargedAttackSheet = "assets/Animations/Animations 2/Power_Strike.png";

        ChargedAttack = File.Exists(chargedAttackSheet)
            ? new FrameAnimation(chargedAttackSheet, Program.CHARGED_ATTACK_FPS, Program.CHARGED_ATTACK_FRAME_COUNT)
            : Walk;

        string skillChargingSheet = "assets/Animations/Animations 2/Skill_Charging.png";

        SkillCharging = File.Exists(skillChargingSheet)
            ? new FrameAnimation(skillChargingSheet, 10f, 6)
            : Walk;

        PlatformJump = File.Exists(platformJumpSheet)
            ? new FrameAnimation(platformJumpSheet, 10f, 7)
            : Jumping;

        Landing = File.Exists(landingSheet)
            ? new FrameAnimation(landingSheet, 10f, 6)
            : Walk;

        LandingWithImpact = File.Exists(landingWithImpactSheet)
            ? new FrameAnimation(landingWithImpactSheet, 12f, 13)
            : Landing;

        Death = new FrameAnimation("assets/player/death", 9f);
    }

    public void Unload()
    {
        Idle.Unload();
        Walk.Unload();
        if (Sprint != Walk) Sprint.Unload();
        if (StopRunning != Walk) StopRunning.Unload();
        if (Jumping != Walk) Jumping.Unload();
        if (DoubleJump != Jumping && DoubleJump != Walk) DoubleJump.Unload();
        if (JumpWithStrike != Jumping && JumpWithStrike != Walk) JumpWithStrike.Unload();
        if (Attack1 != Walk) Attack1.Unload();
        if (Attack2 != Walk && Attack2 != Attack1) Attack2.Unload();
        if (ChargedAttack != Walk) ChargedAttack.Unload();
        if (SkillCharging != Walk) SkillCharging.Unload();
        if (PlatformJump != Jumping && PlatformJump != Walk) PlatformJump.Unload();
        if (Landing != Walk) Landing.Unload();
        if (LandingWithImpact != Landing && LandingWithImpact != Walk) LandingWithImpact.Unload();
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
    public bool Sprinting;
    public bool CanDoubleJump;
    public bool UsedDoubleJump;
    public bool IsAttacking;
    public int AttackIndex;
    public float AttackTimer;
    public float AttackHoldTimer;
    public bool IsAttackHolding;
    public float SkillEnergy;
    public float SkillChargingTimer;
    public bool IsChargingSkill;
    public float JumpStrikeAnimTimer;
    public float LastFallVelocity;
    public bool JumpStarted;
    public float DoubleJumpAnimTimer;
}

public static class Program
{
    const int SCREEN_WIDTH = 1280;
    const int SCREEN_HEIGHT = 720;

    const float GRAVITY = 1200f;
    const float MOVE_SPEED = 175f;
    const float SPRINT_SPEED = 280f;
    const float JUMP_SPEED = 455f;
    const float MAX_FALL_SPEED = 800f;
    const float PLAYER_DRAW_SCALE_X = 3.85f;
    const float PLAYER_DRAW_SCALE_Y = 3.15f;
    const float JUMPING_VISUAL_SCALE_BOOST = 1.14f;
    const float LANDING_IMPACT_THRESHOLD = 450f;
    const float PLATFORM_EDGE_DETECTION_RADIUS = 30f;
    const KeyboardKey ATTACK_KEY = KeyboardKey.E;
    const KeyboardKey SKILL_CHARGE_KEY = KeyboardKey.Q;
    const float SKILL_ENERGY_MAX = 100f;
    const float SKILL_CHARGE_RATE = 30f;
    const float ATTACK_CHARGE_THRESHOLD = 0.35f;
    public const float CHARGED_ATTACK_FPS = 8f;
    public const int CHARGED_ATTACK_FRAME_COUNT = 11;
    public const float CHARGED_ATTACK_DURATION = CHARGED_ATTACK_FRAME_COUNT / CHARGED_ATTACK_FPS;
    const float CHARGE_ANIMATION_SCALE = 1.0f;
    const float CHARGE_ANIMATION_ALPHA = 0.5f;

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
    static PlayerAnimState previousState = PlayerAnimState.Idle;
    static bool wasSprintingLastFrame = false;

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
        Level l = CreateRoomLevel(3000, 900, 760, 980, new Vector2(120, 620));

        AddP(l, 0, 760, 360, 140, PlatformType.Solid);
        AddP(l, 430, 720, 90, 16, PlatformType.Solid);
        AddP(l, 580, 680, 92, 16, PlatformType.Solid);
        AddP(l, 735, 645, 92, 16, PlatformType.Fake);
        AddP(l, 900, 610, 94, 16, PlatformType.Solid);
        AddP(l, 1065, 575, 92, 16, PlatformType.Moving, 1165, 575, 60f);
        AddP(l, 1240, 540, 92, 16, PlatformType.Solid);
        AddP(l, 1410, 505, 90, 16, PlatformType.Falling);
        AddP(l, 1580, 470, 94, 16, PlatformType.Solid);
        AddP(l, 1755, 435, 94, 16, PlatformType.Solid);
        AddP(l, 1935, 400, 96, 16, PlatformType.Moving, 1935, 340, 62f);
        AddP(l, 2120, 365, 94, 16, PlatformType.Solid);
        AddP(l, 2300, 330, 94, 16, PlatformType.Fake);
        AddP(l, 2480, 295, 96, 16, PlatformType.Solid);
        AddP(l, 2660, 260, 136, 16, PlatformType.Solid);

        AddSpike(l, 385, 742, 30, 18);
        AddPopup(l, 660, 760, 20, 0, 660, 706, 20, 54);
        AddMovingSpike(l, 1120, 742, 24, 18, 1120, 592, 74f);
        AddSaw(l, 1625, 486, 28, 28, 1805, 486, 72f);
        AddTeleportSpike(l, 2055, 742, 26, 18, 2055, 382, 1.2f);
        AddPopup(l, 2365, 330, 22, 0, 2365, 276, 22, 54);

        l.ExitDoor = new Door(2715, 226, 24, 34);
        return l;
    }

    static Level BuildLevel2()
    {
        Level l = CreateRoomLevel(3500, 950, 800, 920, new Vector2(110, 655));

        AddP(l, 0, 800, 330, 150, PlatformType.Solid);
        AddP(l, 405, 760, 96, 16, PlatformType.Moving, 545, 760, 70f);
        AddP(l, 600, 725, 92, 16, PlatformType.Solid);
        AddP(l, 765, 690, 90, 16, PlatformType.Falling);
        AddP(l, 935, 655, 94, 16, PlatformType.Solid);
        AddP(l, 1110, 620, 94, 16, PlatformType.Fake);
        AddP(l, 1285, 585, 96, 16, PlatformType.Solid);
        AddP(l, 1465, 550, 96, 16, PlatformType.Moving, 1465, 470, 74f);
        AddP(l, 1665, 515, 92, 16, PlatformType.Solid);
        AddP(l, 1845, 480, 92, 16, PlatformType.Falling);
        AddP(l, 2025, 445, 94, 16, PlatformType.Solid);
        AddP(l, 2210, 410, 96, 16, PlatformType.Solid);
        AddP(l, 2400, 375, 96, 16, PlatformType.Moving, 2525, 375, 72f);
        AddP(l, 2610, 340, 92, 16, PlatformType.Solid);
        AddP(l, 2790, 305, 92, 16, PlatformType.Fake);
        AddP(l, 2975, 270, 96, 16, PlatformType.Solid);
        AddP(l, 3170, 235, 140, 16, PlatformType.Solid);

        AddSpike(l, 350, 782, 32, 18);
        AddPopup(l, 705, 800, 22, 0, 705, 744, 22, 56);
        AddMovingSpike(l, 1245, 782, 26, 18, 1245, 565, 94f);
        AddSaw(l, 1745, 528, 30, 30, 1940, 528, 96f);
        AddTeleportSpike(l, 2335, 782, 30, 18, 2470, 356, 1.0f);
        AddPopup(l, 2865, 305, 22, 0, 2865, 249, 22, 56);
        AddSaw(l, 3045, 205, 30, 30, 3245, 205, 102f);

        l.ExitDoor = new Door(3245, 201, 24, 34);
        return l;
    }

    static Level BuildLevel3()
    {
        Level l = CreateRoomLevel(3900, 1000, 840, 960, new Vector2(120, 690));

        AddP(l, 0, 840, 340, 160, PlatformType.Solid);
        AddP(l, 430, 800, 92, 16, PlatformType.Fake);
        AddP(l, 610, 765, 94, 16, PlatformType.Solid);
        AddP(l, 790, 730, 94, 16, PlatformType.Falling);
        AddP(l, 970, 695, 96, 16, PlatformType.Solid);
        AddP(l, 1155, 660, 96, 16, PlatformType.Moving, 1285, 660, 74f);
        AddP(l, 1365, 625, 94, 16, PlatformType.Solid);
        AddP(l, 1545, 590, 94, 16, PlatformType.Fake);
        AddP(l, 1730, 555, 96, 16, PlatformType.Solid);
        AddP(l, 1915, 520, 96, 16, PlatformType.Moving, 1915, 445, 74f);
        AddP(l, 2120, 485, 94, 16, PlatformType.Solid);
        AddP(l, 2310, 450, 94, 16, PlatformType.Falling);
        AddP(l, 2495, 415, 96, 16, PlatformType.Solid);
        AddP(l, 2685, 380, 96, 16, PlatformType.Solid);
        AddP(l, 2875, 345, 96, 16, PlatformType.Moving, 3005, 345, 74f);
        AddP(l, 3090, 310, 92, 16, PlatformType.Solid);
        AddP(l, 3275, 275, 92, 16, PlatformType.Fake);
        AddP(l, 3465, 240, 92, 16, PlatformType.Solid);
        AddP(l, 3655, 205, 146, 16, PlatformType.Solid);

        AddSpike(l, 365, 822, 34, 18);
        AddPopup(l, 540, 840, 22, 0, 540, 784, 22, 56);
        AddMovingSpike(l, 1225, 822, 26, 18, 1225, 640, 100f);
        AddSaw(l, 1805, 536, 30, 30, 1995, 536, 98f);
        AddTeleportSpike(l, 2350, 822, 30, 18, 2515, 395, 1.0f);
        AddPopup(l, 3335, 275, 22, 0, 3335, 219, 22, 56);
        AddSaw(l, 3515, 175, 30, 30, 3705, 175, 106f);

        l.ExitDoor = new Door(3735, 171, 24, 34);
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
            State = PlayerAnimState.Idle,
            Sprinting = false,
            CanDoubleJump = false,
            UsedDoubleJump = false,
            IsAttacking = false,
            AttackIndex = 0,
            AttackTimer = 0f,
            AttackHoldTimer = 0f,
            IsAttackHolding = false,
            SkillEnergy = 0f,
            SkillChargingTimer = 0f,
            IsChargingSkill = false,
            JumpStrikeAnimTimer = 0f,
            LastFallVelocity = 0f,
            JumpStarted = false,
            DoubleJumpAnimTimer = 0f
        };

        previousState = PlayerAnimState.Idle;
        wasSprintingLastFrame = false;

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
        visuals.Jumping.Reset();
        visuals.DoubleJump.Reset();
        visuals.JumpWithStrike.Reset();
        visuals.Attack1.Reset();
        visuals.Attack2.Reset();
        visuals.ChargedAttack.Reset();
        visuals.SkillCharging.Reset();
        visuals.PlatformJump.Reset();
        visuals.Landing.Reset();
        visuals.LandingWithImpact.Reset();
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
        player.Sprinting = false;
        player.CanDoubleJump = false;
        player.UsedDoubleJump = false;
        player.IsAttacking = false;
        player.AttackIndex = 0;
        player.AttackTimer = 0f;
        player.AttackHoldTimer = 0f;
        player.IsAttackHolding = false;
        player.AttackHoldTimer = 0f;
        player.IsAttackHolding = false;
        player.SkillEnergy = 0f;
        player.SkillChargingTimer = 0f;
        player.IsChargingSkill = false;
        player.JumpStrikeAnimTimer = 0f;
        player.LastFallVelocity = 0f;
        player.JumpStarted = false;
        player.DoubleJumpAnimTimer = 0f;
        previousState = PlayerAnimState.Idle;
        wasSprintingLastFrame = false;

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
        visuals.Attack1.Reset();
        visuals.Attack2.Reset();
        visuals.SkillCharging.Reset();
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

        bool shiftPressed = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        player.Sprinting = shiftPressed && MathF.Abs(move) > 0.01f && player.OnGround;

        float currentSpeed = player.Sprinting ? SPRINT_SPEED : MOVE_SPEED;
        player.Vel.X = move * currentSpeed;

        // Handle attack input
        bool attackDown = Raylib.IsKeyDown(ATTACK_KEY);
        bool attackPressed = Raylib.IsKeyPressed(ATTACK_KEY);
        bool attackReleased = Raylib.IsKeyReleased(ATTACK_KEY);

        // Handle charge on A
        bool skillChargeHeld = Raylib.IsKeyDown(SKILL_CHARGE_KEY);
        if (skillChargeHeld && player.SkillEnergy < SKILL_ENERGY_MAX)
        {
            player.IsChargingSkill = true;
            player.SkillChargingTimer += dt;
            player.SkillEnergy += SKILL_CHARGE_RATE * dt;
            if (player.SkillEnergy > SKILL_ENERGY_MAX) player.SkillEnergy = SKILL_ENERGY_MAX;
        }
        else
        {
            player.IsChargingSkill = false;
            player.SkillChargingTimer = 0f;
        }

        // Handle E hold for charged attack
        if (attackPressed && player.AttackTimer <= 0f)
        {
            player.IsAttackHolding = true;
            player.AttackHoldTimer = 0f;
        }

        if (attackDown && player.IsAttackHolding)
        {
            player.AttackHoldTimer += dt;
        }

        if (attackReleased && player.IsAttackHolding)
        {
            player.IsAttackHolding = false;
            if (player.AttackHoldTimer >= ATTACK_CHARGE_THRESHOLD && player.OnGround && player.SkillEnergy >= 20f)
            {
                player.AttackIndex = 3;
                player.AttackTimer = CHARGED_ATTACK_DURATION;
                player.SkillEnergy -= 20f;
                player.IsAttacking = true;
                player.State = PlayerAnimState.ChargedAttack;
                visuals.ChargedAttack.Reset();
            }
            else if (player.OnGround && player.AttackTimer <= 0f)
            {
                player.AttackIndex = player.AttackIndex == 1 ? 2 : 1;
                player.AttackTimer = player.AttackIndex == 1 ? 0.42f : 0.28f;
                player.IsAttacking = true;
            }
            else if (!player.OnGround)
            {
                player.JumpStrikeAnimTimer = 0.20f;
            }
        }

        if (player.AttackTimer > 0f)
        {
            player.AttackTimer -= dt;
            if (player.AttackTimer <= 0f)
            {
                player.AttackTimer = 0f;
                player.IsAttacking = false;
            }
            else
            {
                player.IsAttacking = true;
            }
        }
        else
        {
            player.IsAttacking = false;
        }

        // Handle jump input
        bool jumpPressed = Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up);
        if (jumpPressed)
        {
            if (player.OnGround)
            {
                player.Vel.Y = -JUMP_SPEED;
                player.OnGround = false;
                player.CanDoubleJump = true;
                player.UsedDoubleJump = false;
                player.JumpStarted = true;
            }
            else if (player.CanDoubleJump)
            {
                // Double jump
                player.Vel.Y = -JUMP_SPEED;
                player.CanDoubleJump = false;
                player.UsedDoubleJump = true;
                player.DoubleJumpAnimTimer = 0.62f;
                player.JumpStarted = true;
            }
        }

        if (player.DoubleJumpAnimTimer > 0f)
        {
            player.DoubleJumpAnimTimer -= dt;
            if (player.DoubleJumpAnimTimer < 0f) player.DoubleJumpAnimTimer = 0f;
        }

        if (player.JumpStrikeAnimTimer > 0f)
        {
            player.JumpStrikeAnimTimer -= dt;
            if (player.JumpStrikeAnimTimer < 0f) player.JumpStrikeAnimTimer = 0f;
        }

        // Track fall velocity for landing impact detection
        if (!player.OnGround)
        {
            player.LastFallVelocity = player.Vel.Y;
        }

        player.Vel.Y += GRAVITY * dt;
        if (player.Vel.Y > MAX_FALL_SPEED) player.Vel.Y = MAX_FALL_SPEED;

        player.Rect.X += player.Vel.X * dt;
        ResolveHorizontal();

        player.Rect.Y += player.Vel.Y * dt;
        player.OnGround = false;
        ResolveVertical(dt);

        // Reset jump started flag when landing
        if (player.OnGround)
        {
            player.JumpStarted = false;
            player.CanDoubleJump = false;
            player.UsedDoubleJump = false;
            player.DoubleJumpAnimTimer = 0f;
            player.JumpStrikeAnimTimer = 0f;
        }

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

        // If we were sprinting and now we're not (and on ground), play stop_running first
        if (wasSprintingLastFrame && !player.Sprinting && player.OnGround && MathF.Abs(move) < 0.01f)
        {
            player.State = PlayerAnimState.StopRunning;
            return;
        }

        // Handle landing animations when transitioning from airborne to ground
        if (!previousState.ToString().Contains("Landing") && player.OnGround && 
            (previousState == PlayerAnimState.Jumping || previousState == PlayerAnimState.DoubleJump || 
             previousState == PlayerAnimState.JumpWithStrike || previousState == PlayerAnimState.PlatformJump))
        {
            // Determine which landing animation based on fall velocity
            if (player.LastFallVelocity > LANDING_IMPACT_THRESHOLD)
            {
                player.State = PlayerAnimState.LandingWithImpact;
            }
            else
            {
                player.State = PlayerAnimState.Landing;
            }
            return;
        }

        // If in air, handle jump animation selection
        if (!player.OnGround)
        {
            // Keep jump-strike animation stable for a short time after trigger.
            if (player.JumpStrikeAnimTimer > 0f)
            {
                player.State = PlayerAnimState.JumpWithStrike;
                return;
            }

            // Keep double-jump animation stable for a short time after trigger.
            if (player.DoubleJumpAnimTimer > 0f)
            {
                player.State = PlayerAnimState.DoubleJump;
                return;
            }

            // Priority 1: Jump + Attack = JumpWithStrike
            if (player.IsAttacking && player.JumpStarted)
            {
                player.State = PlayerAnimState.JumpWithStrike;
                return;
            }

            // Priority 2: Near platform edge while falling = PlatformJump
            if (player.Vel.Y > 30f && IsNearPlatformEdge())
            {
                player.State = PlayerAnimState.PlatformJump;
                return;
            }

            // Default: Jumping
            player.State = PlayerAnimState.Jumping;
            return;
        }

        if (player.OnGround && player.AttackTimer > 0f)
        {
            if (player.AttackIndex == 3)
            {
                player.State = PlayerAnimState.ChargedAttack;
            }
            else
            {
                player.State = player.AttackIndex == 2 ? PlayerAnimState.Attack2 : PlayerAnimState.Attack1;
            }
            return;
        }

        if (player.Sprinting)
        {
            player.State = PlayerAnimState.Sprint;
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

    static bool IsNearPlatformEdge()
    {
        // Check if player is near the horizontal edge of a platform while jumping
        Rectangle playerRect = player.Rect;
        float checkRadius = PLATFORM_EDGE_DETECTION_RADIUS;

        foreach (var p in CurrentLevel().Platforms)
        {
            if (!p.Active) continue;

            // Check if player is horizontally near platform edges (left or right)
            float playerCenterX = playerRect.X + playerRect.Width / 2f;
            float platformLeft = p.Rect.X;
            float platformRight = p.Rect.X + p.Rect.Width;

            bool nearLeftEdge = playerCenterX > platformLeft - checkRadius && playerCenterX < platformLeft + checkRadius;
            bool nearRightEdge = playerCenterX > platformRight - checkRadius && playerCenterX < platformRight + checkRadius;

            // Only trigger if player is above the platform
            if ((nearLeftEdge || nearRightEdge) && playerRect.Y + playerRect.Height <= p.Rect.Y + 10f)
            {
                return true;
            }
        }

        return false;
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
        if (player.State != previousState)
        {
            switch (player.State)
            {
                case PlayerAnimState.Idle:
                    visuals.Idle.Reset();
                    break;
                case PlayerAnimState.Walk:
                    visuals.Walk.Reset();
                    break;
                case PlayerAnimState.Sprint:
                    visuals.Sprint.Reset();
                    break;
                case PlayerAnimState.StopRunning:
                    visuals.StopRunning.Reset();
                    break;
                case PlayerAnimState.Attack1:
                    visuals.Attack1.Reset();
                    break;
                case PlayerAnimState.Attack2:
                    visuals.Attack2.Reset();
                    break;
                case PlayerAnimState.ChargedAttack:
                    visuals.ChargedAttack.Reset();
                    break;
                case PlayerAnimState.Jumping:
                    visuals.Jumping.Reset();
                    break;
                case PlayerAnimState.DoubleJump:
                    visuals.DoubleJump.Reset();
                    break;
                case PlayerAnimState.JumpWithStrike:
                    visuals.JumpWithStrike.Reset();
                    break;
                case PlayerAnimState.PlatformJump:
                    visuals.PlatformJump.Reset();
                    break;
                case PlayerAnimState.Landing:
                    visuals.Landing.Reset();
                    break;
                case PlayerAnimState.LandingWithImpact:
                    visuals.LandingWithImpact.Reset();
                    break;
                case PlayerAnimState.Death:
                    visuals.Death.Reset();
                    break;
            }
        }

        switch (player.State)
        {
            case PlayerAnimState.Idle:
                visuals.Idle.Update(dt);
                break;
            case PlayerAnimState.Walk:
                visuals.Walk.Update(dt);
                break;
            case PlayerAnimState.Sprint:
                visuals.Sprint.Update(dt);
                break;
            case PlayerAnimState.StopRunning:
                visuals.StopRunning.Update(dt);
                break;
            case PlayerAnimState.Attack1:
                visuals.Attack1.Update(dt, false);
                if (!visuals.Attack1.IsPlaying)
                {
                    player.AttackTimer = 0f;
                    player.IsAttacking = false;
                    player.State = PlayerAnimState.Idle;
                }
                break;
            case PlayerAnimState.Attack2:
                visuals.Attack2.Update(dt, false);
                if (!visuals.Attack2.IsPlaying)
                {
                    player.AttackTimer = 0f;
                    player.IsAttacking = false;
                    player.State = PlayerAnimState.Idle;
                }
                break;
            case PlayerAnimState.ChargedAttack:
                visuals.ChargedAttack.Update(dt, false);
                if (!visuals.ChargedAttack.IsPlaying)
                {
                    player.AttackTimer = 0f;
                    player.IsAttacking = false;
                    player.State = PlayerAnimState.Idle;
                }
                break;
            case PlayerAnimState.Jumping:
                visuals.Jumping.Update(dt, false);
                break;
            case PlayerAnimState.DoubleJump:
                visuals.DoubleJump.Update(dt, false);
                break;
            case PlayerAnimState.JumpWithStrike:
                visuals.JumpWithStrike.Update(dt);
                break;
            case PlayerAnimState.PlatformJump:
                visuals.PlatformJump.Update(dt);
                break;
            case PlayerAnimState.Landing:
                visuals.Landing.Update(dt, false);
                // Auto-transition to Idle when landing animation completes
                if (!visuals.Landing.IsPlaying)
                {
                    player.State = PlayerAnimState.Idle;
                }
                break;
            case PlayerAnimState.LandingWithImpact:
                visuals.LandingWithImpact.Update(dt, false);
                // Auto-transition to Idle when landing animation completes
                if (!visuals.LandingWithImpact.IsPlaying)
                {
                    player.State = PlayerAnimState.Idle;
                }
                break;
            case PlayerAnimState.Death:
                visuals.Death.Update(dt, false);
                break;
        }

        // Update skill charging animation
        if (player.IsChargingSkill)
        {
            visuals.SkillCharging.Update(dt);
        }

        previousState = player.State;
        wasSprintingLastFrame = player.Sprinting;
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

        DrawHUD();

        if (finishedGame)
        {
            Raylib.DrawRectangle(360, 250, 560, 150, Raylib.Fade(new Color(30, 20, 20, 255), 0.86f));
            Raylib.DrawRectangleLines(360, 250, 560, 150, new Color(230, 210, 170, 255));
            Raylib.DrawText("FIN DU PROTOTYPE", 447, 292, 34, new Color(240, 220, 170, 255));
        }

        Raylib.EndDrawing();
    }

    static void DrawHUD()
    {
        // Skill energy bar in top-left corner
        int barX = 20;
        int barY = 20;
        int barWidth = 200;
        int barHeight = 20;

        // Background
        Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(50, 50, 50, 200));
        Raylib.DrawRectangleLines(barX, barY, barWidth, barHeight, new Color(150, 150, 150, 255));

        // Energy fill
        float energyPercent = Math.Clamp(player.SkillEnergy / SKILL_ENERGY_MAX, 0f, 1f);
        int fillWidth = (int)(barWidth * energyPercent);
        
        Color energyColor = energyPercent < 0.3f ? new Color(255, 50, 50, 255) : 
                           energyPercent < 0.7f ? new Color(255, 200, 50, 255) : 
                           new Color(100, 200, 100, 255);
        
        if (fillWidth > 0)
            Raylib.DrawRectangle(barX, barY, fillWidth, barHeight, energyColor);

        // Display energy value
        Raylib.DrawText($"{player.SkillEnergy:F0}/100", barX + barWidth + 10, barY + 2, 16, new Color(220, 220, 220, 255));

        // Indicator when ready to use charged attack
        if (player.SkillEnergy >= SKILL_ENERGY_MAX)
        {
            Raylib.DrawText("CHARGE PRET", barX + 20, barY - 25, 14, new Color(100, 255, 100, 255));
        }

        // Help text for controls
        Raylib.DrawText("A = CHARGER / E = ATTAQUE", barX, barY + barHeight + 10, 14, new Color(220, 220, 220, 220));
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
            PlayerAnimState.Sprint => visuals.Sprint.Current(),
            PlayerAnimState.StopRunning => visuals.StopRunning.Current(),
            PlayerAnimState.Jumping => visuals.Jumping.Current(),
            PlayerAnimState.DoubleJump => visuals.DoubleJump.Current(),
            PlayerAnimState.JumpWithStrike => visuals.JumpWithStrike.Current(),
            PlayerAnimState.Attack1 => visuals.Attack1.Current(),
            PlayerAnimState.Attack2 => visuals.Attack2.Current(),
            PlayerAnimState.ChargedAttack => visuals.ChargedAttack.Current(),
            PlayerAnimState.PlatformJump => visuals.PlatformJump.Current(),
            PlayerAnimState.Landing => visuals.Landing.Current(),
            PlayerAnimState.LandingWithImpact => visuals.LandingWithImpact.Current(),
            PlayerAnimState.Death => visuals.Death.Current(),
            _ => null
        };

        if (tex.HasValue)
        {
            Texture2D t = tex.Value;
            Rectangle src = new Rectangle(0, 0, t.Width, t.Height);
            Rectangle drawRect = GetPlayerDrawRect();

            if (!player.FacingRight)
                src = new Rectangle(t.Width, 0, -t.Width, t.Height);

            Raylib.DrawTexturePro(t, src, drawRect, Vector2.Zero, 0f, Color.White);
        }
        else
        {
            Raylib.DrawRectangleRec(GetPlayerDrawRect(), Color.Black);
        }

        // Draw charging animation on top of player if charging
        if (player.IsChargingSkill)
        {
            Texture2D? chargeTex = visuals.SkillCharging.Current();
            if (chargeTex.HasValue)
            {
                Rectangle playerRect = GetPlayerDrawRect();
                float chargeWidth = playerRect.Width * CHARGE_ANIMATION_SCALE;
                float chargeHeight = playerRect.Height * CHARGE_ANIMATION_SCALE;
                float chargeX = playerRect.X + (playerRect.Width - chargeWidth) / 2f;
                float chargeY = playerRect.Y + (playerRect.Height - chargeHeight) / 2f;
                Rectangle chargeSrc = new Rectangle(0, 0, chargeTex.Value.Width, chargeTex.Value.Height);
                Rectangle chargeDst = new Rectangle(chargeX, chargeY, chargeWidth, chargeHeight);

                if (!player.FacingRight)
                    chargeSrc = new Rectangle(chargeTex.Value.Width, 0, -chargeTex.Value.Width, chargeTex.Value.Height);

                Raylib.DrawTexturePro(chargeTex.Value, chargeSrc, chargeDst, Vector2.Zero, 0f, Raylib.Fade(Color.White, CHARGE_ANIMATION_ALPHA));
            }
        }
    }

    static Rectangle GetPlayerDrawRect()
    {
        float stateScale = player.State == PlayerAnimState.Jumping ? JUMPING_VISUAL_SCALE_BOOST : 1f;
        float drawW = player.Rect.Width * PLAYER_DRAW_SCALE_X * stateScale;
        float drawH = player.Rect.Height * PLAYER_DRAW_SCALE_Y * stateScale;
        float drawX = player.Rect.X + (player.Rect.Width - drawW) / 2f;
        float drawY = player.Rect.Y + player.Rect.Height - drawH;
        return new Rectangle(drawX, drawY, drawW, drawH);
    }
}
