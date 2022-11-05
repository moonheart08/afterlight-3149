using System.Diagnostics.Contracts;

namespace Content.Server._00OuterRim.Worldgen2;

public static class WorldGen
{
    public const int ChunkSize = 128;

    [Pure]
    public static Vector2i WorldToChunkCoords(Vector2i inp)
    {
        return inp / 128;
    }

    [Pure]
    public static Vector2i ChunkToWorldCoords(Vector2i inp)
    {
        return inp * 128;
    }
}
