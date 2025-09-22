using STROOP.Core;
using STROOP.Structs;
using STROOP.Variables.SM64MemoryLayout;
using System;
using System.Windows.Forms;

namespace STROOP
{
    partial class StroopMainForm
    {
        public static void UpdateRomVersion(ComboBox comboBoxRomVersion)
        {
            RomVersionSelection romVersionSelection = (RomVersionSelection)comboBoxRomVersion.SelectedItem;
            switch (romVersionSelection)
            {
                case RomVersionSelection.AUTO:
                case RomVersionSelection.AUTO_US:
                case RomVersionSelection.AUTO_JP:
                case RomVersionSelection.AUTO_SH:
                case RomVersionSelection.AUTO_EU:
                    RomVersion? autoRomVersionNullable = RomVersionConfig.GetRomVersionUsingTell();
                    if (!autoRomVersionNullable.HasValue) return;
                    RomVersion autoRomVersion = autoRomVersionNullable.Value;
                    RomVersionConfig.Version = autoRomVersion;
                    if (!comboBoxRomVersion.DroppedDown)
                    {
                        switch (autoRomVersion)
                        {
                            case RomVersion.US:
                                comboBoxRomVersion.SelectedItem = RomVersionSelection.AUTO_US;
                                break;
                            case RomVersion.JP:
                                comboBoxRomVersion.SelectedItem = RomVersionSelection.AUTO_JP;
                                break;
                            case RomVersion.SH:
                                comboBoxRomVersion.SelectedItem = RomVersionSelection.AUTO_SH;
                                break;
                            case RomVersion.EU:
                                comboBoxRomVersion.SelectedItem = RomVersionSelection.AUTO_EU;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    break;
                case RomVersionSelection.US:
                    RomVersionConfig.Version = RomVersion.US;
                    break;
                case RomVersionSelection.JP:
                    RomVersionConfig.Version = RomVersion.JP;
                    break;
                case RomVersionSelection.SH:
                    RomVersionConfig.Version = RomVersion.SH;
                    break;
                case RomVersionSelection.EU:
                    RomVersionConfig.Version = RomVersion.EU;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
