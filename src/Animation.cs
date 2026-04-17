using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace AnimationGame
{
    /// <summary>
    /// Gère les animations basées sur des images PNG
    /// </summary>
    public class Animation
    {
        public List<Texture2D> Frames { get; private set; }
        public float FrameDuration { get; private set; }
        public bool IsLooping { get; private set; }
        
        private int _currentFrame = 0;
        private float _elapsedTime = 0;
        private bool _isPlaying = true;
        public bool HasFinished { get; private set; } = false;

        public Animation(List<Texture2D> frames, float frameDuration = 0.1f, bool isLooping = true)
        {
            Frames = frames ?? new List<Texture2D>();
            FrameDuration = frameDuration;
            IsLooping = isLooping;
        }

        public Texture2D GetCurrentFrame()
        {
            if (Frames.Count == 0) return null;
            return Frames[_currentFrame];
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying || Frames.Count == 0) return;

            _elapsedTime += deltaTime;

            while (_elapsedTime >= FrameDuration)
            {
                _elapsedTime -= FrameDuration;
                _currentFrame++;

                if (_currentFrame >= Frames.Count)
                {
                    if (IsLooping)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _currentFrame = Frames.Count - 1;
                        _isPlaying = false;
                        HasFinished = true;
                    }
                }
            }
        }

        public void Reset()
        {
            _currentFrame = 0;
            _elapsedTime = 0;
            _isPlaying = true;
            HasFinished = false;
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public void Play()
        {
            _isPlaying = true;
        }
    }
}
