namespace StroopNext.Game;

public enum LevelGroup { Castle, Course, Bowser, Secret }

public sealed record LevelArea(int Index, string Name, byte ArrivalNode);

public sealed record GameLevel(int Id, string Name, LevelGroup Group, IReadOnlyList<LevelArea> Areas)
{
    public LevelArea? Area(int index) => Areas.FirstOrDefault(a => a.Index == index);

    public string Describe(LevelArea area) => Areas.Count == 1 ? Name : $"{Name}, {area.Name}";
}

// The levels the game can enter by itself (level numbers of the decomp's level_defines.h). Each area lists the warp
// node Mario arrives at: an object of that area with a spawn behavior (bhvSpinAirborneWarp...) that also has a
// WARP_NODE entry. Not listed: the areas without one (Dire Dire Docks' second part, Wet-Dry World's town, Eyerok's
// room...), only reachable from inside the level, and the castle courtyard, whose only nodes are the exits after
// a star (save dialog) or a death (costs a life). Nodes are those of the vanilla scripts; the star and death
// ones are avoided everywhere.
public static class GameLevels
{
    private const byte Start = 0x0A;

    private static LevelArea A(int index, string name, byte node = Start) => new(index, name, node);

    private static GameLevel L(int id, string name, LevelGroup group, params LevelArea[] areas) => new(id, name, group, areas);

    public static IReadOnlyList<GameLevel> All { get; } =
    [
        L(16, "Castle Grounds", LevelGroup.Castle, A(1, "Grounds", 0x04)),
        L(6, "Castle Inside", LevelGroup.Castle, A(1, "Ground floor", 0x1F), A(2, "Upstairs", 0x00), A(3, "Basement", 0x00)),

        L(9, "Bob-omb Battlefield", LevelGroup.Course, A(1, "Battlefield")),
        L(24, "Whomp's Fortress", LevelGroup.Course, A(1, "Fortress")),
        L(12, "Jolly Roger Bay", LevelGroup.Course, A(1, "Bay"), A(2, "Area 2")),
        L(5, "Cool, Cool Mountain", LevelGroup.Course, A(1, "Mountain"), A(2, "Slide")),
        L(4, "Big Boo's Haunt", LevelGroup.Course, A(1, "Mansion")),
        L(7, "Hazy Maze Cave", LevelGroup.Course, A(1, "Cave")),
        L(22, "Lethal Lava Land", LevelGroup.Course, A(1, "Island"), A(2, "Volcano")),
        L(8, "Shifting Sand Land", LevelGroup.Course, A(1, "Desert"), A(2, "Pyramid")),
        L(23, "Dire, Dire Docks", LevelGroup.Course, A(1, "Docks")),
        L(10, "Snowman's Land", LevelGroup.Course, A(1, "Outside"), A(2, "Igloo")),
        L(11, "Wet-Dry World", LevelGroup.Course, A(1, "Main area")),
        L(36, "Tall, Tall Mountain", LevelGroup.Course, A(1, "Mountain"), A(2, "Area 2"), A(4, "Area 4")),
        L(13, "Tiny-Huge Island", LevelGroup.Course, A(1, "Huge island"), A(2, "Tiny island"), A(3, "Wiggler's cave")),
        L(14, "Tick Tock Clock", LevelGroup.Course, A(1, "Clock")),
        L(15, "Rainbow Ride", LevelGroup.Course, A(1, "Ride")),

        L(17, "Bowser in the Dark World", LevelGroup.Bowser, A(1, "Course")),
        L(30, "Bowser 1 arena (Dark World)", LevelGroup.Bowser, A(1, "Arena")),
        L(19, "Bowser in the Fire Sea", LevelGroup.Bowser, A(1, "Course")),
        L(33, "Bowser 2 arena (Fire Sea)", LevelGroup.Bowser, A(1, "Arena")),
        L(21, "Bowser in the Sky", LevelGroup.Bowser, A(1, "Course")),
        L(34, "Bowser 3 arena (Sky)", LevelGroup.Bowser, A(1, "Arena")),

        L(27, "The Princess's Secret Slide", LevelGroup.Secret, A(1, "Slide")),
        L(20, "The Secret Aquarium", LevelGroup.Secret, A(1, "Aquarium")),
        L(31, "Wing Mario Over the Rainbow", LevelGroup.Secret, A(1, "Rainbow")),
        L(29, "Tower of the Wing Cap", LevelGroup.Secret, A(1, "Tower")),
        L(28, "Cavern of the Metal Cap", LevelGroup.Secret, A(1, "Cavern")),
        L(18, "Vanish Cap Under the Moat", LevelGroup.Secret, A(1, "Moat")),
    ];

    public static GameLevel? Find(int id) => All.FirstOrDefault(l => l.Id == id);

    public static string GroupName(LevelGroup group) => group switch
    {
        LevelGroup.Castle => "Castle",
        LevelGroup.Course => "Courses",
        LevelGroup.Bowser => "Bowser",
        _ => "Secret levels",
    };
}
