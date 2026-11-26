namespace MirrorWorld.Core;

public interface IGameSystem
{
    void Init(GameContext c);
    void Update(float dt, GameContext c);
    void Render(GameContext c);
}
