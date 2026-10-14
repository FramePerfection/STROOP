using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using STROOP.Core;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Tabs.MapTab.DataUtil;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Utilities
{
    public static class DecompHackDetector
    {
        const uint OBJECT_SIZE = 0x260, OBJECT_LIST_SIZE = 0x68;
        const int OBJECT_LIST_COUNT = 13, MAX_OBJECT_SLOTS = 240;
        const uint MARIO_STATE_SIZE = 0xC8;
        const uint G_MARIO_STATE_US = 0x8032D93C, SEGMENT_TABLE_US = 0x8033B400;

        static bool SameStride(uint a, uint b) => ((long)a - b) % OBJECT_SIZE == 0;

        static readonly Stopwatch sinceScan = Stopwatch.StartNew();
        static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(1.5);
        static Rdram scanRam;

        const int SYMBOL_COUNT = 7;

        static bool complete, vanillaLayout;

        public static void Update()
        {
            if (Config.Stream == null || MappingConfig.HasUserMapping)
                return;
            if (RomVersionConfig.IsKnownBuild()
                || RomVersionConfig.Version != RomVersion.US && RomVersionConfig.Version != RomVersion.JP)
            {
                MappingConfig.ClearAutoMapping();
                complete = vanillaLayout = false;
                return;
            }

            if ((MappingConfig.IsAutoMapping && complete || vanillaLayout) && MarioStateOk(StreamReader, MarioConfig.StructAddress))
                return;
            if (sinceScan.Elapsed < ScanInterval)
                return;
            sinceScan.Restart();

            scanRam ??= new Rdram(new byte[Config.RamSize]);
            Config.Stream.CopyCachedRam(scanRam.data);
            if (scanRam.data.AsSpan().IndexOfAnyExcept((byte)0) < 0)
                return;
            var found = Detect(scanRam);
            if (found == null)
                return;
            vanillaLayout = found.All(entry => entry.Key == entry.Value);
            if (vanillaLayout)
            {
                MappingConfig.ClearAutoMapping();
                return;
            }

            MappingConfig.SetAutoMapping(found);
            complete = found.Count == SYMBOL_COUNT;
        }

        static readonly Func<uint, uint> StreamReader = a => Config.Stream.GetUInt32(a);

        public static Dictionary<uint, uint> Detect(Rdram ram)
        {
            uint U32(uint a) => ram.U32(a);
            uint marioState = FindMarioState(ram);
            if (marioState == 0)
                return null;
            uint marioObject = U32(marioState + 0x88), area = U32(marioState + 0x90);
            var result = new Dictionary<uint, uint> { [MarioConfig.StructAddressUS] = marioState };

            uint listArray = FindObjectListArray(ram, marioObject);
            uint pool = listArray != 0 ? FindObjectPool(ram, listArray) : 0;
            if (listArray != 0)
                result[ObjectSlotsConfig.ProcessGroupsStartAddressUS] = listArray;
            if (pool != 0)
                result[ObjectSlotsConfig.ObjectSlotsStartAddressUS] = pool;

            var inside = new List<(uint start, uint end)> { (marioState, marioState + MARIO_STATE_SIZE) };
            if (listArray != 0)
                inside.Add((listArray, listArray + OBJECT_LIST_COUNT * OBJECT_LIST_SIZE));
            if (pool != 0)
                inside.Add((pool, pool + MAX_OBJECT_SLOTS * OBJECT_SIZE));
            uint anchor = listArray != 0 ? listArray : marioState;

            var references = FindReferences(ram, new[] { marioState, marioObject, area }, inside);
            void Pick(uint value, uint vanilla)
            {
                if (!references.TryGetValue(value, out var addresses) || addresses.Count == 0)
                    return;
                result[vanilla] = addresses.OrderBy(a => Math.Abs((long)a - anchor)).First();
            }

            Pick(marioState, G_MARIO_STATE_US);
            Pick(marioObject, MarioObjectConfig.PointerAddressUS);
            Pick(area, AreaConfig.CurrentAreaPointerAddressUS);

            uint segmentTable = FindSegmentTable(ram, area);
            if (segmentTable != 0)
                result[Config.ObjectAssociations?.SegmentTableUS ?? SEGMENT_TABLE_US] = segmentTable;
            return result;
        }

        #region MarioState

        static bool MarioStateOk(Func<uint, uint> u32, uint m)
        {
            float F32(uint a) => BitConverter.Int32BitsToSingle((int)u32(a));
            if (!Rdram.IsPointer(m) || !Rdram.IsPointer(m + MARIO_STATE_SIZE))
                return false;
            uint obj = u32(m + 0x88), area = u32(m + 0x90);
            if (!Rdram.IsPointer(obj) || !Rdram.IsPointer(area) || u32(m + 0x0C) == 0)
                return false;
            short health = (short)u32(m + 0xAC);
            if (health < 0 || health > 0x880)
                return false;
            uint objType = u32(obj) >> 16;
            if (objType != 0x018 && objType != 9)
                return false;
            bool any = false;
            for (uint i = 0; i < 3; i++)
            {
                float p = F32(m + 0x3C + 4 * i);
                if (p != 0 && (!float.IsNormal(p) || Math.Abs(p) > 1e6f) || p != F32(obj + 0xA0 + 4 * i))
                    return false;
                any |= p != 0;
            }

            return any;
        }

        static uint FindMarioState(Rdram ram)
        {
            uint Reader(uint a) => ram.U32(a);
            for (uint p = 0x88; p + 4 <= ram.data.Length; p += 4)
            {
                uint value = ram.U32Phys(p);
                if (!Rdram.IsPointer(value) || (value & 3) != 0)
                    continue;
                uint objType = ram.U16(value);
                if (objType != 0x018 && objType != 9)
                    continue;
                uint m = 0x80000000 | (p - 0x88);
                if (MarioStateOk(Reader, m))
                    return m;
            }

            return 0;
        }

        #endregion

        #region objects

        static uint FindObjectListArray(Rdram ram, uint marioObject)
        {
            uint n = ram.U32(marioObject + 0x60);
            for (int i = 0; i < 1024 && Rdram.IsPointer(n); i++)
            {
                if (!SameStride(n, marioObject))
                    return CountClosedLists(ram, n) >= OBJECT_LIST_COUNT - 1 ? n : 0;
                n = ram.U32(n + 0x60);
            }

            return 0;
        }

        static IEnumerable<uint> ListMembers(Rdram ram, uint head)
        {
            uint n = ram.U32(head + 0x60);
            for (int i = 0; i < 1024 && Rdram.IsPointer(n) && n != head; i++)
            {
                yield return n;
                n = ram.U32(n + 0x60);
            }
        }

        static bool IsClosed(Rdram ram, uint head)
        {
            uint n = ram.U32(head + 0x60);
            for (int i = 0; i < 1024 && Rdram.IsPointer(n); i++)
            {
                if (n == head)
                    return true;
                n = ram.U32(n + 0x60);
            }

            return false;
        }

        static int CountClosedLists(Rdram ram, uint listArray)
        {
            int closed = 0;
            for (uint i = 0; i < OBJECT_LIST_COUNT; i++)
                if (IsClosed(ram, listArray + i * OBJECT_LIST_SIZE))
                    closed++;
            return closed;
        }

        static uint FindObjectPool(Rdram ram, uint listArray)
        {
            var objects = new List<uint>();
            for (uint i = 0; i < OBJECT_LIST_COUNT; i++)
            {
                uint head = listArray + i * OBJECT_LIST_SIZE;
                if (IsClosed(ram, head))
                    objects.AddRange(ListMembers(ram, head));
            }

            if (objects.Count == 0)
                return 0;
            uint start = objects.Min(), end = objects.Max() + OBJECT_SIZE;
            if (objects.Any(o => !SameStride(o, start)))
                return 0;

            bool InPool(uint p) => p == 0 || (Rdram.IsPointer(p) && SameStride(p, start)
                                              && Math.Abs((long)p - start) < (long)OBJECT_SIZE * MAX_OBJECT_SLOTS);
            while ((end - (start - OBJECT_SIZE)) / OBJECT_SIZE <= MAX_OBJECT_SLOTS)
            {
                uint slot = start - OBJECT_SIZE;
                uint next = ram.U32(slot + 0x60), prev = ram.U32(slot + 0x64);
                if (next == 0 && prev == 0 || !InPool(next) || !InPool(prev))
                    break;
                start = slot;
            }

            return start;
        }

        #endregion

        static Dictionary<uint, List<uint>> FindReferences(Rdram ram, uint[] values, List<(uint start, uint end)> exclude)
        {
            var found = values.Distinct().ToDictionary(v => v, _ => new List<uint>());
            for (uint p = 0; p + 4 <= ram.data.Length; p += 4)
            {
                uint value = ram.U32Phys(p);
                if (!Rdram.IsPointer(value) || !found.TryGetValue(value, out var list))
                    continue;
                uint address = 0x80000000 | p;
                if (!exclude.Any(r => address >= r.start && address < r.end))
                    list.Add(address);
            }

            return found;
        }

        #region segment table

        static bool PlausibleOpcode(uint op) =>
            op >= 0xB1 && op <= 0xBF || op >= 0xD7 && op <= 0xFF || op <= 0x07;

        static bool LooksLikeDisplayList(Rdram ram, uint physical)
        {
            for (uint i = 0; i < 1024; i++, physical += 8)
            {
                if (!ram.Ok(physical, 8))
                    return false;
                uint w0 = ram.U32Phys(physical), op = w0 >> 24;
                if (w0 == 0xB8000000 || w0 == 0xDF000000)
                    return i > 0;
                if (!PlausibleOpcode(op) || w0 == 0 && ram.U32Phys(physical + 4) == 0)
                    return false;
            }

            return false;
        }

        static List<uint> SampleDisplayLists(Rdram ram, uint area)
        {
            var samples = new List<uint>();
            uint root = ram.U32(area + 4);
            var stack = new Stack<uint>();
            var seen = new HashSet<uint>();
            stack.Push(root);
            while (stack.Count > 0 && seen.Count < 4096 && samples.Count < 24)
            {
                uint n = stack.Pop();
                if (!Rdram.IsPointer(n) || !seen.Add(n))
                    continue;
                uint field = ram.U32(n + 0x14);
                uint segment = field >> 24;
                if (segment >= 0x01 && segment <= 0x0F && (field & 7) == 0 && !samples.Contains(field))
                    samples.Add(field);
                uint first = ram.U32(n + 0x10), c = first;
                for (int i = 0; i < 256 && Rdram.IsPointer(c); i++)
                {
                    stack.Push(c);
                    c = ram.U32(c + 0x08);
                    if (c == first)
                        break;
                }
            }

            return samples;
        }

        static uint FindSegmentTable(Rdram ram, uint area)
        {
            var samples = SampleDisplayLists(ram, area);
            if (samples.Count == 0)
                return 0;
            uint best = 0;
            int bestScore = 0;
            var entries = new uint[32];
            uint usedSegment = samples.GroupBy(s => s >> 24).OrderByDescending(g => g.Count()).First().Key;
            for (uint p = 0; p + 32 * 4 <= ram.data.Length; p += 4)
            {
                uint used = ram.U32Phys(p + 4 * usedSegment);
                if (used == 0 || used >= 0x00800000 || (used & 7) != 0 || ram.U32Phys(p) != 0)
                    continue;
                int nonZero = 0;
                bool ok = true;
                for (uint i = 0; i < 32 && ok; i++)
                {
                    entries[i] = ram.U32Phys(p + 4 * i);
                    ok = entries[i] < 0x00800000 && (entries[i] & 7) == 0;
                    if (entries[i] != 0)
                        nonZero++;
                }

                if (!ok || nonZero < 3)
                    continue;
                int score = 0, needed = Math.Max(bestScore + 1, (samples.Count + 1) / 2);
                for (int s = 0; s < samples.Count && score + samples.Count - s >= needed; s++)
                {
                    uint dl = samples[s], baseAddress = entries[(dl >> 24) & 31];
                    if (baseAddress != 0 && LooksLikeDisplayList(ram, (baseAddress + (dl & 0xFFFFFF)) & 0x7FFFFF))
                        score++;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = 0x80000000 | p;
                }
            }

            return bestScore * 2 >= samples.Count ? best : 0;
        }

        #endregion
    }
}
