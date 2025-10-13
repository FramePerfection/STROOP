using STROOP.Core.GameMemoryAccess;
using System.ComponentModel;
using System.Diagnostics;
using static STROOP.Core.Kernal32NativeMethods;

namespace STROOP.Core.Emulators;

public class WindowsProcessRamIO : BaseProcessIO, IDisposable
{
    protected IntPtr _processHandle;
    protected Process _process;
    protected bool _isSuspended = false;
    protected UIntPtr _baseOffset;
    protected Emulator _emulator;

    public override bool IsSuspended => _isSuspended;

    protected override EndiannessType Endianness => _emulator.Endianness;
    protected override UIntPtr BaseOffset => _baseOffset;

    public override string Name => _process.ProcessName;
    public override Process Process => _process;

    public override event EventHandler OnClose;

    public WindowsProcessRamIO(Process process, Emulator emulator) : base()
    {
        _process = process;
        _emulator = emulator;

        _process.EnableRaisingEvents = true;

        ProcessAccess accessFlags = ProcessAccess.PROCESS_QUERY_LIMITED_INFORMATION | ProcessAccess.SUSPEND_RESUME
                                                                                    | ProcessAccess.VM_OPERATION | ProcessAccess.VM_READ | ProcessAccess.VM_WRITE;
        _processHandle = ProcessGetHandleFromId(accessFlags, false, _process.Id);
        try
        {
            CalculateOffset();
        }
        catch (Exception e)
        {
            CloseProcess(_processHandle);
            throw;
        }

        _process.Exited += _process_Exited;
    }

    private void _process_Exited(object sender, EventArgs e)
    {
        Dispose();
        OnClose.Invoke(sender, e);
    }

    protected override bool ReadFunc(UIntPtr address, byte[] buffer)
    {
        int numOfBytes = 0;
        return ProcessReadMemory(_processHandle, address, buffer, (IntPtr)buffer.Length, ref numOfBytes);
    }

    protected override bool WriteFunc(UIntPtr address, byte[] buffer)
    {
        int numOfBytes = 0;
        return ProcessWriteMemory(_processHandle, address, buffer, (IntPtr)buffer.Length, ref numOfBytes);
    }

    public override byte[] ReadAllMemory()
    {
        List<byte> output = new List<byte>();
        byte[] buffer = new byte[1];
        int numBytes = 1;

        for (uint address = 0; true; address++)
        {
            bool success = ProcessReadMemory(_processHandle, (UIntPtr)address, buffer, (IntPtr)buffer.Length, ref numBytes);
            if (!success) break;
            output.Add(buffer[0]);
        }

        return output.ToArray();
    }

    private bool CompareBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;
        return true;
    }

    // see https://msdn.microsoft.com/en-us/library/windows/desktop/ms684139%28v=vs.85%29.aspx
    public static bool Is64Bit(Process process)
        => Environment.Is64BitOperatingSystem
           && IsWow64Process(process.Handle, out bool isWow64)
            ? !isWow64
            : throw new Win32Exception();

    protected virtual void CalculateOffset()
    {
        // Find CORE_RDRAM export from mupen if present
        Win32SymbolInfo symbol = Win32SymbolInfo.Create();
        if (SymInitialize(_process.Handle, null, true))
        {
            try
            {
                if (SymFromName(_process.Handle, "CORE_RDRAM", ref symbol))
                {
                    bool is64Bit = Is64Bit(_process);
                    byte[]? buffer = new byte[is64Bit ? 8 : 4];
                    ReadAbsolute((UIntPtr)symbol.Address, buffer, EndiannessType.Little);
                    _baseOffset = (UIntPtr)(is64Bit ? BitConverter.ToUInt64(buffer, 0) : (ulong)BitConverter.ToUInt32(buffer, 0));
                    return;
                }
            }
            finally
            {
                if (!SymCleanup(_process.Handle))
                    throw new Win32Exception();
            }
        }
        else
        {
            // documentation doesn't say what to do when SymInitialize returns false, so just call this and don't care for its result for good (or bad) measure :shrug:
            // https://learn.microsoft.com/en-us/windows/win32/api/dbghelp/nf-dbghelp-syminitialize
            SymCleanup(_process.Handle);
        }

        // Find DLL offset if needed
        IntPtr dllOffset = new IntPtr();

        if (_emulator != null && _emulator.Dll != null)
        {
            ProcessModule dll = _process.Modules.Cast<ProcessModule>()
                ?.FirstOrDefault(d => d.ModuleName == _emulator.Dll);

            if (dll == null)
                throw new ArgumentNullException("Could not find ");

            dllOffset = dll.BaseAddress;
        }

        _baseOffset = (UIntPtr)(_emulator.RamStart + (ulong)dllOffset.ToInt64());

        if (!_emulator.AllowAutoDetect)
            return;

        // Address of create_thread
        foreach ((string name, uint offset) x in new[] { ("US", 0x246338), ("JP", 0x246338) })
        {
            string? path = $"Resources/AutoDetectFile {x.name}.bin";
            if (!File.Exists(path))
                continue;

            byte[]? autoDetectPattern = File.ReadAllBytes(path);
            byte[]? comparisonBuffer = new byte[autoDetectPattern.Length];

            SigScanSharp? processScanner = new SigScanSharp(Process.Handle);
            int minOffset = 0;
            while (processScanner.SelectModule(Process.MainModule))
            {
                IntPtr foundPatternAddress = processScanner.FindPattern(autoDetectPattern, ref minOffset, out long t);
                minOffset += autoDetectPattern.Length;
                if (foundPatternAddress != IntPtr.Zero)
                {
                    UIntPtr newBaseOffset = UIntPtr.Subtract((UIntPtr)(long)foundPatternAddress, (int)x.offset);
                    _baseOffset = newBaseOffset;
                    if (VerifyCandidate(newBaseOffset))
                        goto verified;
                }
                else
                    break;
            }
        }

        messageLogBuilder.AppendLine("Unable to verify or correct RAM start.\r\nVerify that the game is currently running.");
    verified: ;

        bool VerifyCandidate(UIntPtr candidate)
        {
            try
            {
                byte[]? mem = new byte[0x200];
                byte?[]? expectedSignature = new byte?[]
                {
                    null, 0x80, 0x1a, 0x3c,
                    null, null, 0x5a, 0x27,
                    0x08, 0x00, 0x40, 0x03,
                    0x00, 0x00, 0x00, 0x00,
                };
                if (!ReadFunc(candidate, mem))
                    return false;

                for (int i = 0; i < expectedSignature.Length; i++)
                {
                    if (expectedSignature[i].HasValue && mem[i] != expectedSignature[i].Value)
                        return false;
                    else if (!expectedSignature[i].HasValue)
                        expectedSignature[i] = mem[i];
                }

                foreach (int location in new[] { 0x80, 0x100, 0x180 })
                    for (int i = 0; i < expectedSignature.Length; i++)
                        if (expectedSignature[i].Value != mem[i + location])
                            return false;
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }

    public override bool Suspend()
    {
        SuspendProcess(_process);
        _isSuspended = true;
        return true;
    }

    public override bool Resume()
    {
        // Resume all threads
        ResumeProcess(_process);
        _isSuspended = false;
        return true;
    }

    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (IsSuspended)
                    Resume();
                _process.Exited -= _process_Exited;
            }

            // Close old process
            CloseProcess(_processHandle);

            disposedValue = true;
        }
    }

    ~WindowsProcessRamIO()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
