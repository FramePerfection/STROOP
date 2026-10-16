using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.DataUtil
{
    public class Texture
    {
        public int width, height;
        public byte[] rgba;
        public int wrapS, wrapT;
    }

    public enum CullMode
    {
        None,
        Back,
        Front,
    }

    public class Batch
    {
        public int texture;
        public int layer;
        public bool decal;
        public CullMode cull;
        public int first, count;

        public int uvAnimation = -1;
        public int texGen;
        public ulong fog;
    }

    public class Mesh
    {
        public readonly List<Batch> batches = new List<Batch>();
        public int triangleCount;
    }

    public class MeshStore
    {
        public const int VertexSize = 36;

        public byte[] data = new byte[VertexSize * 4096];
        public int length;

        public Mesh Add(MeshBuilder builder)
        {
            var mesh = new Mesh { triangleCount = builder.triangleCount };
            var keys = new List<(int texture, int layer, bool decal, CullMode cull, int uvAnimation, int texGen, ulong fog)>(builder.meshes.Keys);
            keys.Sort((a, b) => a.layer != b.layer ? a.layer.CompareTo(b.layer) : a.texture.CompareTo(b.texture));
            foreach (var key in keys)
            {
                var vertices = builder.meshes[key];
                if (length + vertices.Count > data.Length)
                    Array.Resize(ref data, Math.Max(length + vertices.Count, data.Length * 2));
                vertices.CopyTo(data, length);
                mesh.batches.Add(new Batch
                {
                    texture = key.texture,
                    layer = key.layer,
                    decal = key.decal,
                    cull = key.cull,
                    uvAnimation = key.uvAnimation,
                    texGen = key.texGen,
                    fog = key.fog,
                    first = length / VertexSize,
                    count = vertices.Count / VertexSize,
                });
                length += vertices.Count;
            }

            return mesh;
        }
    }

    public class MeshBuilder
    {
        public readonly Dictionary<(int texture, int layer, bool decal, CullMode cull, int uvAnimation, int texGen, ulong fog), List<byte>> meshes = new Dictionary<(int, int, bool, CullMode, int, int, ulong), List<byte>>();
        public int triangleCount;
    }

    public class TextureSet
    {
        public readonly List<Texture> textures = new List<Texture>();
        readonly Dictionary<(uint addr, int fmt, int siz, int w, int h, uint tlut, int pal, int tlutType, int cms, int cmt), int> indices = new Dictionary<(uint, int, int, int, int, uint, int, int, int, int), int>();

        public int Get(Rdram ram, uint addr, int fmt, int siz, int w, int h, uint? tlut, int pal, int tlutType, int cms, int cmt)
        {
            var key = (addr, fmt, siz, w, h, tlut ?? 0xFFFFFFFF, pal, tlutType, cms, cmt);
            if (!indices.TryGetValue(key, out int index))
            {
                textures.Add(new Texture
                {
                    width = w,
                    height = h,
                    rgba = DecodeTexture(ram, addr, fmt, siz, w, h, tlut, pal, tlutType),
                    wrapS = (cms & 1) != 0 ? 1 : (cms & 2) != 0 ? 2 : 0,
                    wrapT = (cmt & 1) != 0 ? 1 : (cmt & 2) != 0 ? 2 : 0,
                });
                indices[key] = index = textures.Count - 1;
            }

            return index;
        }

        static int C5(int v) => (v << 3) | (v >> 2);

        static void Rgba16(int v, byte[] o, int i)
        {
            o[i] = (byte)C5((v >> 11) & 31);
            o[i + 1] = (byte)C5((v >> 6) & 31);
            o[i + 2] = (byte)C5((v >> 1) & 31);
            o[i + 3] = (byte)((v & 1) != 0 ? 255 : 0);
        }

        public static byte[] DecodeTexture(Rdram ram, uint addr, int fmt, int siz, int w, int h, uint? tlut, int palette, int tlutType)
        {
            int bits = 4 << siz;
            uint src = ram.Phys(addr);
            var o = new byte[w * h * 4];
            uint tlutPhys = tlut.HasValue ? ram.Phys(tlut.Value) : 0;
            for (int i = 0; i < w * h; i++)
            {
                int v;
                switch (bits)
                {
                    case 4:
                        int b = ram.U8Phys(src + (uint)(i >> 1));
                        v = (i & 1) == 0 ? b >> 4 : b & 15;
                        break;
                    case 8:
                        v = ram.U8Phys(src + (uint)i);
                        break;
                    case 16:
                        v = ram.U16Phys(src + (uint)(2 * i));
                        break;
                    default:
                        v = (int)ram.U32Phys(src + (uint)(4 * i));
                        break;
                }

                int k = 4 * i;
                switch (fmt)
                {
                    case 0:
                        if (bits == 16)
                            Rgba16(v, o, k);
                        else
                        {
                            o[k] = (byte)(v >> 24);
                            o[k + 1] = (byte)(v >> 16);
                            o[k + 2] = (byte)(v >> 8);
                            o[k + 3] = (byte)v;
                        }

                        break;
                    case 2:
                        int index = v + (bits == 4 ? palette * 16 : 0);
                        if (!tlut.HasValue)
                        {
                            byte g = (byte)(siz == 0 ? index * 17 : index);
                            o[k] = o[k + 1] = o[k + 2] = g;
                            o[k + 3] = 255;
                        }
                        else
                        {
                            int p = ram.U16Phys(tlutPhys + (uint)(2 * index));
                            if (tlutType == 3)
                            {
                                o[k] = o[k + 1] = o[k + 2] = (byte)(p >> 8);
                                o[k + 3] = (byte)p;
                            }
                            else
                                Rgba16(p, o, k);
                        }

                        break;
                    case 3:
                        if (bits == 4)
                        {
                            int i3 = (v >> 1) & 7;
                            o[k] = o[k + 1] = o[k + 2] = (byte)((i3 << 5) | (i3 << 2) | (i3 >> 1));
                            o[k + 3] = (byte)((v & 1) != 0 ? 255 : 0);
                        }
                        else if (bits == 8)
                        {
                            o[k] = o[k + 1] = o[k + 2] = (byte)((v >> 4) * 17);
                            o[k + 3] = (byte)((v & 15) * 17);
                        }
                        else
                        {
                            o[k] = o[k + 1] = o[k + 2] = (byte)(v >> 8);
                            o[k + 3] = (byte)v;
                        }

                        break;
                    default:
                        o[k] = o[k + 1] = o[k + 2] = o[k + 3] = (byte)(bits == 4 ? v * 17 : v & 255);
                        break;
                }
            }

            return o;
        }
    }

    public enum Microcode
    {
        F3D,
        F3DEX,
        F3DEX2,
    }

    public class F3D
    {
        const int G_MOVEMEM = 0x03, G_VTX = 0x04, G_DL = 0x06;

        const int EX_G_BRANCH_Z = 0xB0, EX_G_TRI2 = 0xB1, EX_G_RDPHALF_1 = 0xB4, EX_G_QUAD = 0xB5;

        const int EX2_G_VTX = 0x01, EX2_G_BRANCH_Z = 0x04, EX2_G_TRI1 = 0x05, EX2_G_TRI2 = 0x06, EX2_G_QUAD = 0x07;
        const int EX2_G_TEXTURE = 0xD7, EX2_G_GEOMETRYMODE = 0xD9, EX2_G_MOVEWORD = 0xDB, EX2_G_MOVEMEM = 0xDC;
        const int EX2_G_DL = 0xDE, EX2_G_ENDDL = 0xDF, EX2_G_RDPHALF_1 = 0xE1, EX2_G_SETOTHERMODE_H = 0xE3;
        const int EX2_G_MV_LIGHT = 0x0A;

        const uint EX2_G_CULL_FRONT = 0x00000200, EX2_G_CULL_BACK = 0x00000400, EX2_G_SHADING_SMOOTH = 0x00200000;
        const uint F3D_G_SHADING_SMOOTH = 0x00000200;
        const uint SHARED_GEO_BITS = 0x00000001 | 0x00000004 | 0x00010000 | 0x00020000 | 0x00040000 | 0x00080000 | 0x00100000;

        public Microcode ucode = Microcode.F3D;
        uint rdpHalf1;
        const int G_CLEARGEOMETRYMODE = 0xB6, G_SETGEOMETRYMODE = 0xB7, G_ENDDL = 0xB8, G_SETOTHERMODE_H = 0xBA;
        const int G_TEXTURE = 0xBB, G_MOVEWORD = 0xBC, G_TRI1 = 0xBF;
        const int G_LOADTLUT = 0xF0, G_SETTILESIZE = 0xF2, G_LOADBLOCK = 0xF3, G_LOADTILE = 0xF4, G_SETTILE = 0xF5;
        const int G_SETFOGCOLOR = 0xF8, G_SETPRIMCOLOR = 0xFA, G_SETENVCOLOR = 0xFB, G_SETCOMBINE = 0xFC, G_SETTIMG = 0xFD;
        const uint G_TEXTURE_GEN = 0x00040000, G_TEXTURE_GEN_LINEAR = 0x00080000, G_FOG = 0x00010000;
        const uint G_LIGHTING = 0x00020000, G_CULL_FRONT = 0x00001000, G_CULL_BACK = 0x00002000;
        const int G_MW_NUMLIGHT = 0x02, G_MW_FOG = 0x08, G_MW_LIGHTCOL = 0x0A;

        public const uint GEO_INIT = 0x00000004 | 0x00000200 | G_CULL_BACK | G_LIGHTING;

        public const uint COMBINE_SHADE_W1 = 4 << 15;

        internal class Tile
        {
            public int fmt, siz = 2, tmem, pal, cms, cmt, masks, maskt, uls, ult, lrs, lrt;

            public Tile MemberwiseCloneTile() => (Tile)MemberwiseClone();
        }

        struct Vtx
        {
            public float x, y, z;
            public short s, t;
            public byte r, g, b, a;

            public bool lit;
            public sbyte nx, ny, nz, lx, ly, lz;
            public byte ar, ag, ab;
        }

        readonly Rdram ram;
        readonly TextureSet textureSet;
        readonly MeshBuilder output;

        public uint geo = GEO_INIT;
        public int uvAnimation = -1;
        bool texOn;
        double texScaleS = 1, texScaleT = 1;
        readonly Tile[] tiles = new Tile[8];
        uint timgAddr;
        readonly Dictionary<int, uint> tmemSrc = new Dictionary<int, uint>();
        uint? tlut;
        int tlutType;
        byte[] prim = { 255, 255, 255, 255 }, env = { 255, 255, 255, 255 };
        public uint combine0, combine1;
        uint fogColor, fogFactors;
        readonly Dictionary<int, (byte r, byte g, byte b, sbyte dx, sbyte dy, sbyte dz)> lights = new Dictionary<int, (byte, byte, byte, sbyte, sbyte, sbyte)>();
        int numLights = 1;
        readonly Vtx?[] vbuf = new Vtx?[64];
        readonly byte[] raw = new byte[16];

        public F3D(Rdram ram, TextureSet textureSet, MeshBuilder output)
        {
            this.ram = ram;
            this.textureSet = textureSet;
            this.output = output;
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = new Tile();
        }

        public sealed class State
        {
            internal uint geo, timgAddr, combine0, combine1, fogColor, fogFactors;
            internal bool texOn;
            internal double texScaleS, texScaleT;
            internal Tile[] tiles;
            internal KeyValuePair<int, uint>[] tmemSrc;
            internal uint? tlut;
            internal int tlutType, numLights;
            internal byte[] prim, env;
            internal KeyValuePair<int, (byte, byte, byte, sbyte, sbyte, sbyte)>[] lights;
            internal Microcode ucode;
            int hash;

            public int Hash
            {
                get
                {
                    if (hash != 0)
                        return hash;
                    var h = new HashCode();
                    h.Add(geo);
                    h.Add(timgAddr);
                    h.Add(combine0);
                    h.Add(combine1);
                    h.Add(fogColor);
                    h.Add(fogFactors);
                    h.Add(texOn);
                    h.Add(texScaleS);
                    h.Add(texScaleT);
                    foreach (var t in tiles)
                        h.Add(HashCode.Combine(t.fmt, t.siz, t.tmem, t.pal, t.cms, t.cmt, HashCode.Combine(t.masks, t.maskt), HashCode.Combine(t.uls, t.ult, t.lrs, t.lrt)));
                    foreach (var kv in tmemSrc)
                        h.Add(HashCode.Combine(kv.Key, kv.Value));
                    h.Add(tlut);
                    h.Add(tlutType);
                    h.Add(numLights);
                    h.Add(BitConverter.ToInt32(prim));
                    h.Add(BitConverter.ToInt32(env));
                    foreach (var kv in lights)
                        h.Add(HashCode.Combine(kv.Key, kv.Value));
                    h.Add((int)ucode);
                    return hash = h.ToHashCode() | 1;
                }
            }
        }

        public State Save()
        {
            var t = new Tile[tiles.Length];
            for (int i = 0; i < t.Length; i++)
                t[i] = (Tile)tiles[i].MemberwiseCloneTile();
            var tmem = new List<KeyValuePair<int, uint>>(tmemSrc);
            tmem.Sort((a, b) => a.Key.CompareTo(b.Key));
            var l = new List<KeyValuePair<int, (byte, byte, byte, sbyte, sbyte, sbyte)>>(lights);
            l.Sort((a, b) => a.Key.CompareTo(b.Key));
            return new State
            {
                geo = geo, timgAddr = timgAddr, combine0 = combine0, combine1 = combine1, fogColor = fogColor, fogFactors = fogFactors, texOn = texOn,
                texScaleS = texScaleS, texScaleT = texScaleT, tiles = t, tmemSrc = tmem.ToArray(), tlut = tlut,
                tlutType = tlutType, numLights = numLights, prim = (byte[])prim.Clone(), env = (byte[])env.Clone(),
                lights = l.ToArray(), ucode = ucode,
            };
        }

        public void Load(State s)
        {
            geo = s.geo;
            timgAddr = s.timgAddr;
            combine0 = s.combine0;
            combine1 = s.combine1;
            fogColor = s.fogColor;
            fogFactors = s.fogFactors;
            texOn = s.texOn;
            texScaleS = s.texScaleS;
            texScaleT = s.texScaleT;
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = s.tiles[i].MemberwiseCloneTile();
            tmemSrc.Clear();
            foreach (var kv in s.tmemSrc)
                tmemSrc[kv.Key] = kv.Value;
            tlut = s.tlut;
            tlutType = s.tlutType;
            numLights = s.numLights;
            prim = (byte[])s.prim.Clone();
            env = (byte[])s.env.Clone();
            lights.Clear();
            foreach (var kv in s.lights)
                lights[kv.Key] = kv.Value;
            ucode = s.ucode;
        }

        void Combiner(out int a0, out int b0, out int c0, out int d0)
        {
            a0 = (int)(combine0 >> 20) & 15;
            c0 = (int)(combine0 >> 15) & 31;
            b0 = (int)(combine1 >> 28) & 15;
            d0 = (int)(combine1 >> 15) & 7;
        }

        int ColorSource(out bool usesTex)
        {
            Combiner(out int a0, out int b0, out int c0, out int d0);
            usesTex = a0 == 1 || b0 == 1 || c0 == 1 || d0 == 1;
            if (a0 == 4 || b0 == 4 || c0 == 4 || d0 == 4) return 1;
            if (a0 == 3 || b0 == 3 || c0 == 3 || d0 == 3) return 2;
            if (a0 == 5 || b0 == 5 || c0 == 5 || d0 == 5) return 3;
            return 0;
        }

        byte AlphaSource(byte shadeAlpha)
        {
            int a = (int)(combine0 >> 12) & 7, c = (int)(combine0 >> 9) & 7, b = (int)(combine1 >> 12) & 7, d = (int)(combine1 >> 9) & 7;
            if (a == 5 || b == 5 || c == 5 || d == 5) return env[3];
            if (a == 3 || b == 3 || c == 3 || d == 3) return prim[3];
            if (a == 4 || b == 4 || c == 4 || d == 4) return shadeAlpha;
            return 255;
        }

        bool Decal()
        {
            Combiner(out int a0, out int b0, out int c0, out int d0);
            return a0 == 1 && b0 == 4 && c0 == 8 && d0 == 4;
        }

        void Shade(ref Vtx v, in Matrix4 m)
        {
            if ((geo & G_LIGHTING) == 0)
            {
                v.r = raw[12];
                v.g = raw[13];
                v.b = raw[14];
                v.a = raw[15];
                return;
            }

            var n = Vector3.TransformNormal(new Vector3((sbyte)raw[12], (sbyte)raw[13], (sbyte)raw[14]), m);
            if (n.LengthSquared > 0)
                n = n.Normalized() * 127;
            v.lit = true;
            v.nx = (sbyte)Math.Round(n.X);
            v.ny = (sbyte)Math.Round(n.Y);
            v.nz = (sbyte)Math.Round(n.Z);
            v.a = raw[15];

            double ar = 128, ag = 128, ab = 128;
            if (lights.TryGetValue(numLights + 1, out var amb))
                (ar, ag, ab) = (amb.r, amb.g, amb.b);
            var light = lights.TryGetValue(1, out var first) ? first : ((byte)255, (byte)255, (byte)255, (sbyte)0x28, (sbyte)0x28, (sbyte)0x28);
            v.r = light.Item1;
            v.g = light.Item2;
            v.b = light.Item3;
            v.lx = light.Item4;
            v.ly = light.Item5;
            v.lz = light.Item6;

            for (int i = 2; i <= numLights; i++)
            {
                var l = lights.TryGetValue(i, out var x) ? x : ((byte)255, (byte)255, (byte)255, (sbyte)0x28, (sbyte)0x28, (sbyte)0x28);
                double ln = Math.Sqrt(l.Item4 * l.Item4 + l.Item5 * l.Item5 + l.Item6 * l.Item6);
                if (ln == 0) ln = 1;
                double k = Math.Max(0, ((sbyte)raw[12] * l.Item4 + (sbyte)raw[13] * l.Item5 + (sbyte)raw[14] * l.Item6) / (127.0 * ln));
                ar += l.Item1 * k;
                ag += l.Item2 * k;
                ab += l.Item3 * k;
            }

            v.ar = (byte)Math.Min(255, (int)ar);
            v.ag = (byte)Math.Min(255, (int)ag);
            v.ab = (byte)Math.Min(255, (int)ab);
        }

        int GetTexture(bool usesTex, out int w, out int h, out int uls, out int ult)
        {
            var t = tiles[0];
            w = ((t.lrs - t.uls) >> 2) + 1;
            h = ((t.lrt - t.ult) >> 2) + 1;
            uls = t.uls;
            ult = t.ult;
            if (!(texOn && usesTex))
                return -1;
            if (!tmemSrc.TryGetValue(t.tmem, out uint addr) || !(0 < w && w <= 256 && 0 < h && h <= 256))
                return -1;
            return textureSet.Get(ram, addr, t.fmt, t.siz, w, h, t.fmt == 2 ? tlut : null, t.siz == 0 ? t.pal : 0, tlutType,
                t.cms | (t.masks == 0 ? 2 : 0), t.cmt | (t.maskt == 0 ? 2 : 0));
        }

        public Vector2 TexCoordScale()
        {
            var t = tiles[0];
            int w = ((t.lrs - t.uls) >> 2) + 1, h = ((t.lrt - t.ult) >> 2) + 1;
            return w > 0 && h > 0 ? new Vector2((float)(texScaleS / (32.0 * w)), (float)(texScaleT / (32.0 * h))) : Vector2.Zero;
        }

        void LoadVertices(uint addr, int n, int v0, in Matrix4 m)
        {
            uint p = ram.Phys(addr);
            for (int i = 0; i < n; i++, p += 16)
            {
                if (!ram.Ok(p, 16))
                    return;
                for (uint k = 0; k < 16; k++)
                    raw[k] = ram.U8Phys(p + k);
                StoreVertex(v0 + i, m);
            }
        }

        public void LoadVertices(byte[] vertices, int n, int v0, in Matrix4 m)
        {
            for (int i = 0; i < n && (i + 1) * 16 <= vertices.Length; i++)
            {
                Buffer.BlockCopy(vertices, i * 16, raw, 0, 16);
                StoreVertex(v0 + i, m);
            }
        }

        void StoreVertex(int index, in Matrix4 m)
        {
            var pos = new Vector3((short)(raw[0] << 8 | raw[1]), (short)(raw[2] << 8 | raw[3]), (short)(raw[4] << 8 | raw[5]));
            var world = Vector3.TransformPosition(pos, m);
            var v = new Vtx
            {
                x = world.X,
                y = world.Y,
                z = world.Z,
                s = (short)(raw[8] << 8 | raw[9]),
                t = (short)(raw[10] << 8 | raw[11]),
            };
            Shade(ref v, m);
            if (index >= 0 && index < vbuf.Length)
                vbuf[index] = v;
        }

        void Triangle(int i0, int i1, int i2, int layer)
        {
            if (i0 >= vbuf.Length || i1 >= vbuf.Length || i2 >= vbuf.Length || !vbuf[i0].HasValue || !vbuf[i1].HasValue || !vbuf[i2].HasValue)
                return;
            var cull = (geo & G_CULL_BACK) != 0 ? CullMode.Back : CullMode.None;
            if ((geo & G_CULL_FRONT) != 0)
            {
                if (cull == CullMode.Back)
                    return;
                cull = CullMode.Front;
            }

            int src = ColorSource(out bool usesTex);
            int texture = GetTexture(usesTex, out int w, out int h, out int uls, out int ult);
            bool decal = texture >= 0 && Decal();
            int texGen = texture >= 0 && (geo & G_TEXTURE_GEN) != 0 ? (geo & G_TEXTURE_GEN_LINEAR) != 0 ? 2 : 1 : 0;
            ulong fog = (geo & G_FOG) != 0 ? (ulong)fogColor << 32 | fogFactors : 0;
            var key = (texture, layer, decal, cull, uvAnimation, texGen, fog);
            if (!output.meshes.TryGetValue(key, out var mesh))
                output.meshes[key] = mesh = new List<byte>(MeshStore.VertexSize * 3 * 32);

            float fu = 0, fv = 0, ou = 0, ov = 0;
            if (texture >= 0)
            {
                fu = (float)(texScaleS / (32.0 * w));
                fv = (float)(texScaleT / (32.0 * h));
                ou = uls / (4.0f * w);
                ov = ult / (4.0f * h);
            }

            Vertex(mesh, vbuf[i0].Value, texture, fu, fv, ou, ov, src, texGen != 0);
            Vertex(mesh, vbuf[i1].Value, texture, fu, fv, ou, ov, src, texGen != 0);
            Vertex(mesh, vbuf[i2].Value, texture, fu, fv, ou, ov, src, texGen != 0);
            output.triangleCount++;
        }

        void Vertex(List<byte> mesh, in Vtx v, int texture, float fu, float fv, float ou, float ov, int src, bool texGen)
        {
            AddFloat(mesh, v.x);
            AddFloat(mesh, v.y);
            AddFloat(mesh, v.z);
            AddFloat(mesh, texture < 0 ? 0 : texGen ? 32768 * fu : v.s * fu - ou);
            AddFloat(mesh, texture < 0 ? 0 : texGen ? 32768 * fv : v.t * fv - ov);
            switch (src)
            {
                case 1:
                    mesh.Add(v.r);
                    mesh.Add(v.g);
                    mesh.Add(v.b);
                    mesh.Add(v.a);
                    break;
                case 2:
                    mesh.AddRange(prim);
                    break;
                case 3:
                    mesh.AddRange(env);
                    break;
                default:
                    mesh.Add(255);
                    mesh.Add(255);
                    mesh.Add(255);
                    mesh.Add(v.a);
                    break;
            }

            mesh[mesh.Count - 1] = AlphaSource(v.a);

            bool lit = src == 1 && v.lit, normal = lit || (texGen && v.lit);
            mesh.Add(lit ? v.ar : (byte)0);
            mesh.Add(lit ? v.ag : (byte)0);
            mesh.Add(lit ? v.ab : (byte)0);
            mesh.Add(lit ? (byte)255 : (byte)0);
            mesh.Add((byte)(normal ? v.nx : 0));
            mesh.Add((byte)(normal ? v.ny : 0));
            mesh.Add((byte)(normal ? v.nz : 0));
            mesh.Add(0);
            mesh.Add((byte)(lit ? v.lx : 0));
            mesh.Add((byte)(lit ? v.ly : 0));
            mesh.Add((byte)(lit ? v.lz : 0));
            mesh.Add(0);
        }

        static void AddFloat(List<byte> list, float f)
        {
            int b = BitConverter.SingleToInt32Bits(f);
            list.Add((byte)b);
            list.Add((byte)(b >> 8));
            list.Add((byte)(b >> 16));
            list.Add((byte)(b >> 24));
        }

        public void SetEnvColor(byte r, byte g, byte b, byte a) => env = new[] { r, g, b, a };

        public void MoveMemLight(int index, uint address)
        {
            if (index < 0x86 || index > 0x94 || index % 2 != 0)
                return;
            ReadLight((index - 0x86) / 2 + 1, address);
        }

        void ReadLight(int n, uint address)
        {
            uint p = ram.Phys(address);
            if (ram.Ok(p, 16))
                lights[n] = (ram.U8Phys(p), ram.U8Phys(p + 1), ram.U8Phys(p + 2), (sbyte)ram.U8Phys(p + 8), (sbyte)ram.U8Phys(p + 9), (sbyte)ram.U8Phys(p + 10));
        }

        public void Run(uint dl, int layer, in Matrix4 m, int depth = 0)
        {
            uint pc = dl;
            for (int iteration = 0; iteration < 100000; iteration++)
            {
                if (!ram.Ok(ram.Phys(pc), 8))
                    return;
                uint w0 = ram.U32(pc), w1 = ram.U32(pc + 4);
                pc += 8;
                if (!Command(w0, w1, ucode, layer, m, depth))
                    return;
            }
        }

        public bool Command(uint w0, uint w1, int layer, in Matrix4 m, int depth = 0) => CommandF3D(w0, w1, false, layer, m, depth);

        public bool Command(uint w0, uint w1, Microcode microcode, int layer, in Matrix4 m, int depth = 0)
        {
            switch (microcode)
            {
                case Microcode.F3DEX2:
                    return CommandF3DEX2(w0, w1, layer, m, depth);
                case Microcode.F3DEX:
                    return CommandF3D(w0, w1, true, layer, m, depth);
                default:
                    return CommandF3D(w0, w1, false, layer, m, depth);
            }
        }

        bool Branch(uint dl, uint w0, int layer, in Matrix4 m, int depth)
        {
            if (depth < 16)
                Run(dl, layer, m, depth + 1);
            return ((w0 >> 16) & 0xFF) == 0;
        }

        void Triangle2x(uint w, int layer) => Triangle((int)((w >> 16) & 0xFF) / 2, (int)((w >> 8) & 0xFF) / 2, (int)(w & 0xFF) / 2, layer);

        void OtherModeH(int shift, int length, uint w1)
        {
            if (shift <= 14 && 14 < shift + length)
                tlutType = (int)(w1 >> 14) & 3;
        }

        static uint GeoEx2ToF3D(uint bits)
        {
            uint r = bits & SHARED_GEO_BITS;
            if ((bits & EX2_G_CULL_FRONT) != 0) r |= G_CULL_FRONT;
            if ((bits & EX2_G_CULL_BACK) != 0) r |= G_CULL_BACK;
            if ((bits & EX2_G_SHADING_SMOOTH) != 0) r |= F3D_G_SHADING_SMOOTH;
            return r;
        }

        bool CommandF3DEX2(uint w0, uint w1, int layer, in Matrix4 m, int depth)
        {
            int op = (int)(w0 >> 24);
            if (op >= 0xE4)
            {
                Rdp(op, w0, w1);
                return true;
            }

            switch (op)
            {
                case EX2_G_ENDDL:
                    return false;
                case EX2_G_DL:
                    return Branch(w1, w0, layer, m, depth);
                case EX2_G_BRANCH_Z:
                    if (depth < 16)
                        Run(rdpHalf1, layer, m, depth + 1);
                    return false;
                case EX2_G_RDPHALF_1:
                    rdpHalf1 = w1;
                    break;
                case EX2_G_VTX:
                    int n = (int)(w0 >> 12) & 0xFF;
                    LoadVertices(w1, n, (int)((w0 >> 1) & 0x7F) - n, m);
                    break;
                case EX2_G_TRI1:
                    Triangle2x(w0, layer);
                    break;
                case EX2_G_TRI2:
                case EX2_G_QUAD:
                    Triangle2x(w0, layer);
                    Triangle2x(w1, layer);
                    break;
                case EX2_G_GEOMETRYMODE:
                    geo = (geo & ~GeoEx2ToF3D(~w0 & 0xFFFFFF)) | GeoEx2ToF3D(w1);
                    break;
                case EX2_G_TEXTURE:
                    texOn = ((w0 >> 1) & 0x7F) != 0;
                    SetTextureScale(w1);
                    break;
                case EX2_G_MOVEMEM:
                    if ((w0 & 0xFF) == EX2_G_MV_LIGHT)
                    {
                        int offset = (int)((w0 >> 8) & 0xFF) * 8;
                        if (offset % 0x18 == 0 && offset >= 0x30)
                            ReadLight(offset / 0x18 - 1, w1);
                    }

                    break;
                case EX2_G_MOVEWORD:
                    int index = (int)(w0 >> 16) & 0xFF;
                    if (index == G_MW_NUMLIGHT)
                        numLights = (int)Math.Clamp(w1 / 24, 1, 7);
                    else if (index == G_MW_FOG)
                        fogFactors = w1;
                    else if (index == G_MW_LIGHTCOL)
                    {
                        int offset = (int)(w0 & 0xFFFF);
                        if (offset % 0x18 == 0)
                        {
                            int light = offset / 0x18 + 1;
                            var l = lights.TryGetValue(light, out var x) ? x : ((byte)255, (byte)255, (byte)255, (sbyte)0x28, (sbyte)0x28, (sbyte)0x28);
                            lights[light] = ((byte)(w1 >> 24), (byte)(w1 >> 16), (byte)(w1 >> 8), l.Item4, l.Item5, l.Item6);
                        }
                    }

                    break;
                case EX2_G_SETOTHERMODE_H:
                    int len = (int)(w0 & 0xFF) + 1;
                    OtherModeH(32 - (int)((w0 >> 8) & 0xFF) - len, len, w1);
                    break;
            }

            return true;
        }

        void SetTextureScale(uint w1)
        {
            uint s = (w1 >> 16) & 0xFFFF, t = w1 & 0xFFFF;
            texScaleS = s == 0xFFFF ? (s + 1) / 65536.0 : s / 65536.0;
            texScaleT = t == 0xFFFF ? (t + 1) / 65536.0 : t / 65536.0;
        }

        bool CommandF3D(uint w0, uint w1, bool ex, int layer, in Matrix4 m, int depth)
        {
            switch ((int)(w0 >> 24))
            {
                case G_ENDDL:
                    return false;
                case G_DL:
                    return Branch(w1, w0, layer, m, depth);
                case G_VTX:
                    if (ex)
                        LoadVertices(w1, (int)(w0 >> 10) & 0x3F, (int)((w0 >> 16) & 0xFF) / 2, m);
                    else
                        LoadVertices(w1, (int)((w0 >> 20) & 15) + 1, (int)(w0 >> 16) & 15, m);
                    break;
                case G_TRI1:
                    if (ex)
                        Triangle2x(w1, layer);
                    else
                        Triangle((int)((w1 >> 16) & 0xFF) / 10, (int)((w1 >> 8) & 0xFF) / 10, (int)(w1 & 0xFF) / 10, layer);
                    break;
                case EX_G_TRI2 when ex:
                case EX_G_QUAD when ex:
                    Triangle2x(w0, layer);
                    Triangle2x(w1, layer);
                    break;
                case EX_G_RDPHALF_1 when ex:
                    rdpHalf1 = w1;
                    break;
                case EX_G_BRANCH_Z when ex:
                    if (depth < 16)
                        Run(rdpHalf1, layer, m, depth + 1);
                    return false;
                case G_SETGEOMETRYMODE:
                    geo |= w1;
                    break;
                case G_CLEARGEOMETRYMODE:
                    geo &= ~w1;
                    break;
                case G_TEXTURE:
                    texOn = (w0 & 0xFF) != 0;
                    SetTextureScale(w1);
                    break;
                case G_MOVEMEM:
                    MoveMemLight((int)(w0 >> 16) & 0xFF, w1);
                    break;
                case G_MOVEWORD:
                    if ((w0 & 0xFF) == G_MW_NUMLIGHT)
                        numLights = (int)Math.Max(1, (((long)w1 - 0x80000000L) >> 5) - 1);
                    else if ((w0 & 0xFF) == G_MW_FOG)
                        fogFactors = w1;
                    break;
                case G_SETOTHERMODE_H:
                    OtherModeH((int)(w0 >> 8) & 0xFF, (int)w0 & 0xFF, w1);
                    break;
                default:
                    Rdp((int)(w0 >> 24), w0, w1);
                    break;
            }

            return true;
        }

        void Rdp(int op, uint w0, uint w1)
        {
            switch (op)
            {
                case G_SETFOGCOLOR:
                    fogColor = w1;
                    break;
                case G_SETTIMG:
                    timgAddr = w1;
                    break;
                case G_SETTILE:
                    var tile = tiles[(w1 >> 24) & 7];
                    tile.fmt = (int)(w0 >> 21) & 7;
                    tile.siz = (int)(w0 >> 19) & 3;
                    tile.tmem = (int)w0 & 0x1FF;
                    tile.pal = (int)(w1 >> 20) & 15;
                    tile.cmt = (int)(w1 >> 18) & 3;
                    tile.cms = (int)(w1 >> 8) & 3;
                    tile.maskt = (int)(w1 >> 14) & 15;
                    tile.masks = (int)(w1 >> 4) & 15;
                    break;
                case G_SETTILESIZE:
                    var sizedTile = tiles[(w1 >> 24) & 7];
                    sizedTile.uls = (int)(w0 >> 12) & 0xFFF;
                    sizedTile.ult = (int)w0 & 0xFFF;
                    sizedTile.lrs = (int)(w1 >> 12) & 0xFFF;
                    sizedTile.lrt = (int)w1 & 0xFFF;
                    break;
                case G_LOADBLOCK:
                case G_LOADTILE:
                    tmemSrc[tiles[(w1 >> 24) & 7].tmem] = timgAddr;
                    break;
                case G_LOADTLUT:
                    tlut = timgAddr;
                    break;
                case G_SETPRIMCOLOR:
                    prim = new[] { (byte)(w1 >> 24), (byte)(w1 >> 16), (byte)(w1 >> 8), (byte)w1 };
                    break;
                case G_SETENVCOLOR:
                    env = new[] { (byte)(w1 >> 24), (byte)(w1 >> 16), (byte)(w1 >> 8), (byte)w1 };
                    break;
                case G_SETCOMBINE:
                    combine0 = w0 & 0xFFFFFF;
                    combine1 = w1;
                    break;
            }
        }

        public static Microcode Detect(Rdram ram, IEnumerable<uint> displayLists, int maxCommands = 512)
        {
            int f3d = 0, f3dex = 0, f3dex2 = 0;
            foreach (uint dl in displayLists)
            {
                if (dl == 0)
                    continue;
                bool ex1 = false;
                uint pc = dl;
                for (int i = 0; i < maxCommands; i++, pc += 8)
                {
                    uint p = ram.Phys(pc);
                    if (!ram.Ok(p, 8))
                        break;
                    uint w0 = ram.U32Phys(p);
                    int op = (int)(w0 >> 24);
                    if (w0 == 0xDF000000)
                    {
                        f3dex2++;
                        break;
                    }

                    if (w0 == 0xB8000000)
                    {
                        if (ex1) f3dex++;
                        else f3d++;
                        break;
                    }

                    if (op == G_VTX && (w0 & 0xF) == 0xF && ((w0 >> 10) & 0x3F) * 16 - 1 == (w0 & 0x3FF))
                        ex1 = true;
                    else if (op == EX_G_TRI2)
                        ex1 = true;
                }
            }

            if (f3dex2 > f3d && f3dex2 > f3dex)
                return Microcode.F3DEX2;
            if (f3dex > f3d)
                return Microcode.F3DEX;
            return Microcode.F3D;
        }
    }
}
