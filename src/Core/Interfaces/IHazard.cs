using OpenTK.Mathematics;

namespace MirrorWorld.Core;

public interface IHazard
{
    WorldId World { get; }
    bool Kills(Box2 characterBounds);
}
