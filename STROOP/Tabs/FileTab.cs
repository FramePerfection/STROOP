using System;
using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Variables.SM64MemoryLayout;
using System.Drawing;

namespace STROOP.Tabs
{
    public partial class FileTab : STROOPTab
    {
        public enum FileMode
        {
            FileA,
            FileB,
            FileC,
            FileD,
            FileASaved,
            FileBSaved,
            FileCSaved,
            FileDSaved
        };

        private enum AllCoinsMeaning
        {
            Coins100,
            Coins255,
            MaxWithoutGlitches,
            MaxWithGlitches
        };

        private enum FileCategory
        {
            Stars,
            Cannons,
            Doors,
            Coins,
            Misc
        };

        uint CurrentFileAddress => FileConfig.CurrentFileAddress;

        FileImageGui gui;

        public FileMode CurrentFileMode { get; private set; }

        private AllCoinsMeaning currentAllCoinsMeaning;

        List<FilePictureBox> _filePictureBoxList;
        List<FileTextbox> _fileTextboxList;

        int numRows = 26;

        // Keep track of each row's address and masks
        uint[] _courseStarsAddressOffsets;
        byte[] _courseStarsMasks;
        uint?[] _courseCannonAddressOffsets;
        byte?[] _courseCannonMasks;
        uint?[] _courseDoorAddressOffsets;
        byte?[] _courseDoorMasks;

        byte[] _copiedFile;

        private static readonly List<string> ALL_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.Advanced,
                VariableGroup.Custom,
            };

