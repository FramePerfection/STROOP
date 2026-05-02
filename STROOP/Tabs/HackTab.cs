using STROOP.Core;
using System;
using System.Collections.Generic;
using STROOP.Structs;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class HackTab : STROOPTab
    {
        List<RomHack> _hacks;

        object _listLocker = new object();

        public HackTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Hacks";

        public override void InitializeTab()
        {
            base.InitializeTab();

            _hacks = XmlConfigParser.OpenHacks(@"Config/Hacks.xml");

            // Load spawn objects codes
            foreach (var code in Config.ObjectAssociations.SpawnHacks)
                listBoxSpawn.Items.Add(code);

            // Load hack lists
            foreach (var hack in _hacks)
                checkedListBoxHacks.Items.Add(hack);

            checkedListBoxHacks.ItemCheck += _checkList_ItemCheck;
            listBoxSpawn.SelectedIndexChanged += _spawnList_SelectedIndexChanged;
            buttonHackSpawn.Click += SpawnButton_Click;
            buttonSpawnReset.Click += ResetButton_Click;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            HackConfig.SpawnHack.ClearPayload();
        }

        private void _spawnList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSpawn.SelectedItems.Count == 0)
                return;

            var selectedHack = listBoxSpawn.SelectedItem as SpawnHack;

            textBoxSpawnBehavior.Text = HexUtilities.FormatValue(selectedHack.Behavior, 8);
            textBoxSpawnGfxId.Text = HexUtilities.FormatValue(selectedHack.GfxId, 2);
            textBoxSpawnExtra.Text = HexUtilities.FormatValue(selectedHack.Extra, 2);
        }

        private void SpawnButton_Click(object sender, EventArgs e)
        {
            if (listBoxSpawn.SelectedItems.Count == 0)
                return;

            uint behavior, gfxId, extra;
            if (!ParsingUtilities.TryParseHex(textBoxSpawnBehavior.Text, out behavior))
            {
                MessageBox.Show("Could not parse behavior!");
                return;
            }

            if (!ParsingUtilities.TryParseHex(textBoxSpawnGfxId.Text, out gfxId))
            {
                MessageBox.Show("Could not parse gfxId!");
                return;
            }

            if (!ParsingUtilities.TryParseHex(textBoxSpawnExtra.Text, out extra))
            {
                MessageBox.Show("Could not parse extra!");
                return;
            }

            using (Config.Stream.Suspend())
            {
                HackConfig.SpawnHack.LoadPayload();

                Config.Stream.SetValue(behavior, HackConfig.BehaviorAddress);
                Config.Stream.SetValue((UInt16)gfxId, HackConfig.GfxIdAddress);
                Config.Stream.SetValue((UInt16)extra, HackConfig.ExtraAddress);
            }
        }

        bool suspendCheckHandler = false;

        private void _checkList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (suspendCheckHandler)
                return;
            var hack = (RomHack)checkedListBoxHacks.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
                hack.LoadPayload();
            else
                hack.ClearPayload();
        }

        public override void Update(bool active)
        {
            suspendCheckHandler = true;
            // Update rom hack statuses
            for (int i = 0; i < checkedListBoxHacks.Items.Count; i++)
            {
                var hack = (RomHack)checkedListBoxHacks.Items[i];
                hack.UpdateEnabledStatus();

                if (checkedListBoxHacks.GetItemCheckState(i) != (CheckState)hack.Status)
                    checkedListBoxHacks.SetItemCheckState(i, (CheckState)hack.Status);
            }

            suspendCheckHandler = false;
        }

        private void buttonInjectFile_Click(object sender, EventArgs e)
        {
            if (ParsingUtilities.TryParseHex(textBoxInjectFileAddress.Text, out uint address))
            {
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (!Config.Stream.WriteRam(System.IO.File.ReadAllBytes(dlg.FileName), address, EndiannessType.Big))
                        MessageBox.Show("Failed to write memory");
                }
            }
            else
                MessageBox.Show("Invalid address.");
        }

        private void buttonInjectDirect_Click(object sender, EventArgs e)
        {
            if (ParsingUtilities.TryParseHex(textBoxInjectFileAddress.Text, out uint address))
            {
                if (ParsingUtilities.ParseByteString(DialogUtilities.GetStringFromDialog("", "Enter byte data:")?.Trim() ?? "", out var bytes))
                {
                    if (bytes.Length > 0 && !Config.Stream.WriteRam(bytes, address, EndiannessType.Big))
                        MessageBox.Show("Failed to write memory");
                }
                else
                    MessageBox.Show("Invalid byte string!");
            }
            else
                MessageBox.Show("Invalid address.");
        }

        private void buttonBrowseInGameFunctionCallFile_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxInGameFunctionCall.Text = System.IO.File.ReadAllText(dlg.FileName);
        }

        private void buttonRunInGameFunctionCall_Click(object sender, EventArgs e)
        {
            var cmdAndArgs = ParsingUtilities.ParseHexList(textBoxInGameFunctionCall.Text);
            if (cmdAndArgs.Count == 0)
                MessageBox.Show("No valid function call specified.");
            else if (cmdAndArgs.Count > 5)
                MessageBox.Show("Only 4 arguments supported at this time.");
            else
            {
                var command = cmdAndArgs[0];
                cmdAndArgs.RemoveAt(0);
                InGameFunctionCall.WriteInGameFunctionCall(command, cmdAndArgs.ToArray());
            }
        }

        private void buttonBrowseLevelScriptCommandsFile_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxLevelScriptCommand.Text = System.IO.File.ReadAllText(dlg.FileName);
        }

        private void buttonRunLevelscriptCommand_Click(object sender, EventArgs e)
        {
            if (ParsingUtilities.ParseByteString(textBoxLevelScriptCommand.Text, out var bytes))
            {
                if (bytes.Length % 4 != 0)
                    MessageBox.Show("Level script commands should be multiple of 4 bytes long!");
                else
                {
                    var uints = new uint[bytes.Length / 4];
                    for (int i = 0; i < uints.Length; i++)
                        uints[i] = BitConverter.ToUInt32(bytes, i * 4);
                    InGameFunctionCall.WriteInGameLevelScriptCall(uints);
                }
            }
            else
                MessageBox.Show("Invalid byte string!");
        }
    }
}
