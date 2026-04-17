using Raylib_cs;
using System.Numerics;

namespace LevelDevilBase;

public class Player
{
    private Vector2 _position;
    private Vector2 _velocity;
    private readonly int _width = 48;
    private readonly int _height = 64;

    private readonly float _walkSpeed = 240f;
    private readonly float _runSpeed = 430f;
    private readonly float _jumpForce = -560f;
    private readonly float _gravity = 1500f;

    private bool _isGrounded;
    private bool _facingRight = true;

    private readonly Animation _runAnimation;
    private readonly Animation _idleAnimation;
    private readonly Animation _jumpAnimation;

    public Player(Vector2 startPosition)
    {
        _position = startPosition;

        _idleAnimation = new Animation("assets/player/idle", 8f);
        _runAnimation = new Animation("assets/player/run", 14f);
        _jumpAnimation = new Animation("assets/player/jump", 8f);
    }

    public Rectangle Bounds => new(_position.X, _position.Y, _width, _height);

    public void Update(float dt, List<Rectangle> platforms)
    {
        float moveInput = 0f;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) moveInput -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) moveInput += 1f;

        bool wantsRun = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        float currentSpeed = wantsRun ? _runSpeed : _walkSpeed;

        _velocity.X = moveInput * currentSpeed;

        if (moveInput > 0) _facingRight = true;
        if (moveInput < 0) _facingRight = false;

        if (_isGrounded && (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Up)))
        {
            _velocity.Y = _jumpForce;
            _isGrounded = false;
        }

        _velocity.Y += _gravity * dt;

        // Horizontal move
        _position.X += _velocity.X * dt;

        // Vertical move
        _position.Y += _velocity.Y * dt;
        _isGrounded = false;

        Rectangle future = Bounds;

        foreach (var platform in platforms)
        {
            if (Raylib.CheckCollisionRecs(future, platform))
            {
                Rectangle previous = new(_position.X, _position.Y - _velocity.Y * dt, _width, _height);

                if (previous.Y + previous.Height <= platform.Y + 5)
                {
                    _position.Y = platform.Y - _height;
                    _velocity.Y = 0;
                    _isGrounded = true;
                    future = Bounds;
                }
            }
        }

        GetCurrentAnimation().Update(dt, MathF.Abs(_velocity.X) > 0.1f && _isGrounded);
    }

    public void Draw()
    {
        Texture2D? texture = GetCurrentAnimation().GetCurrentFrame();

        if (texture.HasValue)
        {
            Texture2D tex = texture.Value;

            Rectangle source = new(0, 0, tex.Width, tex.Height);
            if (!_facingRight)
            {
                source = new(tex.Width, 0, -tex.Width, tex.Height);
            }

            Rectangle dest = new(_position.X, _position.Y, _width, _height);
            Raylib.DrawTexturePro(tex, source, dest, Vector2.Zero, 0f, Color.White);
        }
        else
        {
            Raylib.DrawRectangleRec(Bounds, new Color(70, 160, 255, 255));
            Raylib.DrawRectangle((int)_position.X + 10, (int)_position.Y + 12, 8, 8, Color.White);
        }
    }

    public void Unload()
    {
        _runAnimation.Unload();
        _idleAnimation.Unload();
        _jumpAnimation.Unload();
    }

    private Animation GetCurrentAnimation()
    {
        if (!_isGrounded) return _jumpAnimation;
        if (MathF.Abs(_velocity.X) > 0.1f) return _runAnimation;
        return _idleAnimation;
    }
}
