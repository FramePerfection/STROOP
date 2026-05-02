using STROOP.Core;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace STROOP.Structs
{
    public class RomHack
    {
        // Compatible with System.Windows.Forms.CheckState
        public enum EnabledStatus
        {
            Disabled = 0,
            Enabled = 1,
            Changed = 2
        }

        public EnabledStatus Status { get; private set; }
        public string Name;
        List<Tuple<uint, byte[]>> _payload = new List<Tuple<uint, byte[]>>();
        List<Tuple<uint, byte[]>> _originalMemory = new List<Tuple<uint, byte[]>>();

        public void AddPayload(uint address, byte[] newPayload)
        {
            _payload.Add(new Tuple<uint, byte[]>(address, newPayload));
        }

        public RomHack(string hackFileName, string hackName)
        {
            Name = hackName;
            if (File.Exists(hackFileName))
                LoadHackFromFile(hackFileName);
        }

        void LoadHackFromFile(string hackFileName)
        {
            // Load file and remove whitespace
            var dataUntrimmed = File.ReadAllText(hackFileName);
            var data = Regex.Replace(dataUntrimmed, @"\s+", "");

            int nextEnd;
            int prevEnd = data.IndexOf(":");

            // Failed to parse file
            if (prevEnd < 8 || prevEnd == data.Length - 1)
                return;

            string remData = data.Substring(prevEnd + 1);

            do
            {
                nextEnd = remData.IndexOf(":");

                if (ParsingUtilities.TryParseHex(data.Substring(prevEnd - 8, 8), out uint address))
                {
                    string byteData = (nextEnd == -1) ? remData : remData.Substring(0, nextEnd - 8);

                    var hackBytes = new byte[byteData.Length / 2];
                    for (int i = 0; i < hackBytes.Length; i++)
                        if (ParsingUtilities.TryParseHex(byteData.Substring(i * 2, 2), out uint b))
                            hackBytes[i] = (byte)b;
                        else
                            goto invalidPayload;

                    _payload.Add(new Tuple<uint, byte[]>(address, hackBytes));
                invalidPayload:;
                }

                remData = remData.Substring(nextEnd + 1);
                prevEnd += nextEnd + 1;
            } while (nextEnd != -1);
        }

        public void LoadPayload()
        {
            bool success = true;

            Status = EnabledStatus.Disabled;
            _originalMemory.Clear();

            using (Config.Stream.Suspend())
            {
                foreach (var (address, data) in _payload)
                {
                    // Hacks are entered as big endian; we need to swap the address endianess before writing 
                    var fixedAddress = EndiannessUtilities.SwapAddressEndianness(address, data.Length);

                    // Read original memory before replacing
                    _originalMemory.Add(new Tuple<uint, byte[]>(fixedAddress, Config.Stream.ReadRam((UIntPtr)fixedAddress, data.Length, EndiannessType.Big)));
                    success &= Config.Stream.WriteRam(data, fixedAddress, EndiannessType.Big);
                    if (success)
                        Status = EnabledStatus.Changed;
                }
            }

            Status = EnabledStatus.Enabled;
        }

        public bool ClearPayload()
        {
            if (_originalMemory.Count == 0)
                return false;

            bool success = true;

            if (_originalMemory.Count != _payload.Count)
            {
                Status = EnabledStatus.Changed;
                return false;
            }

            using (Config.Stream.Suspend())
            {
                foreach (var address in _originalMemory)
                    success &= Config.Stream.WriteRam(address.Item2, address.Item1, EndiannessType.Big);
            }

            if (success)
            {
                Status = EnabledStatus.Disabled;
                _originalMemory.Clear();
            }

            return success;
        }

        public void UpdateEnabledStatus()
        {
            Status = EnabledStatus.Enabled;
            int i = 0;
            foreach (var address in _payload)
            {
                var gameMemory = Config.Stream.ReadRam(address.Item1, address.Item2.Length, EndiannessType.Big);
                if (!address.Item2.SequenceEqual(gameMemory))
                {
                    Status = EnabledStatus.Disabled;
                    if (_originalMemory.Count > i && !_originalMemory[i].Item2.SequenceEqual(gameMemory))
                    {
                        Status = EnabledStatus.Changed;
                        return;
                    }
                }

                i++;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
