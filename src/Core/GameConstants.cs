using OpenTK.Mathematics;

namespace MirrorWorld.Core;

public static class GameConstants
{
    public const int WindowWidth = 1280;
    public const int WindowHeight = 720;
    public const string WindowTitle = "Mirror World";

    public const int TileSize = 32;
    public const int LevelColumns = 24;
    public const int LevelRows = 9;
    public const float WorldWidth = TileSize * LevelColumns;
    public const float WorldHeight = TileSize * LevelRows;
    public const float MirrorLineThickness = 4f;

    public const float Gravity = 980f;
    public const float JumpSpeed = 420f;
    public const float MoveSpeed = 220f;
    public const float MaxFallSpeed = 650f;
    public const float MaxDeltaTime = 0.05f;

    public static readonly Color4 BackgroundClearColor = new(0.06f, 0.08f, 0.14f, 1.0f);
    public static readonly Color4 RealWorldClearColor = new(0.08f, 0.10f, 0.18f, 1.0f);
    public static readonly Color4 MirrorWorldClearColor = new(0.08f, 0.06f, 0.18f, 1.0f);
}
