using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STROOP.Structs;
using System.ComponentModel;
using STROOP.Utilities;
using STROOP.Variables.Utilities;

namespace STROOP.M64
{
    public class M64Header
    {
        public enum MovieStartTypeEnum : ushort
        {
            FromSnapshot = 1, //movie begins from snapshot (the snapshot will be loaded from an external file with the movie filename and a .st extension)
            FromStart = 2, //movie begins from power-on
            FromEEPRom = 4, //movie begins from power-on but doesn't reset eeprom
        }

        private readonly M64File _m64File;
        private readonly Tabs.M64Tab _gui;

        // 018 4-byte little-endian unsigned int: number of input samples for any controllers
        private int _numInputs;

        [Category("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200B\u200B\u200B\u200BNum Inputs")]
        public int NumInputs
        {
            get => _numInputs;
            set
            {
                _numInputs = value;
                NotifyChange();
            }
        }

        // 00C 4-byte little-endian unsigned int: number of frames(vertical interrupts)
        private int _numVis;

        [Category("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200B\u200B\u200BNum VIs")]
        public int NumVis
        {
            get => _numVis;
            set
            {
                _numVis = value;
                NotifyChange();
            }
        }

        // 010 4-byte little-endian unsigned int: rerecord count
        private int _numRerecords;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200B\u200BNum Rerecords")]
        public int NumRerecords
        {
            get => _numRerecords;
            set
            {
                _numRerecords = value;
                NotifyChange();
            }
        }

        // 01C 2-byte unsigned int: movie start type
        private MovieStartTypeEnum _movieStartType;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("\u200BMovie Start Type")]
        public MovieStartTypeEnum MovieStartType
        {
            get => _movieStartType;
            set
            {
                _movieStartType = value;
                NotifyChange();
            }
        }

