using MirrorWorld.Core;

namespace MirrorWorld;

public static class Program
{
    public static void Main(string[] args)
    {
        var context = new GameContext(new GameEventBus());
        using var window = new GameWindow(context);
        window.Run();
    }
}
