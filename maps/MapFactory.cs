using Raylib_cs;
using System.Numerics;

namespace LowCortisolIO;

public static class MapFactory
{
    public static List<LevelData> CreateAll()
    {
        return new List<LevelData>
        {
            CreateLevel1(),
            CreateLevel2(),
            CreateLevel3()
        };
    }

    private static LevelData CreateBase(
        int width,
        int height,
        float floorY,
        Vector2 spawn,
        Color bg,
        Color room,
        Color border,
        Color voidColor,
        Color platform,
        Color platformEdge)
    {
        LevelData level = new(width, height, floorY, spawn, bg, room, border, voidColor, platform, platformEdge);

        level.Platforms.Add(new PlatformBlock(-60, 0, 60, height, PlatformType.Solid));
        level.Platforms.Add(new PlatformBlock(width, 0, 60, height, PlatformType.Solid));
        level.Platforms.Add(new PlatformBlock(0, -60, width, 60, PlatformType.Solid));
        level.Platforms.Add(new PlatformBlock(0, floorY, width, height - floorY + 260, PlatformType.Solid));

        return level;
    }

    private static void AddPlatform(LevelData level, float x, float y, float w, float h, PlatformType type)
    {
        level.Platforms.Add(new PlatformBlock(x, y, w, h, type));
    }

    private static void AddMovingPlatform(LevelData level, float x, float y, float w, float h, float endX, float endY, float speed)
    {
        PlatformBlock p = new(x, y, w, h, PlatformType.Moving)
        {
            EndRect = new Rectangle(endX, endY, w, h),
            Speed = speed
        };
        level.Platforms.Add(p);
    }

    private static void AddFallingPlatform(LevelData level, float x, float y, float w, float h)
    {
        level.Platforms.Add(new PlatformBlock(x, y, w, h, PlatformType.Falling));
    }

    private static void AddFakePlatform(LevelData level, float x, float y, float w, float h)
    {
        level.Platforms.Add(new PlatformBlock(x, y, w, h, PlatformType.Fake));
    }

    private static void AddSpike(LevelData level, float x, float y, float w, float h)
    {
        level.Traps.Add(new Trap(x, y, w, h, TrapType.Spike));
    }

    private static void AddMovingSpike(LevelData level, float x, float y, float w, float h, float endX, float endY, float speed)
    {
        Trap t = new(x, y, w, h, TrapType.MovingSpike)
        {
            EndRect = new Rectangle(endX, endY, w, h),
            Speed = speed
        };
        level.Traps.Add(t);
    }

    private static void AddPopup(LevelData level, float hiddenX, float hiddenY, float hiddenW, float hiddenH, float visibleX, float visibleY, float visibleW, float visibleH)
    {
        Trap t = new(hiddenX, hiddenY, hiddenW, hiddenH, TrapType.PopupSpike)
        {
            HiddenRect = new Rectangle(hiddenX, hiddenY, hiddenW, hiddenH),
            VisibleRect = new Rectangle(visibleX, visibleY, visibleW, visibleH),
            Visible = false
        };
        level.Traps.Add(t);
    }

    private static void AddSaw(LevelData level, float x, float y, float w, float h, float endX, float endY, float speed)
    {
        Trap t = new(x, y, w, h, TrapType.Saw)
        {
            EndRect = new Rectangle(endX, endY, w, h),
            Speed = speed
        };
        level.Traps.Add(t);
    }

    private static void AddTeleportSpike(LevelData level, float startX, float startY, float w, float h, float endX, float endY, float interval)
    {
        Trap t = new(startX, startY, w, h, TrapType.TeleportSpike)
        {
            StartRect = new Rectangle(startX, startY, w, h),
            EndRect = new Rectangle(endX, endY, w, h),
            Interval = interval
        };
        level.Traps.Add(t);
    }

    public static LevelData CreateLevel1()
    {
        LevelData level = CreateBase(
            3600, 960, 820,
            new Vector2(120, 750),
            new Color(120, 45, 22, 255),
            new Color(242, 145, 126, 255),
            new Color(215, 110, 82, 255),
            new Color(108, 28, 20, 255),
            new Color(196, 76, 41, 255),
            new Color(155, 52, 30, 255)
        );

        AddPlatform(level, 0, 820, 520, 140, PlatformType.Solid);
        AddPlatform(level, 110, 700, 760, 24, PlatformType.Solid);
        AddPlatform(level, 420, 590, 980, 24, PlatformType.Solid);
        AddPlatform(level, 180, 470, 1650, 24, PlatformType.Solid);
        AddPlatform(level, 300, 350, 1150, 24, PlatformType.Solid);

        AddPlatform(level, 2020, 760, 140, 16, PlatformType.Solid);
        AddMovingPlatform(level, 2240, 700, 90, 16, 2420, 700, 70f);
        AddFakePlatform(level, 2520, 650, 88, 16);
        AddFallingPlatform(level, 2680, 590, 88, 16);
        AddMovingPlatform(level, 2840, 525, 90, 16, 2980, 525, 60f);
        AddPlatform(level, 3070, 470, 100, 16, PlatformType.Solid);
        AddPlatform(level, 3250, 410, 120, 18, PlatformType.Solid);

        AddSpike(level, 1435, 802, 36, 18);
        AddPopup(level, 1890, 820, 20, 0, 1890, 768, 20, 52);
        AddMovingSpike(level, 2460, 802, 24, 18, 2460, 620, 85f);
        AddTeleportSpike(level, 2920, 802, 24, 18, 3005, 507, 1.0f);
        AddSaw(level, 3335, 382, 28, 28, 3480, 382, 80f);

        level.ExitDoor = new Door(3440, 372, 26, 38);
        return level;
    }

