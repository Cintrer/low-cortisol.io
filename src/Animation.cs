using Raylib_cs;

namespace LevelDevilBase;

public class Animation
{
    private readonly List<Texture2D> _frames = new();
    private readonly float _fps;
    private float _timer;
    private int _currentFrame;

    public Animation(string folderPath, float fps)
    {
        _fps = fps;

        if (!Directory.Exists(folderPath))
            return;

        string[] files = Directory.GetFiles(folderPath)
            .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToArray();

        foreach (var file in files)
        {
            _frames.Add(Raylib.LoadTexture(file));
        }
    }

    public void Update(float dt, bool animate)
    {
        if (_frames.Count <= 1 || !animate)
            return;

        _timer += dt;
        float frameDuration = 1f / _fps;

        while (_timer >= frameDuration)
        {
            _timer -= frameDuration;
            _currentFrame = (_currentFrame + 1) % _frames.Count;
        }
    }

    public Texture2D? GetCurrentFrame()
    {
        if (_frames.Count == 0) return null;
        return _frames[_currentFrame];
    }

    public void Unload()
    {
        foreach (var frame in _frames)
        {
            Raylib.UnloadTexture(frame);
        }
    }
}
