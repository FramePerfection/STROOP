using System.Globalization;
using System.Text.RegularExpressions;

namespace StroopNext.TasEngine;

public sealed record SearchOptions
{
    public required string Savestate { get; init; }
    public string? Movie { get; init; }
    public string? Task { get; init; }
    public string? Rom { get; init; }
    public required string Output { get; init; }
    public double Minutes { get; init; }
    public bool KeepGoing { get; init; }
    public bool CpuOnly { get; init; }
    public bool NoMupenCheck { get; init; }
    public bool NoCompetitors { get; init; }

    public IReadOnlyList<string> Arguments(string moteurPy)
    {
        var args = new List<string> { moteurPy, "battre", "--st", Savestate };
        if (!string.IsNullOrWhiteSpace(Movie))
            args.AddRange(["--film", Movie]);
        args.AddRange(["--minutes", Minutes.ToString("0.###", CultureInfo.InvariantCulture), "--sortie", Output]);
        if (!string.IsNullOrWhiteSpace(Task))
            args.AddRange(["--tache", Task]);
        if (!string.IsNullOrWhiteSpace(Rom))
            args.AddRange(["--rom", Rom]);
        if (KeepGoing)
            args.Add("--continuer");
        if (CpuOnly)
            args.Add("--cpu");
        if (NoMupenCheck)
            args.Add("--sans-mupen");
        if (NoCompetitors)
            args.Add("--sans-concurrents");
        return args;
    }

    public static string? SavestateFor(string movie)
    {
        foreach (var extension in new[] { ".st", ".savestate" })
        {
            string candidate = Path.ChangeExtension(movie, extension);
            if (File.Exists(candidate))
                return candidate;
        }
        return null;
    }

    public static string? TaskFor(string savestate)
    {
        string candidate = Path.ChangeExtension(savestate, null) + ".tache.json";
        return File.Exists(candidate) ? candidate : null;
    }
}

public sealed record SearchRecord(int Frames, double Minutes, string Movie)
{
    public string Savestate => Path.ChangeExtension(Movie, ".st");
}

public readonly record struct CurvePoint(double Minutes, int Best, string Phase);

public sealed partial class SearchProgress
{
    private readonly List<SearchRecord> _records = new();

    public int? Port { get; private set; }
    public string? Output { get; private set; }
    public int? StartFrames { get; private set; }
    public IReadOnlyList<SearchRecord> Records => _records;
    public int? Best => _records.Count > 0 ? _records.Min(r => r.Frames) : StartFrames;

    public string? LastPhase { get; private set; }

    [GeneratedRegex(@"sur le port (\d+)")]
    private static partial Regex PortLine();
    [GeneratedRegex(@"\| sortie (.+)$")]
    private static partial Regex OutputLine();
    [GeneratedRegex(@"départ(?: \(chrono de la tâche écrite\))? : (\d+) frames")]
    private static partial Regex StartLine();
    [GeneratedRegex(@"RECORD (\d+) f à ([\d.]+) min.* -> (\S+\.m64)\s*$")]
    private static partial Regex RecordLine();
    [GeneratedRegex(@"^\d\d:\d\d:\d\d ")]
    private static partial Regex Clock();

    public SearchRecord? Read(string line)
    {
        if (Port is null && PortLine().Match(line) is { Success: true } port)
            Port = int.Parse(port.Groups[1].Value, CultureInfo.InvariantCulture);
        if (OutputLine().Match(line) is { Success: true } output)
            Output ??= output.Groups[1].Value.Trim();
        if (StartFrames is null && StartLine().Match(line) is { Success: true } start)
            StartFrames = int.Parse(start.Groups[1].Value, CultureInfo.InvariantCulture);
        if (RecordLine().Match(line) is { Success: true } record)
        {
            string file = record.Groups[3].Value;
            string movie = Output is null ? file : Path.Combine(Output, "beat_" + Path.GetFileNameWithoutExtension(file), file);
            var r = new SearchRecord(int.Parse(record.Groups[1].Value, CultureInfo.InvariantCulture),
                double.Parse(record.Groups[2].Value, CultureInfo.InvariantCulture), movie);
            _records.Add(r);
            return r;
        }
        string text = Clock().Replace(line, "").Trim();
        if (text.Length > 0)
            LastPhase = text;
        return null;
    }

    public static string? LiveFile(string output)
    {
        try
        {
            return Directory.Exists(output)
                ? Directory.EnumerateFiles(output, "live.json", SearchOption.AllDirectories).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault()
                : null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static IReadOnlyList<SearchRecord> StoredRecords(string output)
    {
        var list = new List<SearchRecord>();
        try
        {
            if (!Directory.Exists(output))
                return list;
            foreach (var folder in Directory.EnumerateDirectories(output, "beat_record_*"))
                foreach (var movie in Directory.EnumerateFiles(folder, "record_*.m64"))
                    if (!movie.EndsWith("_complet.m64", StringComparison.OrdinalIgnoreCase)
                        && Regex.Match(Path.GetFileName(movie), @"^record_(\d+)f_") is { Success: true } m)
                        list.Add(new SearchRecord(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), double.NaN, movie));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
        return list.OrderBy(r => r.Frames).ToList();
    }

    [GeneratedRegex(@"^(\d\d:\d\d:\d\d) \[[\d.]+ s\] ")]
    private static partial Regex JournalStamp();

    public static IReadOnlyList<string> JournalTail(string output, int count = 8)
    {
        try
        {
            if (!Directory.Exists(output))
                return [];
            string? file = Directory.EnumerateFiles(output, "journal.log", SearchOption.AllDirectories)
                .Where(f => new FileInfo(f).Length > 0).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
            if (file is null)
                return [];
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);
            var tail = new Queue<string>();
            while (reader.ReadLine() is { } line)
            {
                if (line.Trim().Length == 0)
                    continue;
                tail.Enqueue(JournalStamp().Replace(line, "$1 ").Trim());
                if (tail.Count > count)
                    tail.Dequeue();
            }
            return tail.ToList();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    public string? Doing => LastPhase is { } p && p.StartsWith("phase : ", StringComparison.Ordinal) ? p["phase : ".Length..] : LastPhase;

    public static IReadOnlyList<CurvePoint> Curve(string output)
    {
        var points = new List<CurvePoint>();
        string file = Path.Combine(output, "curve.csv");
        try
        {
            if (!File.Exists(file))
                return points;
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line)
            {
                var cells = line.Split(',', 3);
                if (cells.Length >= 2
                    && double.TryParse(cells[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double minutes)
                    && int.TryParse(cells[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int best))
                    points.Add(new CurvePoint(minutes, best, cells.Length > 2 ? cells[2].Trim('"') : ""));
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
        return points;
    }
}