        private static readonly List<string> VISIBLE_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.Custom,
            };

        public FileTab()
        {
            InitializeComponent();
            watchVariablePanelFile.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
        }

        public override string GetDisplayName() => "File";

        public override void InitializeTab()
        {
            base.InitializeTab();
            gui = Config.FileImageGui;

            _filePictureBoxList = new List<FilePictureBox>();
            _fileTextboxList = new List<FileTextbox>();
            _courseStarsAddressOffsets = new uint[numRows];
            _courseStarsMasks = new byte[numRows];
            _courseCannonAddressOffsets = new uint?[numRows];
            _courseCannonMasks = new byte?[numRows];
            _courseDoorAddressOffsets = new uint?[numRows];
            _courseDoorMasks = new byte?[numRows];

            CurrentFileMode = FileMode.FileA;
            currentAllCoinsMeaning = AllCoinsMeaning.Coins100;

            radioButtonFileA.Click
                += (sender, e) => FileMode_Click(FileMode.FileA);
            radioButtonFileB.Click
                += (sender, e) => FileMode_Click(FileMode.FileB);
            radioButtonFileC.Click
                += (sender, e) => FileMode_Click(FileMode.FileC);
            radioButtonFileD.Click
                += (sender, e) => FileMode_Click(FileMode.FileD);
            radioButtonFileASaved.Click
                += (sender, e) => FileMode_Click(FileMode.FileASaved);
            radioButtonFileBSaved.Click
                += (sender, e) => FileMode_Click(FileMode.FileBSaved);
            radioButtonFileCSaved.Click
                += (sender, e) => FileMode_Click(FileMode.FileCSaved);
            radioButtonFileDSaved.Click
                += (sender, e) => FileMode_Click(FileMode.FileDSaved);


            // stars
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    string controlName = String.Format("filePictureBoxTableRow{0}Col{1}", row + 1, col + 1);
                    FileStarPictureBox fileStarPictureBox = tableLayoutPanelFile.Controls[controlName] as FileStarPictureBox;
                    if (fileStarPictureBox == null) continue;

                    uint addressOffset = GetStarAddressOffset(row);
                    byte mask = GetStarMask(row, col);
                    string missionName = TableConfig.Missions.GetMissionName(row + 1, col + 1);
                    fileStarPictureBox.Initialize(gui, addressOffset, mask, gui.PowerStarImage, gui.PowerStarBlackImage, missionName);
                    _filePictureBoxList.Add(fileStarPictureBox);

                    _courseStarsAddressOffsets[row] = addressOffset;
                    _courseStarsMasks[row] = (byte)(_courseStarsMasks[row] | mask);
                }
            }

            // course labels
            for (int row = 0; row < numRows; row++)
            {
                string controlName = String.Format("labelFileTableRow{0}", row + 1);
                FileCourseLabel fileCourseLabel = tableLayoutPanelFile.Controls[controlName] as FileCourseLabel;
                fileCourseLabel.Initialize(_courseStarsAddressOffsets[row], _courseStarsMasks[row], row + 1);
            }

            // cannons
            for (int row = 0; row < numRows; row++)
            {
                int col = 7;
                string controlName = String.Format("filePictureBoxTableRow{0}Col{1}", row + 1, col + 1);
                FileBinaryPictureBox fileCannonPictureBox = tableLayoutPanelFile.Controls[controlName] as FileBinaryPictureBox;
                if (fileCannonPictureBox == null) continue;

                uint addressOffset = GetCannonAddressOffset(row);
                byte mask = FileConfig.CannonMask;
                fileCannonPictureBox.Initialize(addressOffset, mask, gui.CannonImage, gui.CannonLidImage);
                _filePictureBoxList.Add(fileCannonPictureBox);

                _courseCannonAddressOffsets[row] = addressOffset;
                _courseCannonMasks[row] = mask;
            }

            // doors
            for (int row = 0; row < numRows; row++)
            {
                int col = 8;
                string controlName = String.Format("filePictureBoxTableRow{0}Col{1}", row + 1, col + 1);
                FileBinaryPictureBox fileBinaryPictureBox = tableLayoutPanelFile.Controls[controlName] as FileBinaryPictureBox;
                if (fileBinaryPictureBox == null) continue;

                uint addressOffset = GetDoorAddressOffset(row);
                byte mask = GetDoorMask(row);
                (Image onImage, Image offImage) = GetDoorImages(row);
                fileBinaryPictureBox.Initialize(addressOffset, mask, onImage, offImage);
                _filePictureBoxList.Add(fileBinaryPictureBox);

                _courseDoorAddressOffsets[row] = addressOffset;
                _courseDoorMasks[row] = mask;
            }

            // coin scores
            for (int row = 0; row < 15; row++)
            {
                int col = 9;
                string controlName = String.Format("textBoxTableRow{0}Col{1}", row + 1, col + 1);
                FileCoinScoreTextbox fileCoinScoreTextBox = tableLayoutPanelFile.Controls[controlName] as FileCoinScoreTextbox;
                fileCoinScoreTextBox.Initialize(FileConfig.CoinScoreOffsetStart + (uint)row);
                _fileTextboxList.Add(fileCoinScoreTextBox);
            }

            // hat location radio button pictures
            filePictureBoxHatLocationMario.Initialize(HatLocation.Mario, gui.HatOnMarioImage, gui.HatOnMarioGreyImage);
            _filePictureBoxList.Add(filePictureBoxHatLocationMario);

            filePictureBoxHatLocationKlepto.Initialize(HatLocation.SSLKlepto, gui.HatOnKleptoImage, gui.HatOnKleptoGreyImage);
            _filePictureBoxList.Add(filePictureBoxHatLocationKlepto);

            filePictureBoxHatLocationSnowman.Initialize(HatLocation.SLSnowman, gui.HatOnSnowmanImage, gui.HatOnSnowmanGreyImage);
            _filePictureBoxList.Add(filePictureBoxHatLocationSnowman);

            filePictureBoxHatLocationUkiki.Initialize(HatLocation.TTMUkiki, gui.HatOnUkikiImage, gui.HatOnUkikiGreyImage);
            _filePictureBoxList.Add(filePictureBoxHatLocationUkiki);

            filePictureBoxHatLocationSSLGround.Initialize(HatLocation.SSLGround, gui.HatOnGroundInSSLImage, gui.HatOnGroundInSSLGreyImage);
            _filePictureBoxList.Add(filePictureBoxHatLocationSSLGround);

            filePictureBoxHatLocationSLGround.Initialize(HatLocation.SLGround, gui.HatOnGroundInSLImage, gui.HatOnGroundInSLGreyImage);
            _filePictureBoxList.Add(filePictureBoxHatLocationSLGround);

            filePictureBoxHatLocationTTMGround.Initialize(HatLocation.TTMGround, gui.HatOnGroundInTTMImage, gui.HatOnGroundInTTMGreyImage);
            _filePictureBoxList.Add(filePictureBoxHatLocationTTMGround);

            // hat position textboxes
            textBoxHatLocationPositionX.Initialize(FileConfig.HatPositionXOffset);
            _fileTextboxList.Add(textBoxHatLocationPositionX);

            textBoxHatLocationPositionY.Initialize(FileConfig.HatPositionYOffset);
            _fileTextboxList.Add(textBoxHatLocationPositionY);

            textBoxHatLocationPositionZ.Initialize(FileConfig.HatPositionZOffset);
            _fileTextboxList.Add(textBoxHatLocationPositionZ);

            // miscellaneous checkbox pictures
            filePictureBoxFileStarted.Initialize(FileConfig.FileStartedOffset, FileConfig.FileStartedMask, gui.FileStartedImage, gui.FileNotStartedImage);
            _filePictureBoxList.Add(filePictureBoxFileStarted);

            filePictureBoxRedCapSwitchPressed.Initialize(FileConfig.CapSwitchPressedOffset, FileConfig.RedCapSwitchMask, gui.CapSwitchRedPressedImage, gui.CapSwitchRedUnpressedImage);
            _filePictureBoxList.Add(filePictureBoxRedCapSwitchPressed);

            filePictureBoxGreenCapSwitchPressed.Initialize(FileConfig.CapSwitchPressedOffset, FileConfig.GreenCapSwitchMask, gui.CapSwitchGreenPressedImage, gui.CapSwitchGreenUnpressedImage);
            _filePictureBoxList.Add(filePictureBoxGreenCapSwitchPressed);

            filePictureBoxBlueCapSwitchPressed.Initialize(FileConfig.CapSwitchPressedOffset, FileConfig.BlueCapSwitchMask, gui.CapSwitchBluePressedImage, gui.CapSwitchBlueUnpressedImage);
            _filePictureBoxList.Add(filePictureBoxBlueCapSwitchPressed);

            filePictureBoxKeyDoor1Opened.Initialize(FileConfig.KeyDoorOffset, FileConfig.KeyDoor1KeyMask, FileConfig.KeyDoor1OpenedMask, gui.KeyDoorOpenKeyImage, gui.KeyDoorClosedKeyImage, gui.KeyDoorOpenImage, gui.KeyDoorClosedImage);
            _filePictureBoxList.Add(filePictureBoxKeyDoor1Opened);

            filePictureBoxKeyDoor2Opened.Initialize(FileConfig.KeyDoorOffset, FileConfig.KeyDoor2KeyMask, FileConfig.KeyDoor2OpenedMask, gui.KeyDoorOpenKeyImage, gui.KeyDoorClosedKeyImage, gui.KeyDoorOpenImage, gui.KeyDoorClosedImage);
            _filePictureBoxList.Add(filePictureBoxKeyDoor2Opened);

            filePictureBoxMoatDrained.Initialize(FileConfig.MoatDrainedOffset, FileConfig.MoatDrainedMask, gui.MoatDrainedImage, gui.MoatNotDrainedImage);
            _filePictureBoxList.Add(filePictureBoxMoatDrained);

            filePictureBoxDDDMovedBack.Initialize(FileConfig.DDDMovedBackOffset, FileConfig.DDDMovedBackMask, gui.DDDPaintingMovedBackImage, gui.DDDPaintingNotMovedBackImage);
            _filePictureBoxList.Add(filePictureBoxDDDMovedBack);

            // buttons
            buttonFileSave.Click += FileSaveButton_Click;
            buttonFileErase.Click += FileEraseButton_Click;
            buttonFileCopy.Click += FileCopyButton_Click;
            buttonFilePaste.Click += FilePasteButton_Click;

            buttonAllStars.Click += (sender, e) => FileSetCategory(true, new List<FileCategory> { FileCategory.Stars });
            buttonNoStars.Click += (sender, e) => FileSetCategory(false, new List<FileCategory> { FileCategory.Stars });
            buttonAllCannons.Click += (sender, e) => FileSetCategory(true, new List<FileCategory> { FileCategory.Cannons });
            buttonNoCannons.Click += (sender, e) => FileSetCategory(false, new List<FileCategory> { FileCategory.Cannons });
            buttonAllDoors.Click += (sender, e) => FileSetCategory(true, new List<FileCategory> { FileCategory.Doors });
            buttonNoDoors.Click += (sender, e) => FileSetCategory(false, new List<FileCategory> { FileCategory.Doors });
            buttonAllCoins.Click += (sender, e) => FileSetCategory(true, new List<FileCategory> { FileCategory.Coins });
            buttonNoCoins.Click += (sender, e) => FileSetCategory(false, new List<FileCategory> { FileCategory.Coins });
            buttonEverything.Click += (sender, e) =>
                FileSetCategory(
                    true,
                    new List<FileCategory>
                    {
                        FileCategory.Stars,
                        FileCategory.Cannons,
                        FileCategory.Doors,
                        FileCategory.Coins,
                        FileCategory.Misc
                    });

            buttonNothing.Click += (sender, e) =>
                FileSetCategory(
                    false,
                    new List<FileCategory>
                    {
                        FileCategory.Stars,
                        FileCategory.Cannons,
                        FileCategory.Doors,
                        FileCategory.Coins,
                        FileCategory.Misc
                    });

            buttonFileNumStars.Click += NumStarsButton_Click;

            // everything coin score radio buttons
            radioButtonAllCoinsMeaning100Coins.Click
                += (sender, e) => { currentAllCoinsMeaning = AllCoinsMeaning.Coins100; };
            radioButtonAllCoinsMeaning255Coins.Click
                += (sender, e) => { currentAllCoinsMeaning = AllCoinsMeaning.Coins255; };
            radioButtonAllCoinsMeaningMaxWithoutGlitches.Click
                += (sender, e) => { currentAllCoinsMeaning = AllCoinsMeaning.MaxWithoutGlitches; };
            radioButtonAllCoinsMeaningMaxWithGlitches.Click
                += (sender, e) => { currentAllCoinsMeaning = AllCoinsMeaning.MaxWithGlitches; };
        }

        public void DoEverything()
        {
            FileSetCategory(
                true,
                new List<FileCategory>
                {
                    FileCategory.Stars,
                    FileCategory.Cannons,
                    FileCategory.Doors,
                    FileCategory.Coins,
                    FileCategory.Misc
                });
            short numStars = CalculateNumStars();
            Config.Stream.SetValue(numStars, MarioConfig.StructAddress + HudConfig.StarCountOffset);
            Config.Stream.SetValue(numStars, MarioConfig.StructAddress + HudConfig.StarDisplayOffset);
            Config.Stream.SetValue(numStars, MarioConfig.StructAddress + HudConfig.PreviousStarCountOffset);
        }

        public short CalculateNumStars(uint? nullableFileAddress = null)
        {
            uint fileAddress = nullableFileAddress ?? CurrentFileAddress;
            short starCount = 0;
            byte starByte;

            // go through the 25 contiguous star bytes
            for (int i = 0; i < 25; i++)
            {
                starByte = Config.Stream.GetByte(fileAddress + FileConfig.CourseStarsOffsetStart + (uint)i);
                for (int b = 0; b < 7; b++)
                {
                    starCount += (byte)((starByte >> b) & 1);
                }
            }

            // go through the 1 non-contiguous star byte (for toads and MIPS)
            starByte = Config.Stream.GetByte(fileAddress + FileConfig.ToadMIPSStarsOffset);
            for (int b = 0; b < 7; b++)
            {
                starCount += (byte)((starByte >> b) & 1);
            }

            return starCount;
        }

        private (Image onImage, Image offImage) GetDoorImages(int row)
        {
            switch (row)
            {
                case 1:
                case 18:
                    return (gui.DoorBlackImage, gui.Door1StarImage);
                case 2:
                case 3:
                    return (gui.DoorBlackImage, gui.Door3StarImage);
                case 21:
                case 22:
                case 23:
                    return (gui.StarDoorOpenImage, gui.StarDoorClosedImage);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private uint GetDoorAddressOffset(int row)
        {
            switch (row)
            {
                case 1:
                    return FileConfig.WFDoorOffset;
                case 2:
                    return FileConfig.JRBDoorOffset;
                case 3:
                    return FileConfig.CCMDoorOffset;
                case 18:
                    return FileConfig.PSSDoorOffset;
                case 21:
                    return FileConfig.BitDWDoorOffset;
                case 22:
                    return FileConfig.BitFSDoorOffset;
                case 23:
                    return FileConfig.BitSDoorOffset;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private byte GetDoorMask(int row)
        {
            switch (row)
            {
                case 1:
                    return FileConfig.WFDoorMask;
                case 2:
                    return FileConfig.JRBDoorMask;
                case 3:
                    return FileConfig.CCMDoorMask;
                case 18:
                    return FileConfig.PSSDoorMask;
                case 21:
                    return FileConfig.BitDWDoorMask;
                case 22:
                    return FileConfig.BitFSDoorMask;
                case 23:
                    return FileConfig.BitSDoorMask;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private uint GetCannonAddressOffset(int row)
        {
            if (row == 20)
                return FileConfig.WMotRCannonOffset;
            else
                return FileConfig.MainCourseCannonsOffsetStart + (uint)row;
        }

        private uint GetStarAddressOffset(int row)
        {
            switch (row)
            {
                default:
                    return FileConfig.CourseStarsOffsetStart + (uint)row;
                case 15:
                    return FileConfig.TotWCStarOffset;
                case 16:
                    return FileConfig.CotMCStarOffset;
                case 17:
                    return FileConfig.VCutMStarOffset;
                case 18:
                    return FileConfig.PSSStarsOffset;
                case 19:
                    return FileConfig.SAStarOffset;
                case 20:
                    return FileConfig.WMotRStarOffset;
                case 21:
                    return FileConfig.BitDWStarOffset;
                case 22:
                    return FileConfig.BitFSStarOffset;
                case 23:
                    return FileConfig.BitSStarOffset;
                case 24:
                case 25:
                    return FileConfig.ToadMIPSStarsOffset;
            }
        }

        private byte GetStarMask(int row, int col)
        {
            int bitOffset = row == 25 ? 3 : 0;
            return (byte)Math.Pow(2, col + bitOffset);
        }

        private void NumStarsButton_Click(object sender, EventArgs e)
        {
            short numStars = CalculateNumStars();
            Config.Stream.SetValue(numStars, MarioConfig.StructAddress + HudConfig.StarCountOffset);
            Config.Stream.SetValue(numStars, MarioConfig.StructAddress + HudConfig.StarDisplayOffset);
            Config.Stream.SetValue(numStars, MarioConfig.StructAddress + HudConfig.PreviousStarCountOffset);
        }

        public ushort GetChecksum(uint? nullableFileAddress = null)
        {
            uint fileAddress = nullableFileAddress ?? CurrentFileAddress;
            ushort checksum = (ushort)(FileConfig.ChecksumConstantValue % 256 + FileConfig.ChecksumConstantValue / 256);
            for (uint i = 0; i < FileConfig.FileStructSize - 4; i++)
            {
                byte b = Config.Stream.GetByte(fileAddress + i);
                checksum += b;
            }

            return checksum;
        }

        private void FileSaveButton_Click(object sender, EventArgs e)
        {
            // Get the corresponding unsaved file struct address
            uint nonSavedAddress = GetNonSavedFileAddress();

            // Set the checksum constant
            Config.Stream.SetValue(FileConfig.ChecksumConstantValue, nonSavedAddress + FileConfig.ChecksumConstantOffset);

            // Sum up all bytes to calculate the checksum
            ushort checksum = (ushort)(FileConfig.ChecksumConstantValue % 256 + FileConfig.ChecksumConstantValue / 256);
            for (uint i = 0; i < FileConfig.FileStructSize - 4; i++)
            {
                byte b = Config.Stream.GetByte(nonSavedAddress + i);
                checksum += b;
            }

            // Set the checksum
            Config.Stream.SetValue(checksum, nonSavedAddress + FileConfig.ChecksumOffset);

            // Copy all values from the unsaved struct to the saved struct
            uint savedAddress = nonSavedAddress + FileConfig.FileStructSize;
            for (uint i = 0; i < FileConfig.FileStructSize - 4; i++)
            {
                byte b = Config.Stream.GetByte(nonSavedAddress + i);
                Config.Stream.SetValue(b, savedAddress + i);
            }

            Config.Stream.SetValue(FileConfig.ChecksumConstantValue, savedAddress + FileConfig.ChecksumConstantOffset);
            Config.Stream.SetValue(checksum, savedAddress + FileConfig.ChecksumOffset);
        }

        private void FileEraseButton_Click(object sender, EventArgs e)
        {
            // Get the corresponding unsaved and saved file struct address
            uint nonSavedAddress = GetNonSavedFileAddress();
            uint savedAddress = nonSavedAddress + FileConfig.FileStructSize;

            // Get checksum value
            ushort checksum = (ushort)(FileConfig.ChecksumConstantValue % 256 + FileConfig.ChecksumConstantValue / 256);

            // Set the checksum constant and checksum (in both unsaved and saved)
            Config.Stream.SetValue(FileConfig.ChecksumConstantValue, nonSavedAddress + FileConfig.ChecksumConstantOffset);
            Config.Stream.SetValue(FileConfig.ChecksumConstantValue, savedAddress + FileConfig.ChecksumConstantOffset);
            Config.Stream.SetValue(checksum, nonSavedAddress + FileConfig.ChecksumOffset);
            Config.Stream.SetValue(checksum, savedAddress + FileConfig.ChecksumOffset);

            // Set all bytes to 0 (in both unsaved and saved)
            for (uint i = 0; i < FileConfig.FileStructSize - 4; i++)
            {
                Config.Stream.SetValue((byte)0, nonSavedAddress + i);
                Config.Stream.SetValue((byte)0, savedAddress + i);
            }
        }

        private void FileCopyButton_Click(object sender, EventArgs e)
        {
            uint addressToCopy = checkBoxInGameCopyPaste.Checked ? GetNonSavedFileAddress() : getFileAddress();
            _copiedFile = GetBufferedBytes(addressToCopy);
        }

        private void FilePasteButton_Click(object sender, EventArgs e)
        {
            if (_copiedFile == null) return;

            uint nonSavedAddress = GetNonSavedFileAddress();
            List<uint> addressesToPaste = checkBoxInGameCopyPaste.Checked ? new List<uint> { nonSavedAddress, nonSavedAddress + FileConfig.FileStructSize } : new List<uint> { CurrentFileAddress };

            foreach (uint addressToPaste in addressesToPaste)
            {
                SetBufferedBytes(_copiedFile, addressToPaste);
            }
        }

        public byte[] GetBufferedBytes(uint? nullableFileAddress = null)
        {
            uint fileAddress = nullableFileAddress ?? CurrentFileAddress;
            byte[] bufferedBytes = new byte[FileConfig.FileStructSize];
            for (int i = 0; i < FileConfig.FileStructSize; i++)
            {
                bufferedBytes[i] = Config.Stream.GetByte(fileAddress + (uint)i);
            }

            return bufferedBytes;
        }

        public void SetBufferedBytes(byte[] bufferedBytes, uint? nullableFileAddress = null)
        {
            uint fileAddress = nullableFileAddress ?? CurrentFileAddress;
            for (int i = 0; i < FileConfig.FileStructSize; i++)
            {
                Config.Stream.SetValue(bufferedBytes[i], fileAddress + (uint)i);
            }
        }

        private byte GetCoinScoreForCourse(int courseIndex)
        {
            switch (currentAllCoinsMeaning)
            {
                case AllCoinsMeaning.Coins100:
                    return 100;
                case AllCoinsMeaning.Coins255:
                    return 255;
                case AllCoinsMeaning.MaxWithoutGlitches:
                    return (byte)TableConfig.CourseData.GetMaxCoinsWithoutGlitches(courseIndex);
                case AllCoinsMeaning.MaxWithGlitches:
                    return (byte)TableConfig.CourseData.GetMaxCoinsWithGlitches(courseIndex);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FileSetCategory(bool setOn, List<FileCategory> fileCategories)
        {
            byte[] bufferedBytes = GetBufferedBytes();

            Action<uint?, byte?, bool?> setValues = (uint? addressOffset, byte? mask, bool? newVal) =>
            {
                if (addressOffset == null || mask == null || newVal == null) return;
                byte oldByte = bufferedBytes[(uint)addressOffset];
                byte newByte = MoreMath.ApplyValueToMaskedByte(oldByte, (byte)mask, (bool)newVal);
                bufferedBytes[(uint)addressOffset] = newByte;
            };

            for (int i = 0; i < numRows; i++)
            {
                if (fileCategories.Contains(FileCategory.Stars))
                {
                    setValues(_courseStarsAddressOffsets[i], _courseStarsMasks[i], setOn);
                }

                if (fileCategories.Contains(FileCategory.Cannons))
                {
                    setValues(_courseCannonAddressOffsets[i], _courseCannonMasks[i], setOn);
                }

                if (fileCategories.Contains(FileCategory.Doors))
                {
                    setValues(_courseDoorAddressOffsets[i], _courseDoorMasks[i], setOn);
                }
            }

            if (fileCategories.Contains(FileCategory.Coins))
            {
                for (int i = 0; i < 15; i++)
                {
                    bufferedBytes[FileConfig.CoinScoreOffsetStart + (uint)i] = setOn ? GetCoinScoreForCourse(i + 1) : (byte)0;
                }
            }

            if (fileCategories.Contains(FileCategory.Doors))
            {
                setValues(FileConfig.KeyDoorOffset, FileConfig.KeyDoor1KeyMask, false);
                setValues(FileConfig.KeyDoorOffset, FileConfig.KeyDoor1OpenedMask, setOn);
                setValues(FileConfig.KeyDoorOffset, FileConfig.KeyDoor2KeyMask, false);
                setValues(FileConfig.KeyDoorOffset, FileConfig.KeyDoor2OpenedMask, setOn);
            }

            if (fileCategories.Contains(FileCategory.Misc))
            {
                setValues(FileConfig.FileStartedOffset, FileConfig.FileStartedMask, setOn);
                setValues(FileConfig.CapSwitchPressedOffset, FileConfig.RedCapSwitchMask, setOn);
                setValues(FileConfig.CapSwitchPressedOffset, FileConfig.GreenCapSwitchMask, setOn);
                setValues(FileConfig.CapSwitchPressedOffset, FileConfig.BlueCapSwitchMask, setOn);
                setValues(FileConfig.MoatDrainedOffset, FileConfig.MoatDrainedMask, setOn);
                setValues(FileConfig.DDDMovedBackOffset, FileConfig.DDDMovedBackMask, setOn);
                setValues(FileConfig.HatLocationModeOffset, FileConfig.HatLocationModeMask, false);
            }

            SetBufferedBytes(bufferedBytes);
        }

        private uint GetNonSavedFileAddress(FileMode? nullableMode = null)
        {
            FileMode mode = nullableMode ?? CurrentFileMode;
            switch (mode)
            {
                case FileMode.FileA:
                case FileMode.FileASaved:
                    return FileConfig.FileAAddress;
                case FileMode.FileB:
                case FileMode.FileBSaved:
                    return FileConfig.FileBAddress;
                case FileMode.FileC:
                case FileMode.FileCSaved:
                    return FileConfig.FileCAddress;
                case FileMode.FileD:
                case FileMode.FileDSaved:
                    return FileConfig.FileDAddress;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public uint getFileAddress(FileMode? nullableMode = null)
        {
            FileMode mode = nullableMode ?? CurrentFileMode;
            switch (mode)
            {
                case FileMode.FileA:
                    return FileConfig.FileAAddress;
                case FileMode.FileB:
                    return FileConfig.FileBAddress;
                case FileMode.FileC:
                    return FileConfig.FileCAddress;
                case FileMode.FileD:
                    return FileConfig.FileDAddress;
                case FileMode.FileASaved:
                    return FileConfig.FileASavedAddress;
                case FileMode.FileBSaved:
                    return FileConfig.FileBSavedAddress;
                case FileMode.FileCSaved:
                    return FileConfig.FileCSavedAddress;
                case FileMode.FileDSaved:
                    return FileConfig.FileDSavedAddress;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public uint GetInGameFileAddress()
        {
            short inGameFile = Config.Stream.GetInt16(MiscConfig.CurrentFileAddress);
            switch (inGameFile)
            {
                case 1:
                    return FileConfig.FileAAddress;
                case 2:
                    return FileConfig.FileBAddress;
                case 3:
                    return FileConfig.FileCAddress;
                case 4:
                    return FileConfig.FileDAddress;
                default:
                    return FileConfig.FileAAddress;
            }
        }

        private void FileMode_Click(FileMode mode)
        {
            CurrentFileMode = mode;
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            short currentNumStars = CalculateNumStars();
            buttonFileNumStars.Text = string.Format("Update HUD\r\nto " + (currentNumStars == 1 ? currentNumStars + " Star" : currentNumStars + " Stars"));

            foreach (FilePictureBox filePictureBox in _filePictureBoxList)
            {
                filePictureBox.UpdateImage();
            }

            foreach (FileTextbox fileTextbox in _fileTextboxList)
            {
                fileTextbox.UpdateText();
            }

            base.Update(updateView);
        }
    }
}
