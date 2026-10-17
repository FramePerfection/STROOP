using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using StroopNext.Engine;

namespace StroopNext.Core.Tests;

public class EngineTests
{
    private static string TempFolder()
    {
        string folder = Path.Combine(Path.GetTempPath(), $"stroopnext-engine-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }

    [Fact]
    public void Setup_comes_from_the_engines_config_file_unless_chosen()
    {
        string root = TempFolder();
        try
        {
            string engine = Path.Combine(root, "moteur");
            Directory.CreateDirectory(Path.Combine(engine, "tools", "builder"));
            File.WriteAllText(Path.Combine(engine, "tools", "builder", "builder_server.py"), "");
            string config = Path.Combine(root, "moteur.json");
            File.WriteAllText(config, new JsonObject { ["engine"] = engine, ["python"] = @"C:\Python39\python.exe" }.ToJsonString());

            var found = EngineSetup.Detect(null, null, [config], []);
            Assert.Equal(engine, found.Folder);
            Assert.Equal(@"C:\Python39\python.exe", found.Python);
            Assert.EndsWith(Path.Combine("tools", "romhack", "bridge.lua"), found.BridgeLua);

            var chosen = EngineSetup.Detect(@"X:\other", "python3", [config], []);
            Assert.Equal(@"X:\other", chosen.Folder);
            Assert.Equal("python3", chosen.Python);
            Assert.Contains("not found", chosen.Problem());

            var usual = EngineSetup.Detect(null, null, [], [engine]);
            Assert.Equal(engine, usual.Folder);
            Assert.Equal("python", usual.Python);
            Assert.Null(usual.Problem());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Search_options_become_the_battre_command_line()
    {
        var options = new SearchOptions
        {
            Savestate = @"D:\t\a.st", Movie = @"D:\t\a.m64", Output = @"D:\t\out", Minutes = 2.5, Task = @"D:\t\a.tache.json",
            KeepGoing = true, CpuOnly = true, NoMupenCheck = true, NoCompetitors = true, Rom = @"D:\hack.z64",
        };
        Assert.Equal(
            ["moteur.py", "battre", "--st", @"D:\t\a.st", "--film", @"D:\t\a.m64", "--minutes", "2.5", "--sortie", @"D:\t\out",
             "--tache", @"D:\t\a.tache.json", "--rom", @"D:\hack.z64", "--continuer", "--cpu", "--sans-mupen", "--sans-concurrents"],
            options.Arguments("moteur.py"));

        var bare = new SearchOptions { Savestate = "s.st", Output = "o" };
        Assert.Equal(["m.py", "battre", "--st", "s.st", "--minutes", "0", "--sortie", "o"], bare.Arguments("m.py"));
    }

    [Fact]
    public void The_search_log_gives_its_port_output_start_and_records()
    {
        var p = new SearchProgress();
        Assert.Null(p.Read(@"20:11:02 moteur CPU + GPU sur le port 8071 | film D:\t\a.m64 | 4 min | sortie D:\t\out"));
        Assert.Equal(8071, p.Port);
        Assert.Equal(@"D:\t\out", p.Output);
        p.Read("20:11:09 départ : 226 frames, objectifs [('bhvStar', 1, 226)]");
        Assert.Equal(226, p.StartFrames);
        p.Read("20:11:10 phase : segments 1/3");
        Assert.Equal("phase : segments 1/3", p.LastPhase);

        var record = p.Read("20:12:45 RECORD 225 f à 1.6 min, objectifs [('bhvStar', 225)] -> record_225f_201245.m64");
        Assert.NotNull(record);
        Assert.Equal(225, record!.Frames);
        Assert.Equal(1.6, record.Minutes, 3);
        Assert.Equal(Path.Combine(@"D:\t\out", "beat_record_225f_201245", "record_225f_201245.m64"), record.Movie);
        Assert.Equal(225, p.Best);

        Assert.Equal(219, p.Read("20:20:00 RECORD 219 f à 9.0 min (chrono de la tâche : frames 3 -> 222), objectifs {'box': 106} -> record_219f_202000.m64")!.Frames);
        Assert.Equal(219, p.Best);
    }

    [Fact]
    public void Records_and_curve_are_read_from_the_output_folder()
    {
        string output = TempFolder();
        try
        {
            foreach (var (f, t) in new[] { (225, "201245"), (219, "202000") })
            {
                string dir = Path.Combine(output, $"beat_record_{f}f_{t}");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, $"record_{f}f_{t}.m64"), "");
                File.WriteAllText(Path.Combine(dir, $"record_{f}f_{t}_complet.m64"), "");
            }
            Assert.Equal([219, 225], SearchProgress.StoredRecords(output).Select(r => r.Frames));

            File.WriteAllText(Path.Combine(output, "curve.csv"), "minutes,meilleur,phase\n0.17,226,segments\n1.6,225,\"segments, fenêtre 2\"\n");
            var curve = SearchProgress.Curve(output);
            Assert.Equal(2, curve.Count);
            Assert.Equal(new CurvePoint(1.6, 225, "segments, fenêtre 2"), curve[1]);

            string live = Path.Combine(output, "beat_expert_1", "live.json");
            Directory.CreateDirectory(Path.GetDirectoryName(live)!);
            File.WriteAllText(live, "{}");
            Assert.Equal(live, SearchProgress.LiveFile(output));
        }
        finally
        {
            Directory.Delete(output, true);
        }
    }

    [Fact]
    public void What_the_search_tried_is_the_tail_of_its_journal()
    {
        string output = TempFolder();
        try
        {
            Assert.Empty(SearchProgress.JournalTail(output));
            File.WriteAllText(Path.Combine(output, "journal.log"),
                "15:37:48 [0.1 s] concurrents : 5 films rejoués\n\n15:38:05 [11.4 s] passe 1 : 60 s par segment\n"
                + "15:41:56 [252.0 s] technique de fin 118 → 133 : atteint en 133\n15:42:06 [261.2 s] chef : vitesse non (p 0.17)\n");
            Assert.Equal(["15:41:56 technique de fin 118 → 133 : atteint en 133", "15:42:06 chef : vitesse non (p 0.17)"],
                SearchProgress.JournalTail(output, 2));
            Assert.Equal(4, SearchProgress.JournalTail(output).Count);

            var p = new SearchProgress();
            p.Read("15:41:56 phase : point de passage 1 → 61 (rejoindre le film plus tôt)");
            Assert.Equal("point de passage 1 → 61 (rejoindre le film plus tôt)", p.Doing);
        }
        finally
        {
            Directory.Delete(output, true);
        }
    }

    [Fact]
    public void The_attempts_become_one_ghost_each_named_by_their_expert()
    {
        var json = JsonNode.Parse("""
            {"changed": true, "tries": [
              {"label": "explore", "hit": null, "m": [[1, 2, 3, 0], [4, 5, 6, 100]], "mo": [null, null]},
              {"label": "red", "hit": 37, "m": [[7, 8, 9, 0]], "mo": [null]},
              {"label": "empty", "m": []}]}
            """);
        var ghosts = EngineGhosts.FromTries(json, 1000);
        Assert.Equal(2, ghosts.Count);
        Assert.Equal("Engine try 1: explore", ghosts[0].Name);
        Assert.Equal("Engine try 2: red ✓ 37 f", ghosts[1].Name);
        Assert.Equal(2, ghosts[0].Frames.Count);
        Assert.Equal(999u, ghosts[0].PlaybackBaseFrame);
        Assert.Empty(EngineGhosts.FromTries(JsonNode.Parse("""{"changed": false}"""), 0));
    }

    [Fact]
    public void Engine_frames_become_a_ghost_aligned_on_the_savestates_timer()
    {
        var anim = JsonNode.Parse("""
            {"frames": [
              {"m": [1, 2, 3, 16384, 1], "mo": [0, 16384, 0, 1, 1, 1, 1, 72, 5, true, 0, 16384, 0, null]},
              {"m": [4, 5, 6, 16384, 1], "mo": [10, -16384, 20, 1, 1, 1, 1, 114, 0, true, 0, 0, 0, null]},
              {"m": [7, 8, 9, 0, 1], "mo": null}
            ]}
            """);
        var ghost = EngineGhosts.FromAnimation(anim, "best", 1000)!;
        Assert.Equal(3, ghost.Frames.Count);
        var at1000 = ghost.FrameAt(1000)!.Value;
        Assert.Equal(new System.Numerics.Vector3(1, 2, 3), at1000.Position);
        Assert.Equal(72, at1000.AnimationIndex);
        Assert.Equal(4, at1000.AnimationFrame);
        var at1001 = ghost.FrameAt(1001)!.Value;
        Assert.Equal(0xC000u, at1001.Yaw);
        Assert.Equal(10u, at1001.Pitch);
        Assert.Equal(20u, at1001.Roll);
        Assert.Equal(new System.Numerics.Vector3(7, 8, 9), ghost.FrameAt(1002)!.Value.Position);
        Assert.Null(ghost.FrameAt(1003));

        var reference = EngineGhosts.FromReference(JsonNode.Parse("""{"m": [[1, 1, 1, 0], [2, 2, 2, 0]], "mo": [null, null]}"""), "ref", 50)!;
        Assert.Equal(new System.Numerics.Vector3(2, 2, 2), reference.FrameAt(51)!.Value.Position);
        Assert.Null(EngineGhosts.FromAnimation(JsonNode.Parse("""{"frames": []}"""), "x", 0));
    }

    [Fact]
    public void Movie_inputs_are_read_as_the_engine_lists_them()
    {
        var m64 = new byte[0x400 + 3 * 4];
        BitConverter.TryWriteBytes(m64.AsSpan(0x18), 3u);
        new byte[] { 0x80, 0x00, 0x7F, 0x81 }.CopyTo(m64, 0x400);
        new byte[] { 0x00, 0x20, 0x00, 0x00 }.CopyTo(m64, 0x404);
        new byte[] { 0x20, 0x10, 0xF6, 0x0A }.CopyTo(m64, 0x408);
        var inputs = Movie.Inputs(m64);
        Assert.Equal(3, inputs.Count);
        Assert.Equal([127, -127, 0x80, 0x00], inputs[0]);
        Assert.Equal([0, 0, 0x00, 0x20], inputs[1]);
        Assert.Equal([-10, 10, 0x20, 0x10], inputs[2]);
        Assert.Throws<InvalidDataException>(() => Movie.Inputs(new byte[10]));
    }

    private sealed class FakeServer(Func<HttpRequestMessage, string, (HttpStatusCode, string)> answer) : HttpMessageHandler
    {
        public List<(string Method, string Path, string? ContentType, string Body)> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancel)
        {
            string body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancel);
            Requests.Add((request.Method.Method, request.RequestUri!.PathAndQuery, request.Content?.Headers.ContentType?.MediaType, body));
            var (status, json) = answer(request, body);
            return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task The_client_posts_json_and_turns_engine_errors_into_exceptions()
    {
        var server = new FakeServer((request, body) => request.RequestUri!.AbsolutePath switch
        {
            "/api/live/attach" => (HttpStatusCode.OK, """{"running": true, "attached": true, "mario": {"frame": 3, "action": 1024, "action_name": "IDLE", "pos": [1.5, 2, 3], "speed": 0, "face_deg": 90, "coins": 0, "stars": 1}}"""),
            "/api/live/export" => (HttpStatusCode.BadRequest, """{"error": "le film ne rejoue pas jusqu'à la position de Mario dans Mupen (écart 12 u) : rien d'écrit"}"""),
            "/api/live_goto" => (HttpStatusCode.OK, """{"ok": true}"""),
            "/api/job" =>(HttpStatusCode.OK, """{"progress": {"phase": "Mupen : Mario y va (40 frames)", "experts": {"red": null, "explore": 38, "polish": 41}}, "done": true, "result": {"found": true, "played": 40}, "error": null, "kind": "live_goto"}"""),
            _ => (HttpStatusCode.NotFound, """{"error": "unknown /x"}"""),
        });
        using var client = new EngineClient(new Uri("http://127.0.0.1:8066/"), server);

        var mario = await client.AttachAsync(null);
        Assert.Equal("IDLE", mario!.Action);
        Assert.Equal(1.5f, mario.Position.X);
        Assert.Equal("POST", server.Requests[0].Method);
        Assert.Equal("application/json", server.Requests[0].ContentType);
        Assert.Equal("""{"rom":null}""", server.Requests[0].Body);

        var refusal = await Assert.ThrowsAsync<EngineException>(() => client.LiveExportAsync(@"D:\a.m64", force: false));
        Assert.Contains("écart 12 u", refusal.Message);
        Assert.Equal(400, refusal.Status);

        await client.GoToAsync(100, null, -200, 80, 15, 400);
        var go = JsonNode.Parse(server.Requests[^1].Body)!;
        Assert.Null(go["y"]);
        Assert.Equal(-200, go["z"]!.GetValue<double>());
        Assert.Equal("/api/live_goto", server.Requests[^1].Path);

        var job = await client.JobAsync();
        Assert.Equal(("explore", 38), job.Best);
        Assert.Null(job.Experts["red"]);
        Assert.True(job.Done);
        Assert.Equal("Mupen : Mario y va (40 frames)", job.Phase);
        Assert.Equal(40, job.Result!["played"]!.GetValue<int>());

        Assert.False(await client.IsAliveAsync());
        Assert.Contains("/api/live?file=D%3A%5Cr%5Clive.json&since=12.5&ref=1",
            await Record(server, () => client.LiveFileAsync(@"D:\r\live.json", 12.5)));
    }

    private static async Task<string> Record(FakeServer server, Func<Task> call)
    {
        try
        {
            await call();
        }
        catch (EngineException)
        {
        }
        return server.Requests[^1].Path;
    }
}
