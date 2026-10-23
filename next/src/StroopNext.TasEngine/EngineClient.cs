using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;

namespace StroopNext.TasEngine;

public sealed class EngineException(string message, int status = 0) : Exception(message)
{
    public int Status { get; } = status;
}

public sealed record LiveMario(int Frame, string Action, Vector3 Position, double Speed, double FaceDegrees, int Coins, int Stars)
{
    public static LiveMario? From(JsonNode? node)
    {
        if (node is not JsonObject m || m["pos"] is not JsonArray pos || pos.Count < 3)
            return null;
        return new LiveMario(EngineJson.Int(m["frame"]), m["action_name"]?.ToString() ?? EngineJson.Int(m["action"]).ToString("X", CultureInfo.InvariantCulture),
            new Vector3(EngineJson.Float(pos[0]), EngineJson.Float(pos[1]), EngineJson.Float(pos[2])),
            EngineJson.Double(m["speed"]), EngineJson.Double(m["face_deg"]), EngineJson.Int(m["coins"]), EngineJson.Int(m["stars"]));
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture,
        $"frame {Frame} · {Action} · ({Position.X:0}, {Position.Y:0}, {Position.Z:0}) · speed {Speed:0.#}");
}

public sealed record EngineJob(bool Done, string? Phase, JsonObject? Result, string? Error, string? Kind, IReadOnlyDictionary<string, int?> Experts)
{
    public static EngineJob From(JsonObject json) => new(
        json["done"]?.GetValue<bool>() ?? false,
        json["progress"]?["phase"]?.ToString(),
        json["result"] as JsonObject,
        json["error"]?.ToString(),
        json["kind"]?.ToString(),
        json["progress"]?["experts"] is JsonObject experts
            ? experts.ToDictionary(e => e.Key, e => EngineJson.IntOrNull(e.Value))
            : new Dictionary<string, int?>());

    public (string Expert, int Frames)? Best
        => Experts.Where(e => e.Value is not null).OrderBy(e => e.Value).Select(e => ((string, int)?)(e.Key, e.Value!.Value)).FirstOrDefault();
}

internal static class EngineJson
{
    public static int Int(JsonNode? n) => n is JsonValue v && v.TryGetValue(out double d) ? (int)d : 0;
    public static double Double(JsonNode? n) => n is JsonValue v && v.TryGetValue(out double d) ? d : 0;
    public static float Float(JsonNode? n) => (float)Double(n);
    public static int? IntOrNull(JsonNode? n) => n is JsonValue v && v.TryGetValue(out double d) ? (int)d : null;
}

public sealed class EngineClient : IDisposable
{
    private readonly HttpClient _http;

    public EngineClient(Uri baseAddress, HttpMessageHandler? handler = null)
    {
        _http = handler is null ? new HttpClient() : new HttpClient(handler);
        _http.BaseAddress = baseAddress;
        _http.Timeout = Timeout.InfiniteTimeSpan;
        BaseAddress = baseAddress;
    }

    public static EngineClient Local(int port) => new(new Uri($"http://127.0.0.1:{port}/"));

    public Uri BaseAddress { get; }

    public async Task<bool> IsAliveAsync(CancellationToken cancel = default)
    {
        try
        {
            await GetAsync("api/state", TimeSpan.FromSeconds(3), cancel).ConfigureAwait(false);
            return true;
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or EngineException or System.Text.Json.JsonException)
        {
            return false;
        }
    }