    public static LevelData CreateLevel2()
    {
        LevelData level = CreateBase(
            3000, 930, 790,
            new Vector2(120, 720),
            new Color(164, 86, 40, 255),
            new Color(255, 178, 145, 255),
            new Color(235, 148, 100, 255),
            new Color(114, 38, 24, 255),
            new Color(178, 88, 46, 255),
            new Color(145, 64, 30, 255)
        );

        AddPlatform(level, 0, 790, 500, 140, PlatformType.Solid);
        AddPlatform(level, 130, 655, 760, 22, PlatformType.Solid);
        AddPlatform(level, 480, 540, 760, 22, PlatformType.Solid);
        AddPlatform(level, 850, 425, 980, 22, PlatformType.Solid);
        AddPlatform(level, 600, 315, 940, 22, PlatformType.Solid);

        AddPlatform(level, 1900, 735, 120, 16, PlatformType.Solid);
        AddMovingPlatform(level, 2100, 660, 88, 16, 2230, 570, 68f);
        AddFakePlatform(level, 2300, 575, 88, 16);
        AddFallingPlatform(level, 2455, 510, 88, 16);
        AddPlatform(level, 2615, 445, 98, 16, PlatformType.Solid);

        AddSpike(level, 1260, 772, 34, 18);
        AddSaw(level, 1760, 397, 30, 30, 1930, 397, 84f);
        AddPopup(level, 2050, 790, 22, 0, 2050, 736, 22, 54);
        AddTeleportSpike(level, 2400, 772, 24, 18, 2485, 492, 0.95f);

        level.ExitDoor = new Door(2665, 407, 26, 38);
        return level;
    }

    public static LevelData CreateLevel3()
    {
        LevelData level = CreateBase(
            4300, 1000, 860,
            new Vector2(4020, 790),
            new Color(24, 22, 92, 255),
            new Color(221, 165, 67, 255),
            new Color(60, 80, 180, 255),
            new Color(48, 30, 110, 255),
            new Color(51, 81, 186, 255),
            new Color(32, 58, 138, 255)
        );

        AddPlatform(level, 0, 860, 4300, 140, PlatformType.Solid);

        AddPlatform(level, 3480, 770, 90, 16, PlatformType.Solid);
        AddPlatform(level, 3300, 730, 90, 16, PlatformType.Solid);
        AddPlatform(level, 3120, 690, 90, 16, PlatformType.Solid);
        AddPlatform(level, 2940, 650, 90, 16, PlatformType.Solid);
        AddPlatform(level, 2760, 610, 90, 16, PlatformType.Solid);
        AddPlatform(level, 2580, 570, 90, 16, PlatformType.Solid);
        AddPlatform(level, 2400, 530, 90, 16, PlatformType.Solid);
        AddPlatform(level, 2220, 490, 90, 16, PlatformType.Solid);
        AddPlatform(level, 2040, 450, 90, 16, PlatformType.Solid);
        AddPlatform(level, 1860, 410, 90, 16, PlatformType.Solid);
        AddPlatform(level, 1680, 370, 90, 16, PlatformType.Solid);
        AddPlatform(level, 1500, 330, 90, 16, PlatformType.Solid);
        AddPlatform(level, 1320, 290, 90, 16, PlatformType.Solid);
        AddPlatform(level, 1140, 250, 90, 16, PlatformType.Solid);
        AddPlatform(level, 960, 210, 110, 16, PlatformType.Solid);

        AddFakePlatform(level, 770, 170, 90, 16);
        AddMovingPlatform(level, 580, 130, 90, 16, 430, 130, 72f);
        AddPlatform(level, 250, 90, 120, 16, PlatformType.Solid);

        AddSpike(level, 3350, 712, 36, 18);
        AddSpike(level, 2990, 632, 36, 18);
        AddSpike(level, 2630, 552, 36, 18);
        AddSpike(level, 2270, 472, 36, 18);
        AddSpike(level, 1910, 392, 36, 18);
        AddSpike(level, 1550, 312, 36, 18);
        AddSaw(level, 860, 182, 28, 28, 1010, 182, 90f);
        AddPopup(level, 620, 860, 20, 0, 620, 806, 20, 54);
        AddTeleportSpike(level, 340, 842, 24, 18, 286, 74, 1.0f);

        level.ExitDoor = new Door(282, 52, 26, 38);
        return level;
    }
}
