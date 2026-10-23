namespace StroopNext;

public readonly record struct Argb8Color(byte A, byte R, byte G, byte B)
{
    public static readonly Argb8Color Transparent = new(0, 0, 0, 0);

    public static Argb8Color Rgb(byte r, byte g, byte b) => new(255, r, g, b);
    public static Argb8Color FromHex(uint argb) => new((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    public Argb8Color WithAlpha(double alpha) => this with { A = (byte)Math.Clamp(Math.Round(alpha * 255), 0, 255) };

    public Argb8Color Lerp(Argb8Color to, double t)
    {
        t = Math.Clamp(t, 0, 1);
        byte L(byte a, byte b) => (byte)Math.Round(a + (b - a) * t);
        return new Argb8Color(L(A, to.A), L(R, to.R), L(G, to.G), L(B, to.B));
    }

    public bool IsVisible => A > 0;
}
