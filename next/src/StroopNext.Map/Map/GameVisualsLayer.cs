using System.Numerics;
using StroopNext.Graphics;
using StroopNext.Map3D;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Map;

public sealed class GameVisualsLayer : MapLayer, IMapLayer3D
{
    private readonly GameSceneBuilder _builder = new();

    public override string Id => "game";
    public override string Name => "Game visuals";

    public readonly Setting<bool> ShowLevel;
    public readonly Setting<bool> ShowObjects;
    public readonly Setting<bool> ShowSky;
    public readonly Setting<bool> ShowFog;
    public readonly Setting<bool> ShowShadows;
    public readonly Setting<bool> CullBackFaces;
    public readonly Setting<bool> HideAboveMario;
    public readonly Setting<int> HideAboveOffset;
    public readonly Setting<bool> KeepMarioVisible;

    public GameVisualsLayer()
    {
        ShowLevel = Declare(new Setting<bool>("level", "Textured level", true));
        ShowObjects = Declare(new Setting<bool>("objects", "Objects and Mario models", true));
        ShowSky = Declare(new Setting<bool>("sky", "Sky", true));
        ShowFog = Declare(new Setting<bool>("fog", "Game fog", true));
        ShowShadows = Declare(new Setting<bool>("shadows", "Shadows", true));
        CullBackFaces = Declare(new Setting<bool>("cull", "Cull back faces (as the game)", false));
        HideAboveMario = Declare(new Setting<bool>("hideAbove", "Hide geometry above Mario", false, "Cuts away ceilings and upper floors to see Mario from above."));
        HideAboveOffset = Declare(new Setting<int>("hideAboveOffset", "Hide above: height over Mario", 200) { Range = (0, 10000, 50) });
        KeepMarioVisible = Declare(new Setting<bool>("keepMario", "Keep Mario visible from afar", true, "Scales Mario's model up so he never gets smaller than ~40 pixels."));
    }

    public GameSceneBuilder Builder => _builder;

    public override void Build(MapContext c)
    {
        if (!_builder.Update(c.Frame))
            return;
        var options = Options(c.Frame, c.Settings);
        c.Scene.GameVisuals = _builder.Scene;
        c.Scene.GameVisualsOptions = options;
        c.Scene.GameShadows = c.Settings.Get(ShowShadows);
        c.Scene.KeyBackground(("game", _builder.Scene.LevelVersion, options.Level, options.MaxY));
    }

    private GameVisualsOptions Options(GameFrame frame, SettingsScope settings)
    {
        float maxY = settings.Get(HideAboveMario) && frame.Mario is { } m ? m.Position.Y + settings.Get(HideAboveOffset) : float.PositiveInfinity;
        return new GameVisualsOptions(
            settings.Get(ShowLevel), settings.Get(ShowObjects), settings.Get(ShowSky), settings.Get(ShowFog), settings.Get(CullBackFaces), maxY,
            settings.Get(KeepMarioVisible));
    }

    public void Build3D(Map3DContext c)
    {
        if (!_builder.Update(c.Frame))
            return;
        var scene = _builder.Scene;
        c.Scene.GameVisuals = scene;
        c.Scene.GameVisualsOptions = Options(c.Frame, c.Settings);

        if (c.Settings.Get(ShowShadows))
            foreach (var shadow in scene.Shadows)
                AddShadow(c.Scene, shadow);
    }

    private static void AddShadow(Scene3D scene, GfxShadow s)
    {
        var color = new Argb8Color((byte)Math.Clamp(s.Opacity * 160, 0, 255), 0, 0, 0);
        var center = s.Position + new Vector3(0, 4, 0);
        float r = s.Size / 2;
        if (s.Square)
        {
            Vector3 a = center + new Vector3(-r, 0, -r), b = center + new Vector3(r, 0, -r), d = center + new Vector3(r, 0, r), e = center + new Vector3(-r, 0, r);
            scene.Add(new Triangle3D(a, b, d, color, Edges: false));
            scene.Add(new Triangle3D(a, d, e, color, Edges: false));
            return;
        }
        const int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            double a0 = i * Math.PI * 2 / segments, a1 = (i + 1) * Math.PI * 2 / segments;
            scene.Add(new Triangle3D(center,
                center + new Vector3((float)Math.Sin(a0) * r, 0, (float)Math.Cos(a0) * r),
                center + new Vector3((float)Math.Sin(a1) * r, 0, (float)Math.Cos(a1) * r), color, Edges: false));
        }
    }
}
