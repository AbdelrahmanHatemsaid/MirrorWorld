namespace MirrorWorld.Core;

public sealed class GameEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    public void Subscribe<T>(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!_subscribers.TryGetValue(typeof(T), out var list))
        {
            list = new List<Delegate>();
            _subscribers[typeof(T)] = list;
        }
        list.Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (_subscribers.TryGetValue(typeof(T), out var list))
        {
            list.Remove(handler);
        }
    }

    public void Publish<T>(T evt)
    {
        if (!_subscribers.TryGetValue(typeof(T), out var list) || list.Count == 0)
        {
            return;
        }

        var snapshot = list.ToArray();
        foreach (var entry in snapshot)
        {
            ((Action<T>)entry)(evt);
        }
    }

    public void Clear() => _subscribers.Clear();
}
