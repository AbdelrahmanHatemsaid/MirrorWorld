namespace MirrorWorld.Core;

public readonly record struct InputCommand(float MoveX, bool Jump, bool Restart)
{
    public static InputCommand None => new(0f, false, false);
}
