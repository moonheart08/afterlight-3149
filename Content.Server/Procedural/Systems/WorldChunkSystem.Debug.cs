using System.Collections.Generic;
using Content.Server.EUI;
using Content.Server.Procedural.Euis;
using Content.Shared.Procedural;
using Robust.Server.Player;
using Robust.Shared.IoC;
using Robust.Shared.Maths;

namespace Content.Server.Procedural.Systems;

public partial class WorldChunkSystem
{
    private readonly Dictionary<IPlayerSession, OverworldDebugEui> _openUis = new();

    public void OpenEui(IPlayerSession session)
    {
        if(_openUis.ContainsKey(session))
            CloseEui(session);

        var eui = _openUis[session] = new OverworldDebugEui();
        _euiManager.OpenEui(eui, session);
        eui.StateDirty();
    }

    public void CloseEui(IPlayerSession session)
    {
        if (!_openUis.ContainsKey(session)) return;

        _openUis.Remove(session, out var eui);

        eui?.Close();
    }

    public DebugChunkData[][] GetWorldDebugData(int width, int height, Vector2i topLeft)
    {
        var data = new DebugChunkData[height][];
        for (int y = 0; y < height; y++)
        {
            data[y] = new DebugChunkData[width];
            for (int x = 0; x < width; x++)
            {
                var chunk = topLeft + (x, y);
                data[y][x] = new DebugChunkData()
                {
                    Density = (int)(GetDensityValue(chunk) * 10),
                    Clipped = ShouldClipChunk(chunk),
                    Loaded = _currLoaded.Contains(chunk)
                };
            }
        }

        return data;
    }
}
