using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Map3D;

public sealed record Map3DContext(GameFrame Frame, Camera3D Camera, Scene3D Scene, SettingsScope Settings, Selection Selection);

public interface IMapLayer3D
{
    void Build3D(Map3DContext context);
}
