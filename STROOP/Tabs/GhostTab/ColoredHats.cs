using System;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Core;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.GhostTab
{
    partial class GhostTab
    {
        // <--- static utility and information --->

        const uint COLORED_HATS_CODE_OFFSET = 0x8200u;

        private static int defaultGhostColorCounter = 1;

        private static readonly Vector4[] DefaultGhostColors = new[]
        {
            new Vector4(1, 0, 0, 1),
            new Vector4(1, 0.5f, 0, 1),
            new Vector4(1, 1, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 1, 1, 1),
            new Vector4(0, 0, 1, 1),
            new Vector4(1, 1, 1, 1),
            new Vector4(0.2f, 0.2f, 0.2f, 1),
        };

        const uint VANILLA_BANK_04_OFFSET_US = 0x0007EC20;
        const uint VANILLA_BANK_04_OFFSET_JP = 0x0007BDC0;
        const uint S_SEGMENT_TABLE_OFFSET_JP = 0x8033a090;
        const uint S_SEGMENT_TABLE_OFFSET_US = 0x8033b400;

        Vector4 marioHatColor = new Vector4(1, 0, 0, 1);

        uint COLORED_HATS_LIGHTS_ADDR => GHOST_REGION_BASE + 0x8300u;

        private void EnableColoredHats()
        {
            using (Config.Stream.Suspend())
            {
                // Displaylist nodes that point to these should generate hats dynamically instead.
                var originalDisplayListPointers = new uint[]
                {
                    0x40119A0,
                    0x4011A90,
                    0x4011B80,
                    0x4012030
                };

                var vanillaOffset = RomVersionConfig.Version == RomVersion.JP
                    ? VANILLA_BANK_04_OFFSET_JP
                    : VANILLA_BANK_04_OFFSET_US;

                var bank0x04Size = 0x100000 - vanillaOffset; //Rough estimate, relevant references should be in this range
                var segmentTableOffset = RomVersionConfig.Version == RomVersion.JP ? S_SEGMENT_TABLE_OFFSET_JP : S_SEGMENT_TABLE_OFFSET_US;
                var bank0x04Location = Config.Stream.GetInt32(segmentTableOffset + 0x10);
                var bank0x04Offset = bank0x04Location - vanillaOffset;

                for (uint addr = (uint)bank0x04Location; addr < bank0x04Location + bank0x04Size; addr += 4)
                {
                    if ((Config.Stream.GetInt32(addr) & 0xFFFF0000) == 0x001B0000)
                    {
                        var foundPointer = Config.Stream.GetUInt32(addr + 0x14);
                        if (Array.IndexOf(originalDisplayListPointers, foundPointer) != -1)
                        {
                            Config.Stream.SetValue(GHOST_REGION_BASE + COLORED_HATS_CODE_OFFSET, addr + 0x14);
                            Config.Stream.SetValue((ushort)0x12A, addr);
                        }
                    }
                }

                var findOutWhatToCallThis = RomVersionConfig.Version == RomVersion.JP ? 0x8D720 : 0x90580;
                uint jumpOutOfHeadAddr = (uint)(findOutWhatToCallThis + bank0x04Offset) + 0x8;
                Config.Stream.WriteRam(new byte[] { 0xB8, 0, 0, 0, 0, 0, 0, 0 }, jumpOutOfHeadAddr, EndiannessType.Big);

                var offsetA = 0xf470c - bank0x04Location;

                // Disable low poly Mario by finding the LOD threshold values and replacing them with the maximum distance (0x7fff) as appropriate
                for (uint addr = (uint)bank0x04Location; addr < bank0x04Location + bank0x04Size; addr += 4)
                {
                    var value = Config.Stream.GetUInt32(addr);
                    if (value == 0x02580640)
                        Config.Stream.SetValue(0x02587fff, addr);
                    else if (value == 0x06407fff)
                        Config.Stream.SetValue(0x7fff7fff, addr);
                }
            }
        }

        private static byte[] ColorToLights(Vector4 color)
        {
            var c2 = color * 0.5f;
            var R1 = (byte)Math.Max(0, Math.Min(255, (color.X * 255)));
            var G1 = (byte)Math.Max(0, Math.Min(255, (color.Y * 255)));
            var B1 = (byte)Math.Max(0, Math.Min(255, (color.Z * 255)));

            var R2 = (byte)Math.Max(0, Math.Min(255, (c2.X * 255)));
            var G2 = (byte)Math.Max(0, Math.Min(255, (c2.Y * 255)));
            var B2 = (byte)Math.Max(0, Math.Min(255, (c2.Z * 255)));

            return new byte[] { R2, G2, B2, 0x00, R2, G2, B2, 0x00, R1, G1, B1, 0x00, R1, G1, B1, 0x00, 0x28, 0x28, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        // <--- instance methods --->

        private void SetColorForNewGhost(Ghost newGhost)
        {
            newGhost.hatColor = DefaultGhostColors[defaultGhostColorCounter];
            defaultGhostColorCounter = (defaultGhostColorCounter + 1) % DefaultGhostColors.Length;
        }

        private void WriteGhostColorToStream(int ghostIndex, Ghost[] ghosts)
        {
            var color = ghostIndex < ghosts.Length ? ghosts[ghostIndex].hatColor : new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
            Config.Stream.WriteRam(ColorToLights(color), (UIntPtr)(COLORED_HATS_LIGHTS_ADDR + (ghostIndex + 1) * 0x20), EndiannessType.Big);
        }

        private void WriteMarioColorToStream()
            => Config.Stream.WriteRam(ColorToLights(marioHatColor), (UIntPtr)(COLORED_HATS_LIGHTS_ADDR), EndiannessType.Big);

        // <--- controls --->

        bool suspendSelectedIndexChanged = false;

        private void listBoxGhosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suspendSelectedIndexChanged)
                return;

            Ghost ghost = listBoxGhosts.SelectedItem as Ghost;
            if (ghost == null)
                buttonGhostColor.Enabled = false;
            else
            {
                buttonGhostColor.BackColor = OpenTKUtilities.Vec4ToColor(ghost.hatColor);
                buttonGhostColor.Enabled = true;
            }

            UpdateGhostInfoControls();
        }

        private void buttonMarioColor_Click(object sender, EventArgs e)
        {
            var dlg = new ColorDialog();
            dlg.Color = System.Drawing.Color.Red;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                marioHatColor = OpenTKUtilities.ColorToVec4(dlg.Color);
                buttonMarioColor.BackColor = dlg.Color;
            }
        }

        private void buttonGhostColor_Click(object sender, EventArgs e)
        {
            if (selectedGhost != null)
            {
                var dlg = new ColorDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var g in GetSelectedGhosts())
                        g.hatColor = OpenTKUtilities.ColorToVec4(dlg.Color);
                    buttonGhostColor.BackColor = dlg.Color;
                }
            }
        }
    }
}
