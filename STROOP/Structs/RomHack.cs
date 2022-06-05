﻿using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STROOP.Structs
{
    public class RomHack
    {
        public bool Enabled = false;
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
            }
            while (nextEnd != -1);
        }

        public void LoadPayload(bool suspendStream = true)
        {
            var originalMemory = new List<Tuple<uint, byte[]>>();
            bool success = true;

            if (suspendStream)
                Config.Stream.Suspend();

            foreach (var (address, data) in _payload)
            {
                // Hacks are entered as big endian; we need to swap the address endianess before writing 
                var fixedAddress = EndiannessUtilities.SwapAddressEndianness(address, data.Length);

                // Read original memory before replacing
                originalMemory.Add(new Tuple<uint, byte[]>(fixedAddress, Config.Stream.ReadRam((UIntPtr)fixedAddress, data.Length, EndiannessType.Big)));
                success &= Config.Stream.WriteRam(data, fixedAddress, EndiannessType.Big);
            }

            if (suspendStream)
                Config.Stream.Resume();

            // Update original memory upon success
            if (success)
            {
                _originalMemory.Clear();
                _originalMemory.AddRange(originalMemory);
            }

            Enabled = success;
        }

        public bool ClearPayload()
        {
            bool success = true;

            if (_originalMemory.Count != _payload.Count)
                return false;

            Config.Stream.Suspend();

            foreach (var address in _originalMemory)
                // Read original memory before replacing
                success &= Config.Stream.WriteRam(address.Item2, address.Item1, EndiannessType.Big);

            Config.Stream.Resume();

            Enabled = !success;

            return success;
        }

        public void UpdateEnabledStatus()
        {
            Enabled = true;
            foreach (var address in _payload)
                Enabled &= address.Item2.SequenceEqual(Config.Stream.ReadRam(address.Item1, address.Item2.Length, EndiannessType.Big));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
