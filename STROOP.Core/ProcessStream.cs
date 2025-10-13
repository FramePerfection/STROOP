namespace STROOP.Core;

public class ProcessStream : IDisposable
{
    public const int MAX_RAM_SIZE = 0x800000;

    public static ProcessStream Instance;

    private IEmuRamIO _io;
    public IEmuRamIO IO => _io;

    private byte[] _ram;
    private object _mStreamProcess = new object();

    public event EventHandler OnDisconnect;
    public event EventHandler WarnReadonlyOff;
    public readonly Action OnUpdate;

    public bool Readonly { get; set; } = false;
    public bool ShowWarning { get; set; } = false;

    public byte[] Ram => _ram;
    public string ProcessName => _io?.Name ?? "(No Emulator)";

    public ProcessStream(Action onUpdate)
    {
        OnUpdate = onUpdate;
        _ram = new byte[0x800000];
    }

    public bool SwitchIO(IEmuRamIO newIO)
    {
        lock (_mStreamProcess)
        {
            // Dipose of old process
            (_io as IDisposable)?.Dispose();
            if (_io != null)
                _io.OnClose -= ProcessClosed;

            // Check for no process
            if (newIO == null)
                goto Error;

            try
            {
                // Open and set new process
                _io = newIO;
                _io.OnClose += ProcessClosed;
            }
            catch (Exception) // Failed to create process
            {
                goto Error;
            }

            return true;

        Error:
            _io = null;
            return false;
        }
    }

    private int suspendCounter = 0;

    private class SuspendScope : Scope
    {
        private readonly ProcessStream stream;

        public SuspendScope(ProcessStream stream)
        {
            this.stream = stream;
            if (stream.suspendCounter == 0)
                stream._io?.Suspend();
            stream.suspendCounter++;
        }

        protected override void Close()
        {
            stream.suspendCounter--;
            if (stream.suspendCounter == 0)
                stream._io?.Resume();
        }
    }

    public Scope Suspend() => new SuspendScope(this);

    private void ProcessClosed(object sender, EventArgs e)
    {
        OnDisconnect?.Invoke(this, new EventArgs());
    }

    public UIntPtr GetAbsoluteAddress(uint relativeAddress, int size = 0) => _io?.GetAbsoluteAddress(relativeAddress, size) ?? new UIntPtr(0);

    public uint GetRelativeAddress(UIntPtr relativeAddress, int size) => _io?.GetRelativeAddress(relativeAddress, size) ?? 0;

    public object GetValue(Type type, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (type == typeof(byte)) return GetByte(address, absoluteAddress, mask, shift);
        if (type == typeof(sbyte)) return GetSByte(address, absoluteAddress, mask, shift);
        if (type == typeof(short)) return GetInt16(address, absoluteAddress, mask, shift);
        if (type == typeof(ushort)) return GetUInt16(address, absoluteAddress, mask, shift);
        if (type == typeof(int)) return GetInt32(address, absoluteAddress, mask, shift);
        if (type == typeof(uint)) return GetUInt32(address, absoluteAddress, mask, shift);
        if (type == typeof(float)) return GetSingle(address, absoluteAddress, mask, shift);
        if (type == typeof(double)) return GetDouble(address, absoluteAddress, mask, shift);

        throw new ArgumentOutOfRangeException("Cannot call ProcessStream.GetValue with type " + type);
    }

    public byte GetByte(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        byte value = ReadRam((UIntPtr)address, 1, EndiannessType.Little, absoluteAddress)[0];
        if (mask.HasValue) value = (byte)(value & mask.Value);
        if (shift.HasValue) value = (byte)(value >> shift.Value);
        return value;
    }

    public sbyte GetSByte(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        sbyte value = (sbyte)ReadRam((UIntPtr)address, 1, EndiannessType.Little, absoluteAddress)[0];
        if (mask.HasValue) value = (sbyte)(value & mask.Value);
        if (shift.HasValue) value = (sbyte)(value >> shift.Value);
        return value;
    }

    public short GetInt16(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        short value = BitConverter.ToInt16(ReadRam((UIntPtr)address, 2, EndiannessType.Little, absoluteAddress), 0);
        if (mask.HasValue) value = (short)(value & mask.Value);
        if (shift.HasValue) value = (short)(value >> shift.Value);
        return value;
    }

