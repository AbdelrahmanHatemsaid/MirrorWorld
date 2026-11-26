namespace MirrorWorld.Core;

public interface IGameScreen
{
    IGameScreen Update(float dt, GameContext c);
    void Render(GameContext c);
}
