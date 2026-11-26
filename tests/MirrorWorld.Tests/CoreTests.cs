using MirrorWorld.Core;
using OpenTK.Mathematics;

namespace MirrorWorld.Tests;

public class GameEventBusTests
{
    [Fact]
    public void Publish_DeliversEventToMatchingSubscribersOnly()
    {
        var bus = new GameEventBus();
        KeyCollectedEvent? receivedKey = null;
        LevelOutcomeEvent? receivedOutcome = null;

        bus.Subscribe<KeyCollectedEvent>(e => receivedKey = e);
        bus.Subscribe<LevelOutcomeEvent>(e => receivedOutcome = e);

        bus.Publish(new KeyCollectedEvent("key_3", WorldId.Mirror));

        Assert.NotNull(receivedKey);
        Assert.Equal("key_3", receivedKey.Value.KeyId);
        Assert.Equal(WorldId.Mirror, receivedKey.Value.World);
        Assert.Null(receivedOutcome);
    }

    [Fact]
    public void Unsubscribe_StopsFutureDeliveries()
    {
        var bus = new GameEventBus();
        int count = 0;
        void Handler(KeyCollectedEvent _) => count++;

        bus.Subscribe<KeyCollectedEvent>(Handler);
        bus.Publish(new KeyCollectedEvent("k1", WorldId.Real));
        bus.Unsubscribe<KeyCollectedEvent>(Handler);
        bus.Publish(new KeyCollectedEvent("k2", WorldId.Real));

        Assert.Equal(1, count);
    }

    [Fact]
    public void Publish_AllowsHandlerToUnsubscribeDuringDispatch()
    {
        var bus = new GameEventBus();
        int calls = 0;
        Action<LevelOutcomeEvent>? handler = null;
        handler = _ =>
        {
            calls++;
            bus.Unsubscribe(handler!);
        };

        bus.Subscribe(handler);
        bus.Publish(new LevelOutcomeEvent(true, "door"));
        bus.Publish(new LevelOutcomeEvent(true, "door"));

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Clear_RemovesAllSubscriptions()
    {
        var bus = new GameEventBus();
        bool called = false;
        bus.Subscribe<LevelOutcomeEvent>(_ => called = true);

        bus.Clear();
        bus.Publish(new LevelOutcomeEvent(false, "spikes"));

        Assert.False(called);
    }
}

public class GameContextTests
{
    [Fact]
    public void UpdateViewportSize_ClampsToMinimumOne()
    {
        var context = new GameContext();

        context.UpdateViewportSize(0, -5);

        Assert.Equal(1, context.ViewportWidth);
        Assert.Equal(1, context.ViewportHeight);
    }

    [Fact]
    public void DisposeResources_DisposesInReverseRegistrationOrder()
    {
        var context = new GameContext();
        var log = new List<string>();
        var first = new TrackingDisposable("first", log);
        var second = new TrackingDisposable("second", log);

        context.RegisterSystem(first);
        context.RegisterDisposable(second);
        context.RegisterDisposable(second); // duplicate registration ignored

        context.DisposeResources();

        Assert.Equal(new[] { "second", "first" }, log);
    }

    [Fact]
    public void ResetLevelFlags_ClearsOutcomeAndKeyState()
    {
        var context = new GameContext
        {
            KeyCollected = true,
            LevelFinished = true,
            LevelWon = true
        };

        context.ResetLevelFlags();

        Assert.False(context.KeyCollected);
        Assert.False(context.LevelFinished);
        Assert.False(context.LevelWon);
    }

    [Fact]
    public void GameConstants_WorldDimensionsMatchTileGrid()
    {
        Assert.Equal(GameConstants.TileSize * GameConstants.LevelColumns, GameConstants.WorldWidth);
        Assert.Equal(GameConstants.TileSize * GameConstants.LevelRows, GameConstants.WorldHeight);
        Assert.True(GameConstants.Gravity > 0f);
        Assert.True(GameConstants.JumpSpeed > 0f);
        Assert.Equal(InputCommand.None, new InputCommand(0f, false, false));
    }

    [Fact]
    public void CoreContracts_RunHeadlesslyWithoutOpenGlContext()
    {
        var context = new GameContext();
        KeyCollectedEvent? triggered = null;
        context.EventBus.Subscribe<KeyCollectedEvent>(e => triggered = e);

        IInteractable key = new StubKey("key_1", WorldId.Mirror, new Box2(10f, 10f, 42f, 42f));
        var playerBounds = new Box2(20f, 20f, 36f, 36f);

        Assert.True(key.IsTriggeredBy(playerBounds));
        key.OnTrigger(context, context.EventBus);

        Assert.NotNull(triggered);
        Assert.Equal("key_1", triggered.Value.KeyId);
        Assert.Equal(WorldId.Mirror, triggered.Value.World);
    }

    private sealed class TrackingDisposable : IGameSystem, IDisposable
    {
        private readonly string _name;
        private readonly List<string> _log;

        public TrackingDisposable(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public void Init(GameContext c) { }
        public void Update(float dt, GameContext c) { }
        public void Render(GameContext c) { }
        public void Dispose() => _log.Add(_name);
    }

    private sealed class StubKey : IInteractable
    {
        private readonly Box2 _bounds;

        public StubKey(string id, WorldId world, Box2 bounds)
        {
            Id = id;
            World = world;
            _bounds = bounds;
        }

        public string Id { get; }
        public WorldId World { get; }

        public bool IsTriggeredBy(Box2 characterBounds) =>
            _bounds.Min.X < characterBounds.Max.X &&
            _bounds.Max.X > characterBounds.Min.X &&
            _bounds.Min.Y < characterBounds.Max.Y &&
            _bounds.Max.Y > characterBounds.Min.Y;

        public void OnTrigger(GameContext context, GameEventBus eventBus) =>
            eventBus.Publish(new KeyCollectedEvent(Id, World));
    }
}
