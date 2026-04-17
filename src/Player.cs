using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnimationGame
{
    /// <summary>
    /// Classe du joueur avec animations et physique
    /// </summary>
    public class Player
    {
        // Positions et dimensions
        public Vector2 Position { get; set; }
        public float Width { get; set; } = 80f;
        public float Height { get; set; } = 100f;

        // Physique
        public Vector2 Velocity { get; set; }
        public float Gravity { get; set; } = 980f;
        public float MaxFallSpeed { get; set; } = 500f;
        public float MoveSpeed { get; set; } = 250f;
        public float JumpForce { get; set; } = -500f;

        // État
        public bool IsOnGround { get; set; } = true;
        public bool IsDead { get; set; } = false;
        public string CurrentState { get; private set; } = "idle";

        // Animations
        private Animation _idleAnim;
        private Animation _runAnim;
        private Animation _jumpAnim;
        private Animation _deathAnim;
        private Animation _currentAnim;

        private int _direction = 1; // 1 = droite, -1 = gauche
        private float _groundLevel = 600f;
        private Random _random = new Random();

        public Player(Vector2 startPosition)
        {
            Position = startPosition;
            Velocity = Vector2.Zero;
        }

        /// <summary>
        /// Charge les animations depuis des fichiers PNG
        /// </summary>
        public void LoadAnimations(GraphicsDevice graphicsDevice)
        {
            try
            {
                _idleAnim = LoadAnimationFromFolder(graphicsDevice, "Content/Sprites/Idle", 0.08f, true);
                _runAnim = LoadAnimationFromFolder(graphicsDevice, "Content/Sprites/Run", 0.06f, true);
                _jumpAnim = LoadAnimationFromFolder(graphicsDevice, "Content/Sprites/Jump", 0.1f, false);
                _deathAnim = LoadAnimationFromFolder(graphicsDevice, "Content/Sprites/Death", 0.08f, false);

                _currentAnim = _idleAnim ?? new Animation(new List<Texture2D>());

                Console.WriteLine("✅ Animations chargées avec succès!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Charge une animation depuis un dossier
        /// </summary>
        private Animation LoadAnimationFromFolder(GraphicsDevice gd, string folderPath, float frameDuration, bool isLooping)
        {
            var frames = new List<Texture2D>();

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"⚠️ Dossier manquant: {folderPath}");
                return null;
            }

            var files = Directory.GetFiles(folderPath, "*.png");
            Array.Sort(files);

            foreach (var file in files)
            {
                try
                {
                    using (var stream = File.OpenRead(file))
                    {
                        frames.Add(Texture2D.FromStream(gd, stream));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur fichier {file}: {ex.Message}");
                }
            }

            if (frames.Count > 0)
            {
                Console.WriteLine($"✅ {folderPath}: {frames.Count} frames");
                return new Animation(frames, frameDuration, isLooping);
            }

            Console.WriteLine($"❌ Aucune image PNG dans {folderPath}");
            return null;
        }

        /// <summary>
        /// Met à jour le joueur
        /// </summary>
        public void Update(GameTime gameTime, Rectangle worldBounds)
        {
            if (IsDead)
            {
                if (_currentAnim?.HasFinished == true)
                {
                    // L'animation de mort est terminée
                }
                return;
            }

            HandleInput();
            UpdatePhysics(gameTime, worldBounds);
            UpdateAnimationState();

            if (_currentAnim != null)
                _currentAnim.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        /// <summary>
        /// Gère l'entrée clavier
        /// </summary>
        private void HandleInput()
        {
            var keys = Keyboard.GetState();
            Velocity.X = 0;

            // Mouvement gauche
            if (keys.IsKeyDown(Keys.Left) || keys.IsKeyDown(Keys.Q))
            {
                Velocity.X = -MoveSpeed;
                _direction = -1;
            }
            // Mouvement droite
            else if (keys.IsKeyDown(Keys.Right) || keys.IsKeyDown(Keys.D))
            {
                Velocity.X = MoveSpeed;
                _direction = 1;
            }

            // Saut
            if ((keys.IsKeyDown(Keys.Space) || keys.IsKeyDown(Keys.Up) || keys.IsKeyDown(Keys.Z)) && IsOnGround)
            {
                Velocity.Y = JumpForce;
                IsOnGround = false;
            }

            // Touches spéciales
            if (keys.IsKeyDown(Keys.M))
                Die();

            if (keys.IsKeyDown(Keys.R))
                Reset(new Vector2(640, 500));
        }

        /// <summary>
        /// Met à jour la physique
        /// </summary>
        private void UpdatePhysics(GameTime gameTime, Rectangle worldBounds)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Applique la gravité
            if (!IsOnGround)
            {
                Velocity.Y += Gravity * dt;
                if (Velocity.Y > MaxFallSpeed)
                    Velocity.Y = MaxFallSpeed;
            }

            // Mise à jour position
            Position += Velocity * dt;

            // Collision avec le sol
            IsOnGround = false;
            if (Position.Y + Height >= _groundLevel)
            {
                Position.Y = _groundLevel - Height;
                Velocity.Y = 0;
                IsOnGround = true;
            }

            // Limites gauche/droite
            if (Position.X < 0)
                Position.X = 0;
            if (Position.X + Width > worldBounds.Width)
                Position.X = worldBounds.Width - Width;

            // Mort si trop bas
            if (Position.Y > worldBounds.Height + 100)
                Die();
        }

        /// <summary>
        /// Met à jour l'état d'animation
        /// </summary>
        private void UpdateAnimationState()
        {
            string newState = "idle";

            if (IsDead)
            {
                newState = "death";
            }
            else if (!IsOnGround)
            {
                newState = "jump";
            }
            else if (Velocity.X != 0)
            {
                newState = "run";
            }

            if (newState != CurrentState)
            {
                CurrentState = newState;
                SwitchAnimation(newState);
            }
        }

        /// <summary>
        /// Change l'animation
        /// </summary>
        private void SwitchAnimation(string state)
        {
            var newAnim = state switch
            {
                "idle" => _idleAnim,
                "run" => _runAnim,
                "jump" => _jumpAnim,
                "death" => _deathAnim,
                _ => _idleAnim
            };

            if (newAnim != null && newAnim != _currentAnim)
            {
                newAnim.Reset();
                _currentAnim = newAnim;
            }
        }

        /// <summary>
        /// Tue le joueur
        /// </summary>
        public void Die()
        {
            if (IsDead) return;
            IsDead = true;
            CurrentState = "death";
            SwitchAnimation("death");
            Console.WriteLine("💀 Mort!");
        }

        /// <summary>
        /// Réinitialise le joueur
        /// </summary>
        public void Reset(Vector2 startPos)
        {
            Position = startPos;
            Velocity = Vector2.Zero;
            IsDead = false;
            IsOnGround = true;
            CurrentState = "idle";
            SwitchAnimation("idle");
            Console.WriteLine("🔄 Réinitialisation");
        }

        /// <summary>
        /// Dessine le joueur
        /// </summary>
        public void Draw(SpriteBatch sb)
        {
            if (_currentAnim == null) return;
            var texture = _currentAnim.GetCurrentFrame();
            if (texture == null) return;

            var destRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
            var effect = _direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            sb.Draw(texture, destRect, null, Color.White, 0f, Vector2.Zero, effect, 0f);
        }
    }
}
