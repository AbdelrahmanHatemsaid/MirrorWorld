using OpenTK.Mathematics;

namespace MirrorWorld.Core;

public interface IInteractable
{
    string Id { get; }
    WorldId World { get; }
    bool IsTriggeredBy(Box2 characterBounds);
    void OnTrigger(GameContext context, GameEventBus eventBus);
}