    public ushort GetUInt16(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        ushort value = BitConverter.ToUInt16(ReadRam((UIntPtr)address, 2, EndiannessType.Little, absoluteAddress), 0);
        if (mask.HasValue) value = (ushort)(value & mask.Value);
        if (shift.HasValue) value = (ushort)(value >> shift.Value);
        return value;
    }

    public int GetInt32(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        int value = BitConverter.ToInt32(ReadRam((UIntPtr)address, 4, EndiannessType.Little, absoluteAddress), 0);
        if (mask.HasValue) value = (int)(value & mask.Value);
        if (shift.HasValue) value = (int)(value >> shift.Value);
        return value;
    }

    public uint GetUInt32(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        uint value = BitConverter.ToUInt32(ReadRam((UIntPtr)address, 4, EndiannessType.Little, absoluteAddress), 0);
        if (mask.HasValue) value = (uint)(value & mask.Value);
        if (shift.HasValue) value = (uint)(value >> shift.Value);
        return value;
    }

    public float GetSingle(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null) => BitConverter.ToSingle(ReadRam((UIntPtr)address, 4, EndiannessType.Little, absoluteAddress), 0);

    public double GetDouble(uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null) => BitConverter.ToDouble(ReadRam((UIntPtr)address, 8, EndiannessType.Little, absoluteAddress), 0);

    public byte[] ReadRam(uint address, int length, EndiannessType endianness, bool absoluteAddress = false) => ReadRam((UIntPtr)address, length, endianness, absoluteAddress);

    public byte[] ReadRam(UIntPtr address, int length, EndiannessType endianness, bool absoluteAddress = false)
    {
        byte[] readBytes = new byte[length];

        // Get local address
        uint localAddress;
        if (absoluteAddress)
            localAddress = _io?.GetRelativeAddress(address, length) ?? 0;
        else
            localAddress = address.ToUInt32();
        localAddress &= ~0x80000000;

        if (EndiannessUtilities.DataIsMisaligned(address, length, EndiannessType.Big))
            return readBytes;

        /// Fix endianness
        switch (endianness)
        {
            case EndiannessType.Little:
                // Address is not little endian, fix:
                localAddress = EndiannessUtilities.SwapAddressEndianness(localAddress, length);

                if (localAddress + length > _ram.Length)
                    break;

                Buffer.BlockCopy(_ram, (int)localAddress, readBytes, 0, length);
                break;

            case EndiannessType.Big:
                // Read padded if misaligned address
                byte[] swapBytes;
                uint alignedAddress = EndiannessUtilities.AlignedAddressFloor(localAddress);


                int alignedReadByteCount = readBytes.Length / 4 * 4 + 8;
                if (alignedAddress + alignedReadByteCount > _ram.Length)
                    break;
                swapBytes = new byte[alignedReadByteCount];

                // Read memory
                Buffer.BlockCopy(_ram, (int)alignedAddress, swapBytes, 0, swapBytes.Length);
                swapBytes = EndiannessUtilities.SwapByteEndianness(swapBytes);

                // Copy memory
                Buffer.BlockCopy(swapBytes, (int)(localAddress - alignedAddress), readBytes, 0, readBytes.Length);

                break;
        }


        return readBytes;
    }

    public bool ReadProcessMemory(UIntPtr address, byte[] buffer, EndiannessType endianness) => _io?.ReadAbsolute(address, buffer, endianness) ?? false;

    public byte[] ReadAllMemory() => _io?.ReadAllMemory();

    public bool CheckReadonlyOff()
    {
        if (ShowWarning)
            WarnReadonlyOff?.Invoke(this, new EventArgs());

        return Readonly;
    }

