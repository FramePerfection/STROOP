using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Variables;

public abstract class Variable
{
    public const string NotAvailable = "n/a";

    protected Variable(string id, string label, string group, string? description)
    {
        Id = id;
        Label = label;
        Group = group;
        Description = description;
    }

    public string Id { get; }
    public string Label { get; }
    public string Group { get; }
    public string? Description { get; }

    public abstract bool CanWrite { get; }

    public abstract string Format(GameFrame frame, DisplayOptions options);

    public abstract bool TryReadNumber(GameFrame frame, out double value);

    public abstract string? FormatDelta(GameFrame frame, DisplayOptions options);

    public abstract bool TryWrite(GameFrame frame, string text, DisplayOptions options, RamPatch patch, out string? error);

    public override string ToString() => Id;
}

public delegate void VariableWriter<T>(GameFrame frame, T value, RamPatch patch);

public sealed class Variable<T> : Variable where T : struct
{
    private readonly Func<GameFrame, T?> _read;
    private readonly VariableWriter<T>? _write;

    public Variable(string id, string label, string group, ValueFormat<T> format, Func<GameFrame, T?> read, VariableWriter<T>? write = null, string? description = null)
        : base(id, label, group, description)
    {
        ValueFormat = format;
        _read = read;
        _write = write;
    }

    public ValueFormat<T> ValueFormat { get; }
    public override bool CanWrite => _write is not null;

    public T? Read(GameFrame frame) => frame.IsKnownGame ? _read(frame) : null;

    public override string Format(GameFrame frame, DisplayOptions options)
        => Read(frame) is T value ? ValueFormat.Format(value, options) : NotAvailable;

    public override bool TryReadNumber(GameFrame frame, out double value)
    {
        value = Read(frame) is T v && ValueFormat.ToNumber(v) is double d ? d : 0;
        return Read(frame) is T t && ValueFormat.ToNumber(t) is not null;
    }

    public double? Delta(GameFrame frame)
        => frame.Previous is { } previous && Read(frame) is T now && Read(previous) is T before ? ValueFormat.Delta(now, before) : null;

    public override string? FormatDelta(GameFrame frame, DisplayOptions options)
        => Delta(frame) is double d ? ValueFormat.FormatDelta(d, options) : null;

    public bool Write(GameFrame frame, T value, RamPatch patch)
    {
        if (_write is null || !frame.IsKnownGame)
            return false;
        _write(frame, value, patch);
        return true;
    }

    public override bool TryWrite(GameFrame frame, string text, DisplayOptions options, RamPatch patch, out string? error)
    {
        error = null;
        if (_write is null)
        {
            error = $"{Label} is read-only.";
            return false;
        }
        if (!ValueFormat.TryParse(text, options, out T value))
        {
            error = $"'{text}' is not a valid value for {Label}.";
            return false;
        }
        if (!Write(frame, value, patch))
        {
            error = $"{Label} is not available for this game.";
            return false;
        }
        return true;
    }
}
