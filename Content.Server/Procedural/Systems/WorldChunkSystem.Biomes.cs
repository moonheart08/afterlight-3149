using System;
using Content.Server.Procedural.Tools;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Random;

namespace Content.Server.Procedural.Systems;

public partial class WorldChunkSystem
{
    private const float DensityControllerCoordinateScale = 6f;
    private const float MinDensity = 90.0f;
    private const float MaxDensity = 40.0f;
    private const float DensityScale = MinDensity - MaxDensity;
    private const float DensityClipPointMin = 0.4f;
    private const float DensityClipPointMax = 0.5f;

    private FastNoise _densityController = default!;

    private void ResetNoise()
    {
        _densityController = new FastNoise();
        _densityController.SetSeed(_random.Next());
        _densityController.SetNoiseType(FastNoise.NoiseType.PerlinFractal);
        _densityController.SetFractalType(FastNoise.FractalType.FBM);
        _densityController.SetFractalLacunarity((float) (Math.PI * 2 / 3));
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
        return density is > DensityClipPointMin and < DensityClipPointMax;
    }
}