    public async Task<JsonObject> GetAsync(string path, TimeSpan? timeout = null, CancellationToken cancel = default)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await SendAsync(new HttpRequestMessage(HttpMethod.Get, path), timeout, cancel).ConfigureAwait(false);
            }
            catch (HttpRequestException) when (attempt < 4 && !cancel.IsCancellationRequested)
            {
                await Task.Delay(500 * attempt, cancel).ConfigureAwait(false);
            }
        }
    }

    public Task<JsonObject> PostAsync(string path, JsonObject? body = null, TimeSpan? timeout = null, CancellationToken cancel = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent((body ?? new JsonObject()).ToJsonString(), Encoding.UTF8),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return SendAsync(request, timeout, cancel);
    }

    private async Task<JsonObject> SendAsync(HttpRequestMessage request, TimeSpan? timeout, CancellationToken cancel)
    {
        request.Headers.ConnectionClose = true;
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        limit.CancelAfter(timeout ?? TimeSpan.FromMinutes(10));
        using (request)
        using (var response = await _http.SendAsync(request, limit.Token).ConfigureAwait(false))
        {
            string text = await response.Content.ReadAsStringAsync(limit.Token).ConfigureAwait(false);
            JsonObject json;
            try
            {
                json = JsonNode.Parse(text) as JsonObject ?? new JsonObject();
            }
            catch (System.Text.Json.JsonException)
            {
                throw new EngineException($"the engine answered {(int)response.StatusCode} with something that is not JSON", (int)response.StatusCode);
            }
            if (json["error"] is { } error && error.ToString() is { Length: > 0 } message)
                throw new EngineException(message, (int)response.StatusCode);
            if (!response.IsSuccessStatusCode)
                throw new EngineException($"the engine answered {(int)response.StatusCode}", (int)response.StatusCode);
            return json;
        }
    }


    public async Task<LiveMario?> AttachAsync(string? rom, CancellationToken cancel = default)
        => LiveMario.From((await PostAsync("api/live/attach", new JsonObject { ["rom"] = Blank(rom) ? null : rom }, TimeSpan.FromSeconds(120), cancel).ConfigureAwait(false))["mario"]);

    public async Task<LiveMario?> LiveStateAsync(CancellationToken cancel = default)
    {
        var json = await PostAsync("api/live/state", null, TimeSpan.FromSeconds(10), cancel).ConfigureAwait(false);
        return json["running"]?.GetValue<bool>() == true ? LiveMario.From(json["mario"]) : null;
    }

    public Task<JsonObject> LiveStopAsync(CancellationToken cancel = default) => PostAsync("api/live/stop", null, TimeSpan.FromSeconds(30), cancel);

    public Task<JsonObject> GoToAsync(float x, float? y, float z, double radius, double budgetSeconds, int frames, CancellationToken cancel = default)
        => PostAsync("api/live_goto", new JsonObject
        {
            ["x"] = x, ["y"] = y, ["z"] = z, ["radius"] = radius, ["budget"] = budgetSeconds, ["frames"] = frames,
        }, TimeSpan.FromSeconds(60), cancel);

    public Task<JsonObject> LiveExportAsync(string path, bool force, CancellationToken cancel = default)
        => PostAsync("api/live/export", new JsonObject { ["path"] = path, ["force"] = force }, TimeSpan.FromMinutes(5), cancel);

    public Task<JsonObject> LivePlayAsync(string movie, CancellationToken cancel = default)
        => PostAsync("api/live/play", new JsonObject { ["film"] = movie, ["speed"] = 100 }, TimeSpan.FromMinutes(30), cancel);


    public async Task<EngineJob> JobAsync(CancellationToken cancel = default)
        => EngineJob.From(await GetAsync("api/job", TimeSpan.FromSeconds(10), cancel).ConfigureAwait(false));

    public async Task<(EngineJob Job, string Progress)> JobWithProgressAsync(CancellationToken cancel = default)
    {
        var json = await GetAsync("api/job", TimeSpan.FromSeconds(10), cancel).ConfigureAwait(false);
        return (EngineJob.From(json), json["progress"]?.ToJsonString() ?? "");
    }

    public Task<JsonObject> StopJobAsync(CancellationToken cancel = default) => PostAsync("api/stop", null, TimeSpan.FromSeconds(10), cancel);

    public Task<JsonObject> OpenAsync(string st, string? movie, string? rom, CancellationToken cancel = default)
        => PostAsync("api/open", new JsonObject { ["st"] = st, ["movie"] = movie, ["rom"] = Blank(rom) ? null : rom }, TimeSpan.FromMinutes(5), cancel);

    public Task<JsonObject> MovieAnimationAsync(CancellationToken cancel = default) => GetAsync("api/anim?what=movie", TimeSpan.FromMinutes(5), cancel);

    public Task<JsonObject> VerifyAsync(string st, string movie, string? task, string? rom, bool mupen, CancellationToken cancel = default)
        => PostAsync("api/verifier", new JsonObject
        {
            ["st"] = st, ["film"] = movie, ["tache"] = Blank(task) ? null : task, ["rom"] = Blank(rom) ? null : rom, ["mupen"] = mupen,
        }, TimeSpan.FromMinutes(10), cancel);

    public Task<JsonObject> LiveFileAsync(string liveJson, double since, CancellationToken cancel = default)
        => GetAsync($"api/live?file={Uri.EscapeDataString(liveJson)}&since={since.ToString(CultureInfo.InvariantCulture)}&ref=1", TimeSpan.FromMinutes(2), cancel);

    public Task<JsonObject> TriesAsync(string liveJson, double since, CancellationToken cancel = default)
        => GetAsync($"api/tries?file={Uri.EscapeDataString(liveJson)}&since={since.ToString(CultureInfo.InvariantCulture)}&n=32", TimeSpan.FromMinutes(2), cancel);

    private static bool Blank(string? s) => string.IsNullOrWhiteSpace(s);

    public void Dispose() => _http.Dispose();
}
