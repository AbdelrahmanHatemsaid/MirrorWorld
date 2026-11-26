namespace MirrorWorld.Core;

public sealed class GameContext
{
    private readonly List<IGameSystem> _systems = new();
    private readonly List<IDisposable> _disposables = new();

    public GameContext(GameEventBus? eventBus = null)
    {
        EventBus = eventBus ?? new GameEventBus();
        ViewportWidth = GameConstants.WindowWidth;
        ViewportHeight = GameConstants.WindowHeight;
    }

    public GameEventBus EventBus { get; }
    public int ViewportWidth { get; private set; }
    public int ViewportHeight { get; private set; }
    public int CurrentLevelIndex { get; set; } = 1;
    public bool KeyCollected { get; set; }
    public bool LevelFinished { get; set; }
    public bool LevelWon { get; set; }
    public float ElapsedTime { get; set; }
    public IReadOnlyList<IGameSystem> Systems => _systems;

    public void UpdateViewportSize(int width, int height)
    {
        ViewportWidth = Math.Max(1, width);
        ViewportHeight = Math.Max(1, height);
    }

    public void RegisterSystem(IGameSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        _systems.Add(system);
        if (system is IDisposable disposable)
        {
            RegisterDisposable(disposable);
        }
    }

    public void RegisterDisposable(IDisposable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!_disposables.Contains(resource))
        {
            _disposables.Add(resource);
        }
    }

    public void ResetLevelFlags()
    {
        KeyCollected = false;
        LevelFinished = false;
        LevelWon = false;
    }

    public void DisposeResources()
    {
        for (int i = _disposables.Count - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }
        _disposables.Clear();
    }
}
