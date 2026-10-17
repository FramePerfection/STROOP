using System.Numerics;
using StroopNext.Game;
using StroopNext.Map;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class MapPortTests
{
    private static Surface Tri(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, byte flags = 0)
        => new(0, 0x80200000, true, 0, flags, 0, a, b, c, normal, 0, 0);

    private static readonly Surface Floor = Tri(new(-7, 0, -5), new(3, 0, 11), new(13, 0, -9), new(0, 1, 0));

    [Fact]
    public void Unit_rows_match_the_games_edge_test_point_by_point()
    {
        var expected = new HashSet<(int, int)>();
        for (int z = -20; z <= 20; z++)
            for (int x = -20; x <= 20; x++)
                if (GameCollision.IsInside(Floor, x, z))
                    expected.Add((x, z));
        Assert.NotEmpty(expected);

        var actual = new HashSet<(int, int)>();
        foreach (var (z, xMin, xMax) in GameCollision.UnitRows(Floor, -20, 20))
            for (int x = xMin; x <= xMax; x++)
                actual.Add((x, z));
        Assert.Equal(expected.OrderBy(p => p), actual.OrderBy(p => p));
    }

    [Theory]
    [InlineData(5.9f, 5)]
    [InlineData(-5.9f, -5)]
    [InlineData(-0.5f, 0)]
    [InlineData(65536f + 10.5f, 10)]
    [InlineData(32768f, -32768)]
    public void Collision_coordinates_truncate_to_s16(float coordinate, int unit) => Assert.Equal(unit, GameCollision.Truncate(coordinate));

    [Theory]
    [InlineData(3, 3, 4)]
    [InlineData(-3, -4, -3)]
    [InlineData(0, -1, 1)]
    public void Unit_zero_is_two_units_wide(int unit, float min, float max) => Assert.Equal((min, max), GameCollision.UnitSpan(unit));

    [Fact]
    public void Find_floor_ignores_floors_more_than_78_above()
    {
        var floor = (new Vector3(-500, 100, -500), new Vector3(0, 100, 500), new Vector3(500, 100, -500), new Vector3(0, 1, 0), -100f);
        var ram = GameModelTests.WithSurfaces(TestRam.Us(), 1, floor);
        var surfaces = ram.Frame().Surfaces;
        Assert.Equal(100f, GameCollision.FindFloor(surfaces, 0, 50, 0)?.Height);
        Assert.Null(GameCollision.FindFloor(surfaces, 0, 0, 0));
        Assert.Equal(100f, GameCollision.FindFloor(surfaces, 65536, 50, 0)?.Height);
    }

    [Fact]
    public void Wall_slices_follow_the_wall_at_that_height()
    {
        var wall = Tri(new(0, 0, 0), new(100, 0, 0), new(50, 100, 0), new(0, 0, 1));
        var low = WallsLayer.SliceAt(wall, 10)!.Value;
        var high = WallsLayer.SliceAt(wall, 90)!.Value;
        Assert.Equal(90, Vector2.Distance(low.A, low.B), 3);
        Assert.Equal(10, Vector2.Distance(high.A, high.B), 3);
        Assert.Null(WallsLayer.SliceAt(wall, 150));
    }

    [Fact]
    public void Wall_height_filter_uses_the_games_check_heights()
    {
        var wall = Tri(new(0, 100, 0), new(100, 100, 0), new(50, 200, 0), new(0, 0, 1));
        var grounded = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.Y, 50).WriteF32(TestRam.UsMario + O.FloorY, 50)).Frame().Mario!;
        Assert.True(WallsLayer.AtCheckHeight(wall, grounded));
        var airborne = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.Y, -54).WriteF32(TestRam.UsMario + O.FloorY, -100)).Frame().Mario!;
        Assert.True(WallsLayer.AtCheckHeight(wall, airborne));
        var below = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.Y, -200).WriteF32(TestRam.UsMario + O.FloorY, -200)).Frame().Mario!;
        Assert.False(WallsLayer.AtCheckHeight(wall, below));
    }

    [Fact]
    public void Walls_push_along_their_projection_axis()
    {
        Assert.Equal(0x0000, WallsLayer.PushYaw(Tri(default, default, default, new(0, 0, 1))));
        Assert.Equal(0x8000, WallsLayer.PushYaw(Tri(default, default, default, new(0, 0, -1))));
        Assert.Equal(0x4000, WallsLayer.PushYaw(Tri(default, default, default, new(1, 0, 0), flags: 0x08)));
    }

    [Fact]
    public void Units_shape_is_a_staircase_around_the_accepted_units()
    {
        var shape = HorizontalSurfaceLayer.UnitsShape(Floor, new(-100, -100), new(100, 100));
        Assert.NotNull(shape);
        Assert.All(shape!, p => Assert.Equal(p.X, MathF.Round(p.X)));
    }
}
