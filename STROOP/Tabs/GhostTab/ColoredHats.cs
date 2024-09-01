﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System.Linq;

namespace STROOP.Tabs.GhostTab
{
    partial class GhostTab
    {

        // <--- static utility and information --->

        private static int defaultGhostColorCounter = 1;
        private static readonly Vector4[] DefaultGhostColors = new[] {
            new Vector4(1, 0, 0, 1),
            new Vector4(1, 0.5f, 0, 1),
            new Vector4(1, 1, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 1, 1, 1),
            new Vector4(0, 0, 1, 1),
            new Vector4(1, 1, 1, 1),
            new Vector4(0.2f, 0.2f, 0.2f, 1),
        };

        Vector4 marioHatColor = new Vector4(1, 0, 0, 1);

        const uint COLORED_HATS_CODE_TARGET_ADDR = 0x80408200;
        const uint COLORED_HATS_LIGHTS_ADDR = 0x80408500;
        private static void EnableColoredHats()
        {
            using (Config.Stream.Suspend())
            {
                Config.Stream.WriteRam(File.ReadAllBytes("Resources/Hacks/gfx_generate_colored_hats.bin"), COLORED_HATS_CODE_TARGET_ADDR, EndiannessType.Big);

                uint jumpinOffset = 0xe0;

                // Displaylist nodes that point to these should generate hats dynamically instead.
                var originalDisplayListPointers = new uint[] {
                    0x40119A0,
                    0x4011A90,
                    0x4011B80,
                    0x4012030
                };
                var bank0x04Size = 0x100000 - 0x7EC20; //Rough estimate, relevant references should be in this range
                var bank0x04Location = Config.Stream.GetInt32(0x8033b410);
                var bank0x04Offset = bank0x04Location - 0x0007EC20; // 0x0007EC20 is the offset of bank 4 in vanilla Mario 64
                for (uint addr = (uint)bank0x04Location; addr < bank0x04Location + bank0x04Size; addr += 4)
                {
                    if ((Config.Stream.GetInt32(addr) & 0xFFFF0000) == 0x001B0000)
                    {
                        var foundPointer = Config.Stream.GetUInt32(addr + 0x14);
                        if (Array.IndexOf(originalDisplayListPointers, foundPointer) != -1)
                        {
                            Config.Stream.SetValue((COLORED_HATS_CODE_TARGET_ADDR + jumpinOffset), addr + 0x14);
                            Config.Stream.SetValue((ushort)0x12A, addr);
                        }
                    }
                }

                /* Old code to achieve the same thing in vanilla
                var gfxNodesPerAnimationState = new[] { // addresses at which mario's hat gfx nodes are stored
                    new [] { 0xf0a74, 0xf12f0, 0xf2990, 0xf320c, 0xf4898, 0xf5114},
                    new [] { 0xf0a8c, 0xf1308, 0xf29a8, 0xf3224, 0xf48b0, 0xf512c},
                    new [] { 0xf0aa4, 0xf1320, 0xf29c0, 0xf323c, 0xf48c8, 0xf5144},
                    new [] { 0xf0b1c, 0xf1398, 0xf2a38, 0xf32b4, 0xf4940, 0xf51bc},
                };
                var originalValues = new HashSet<uint>();

                foreach (var gfxNodeLst in gfxNodesPerAnimationState)
                {
                    foreach (var originalAddr in gfxNodeLst)
                    {
                        var addr = originalAddr + bank0x04Offset;
                        originalValues.Add(Config.Stream.GetUInt32((uint)(addr + bank0x04Offset)));
                        Config.Stream.SetValue((COLORED_HATS_CODE_TARGET_ADDR + jumpinOffset), (uint)addr);
                        Config.Stream.SetValue((ushort)0x12A, (uint)(addr - 0x14));
                    }
                }
                */

                uint jumpOutOfHeadAddr = (uint)(0x90580 + bank0x04Offset) + 0x8;
                Config.Stream.WriteRam(new byte[] { 0xB8, 0, 0, 0, 0, 0, 0, 0 }, jumpOutOfHeadAddr, EndiannessType.Big);
                //Disable low poly Mario
                Config.Stream.SetValue(0x02587fff, (uint)(0x800f470c + bank0x04Offset));
                Config.Stream.SetValue(0x7fff7fff, (uint)(0x800f6634 + bank0x04Offset));
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
                buttonGhostColor.BackColor = ColorUtilities.Vec4ToColor(ghost.hatColor);
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
                marioHatColor = ColorUtilities.ColorToVec4(dlg.Color);
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
                        g.hatColor = ColorUtilities.ColorToVec4(dlg.Color);
                    buttonGhostColor.BackColor = dlg.Color;
                }
            }
        }
    }
}