    public bool SetValue(byte value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (shift.HasValue)
        {
            value = (byte)(value << shift.Value);
        }

        if (mask.HasValue)
        {
            byte oldValue = GetByte(address, absoluteAddress);
            value = (byte)(oldValue & ~mask.Value | value & mask.Value);
        }

        bool returnValue = WriteRam(new byte[] { value }, (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool SetValue(sbyte value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (shift.HasValue)
        {
            value = (sbyte)(value << shift.Value);
        }

        if (mask.HasValue)
        {
            sbyte oldValue = GetSByte(address, absoluteAddress);
            value = (sbyte)(oldValue & ~mask.Value | value & mask.Value);
        }

        bool returnValue = WriteRam(new byte[] { (byte)value }, (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool SetValue(short value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (shift.HasValue)
        {
            value = (short)(value << shift.Value);
        }

        if (mask.HasValue)
        {
            short oldValue = GetInt16(address, absoluteAddress);
            value = (short)(oldValue & ~mask.Value | value & mask.Value);
        }

        bool returnValue = WriteRam(BitConverter.GetBytes(value), (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool SetValue(ushort value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (shift.HasValue)
        {
            value = (ushort)(value << shift.Value);
        }

        if (mask.HasValue)
        {
            ushort oldValue = GetUInt16(address, absoluteAddress);
            value = (ushort)(oldValue & ~mask.Value | value & mask.Value);
        }

        bool returnValue = WriteRam(BitConverter.GetBytes(value), (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool SetValue(int value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (shift.HasValue)
        {
            value = (int)(value << shift.Value);
        }

        if (mask.HasValue)
        {
            int oldValue = GetInt32(address, absoluteAddress);
            value = (int)(oldValue & ~mask.Value | value & mask.Value);
        }

        bool returnValue = WriteRam(BitConverter.GetBytes(value), (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool SetValue(uint value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (shift.HasValue)
        {
            value = (uint)(value << shift.Value);
        }

        if (mask.HasValue)
        {
            uint oldValue = GetUInt32(address, absoluteAddress);
            value = (uint)(oldValue & ~mask.Value | value & mask.Value);
        }

        bool returnValue = WriteRam(BitConverter.GetBytes(value), (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool SetValue(float value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        bool returnValue = WriteRam(BitConverter.GetBytes(value), (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool SetValue(double value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        byte[] bytes1 = bytes.Take(4).ToArray();
        byte[] bytes2 = bytes.Skip(4).Take(4).ToArray();
        byte[] bytesSwapped = bytes2.Concat(bytes1).ToArray();

        bool returnValue = WriteRam(bytesSwapped, (UIntPtr)address, EndiannessType.Little, absoluteAddress);
        return returnValue;
    }

    public bool WriteRam(byte[] buffer, uint address, EndiannessType endianness,
        int bufferStart = 0, int? length = null, bool safeWrite = true) => WriteRam(buffer, (UIntPtr)address, endianness, false, bufferStart, length, safeWrite);

    private object ram_write_lock = new object();

    public bool WriteRam(byte[] buffer, UIntPtr address, EndiannessType endianness, bool absoluteAddress = false,
        int bufferStart = 0, int? length = null, bool safeWrite = true)
    {
        lock (ram_write_lock)
        {
            if (length == null)
                length = buffer.Length - bufferStart;

            if (CheckReadonlyOff())
                return false;

            byte[] writeBytes = new byte[length.Value];
            Array.Copy(buffer, bufferStart, writeBytes, 0, length.Value);

            // Attempt to pause the game before writing
            bool preSuspended = _io?.IsSuspended ?? false;
            if (safeWrite)
                _io?.Suspend();

            if (EndiannessUtilities.DataIsMisaligned(address, length.Value, EndiannessType.Big))
                throw new Exception("Misaligned data");

            // Write memory to game/process
            bool result;
            if (absoluteAddress)
                result = _io?.WriteAbsolute(address, writeBytes, endianness) ?? false;
            else
            {
                result = _io?.WriteRelative(address.ToUInt32(), writeBytes, endianness) ?? false;
                if (result && _io.ReadRelative(address.ToUInt32(), writeBytes, endianness))
                    Array.Copy(writeBytes, 0, Ram, address.ToUInt32() & 0x00FFFFFF, writeBytes.Length);
            }

            // Resume stream
            if (safeWrite && !preSuspended)
                _io?.Resume();

            return result;
        }
    }

    public bool RefreshRam()
    {
        lock (_ram)
        {
            try
            {
                // Read whole ram value to buffer
                if (_ram.Length != MAX_RAM_SIZE)
                    _ram = new byte[MAX_RAM_SIZE];

                return _io?.ReadRelative(0, _ram, EndiannessType.Little) ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public bool GetAllRam(out byte[] allRam)
    {
        allRam = new byte[MAX_RAM_SIZE];
        try
        {
            return _io?.ReadRelative(0, allRam, EndiannessType.Little) ?? false;
        }
        catch
        {
            return false;
        }
    }

    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (_io != null)
                {
                    _io.OnClose -= ProcessClosed;
                    (_io as IDisposable)?.Dispose();
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion
}
