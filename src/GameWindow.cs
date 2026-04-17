using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnimationGame
{
    /// <summary>
    /// Classe principale du jeu
    /// </summary>
    public class GameWindow : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _colorTexture;

        private Player _player;
        private Rectangle _worldBounds;
        private bool _showDebug = false;
        private KeyboardState _prevKeyState;

        public GameWindow()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Window.Title = "🎮 Animation Personnage - C#";

            // Résolution
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _worldBounds = new Rectangle(0, 0, 1280, 720);
            _player = new Player(new Vector2(640, 500));
            _player.LoadAnimations(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Crée une texture de couleur
            _colorTexture = new Texture2D(GraphicsDevice, 1, 1);
            _colorTexture.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();

            // Quitter
            if (kb.IsKeyDown(Keys.Escape))
                Exit();

            // Toggle debug
            if (kb.IsKeyDown(Keys.Tab) && !_prevKeyState.IsKeyDown(Keys.Tab))
                _showDebug = !_showDebug;

            _player.Update(gameTime, _worldBounds);
            _prevKeyState = kb;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.53f, 0.81f, 1f)); // Ciel bleu

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Sol
            DrawFilledRectangle(0, 600, 1280, 120, Color.ForestGreen);

            // Joueur
            _player.Draw(_spriteBatch);

            // HUD
            DrawHUD(gameTime);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Dessine l'HUD
        /// </summary>
        private void DrawHUD(GameTime gameTime)
        {
            string info;

            if (_player.IsDead)
            {
                info = "💀 MORT! [R] Recommencer";
            }
            else
            {
                int fps = (int)(1f / gameTime.ElapsedGameTime.TotalSeconds);
                info = $"🎮 État: {_player.CurrentState} | FPS: {Math.Min(fps, 999)} | Au sol: {_player.IsOnGround}\n" +
                       $"[Flèches/ZQSD] Mouvement | [Espace] Saut | [M] Mourir | [Tab] Debug | [Échap] Quitter";
            }

            DrawString(info, 20, 20, Color.Yellow);

            if (_showDebug)
            {
                string debug = $"Pos: ({_player.Position.X:F0}, {_player.Position.Y:F0})\n" +
                              $"Vel: ({_player.Velocity.X:F1}, {_player.Velocity.Y:F1})\n" +
                              $"Gravité: {_player.Gravity}";
                DrawString(debug, 20, 60, Color.Lime);
            }
        }

        /// <summary>
        /// Dessine du texte simple
        /// </summary>
        private void DrawString(string text, int x, int y, Color color)
        {
            // Affiche aussi en console pour les tests
            foreach (var line in text.Split('\n'))
            {
                Console.WriteLine(line);
            }
        }

        /// <summary>
        /// Dessine un rectangle avec couleur
        /// </summary>
        private void DrawFilledRectangle(int x, int y, int w, int h, Color color)
        {
            _spriteBatch.Draw(_colorTexture, new Rectangle(x, y, w, h), color);
        }
    }
}
