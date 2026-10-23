using System.Numerics;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Session;
using StroopNext.Settings;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class Map3DTests
{
    private static Camera3D Camera() => new() { Width = 800, Height = 600, Target = new Vector3(100, 50, -200) };

    [Fact]
    public void Default_camera_looks_down_with_north_up_and_x_right_like_the_2d_map()
    {
        var camera = Camera();
        var center = camera.Project(camera.Target)!.Value;
        Assert.Equal(400, center.X, 1);
        Assert.Equal(300, center.Y, 1);
        Assert.True(camera.Project(camera.Target + new Vector3(0, 0, -100))!.Value.Y < center.Y);
        Assert.True(camera.Project(camera.Target + new Vector3(100, 0, 0))!.Value.X > center.X);
        Assert.True(camera.Position.Y > camera.Target.Y);
    }

    [Fact]
    public void Screen_rays_pass_through_the_projected_point()
    {
        var camera = Camera();
        camera.Orbit(130, -40);
        var world = new Vector3(-300, 120, 400);
        var screen = camera.Project(world)!.Value;
        var ray = camera.ScreenRay(screen);
        float t = Vector3.Dot(world - ray.Origin, ray.Direction);
        Assert.True(Vector3.Distance(ray.At(t), world) < 0.5f);
    }

    [Fact]
    public void Points_behind_the_camera_do_not_project()
    {
        var camera = Camera();
        Assert.Null(camera.Project(camera.Position - camera.Forward * 100));
    }

    [Fact]
    public void Leaving_the_game_camera_keeps_the_same_view()
    {
        var camera = Camera();
        camera.Eye = new Vector3(400, 600, 900);
        var before = camera.Project(new Vector3(-150, 20, 60))!.Value;
        var eye = camera.Position;
        camera.DetachFromEye();
        Assert.Null(camera.Eye);
        Assert.True(Vector3.Distance(eye, camera.Position) < 0.01f);
        var after = camera.Project(new Vector3(-150, 20, 60))!.Value;
        Assert.True(Vector2.Distance(before, after) < 0.01f);
    }

    [Fact]
    public void Pan_moves_the_scene_with_the_mouse()
    {
        var camera = Camera();
        var world = camera.Target;
        var before = camera.Project(world)!.Value;
        camera.Pan(50, 0);
        Assert.Equal(before.X + 50, camera.Project(world)!.Value.X, 0);
    }

    [Fact]
    public void Ray_triangle_and_cylinder_intersections()
    {
        var down = new Ray(new Vector3(10, 100, 10), -Vector3.UnitY);
        Assert.True(Picking.RayTriangle(down, new(0, 0, 0), new(100, 0, 0), new(0, 0, 100), out float t));
        Assert.Equal(100, t, 3);
        Assert.False(Picking.RayTriangle(down with { Origin = new(90, 100, 90) }, new(0, 0, 0), new(100, 0, 0), new(0, 0, 100), out _));

        Assert.True(Picking.RayCylinder(new Ray(new(-100, 50, 0), Vector3.UnitX), Vector3.Zero, 37, 160, out t));
        Assert.Equal(63, t, 3);
        Assert.True(Picking.RayCylinder(new Ray(new(10, 500, 0), -Vector3.UnitY), Vector3.Zero, 37, 160, out t));
        Assert.Equal(340, t, 3);
        Assert.False(Picking.RayCylinder(new Ray(new(-100, 200, 0), Vector3.UnitX), Vector3.Zero, 37, 160, out _));
        Assert.False(Picking.RayCylinder(new Ray(new(100, 50, 0), Vector3.UnitX), Vector3.Zero, 37, 160, out _));
    }

    [Fact]
    public void Picking_returns_the_nearest_hit_and_prefers_points_under_the_cursor()
    {
        var camera = Camera();
        var scene = new Scene3D();
        var floorHit = new CameraHit(new CameraState(default, default, 1));
        var cylinderHit = new CameraHit(new CameraState(default, default, 2));
        var target = camera.Target;
        Assert.True(scene.BeginBackground(this, "level"));
        scene.AddBackground(new Triangle3D(target + new Vector3(-500, 0, -500), target + new Vector3(500, 0, -500), target + new Vector3(0, 0, 500), new Argb8Color(255, 0, 0, 255)) { Hit = floorHit });
        scene.Add(new Cylinder3D(target, 50, 100, new Argb8Color(128, 255, 0, 0)) { Hit = cylinderHit });
        var center = camera.Project(target)!.Value;

        var hit = Picking.Pick(scene, camera, center)!.Value;
        Assert.Same(cylinderHit, hit.Primitive.Hit);
        Assert.InRange(hit.Point.Y - target.Y, 0.01f, 100f);
        Assert.Equal(50, Vector2.Distance(new(hit.Point.X, hit.Point.Z), new(target.X, target.Z)), 1);

        var pointHit = new CameraHit(new CameraState(default, default, 3));
        scene.Add(new Point3D(target + new Vector3(0, 0, -150), 6, new Argb8Color(255, 255, 255, 255)) { Hit = pointHit });
        var dot = camera.Project(target + new Vector3(0, 0, -150))!.Value;
        Assert.Same(pointHit, Picking.Pick(scene, camera, dot + new Vector2(3, 0))!.Value.Primitive.Hit);
    }

    [Fact]
    public void Tessellation_routes_primitives_to_their_pass()
    {
        var buffers = new MeshBuffers();
        buffers.Add(new Triangle3D(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, new Argb8Color(255, 10, 20, 30)));
        buffers.Add(new Triangle3D(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, new Argb8Color(100, 10, 20, 30)));
        buffers.Add(new Cylinder3D(Vector3.Zero, 10, 20, new Argb8Color(255, 1, 2, 3), Segments: 8));
        buffers.Add(new Line3D([Vector3.Zero, Vector3.UnitX, Vector3.UnitY], new Argb8Color(255, 1, 2, 3)));
        buffers.Add(new Point3D(Vector3.Zero, 5, new Argb8Color(255, 1, 2, 3)));
        buffers.Add(new Arrow3D(Vector3.Zero, 0x4000, 100, 10, new Argb8Color(255, 1, 2, 3)));

        Assert.Equal(3 + 8 * 12 + 9, buffers.Opaque.Count);
        Assert.Equal(3, buffers.Transparent.Count);
        Assert.Equal(4 + 8 * 4, buffers.Lines.Count);
        Assert.Equal(1, buffers.Points.Count);
        Assert.Equal(0xFF140A00u, MeshBuffers.Pack(new Argb8Color(255, 0, 10, 20)));
        Assert.Equal(new Vector3(0, -1, 0), buffers.Opaque.Array[0].Normal);

        buffers.Clear();
        Assert.Equal(0, buffers.Opaque.Count);
    }

    [Fact]
    public void Wall_prism_offsets_the_triangle_along_its_projection_axis()
    {
        var wall = new Surface(0, 0, true, 0, 0, 0, new(0, 0, 0), new(100, 0, 0), new(0, 100, 0), new(0, 0, 1), 0, 0);
        var prism = WallsLayer.Prism(wall, 50)!;
        Assert.Equal(8, prism.Length);
        Assert.Equal(new Vector3(0, 0, 50), prism[0].Item1);
        Assert.Equal(new Vector3(0, 0, -50), prism[1].Item1);
        var vertical = wall with { Normal = new(0, 1, 0) };
        Assert.Null(WallsLayer.Prism(vertical, 50));
    }

    [Fact]
    public void Standard_layers_build_a_3d_scene_with_keyed_background()
    {
        var floor = (new Vector3(-500, 0, -500), new Vector3(500, 0, -500), new Vector3(0, 0, 500), new Vector3(0, 1, 0), 0f);
        var wall = (new Vector3(-500, -100, 200), new Vector3(500, -100, 200), new Vector3(0, 400, 200), new Vector3(0, 0, -1), 200f);
        var services = new GameServices();
        var ram = GameModelTests.WithSurfaces(TestRam.Us(), 2, floor, wall);
        ram.Patch.WriteU32(TestRam.UsMario + O.FloorTri, 0x80200000).WriteF32(TestRam.UsMario + O.IntendedMag, 32);
        var store = new SettingsStore();

        var scene = new Scene3D();
        var layers = StandardLayers.Create(StandardLayers.CreateMarioPathTrack()).OfType<IMapLayer3D>().ToList();
        void Build(GameFrame frame)
        {
            scene.Clear();
            var camera = new Camera3D { Width = 800, Height = 600 };
            foreach (var layer in layers)
                layer.Build3D(new Map3DContext(frame, camera, scene, store.Scope($"map.layer.{((IMapLayer)layer).Id}"), new Selection()));
        }

        Build(ram.Frame(services: services));
        Assert.Contains(scene.Background, p => p is Triangle3D { Hit: SurfaceHit { Surface.Class: SurfaceClass.Floor } });
        Assert.Contains(scene.Background, p => p is Triangle3D { Hit: SurfaceHit { Surface.Class: SurfaceClass.Wall } });
        Assert.Contains(scene.Background, p => p is Triangle3D { Edges: false });
        Assert.Contains(scene.Items, p => p is Cylinder3D { Hit: MarioHit });
        Assert.Contains(scene.Items, p => p is Arrow3D a && a.Color == MapPalette.Intended);
        Assert.Contains(scene.Items, p => p is Line3D l && l.Color == MapPalette.Highlight);
        long version = scene.BackgroundVersion;
        var firstFloor = scene.Background.First(p => p.Hit is SurfaceHit);

        ram.Patch.WriteF32(TestRam.UsMario + O.X, 200);
        Build(ram.Frame(services: services));
        Assert.Equal(version, scene.BackgroundVersion);
        Assert.Same(firstFloor, scene.Background.First(p => p.Hit is SurfaceHit));

        ram.Patch.WriteF32(TestRam.UsMario + O.Y, 300);
        Build(ram.Frame(services: services));
        Assert.NotEqual(version, scene.BackgroundVersion);

        int before = scene.Background.Count;
        layers.RemoveAll(l => l is WallsLayer);
        Build(ram.Frame(services: services));
        Assert.True(scene.Background.Count < before);
        Assert.DoesNotContain(scene.Background, p => p.Hit is SurfaceHit { Surface.Class: SurfaceClass.Wall });
    }
}
