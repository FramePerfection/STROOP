/*
 * Name: SigScanSharp
 * Author: Striekcarl/GENESIS @ Unknowncheats
 * Date: 14/05/2017
 * Purpose: Find memory patterns, both individually or simultaneously, as fast as possible
 *
 * Example:
 *  Init:
 *      Process TargetProcess = Process.GetProcessesByName("TslGame")[0];
 *      SigScanSharp Sigscan = new SigScanSharp(TargetProcess.Handle);
 *      Sigscan.SelectModule(procBattlegrounds.MainModule);
 *
 *  Find Patterns (Simultaneously):
 *      Sigscan.AddPattern("Pattern1", "48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6");
 *      Sigscan.AddPattern("Pattern2", "E8 0A EC ? ? FF");
 *
 *      long lTime;
 *      var result = Sigscan.FindPatterns(out lTime);
 *      var offset = result["Pattern1"];
 *
 *  Find Patterns (Individual):
 *      long lTime;
 *      var offset = Sigscan.FindPattern("48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6", out lTime);
 *
 */

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace STROOP.Core.Emulators;

public class SigScanSharp
{
    private IntPtr g_hProcess { get; set; }
    private byte[] g_arrModuleBuffer { get; set; }
    private IntPtr g_lpModuleBase { get; set; }

    private Dictionary<string, string> g_dictStringPatterns { get; }

    public SigScanSharp(IntPtr hProc)
    {
        g_hProcess = hProc;
        g_dictStringPatterns = new Dictionary<string, string>();
    }

    [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
    public bool SelectModule(ProcessModule targetModule)
    {
        g_lpModuleBase = targetModule.BaseAddress;
        g_arrModuleBuffer = new byte[targetModule.ModuleMemorySize];

        g_dictStringPatterns.Clear();
        try
        {
            return Win32.ReadProcessMemory(g_hProcess, g_lpModuleBase, g_arrModuleBuffer, (IntPtr)targetModule.ModuleMemorySize);
        }
        catch (AccessViolationException)
        {
            return false;
        }
    }

    public void AddPattern(string szPatternName, string szPattern)
    {
        g_dictStringPatterns.Add(szPatternName, szPattern);
    }

    private bool PatternCheck(int nOffset, byte[] arrPattern)
    {
        for (int i = 0; i < arrPattern.Length; i++)
        {
            if (arrPattern[i] == 0x0)
                continue;

            if (arrPattern[i] != g_arrModuleBuffer[nOffset + i])
                return false;
        }

        return true;
    }

    public IntPtr FindPattern(byte[] arrPattern, ref int minOffset, out long lTime)
    {
        if (g_arrModuleBuffer == null || g_lpModuleBase == IntPtr.Zero)
            throw new Exception("Selected module is null");

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int nModuleIndex = minOffset; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
        {
            if (g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                continue;

            if (PatternCheck(nModuleIndex, arrPattern))
            {
                lTime = stopwatch.ElapsedMilliseconds;
                minOffset = nModuleIndex;
                return IntPtr.Add(g_lpModuleBase, nModuleIndex);
            }
        }

        lTime = stopwatch.ElapsedMilliseconds;
        return IntPtr.Zero;
    }

    private byte[] ParsePatternString(string szPattern)
    {
        List<byte> patternbytes = new List<byte>();

        foreach (string? szByte in szPattern.Split(' '))
            patternbytes.Add(szByte == "?" ? (byte)0x0 : Convert.ToByte(szByte, 16));

        return patternbytes.ToArray();
    }

    private static class Win32
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead = default(IntPtr));
    }
}