        // 014 1-byte unsigned int: frames(vertical interrupts) per second
        private byte _fps;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("FPS")]
        public byte Fps
        {
            get => _fps;
            set
            {
                _fps = value;
                NotifyChange();
            }
        }

        private byte _extendedVersion;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("Extended Version")]
        public byte ExtendedVersion
        {
            get => _extendedVersion;
            set
            {
                _extendedVersion = value;
                NotifyChange();
            }
        }

        private byte _extendedFlags;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("Extended Flags")]
        public byte ExtendedFlags
        {
            get => _extendedFlags;
            set
            {
                _extendedFlags = value;
                NotifyChange();
            }
        }

        private uint _authorshipTag;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("Authorship Tag")]
        public uint AuthorshipTag
        {
            get => _authorshipTag;
            set
            {
                _authorshipTag = value;
                NotifyChange();
            }
        }

        private uint _bruteforceExtraData;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("Bruteforce Extra Data")]
        public uint BruteforceExtraData
        {
            get => _bruteforceExtraData;
            set
            {
                _bruteforceExtraData = value;
                NotifyChange();
            }
        }

        private uint _numRerecordsHi;

        [CategoryAttribute("\u200B\u200B\u200B\u200B\u200BMain"), DisplayName("Num Rerecords (Hiword)")]
        public uint NumRerecordsHi
        {
            get => _numRerecordsHi;
            set
            {
                _numRerecordsHi = value;
                NotifyChange();
            }
        }

        // 0C4 32-byte ASCII string: internal name of ROM used when recording, directly from ROM
        private string _romName;

        [CategoryAttribute("\u200B\u200B\u200B\u200BRom"), DisplayName("\u200B\u200BRom Name")]
        public string RomName
        {
            get => _romName;
            set
            {
                _romName = StringUtilities.Cap(value, 32);
                NotifyChange();
            }
        }

        // 0E8 2-byte unsigned int: country code of ROM used when recording, directly from ROM
        private ushort _countryCode;

        [CategoryAttribute("\u200B\u200B\u200B\u200BRom"), DisplayName("\u200BCountry Code")]
        public ushort CountryCode
        {
            get => _countryCode;
            set
            {
                _countryCode = value;
                NotifyChange();
            }
        }

        // 0E4 4-byte unsigned int: CRC32 of ROM used when recording, directly from ROM
        private uint _crc32;

        [CategoryAttribute("\u200B\u200B\u200B\u200BRom"), DisplayName("CRC32")]
        public uint Crc32
        {
            get => _crc32;
            set
            {
                _crc32 = value;
                NotifyChange();
            }
        }

        // 222 222-byte UTF-8 string: author name info
        private string _author;

        [CategoryAttribute("\u200B\u200B\u200BDescription"), DisplayName("\u200BAuthor")]
        public string Author
        {
            get => _author;
            set
            {
                _author = StringUtilities.Cap(value, 222);
                NotifyChange();
            }
        }

        // 300 256-byte UTF-8 string: author movie description info
        private string _description;

        [CategoryAttribute("\u200B\u200B\u200BDescription"), DisplayName("Description")]
        public string Description
        {
            get => _description;
            set
            {
                _description = StringUtilities.Cap(value, 256);
                NotifyChange();
            }
        }

        // 015 1-byte unsigned int: number of controllers
        private byte _numControllers;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200B\u200BNum Controllers")]
        public byte NumControllers
        {
            get => _numControllers;
            set
            {
                _numControllers = value;
                NotifyChange();
            }
        }

        // 020 4-byte unsigned int: controller flags
        // bit 0: controller 1 present
        // bit 4: controller 1 has mempak
        // bit 8: controller 1 has rumblepak
        // +1..3 for controllers 2..4.
        private bool _controller1Present;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 1 Present")]
        public bool Controller1Present
        {
            get => _controller1Present;
            set
            {
                _controller1Present = value;
                NotifyChange();
            }
        }

        private bool _controller2Present;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 2 Present")]
        public bool Controller2Present
        {
            get => _controller2Present;
            set
            {
                _controller2Present = value;
                NotifyChange();
            }
        }

        private bool _controller3Present;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 3 Present")]
        public bool Controller3Present
        {
            get => _controller3Present;
            set
            {
                _controller3Present = value;
                NotifyChange();
            }
        }

        private bool _controller4Present;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200B\u200BController 4 Present")]
        public bool Controller4Present
        {
            get => _controller4Present;
            set
            {
                _controller4Present = value;
                NotifyChange();
            }
        }

        private bool _controller1MemPak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 1 MemPak")]
        public bool Controller1MemPak
        {
            get => _controller1MemPak;
            set
            {
                _controller1MemPak = value;
                NotifyChange();
            }
        }

        private bool _controller2MemPak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 2 MemPak")]
        public bool Controller2MemPak
        {
            get => _controller2MemPak;
            set
            {
                _controller2MemPak = value;
                NotifyChange();
            }
        }

        private bool _controller3MemPak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 3 MemPak")]
        public bool Controller3MemPak
        {
            get => _controller3MemPak;
            set
            {
                _controller3MemPak = value;
                NotifyChange();
            }
        }

        private bool _controller4MemPak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("\u200BController 4 MemPak")]
        public bool Controller4MemPak
        {
            get => _controller4MemPak;
            set
            {
                _controller4MemPak = value;
                NotifyChange();
            }
        }

        private bool _controller1RumblePak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 1 RumblePak")]
        public bool Controller1RumblePak
        {
            get => _controller1RumblePak;
            set
            {
                _controller1RumblePak = value;
                NotifyChange();
            }
        }

        private bool _controller2RumblePak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 2 RumblePak")]
        public bool Controller2RumblePak
        {
            get => _controller2RumblePak;
            set
            {
                _controller2RumblePak = value;
                NotifyChange();
            }
        }

        private bool _controller3RumblePak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 3 RumblePak")]
        public bool Controller3RumblePak
        {
            get => _controller3RumblePak;
            set
            {
                _controller3RumblePak = value;
                NotifyChange();
            }
        }

        private bool _controller4RumblePak;

        [CategoryAttribute("\u200B\u200BController"), DisplayName("Controller 4 RumblePak")]
        public bool Controller4RumblePak
        {
            get => _controller4RumblePak;
            set
            {
                _controller4RumblePak = value;
                NotifyChange();
            }
        }

        // 122 64-byte ASCII string: name of video plugin used when recording, directly from plugin
        private string _videoPlugin;

        [CategoryAttribute("\u200BPlugin"), DisplayName("\u200B\u200B\u200BVideo Plugin")]
        public string VideoPlugin
        {
            get => _videoPlugin;
            set
            {
                _videoPlugin = StringUtilities.Cap(value, 64);
                NotifyChange();
            }
        }

        // 162 64-byte ASCII string: name of sound plugin used when recording, directly from plugin
        private string _soundPlugin;

        [CategoryAttribute("\u200BPlugin"), DisplayName("\u200B\u200BSound Plugin")]
        public string SoundPlugin
        {
            get => _soundPlugin;
            set
            {
                _soundPlugin = StringUtilities.Cap(value, 64);
                NotifyChange();
            }
        }

        // 1A2 64-byte ASCII string: name of input plugin used when recording, directly from plugin
        private string _inputPlugin;

        [CategoryAttribute("\u200BPlugin"), DisplayName("\u200BInput Plugin")]
        public string InputPlugin
        {
            get => _inputPlugin;
            set
            {
                _inputPlugin = StringUtilities.Cap(value, 64);
                NotifyChange();
            }
        }

        // 1E2 64-byte ASCII string: name of rsp plugin used when recording, directly from plugin
        private string _rspPlugin;

        [CategoryAttribute("\u200BPlugin"), DisplayName("RSP Plugin")]
        public string RspPlugin
        {
            get => _rspPlugin;
            set
            {
                _rspPlugin = StringUtilities.Cap(value, 64);
                NotifyChange();
            }
        }

        // 000 4-byte signature: 4D 36 34 1A "M64\x1A"
        private uint _signature;

        [CategoryAttribute("Mupen"), DisplayName("\u200B\u200BSignature")]
        public uint Signature
        {
            get => _signature;
            set
            {
                _signature = value;
                NotifyChange();
            }
        }

        // 004 4-byte little-endian unsigned int: version number, should be 3
        private uint _versionNumber;

        [CategoryAttribute("Mupen"), DisplayName("\u200BVersion Number")]
        public uint VersionNumber
        {
            get => _versionNumber;
            set
            {
                _versionNumber = value;
                NotifyChange();
            }
        }

        // 008 4-byte little-endian integer: movie "uid" - identifies the movie-savestate relationship,
        // also used as the recording time in Unix epoch format
        private int _uid;

        [CategoryAttribute("Mupen"), DisplayName("UID")]
        public int Uid
        {
            get => _uid;
            set
            {
                _uid = value;
                NotifyChange();
            }
        }

        public M64Header(M64File m64File, Tabs.M64Tab gui)
        {
            _m64File = m64File;
            _gui = gui;
        }

        private void NotifyChange()
        {
            _m64File.IsModified = true;
            _gui.propertyGridM64Header.Refresh();
        }

        public void LoadBytes(byte[] bytes)
        {
            if (bytes.Length != M64Config.HeaderSize) throw new ArgumentOutOfRangeException();

            Signature = BitConverter.ToUInt32(bytes, 0x000);
            VersionNumber = BitConverter.ToUInt32(bytes, 0x004);
            Uid = BitConverter.ToInt32(bytes, 0x008);
            NumVis = BitConverter.ToInt32(bytes, 0x00C);
            NumRerecords = BitConverter.ToInt32(bytes, 0x010);
            Fps = bytes[0x014];
            NumControllers = bytes[0x015];
            ExtendedVersion = bytes[0x016];
            ExtendedFlags = bytes[0x017];
            NumInputs = BitConverter.ToInt32(bytes, 0x018);

            MovieStartType = (MovieStartTypeEnum)BitConverter.ToUInt16(bytes, 0x01C);

            uint controllerFlagsValue = BitConverter.ToUInt16(bytes, 0x020);
            Controller1Present = (controllerFlagsValue & (1 << 0)) != 0;
            Controller2Present = (controllerFlagsValue & (1 << 1)) != 0;
            Controller3Present = (controllerFlagsValue & (1 << 2)) != 0;
            Controller4Present = (controllerFlagsValue & (1 << 3)) != 0;
            Controller1MemPak = (controllerFlagsValue & (1 << 4)) != 0;
            Controller2MemPak = (controllerFlagsValue & (1 << 5)) != 0;
            Controller3MemPak = (controllerFlagsValue & (1 << 6)) != 0;
            Controller4MemPak = (controllerFlagsValue & (1 << 7)) != 0;
            Controller1RumblePak = (controllerFlagsValue & (1 << 8)) != 0;
            Controller2RumblePak = (controllerFlagsValue & (1 << 9)) != 0;
            Controller3RumblePak = (controllerFlagsValue & (1 << 10)) != 0;
            Controller4RumblePak = (controllerFlagsValue & (1 << 11)) != 0;

            AuthorshipTag = BitConverter.ToUInt32(bytes, 0x024);
            BruteforceExtraData = BitConverter.ToUInt32(bytes, 0x028);
            NumRerecordsHi = BitConverter.ToUInt32(bytes, 0x02C);
            RomName = Encoding.ASCII.GetString(bytes, 0x0C4, 32).TrimEnd('\0');
            Crc32 = BitConverter.ToUInt32(bytes, 0x0E4);
            CountryCode = BitConverter.ToUInt16(bytes, 0x0E8);
            VideoPlugin = Encoding.ASCII.GetString(bytes, 0x122, 64).TrimEnd('\0');
            SoundPlugin = Encoding.ASCII.GetString(bytes, 0x162, 64).TrimEnd('\0');
            InputPlugin = Encoding.ASCII.GetString(bytes, 0x1A2, 64).TrimEnd('\0');
            RspPlugin = Encoding.ASCII.GetString(bytes, 0x1E2, 64).TrimEnd('\0');
            Author = Encoding.UTF8.GetString(bytes, 0x222, 222).TrimEnd('\0');
            Description = Encoding.UTF8.GetString(bytes, 0x300, 256).TrimEnd('\0');

            // Verify that serialization works correctly
            if (!Enumerable.SequenceEqual(bytes, ToBytes())) throw new ArgumentOutOfRangeException();
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(TypeUtilities.GetBytes(Signature));
            bytes.AddRange(TypeUtilities.GetBytes(VersionNumber));
            bytes.AddRange(TypeUtilities.GetBytes(Uid));
            bytes.AddRange(TypeUtilities.GetBytes(NumVis));
            bytes.AddRange(TypeUtilities.GetBytes(NumRerecords));
            bytes.AddRange(TypeUtilities.GetBytes(Fps));
            bytes.AddRange(TypeUtilities.GetBytes(NumControllers));
            bytes.AddRange(TypeUtilities.GetBytes(ExtendedVersion));
            bytes.AddRange(TypeUtilities.GetBytes(ExtendedFlags));
            bytes.AddRange(TypeUtilities.GetBytes(NumInputs));
            bytes.AddRange(TypeUtilities.GetBytes((ushort)MovieStartType));
            bytes.AddRange(new byte[2]);
            bytes.AddRange(TypeUtilities.GetBytes(GetControllerFlagsValue()));
            bytes.AddRange(TypeUtilities.GetBytes(AuthorshipTag));
            bytes.AddRange(TypeUtilities.GetBytes(BruteforceExtraData));
            bytes.AddRange(TypeUtilities.GetBytes(NumRerecordsHi));
            bytes.AddRange(new byte[148]);
            bytes.AddRange(TypeUtilities.GetBytes(RomName, 32, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(Crc32));
            bytes.AddRange(TypeUtilities.GetBytes(CountryCode));
            bytes.AddRange(new byte[56]);
            bytes.AddRange(TypeUtilities.GetBytes(VideoPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(SoundPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(InputPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(RspPlugin, 64, Encoding.ASCII));
            bytes.AddRange(TypeUtilities.GetBytes(Author, 222, Encoding.UTF8));
            bytes.AddRange(TypeUtilities.GetBytes(Description, 256, Encoding.UTF8));
            if (bytes.Count != M64Config.HeaderSize) throw new ArgumentOutOfRangeException();
            return bytes.ToArray();
        }

        private uint GetControllerFlagsValue()
        {
            uint flags = 0;
            uint currentBit = 1;
            foreach (bool boolValue in GetControllerBoolList())
            {
                if (boolValue) flags |= currentBit;
                currentBit <<= 1;
            }

            return flags;
        }

        private List<bool> GetControllerBoolList()
        {
            return new List<bool>()
            {
                Controller1Present,
                Controller2Present,
                Controller3Present,
                Controller4Present,
                Controller1MemPak,
                Controller2MemPak,
                Controller3MemPak,
                Controller4MemPak,
                Controller1RumblePak,
                Controller2RumblePak,
                Controller3RumblePak,
                Controller4RumblePak,
            };
        }

        public void Clear()
        {
            NumInputs = 0;
            NumVis = 0;
            NumRerecords = 0;
            MovieStartType = MovieStartTypeEnum.FromStart;
            Fps = 0;
            ExtendedVersion = 0;
            ExtendedFlags = 0;
            RomName = null;
            CountryCode = 0;
            Crc32 = 0;
            Author = null;
            Description = null;
            NumControllers = 0;
            Controller1Present = false;
            Controller2Present = false;
            Controller3Present = false;
            Controller4Present = false;
            Controller1MemPak = false;
            Controller2MemPak = false;
            Controller3MemPak = false;
            Controller4MemPak = false;
            Controller1RumblePak = false;
            Controller2RumblePak = false;
            Controller3RumblePak = false;
            Controller4RumblePak = false;
            AuthorshipTag = 0;
            BruteforceExtraData = 0;
            NumRerecordsHi = 0;
            VideoPlugin = null;
            SoundPlugin = null;
            InputPlugin = null;
            RspPlugin = null;
            Signature = 0;
            VersionNumber = 0;
            Uid = 0;
        }
    }
}
