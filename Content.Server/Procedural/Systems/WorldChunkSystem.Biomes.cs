using System;
using Content.Server.Procedural.Tools;
using Robust.Shared.Log;
using Robust.Shared.Maths;

namespace Content.Server.Procedural.Systems;

public partial class WorldChunkSystem
{

    private const float DensityControllerCoordinateScale = 10f;
    private const float MinDensity = 90.0f;
    private const float MaxDensity = 40.0f;
    private const float DensityScale = MinDensity - MaxDensity;
    private const float DensityClipPoint = 0.8f;

    private FastNoise _densityController = default!;

    private void ResetNoise()
    {
        _densityController = new FastNoise();
        _densityController.SetNoiseType(FastNoise.NoiseType.Perlin);
    }

    private float GetDensityValue(Vector2i chunk)
    {
        var scaled = chunk * DensityControllerCoordinateScale;
        return (_densityController.GetPerlin(scaled.X, scaled.Y) + 1) / 2; // Scale it to be between 0 and 1.
    }

    private float GetChunkDensity(Vector2i chunk)
    {
        return MaxDensity + (GetDensityValue(chunk) * DensityScale);
    }

    private bool ShouldClipChunk(Vector2i chunk)
    {
        var density = GetDensityValue(chunk);
        Logger.DebugS("worldgen", $"Checking clip with {density}");
        return density > DensityClipPoint;
    }
}
