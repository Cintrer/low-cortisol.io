using Raylib_cs;
using System.Numerics;

namespace LowCortisolIO;

public sealed class Game
{
    private readonly List<LevelData> _levels;
    private int _levelIndex;
    private LevelData _level;
    private Player _player;
    private readonly Camera2D _camera;

    public Game()
    {
        _levels = MapFactory.CreateAll();
        _levelIndex = 0;
        _level = _levels[_levelIndex];
        _player = new Player(_level.Spawn);

        _camera = new Camera2D
        {
            Offset = new Vector2(GameConstants.ScreenWidth / 2f, GameConstants.ScreenHeight / 2f),
            Target = Vector2.Zero,
            Rotation = 0f,
            Zoom = 1f
        };

        ResetCurrentLevel();
    }

    public void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            if (Raylib.IsKeyPressed(KeyboardKey.R))
                ResetCurrentLevel();

            Update(dt);
            GameRenderer.DrawLevel(_level, _player, _levelIndex, _camera);
        }
    }

    private void ResetCurrentLevel()
    {
        _level = _levels[_levelIndex];
        _player = new Player(_level.Spawn);

        foreach (var platform in _level.Platforms)
        {
            platform.Rect = platform.StartRect;
            platform.Active = true;
            platform.Triggered = false;
            platform.Forward = true;
            platform.FallSpeed = 0f;
            platform.ResetTimer = 0f;
        }

        foreach (var trap in _level.Traps)
            trap.Reset();
    }

    private void Update(float dt)
    {
        UpdatePlatforms(dt);
        UpdateTraps(dt);

        if (_player.Dead)
        {
            _player.RespawnTimer -= dt;
            if (_player.RespawnTimer <= 0f)
                _player.ResetToSpawn();

            UpdateCamera();
            return;
        }

        UpdatePlayer(dt);
        CheckDoor();
        UpdateCamera();
    }

    private void UpdatePlatforms(float dt)
    {
        foreach (var platform in _level.Platforms)
        {
            if (platform.Type == PlatformType.Moving && platform.Active)
            {
                Vector2 target = platform.Forward
                    ? new Vector2(platform.EndRect.X, platform.EndRect.Y)
                    : new Vector2(platform.StartRect.X, platform.StartRect.Y);

                Vector2 current = new(platform.Rect.X, platform.Rect.Y);
                Vector2 dir = target - current;

                if (dir.Length() < 2f)
                {
                    platform.Forward = !platform.Forward;
                    target = platform.Forward
                        ? new Vector2(platform.EndRect.X, platform.EndRect.Y)
                        : new Vector2(platform.StartRect.X, platform.StartRect.Y);
                    dir = target - current;
                }

                if (dir != Vector2.Zero)
                    dir = Vector2.Normalize(dir);

                platform.Rect.X += dir.X * platform.Speed * dt;
                platform.Rect.Y += dir.Y * platform.Speed * dt;
            }

            if (platform.Type == PlatformType.Falling)
            {
                if (!platform.Active)
                {
                    platform.ResetTimer += dt;
                    if (platform.ResetTimer >= 2f)
                    {
                        platform.Active = true;
                        platform.Triggered = false;
                        platform.FallSpeed = 0f;
                        platform.Rect = platform.StartRect;
                        platform.ResetTimer = 0f;
                    }
                }
                else if (platform.Triggered)
                {
                    platform.FallSpeed += GameConstants.Gravity * 0.65f * dt;
                    platform.Rect.Y += platform.FallSpeed * dt;

                    if (platform.Rect.Y > _level.Height + 150)
                    {
                        platform.Active = false;
                        platform.ResetTimer = 0f;
                    }
                }
            }

            if (platform.Type == PlatformType.Fake && platform.Triggered)
            {
                platform.Active = false;
                platform.ResetTimer += dt;
                if (platform.ResetTimer >= 1.8f)
                {
                    platform.Active = true;
                    platform.Triggered = false;
                    platform.ResetTimer = 0f;
                }
            }
        }
    }

    private void UpdateTraps(float dt)
    {
        foreach (var trap in _level.Traps)
        {
            if (trap.Type == TrapType.Saw || trap.Type == TrapType.MovingSpike)
            {
                Vector2 target = trap.Forward
                    ? new Vector2(trap.EndRect.X, trap.EndRect.Y)
                    : new Vector2(trap.StartRect.X, trap.StartRect.Y);

                Vector2 current = new(trap.Rect.X, trap.Rect.Y);
                Vector2 dir = target - current;

                if (dir.Length() < 2f)
                {
                    trap.Forward = !trap.Forward;
                    target = trap.Forward
                        ? new Vector2(trap.EndRect.X, trap.EndRect.Y)
                        : new Vector2(trap.StartRect.X, trap.StartRect.Y);
                    dir = target - current;
                }

                if (dir != Vector2.Zero)
                    dir = Vector2.Normalize(dir);

                trap.Rect.X += dir.X * trap.Speed * dt;
                trap.Rect.Y += dir.Y * trap.Speed * dt;
            }

            if (trap.Type == TrapType.TeleportSpike)
            {
                trap.Timer += dt;
                if (trap.Timer >= trap.Interval)
                {
                    trap.Timer = 0f;
                    bool atStart = MathF.Abs(trap.Rect.X - trap.StartRect.X) < 1f && MathF.Abs(trap.Rect.Y - trap.StartRect.Y) < 1f;
                    trap.Rect = atStart ? trap.EndRect : trap.StartRect;
                }
            }

            if (trap.Type == TrapType.PopupSpike)
            {
                if (!trap.Visible && _player.Rect.X + _player.Rect.Width > trap.VisibleRect.X - 28f)
                {
                    trap.Visible = true;
                    trap.Rect = trap.VisibleRect;
                }
            }
        }
    }

    private void UpdatePlayer(float dt)
    {
        float move = 0f;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) move -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move += 1f;

        if (move > 0) _player.FacingRight = true;
        if (move < 0) _player.FacingRight = false;

        _player.Velocity.X = move * GameConstants.MoveSpeed;

        if ((Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up)) && _player.OnGround)
        {
            _player.Velocity.Y = -GameConstants.JumpSpeed;
            _player.OnGround = false;
        }

        _player.Velocity.Y += GameConstants.Gravity * dt;
        if (_player.Velocity.Y > GameConstants.MaxFallSpeed)
            _player.Velocity.Y = GameConstants.MaxFallSpeed;

        _player.Rect.X += _player.Velocity.X * dt;
        ResolveHorizontal();

        _player.Rect.Y += _player.Velocity.Y * dt;
        _player.OnGround = false;
        ResolveVertical(dt);

        if (HitTrap(_player.Rect))
            _player.Kill();
    }

    private void ResolveHorizontal()
    {
        foreach (var platform in _level.Platforms)
        {
            if (!platform.Active) continue;

            if (Raylib.CheckCollisionRecs(_player.Rect, platform.Rect))
            {
                if (_player.Velocity.X > 0)
                    _player.Rect.X = platform.Rect.X - _player.Rect.Width;
                else if (_player.Velocity.X < 0)
                    _player.Rect.X = platform.Rect.X + platform.Rect.Width;
            }
        }
    }

    private void ResolveVertical(float dt)
    {
        foreach (var platform in _level.Platforms)
        {
            if (!platform.Active) continue;

            if (Raylib.CheckCollisionRecs(_player.Rect, platform.Rect))
            {
                if (_player.Velocity.Y > 0)
                {
                    _player.Rect.Y = platform.Rect.Y - _player.Rect.Height;
                    _player.Velocity.Y = 0;
                    _player.OnGround = true;

                    if (platform.Type == PlatformType.Falling && !platform.Triggered)
                    {
                        platform.Triggered = true;
                        platform.FallSpeed = 0f;
                    }

                    if (platform.Type == PlatformType.Fake && !platform.Triggered)
                    {
                        platform.Triggered = true;
                        platform.ResetTimer = 0f;
                    }

                    if (platform.Type == PlatformType.Moving)
                    {
                        Vector2 delta = GetMovingDelta(platform, dt);
                        _player.Rect.X += delta.X;
                        _player.Rect.Y += delta.Y;
                    }
                }
                else if (_player.Velocity.Y < 0)
                {
                    _player.Rect.Y = platform.Rect.Y + platform.Rect.Height;
                    _player.Velocity.Y = 0;
                }
            }
        }
    }

    private static Vector2 GetMovingDelta(PlatformBlock platform, float dt)
    {
        Vector2 target = platform.Forward
            ? new Vector2(platform.EndRect.X, platform.EndRect.Y)
            : new Vector2(platform.StartRect.X, platform.StartRect.Y);

        Vector2 current = new(platform.Rect.X, platform.Rect.Y);
        Vector2 dir = target - current;

        if (dir != Vector2.Zero)
            dir = Vector2.Normalize(dir);

        return dir * platform.Speed * dt;
    }

    private bool HitTrap(Rectangle rect)
    {
        foreach (var trap in _level.Traps)
        {
            if (!trap.Active) continue;
            if (trap.Type == TrapType.PopupSpike && !trap.Visible) continue;

            if (Raylib.CheckCollisionRecs(rect, trap.Rect))
                return true;
        }

        return false;
    }

    private void CheckDoor()
    {
        if (Raylib.CheckCollisionRecs(_player.Rect, _level.ExitDoor.Rect))
        {
            if (_levelIndex < _levels.Count - 1)
            {
                _levelIndex++;
                ResetCurrentLevel();
            }
        }
    }

    private void UpdateCamera()
    {
        float halfW = GameConstants.ScreenWidth / 2f;
        float halfH = GameConstants.ScreenHeight / 2f;

        float tx = Math.Clamp(_player.Rect.X + _player.Rect.Width / 2f, halfW, _level.Width - halfW);
        float ty = Math.Clamp(_player.Rect.Y + _player.Rect.Height / 2f, halfH, _level.Height - halfH);

        _camera.Target = new Vector2(tx, ty);
    }
}
