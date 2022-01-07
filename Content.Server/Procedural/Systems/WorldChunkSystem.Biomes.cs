using System;
using Content.Server.Procedural.Tools;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Random;

namespace Content.Server.Procedural.Systems;

public partial class WorldChunkSystem
{
    // Density
    private const float DensityControllerCoordinateScale = 6f;
    private const float MinDensity = 90.0f;
    private const float MaxDensity = 40.0f;
    private const float DensityScale = MinDensity - MaxDensity;
    private const float DensityClipPointMin = 0.4f;
    private const float DensityClipPointMax = 0.5f;

    // Radiation
    private const float RadiationControllerCoordinateScale = 16f;
    private const float RadiationStormMinimum = 0.7f;
    private const float RadiationMinimum = 0.6f;

    private FastNoise _densityController = default!;
    private FastNoise _radiationController = default!;

    private void ResetNoise()
    {
        _densityController = new FastNoise();
        _densityController.SetSeed(_random.Next());
        _densityController.SetNoiseType(FastNoise.NoiseType.PerlinFractal);
        _densityController.SetFractalType(FastNoise.FractalType.FBM);
        _densityController.SetFractalLacunarity((float) (Math.PI * 2 / 3));
        _radiationController = new FastNoise();
        _radiationController.SetSeed(_random.Next());
        _radiationController.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        _radiationController.SetFractalType(FastNoise.FractalType.RigidMulti);
        _radiationController.SetFractalLacunarity((float) Math.PI * 1 / 5);
    }

    #region Density
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
        return density is > DensityClipPointMin and < DensityClipPointMax;
    }
    #endregion

    #region Radiation
    private float GetRadiationValue(Vector2i chunk)
    {
        var scaled = chunk * RadiationControllerCoordinateScale;
        return (_radiationController.GetPerlin(scaled.X, scaled.Y) + 1) / 2; // Scale it to be between 0 and 1.
    }

    private bool ShouldRadstorm(Vector2i chunk)
    {
        var rad = GetRadiationValue(chunk);
        return rad is > RadiationStormMinimum && !ShouldClipChunk(chunk);
    }

    private float GetRadiationClipped(Vector2i chunk)
    {
        var rad = GetRadiationValue(chunk);
        if (rad < RadiationMinimum || ShouldClipChunk(chunk))
            return 0.0f;
        return (rad - RadiationMinimum) / (1 - RadiationMinimum);
    }
    #endregion
}
