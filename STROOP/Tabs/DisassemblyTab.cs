using System;
using System.Linq;
using System.Windows.Forms;
using STROOP.Utilities;
using System.Drawing;
using STROOP.Structs;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs
{
    public partial class DisassemblyTab : STROOPTab
    {
        const int NumberOfLinesAdd = 40;

        uint _lastAddress;
        int _currentLines = NumberOfLinesAdd;

        public DisassemblyTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Disassembly";

        public override void InitializeTab()
        {
            base.InitializeTab();

            richTextBoxDissasembly.LinkClicked += _output_LinkClicked;
            buttonDisGo.Click += (sender, e) => Disassemble(textBoxDisAddress.Text, _currentLines);
            buttonDisMore.Click += MoreButton_Click;
            textBoxDisAddress.TextChanged += (sender, e) =>
            {
                _currentLines = NumberOfLinesAdd;
                buttonDisGo.Text = "Go";
            };
        }

        private void _output_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            uint address;
            if (!ParsingUtilities.TryParseHex(e.LinkText, out address))
                return;

            textBoxDisAddress.Text = e.LinkText;
            StartShowDisassmbly(address, NumberOfLinesAdd);
        }

        private void MoreButton_Click(object sender, EventArgs e)
        {
            DisassemblyLines(NumberOfLinesAdd);
            _currentLines += NumberOfLinesAdd;
        }

        public void Disassemble(string strAddress, int numberOfLines = NumberOfLinesAdd)
        {
            uint newAddress;
            if (!ParsingUtilities.TryParseHex(strAddress, out newAddress))
            {
                MessageBox.Show(String.Format("Address {0} is not valid!", strAddress),
                    "Address Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentLines = NumberOfLinesAdd;
            textBoxDisAddress.Text = strAddress;
            StartShowDisassmbly(newAddress, _currentLines);
        }

        private void StartShowDisassmbly(uint newAddress, int numberOfLines)
        {
            newAddress &= ~0x03U;

            buttonDisGo.Text = "Refresh";
            buttonDisMore.Visible = true;

            richTextBoxDissasembly.Text = "";
            _lastAddress = newAddress & 0x0FFFFFFF;
            DisassemblyLines(numberOfLines);
        }

        private void DisassemblyLines(int numberOfLines)
        {
            richTextBoxDissasembly.Visible = false;
            var instructionBytes = Config.Stream.ReadRam(_lastAddress, 4 * numberOfLines, EndiannessType.Little);
            for (int i = 0; i < numberOfLines; i++, _lastAddress += 4)
            {
                // Get next bytes
                var nextBytes = new byte[4];
                Array.Copy(instructionBytes, i * 4, nextBytes, 0, 4);

                // Write Address
                richTextBoxDissasembly.AppendText(HexUtilities.FormatValue(_lastAddress | 0x80000000, 8) + ": ", Color.Blue);

                // Write byte-code
                richTextBoxDissasembly.AppendText(BitConverter.ToString(nextBytes.Reverse().ToArray()).Replace('-', ' '), Color.DarkGray);

                // Write Disassembly
                uint instruction = BitConverter.ToUInt32(nextBytes, 0);
                uint address = (uint)(((uint)_lastAddress) & 0x0FFFFFFF);
                string disassembly = "\t" + N64Disassembler.DisassembleInstruction(address, instruction);
                richTextBoxDissasembly.AppendText(disassembly, Color.Red);

                // Replace "span's"
                string searchText = "<span class='dis-reg-";
                int findIndex = richTextBoxDissasembly.Text.IndexOf(searchText);
                ;
                while (findIndex >= 0)
                {
                    richTextBoxDissasembly.ReadOnly = false;
                    richTextBoxDissasembly.Select(findIndex, richTextBoxDissasembly.Text.IndexOf('>', findIndex) - findIndex + 1);
                    richTextBoxDissasembly.SelectedText = "";
                    richTextBoxDissasembly.Select(findIndex, richTextBoxDissasembly.Text.IndexOf('<', findIndex) - findIndex);
                    richTextBoxDissasembly.SelectionColor = Color.Green;
                    richTextBoxDissasembly.Select(richTextBoxDissasembly.Text.IndexOf('<', findIndex), "</span>".Length);
                    richTextBoxDissasembly.SelectedText = "";
                    richTextBoxDissasembly.ReadOnly = true;

                    findIndex = richTextBoxDissasembly.Text.IndexOf(searchText);
                }

                searchText = "<span class='dis-address-jump'>";
                findIndex = richTextBoxDissasembly.Text.IndexOf(searchText);
                ;
                while (findIndex >= 0)
                {
                    richTextBoxDissasembly.ReadOnly = false;
                    richTextBoxDissasembly.Select(findIndex, searchText.Length);
                    richTextBoxDissasembly.SelectedText = "";
                    richTextBoxDissasembly.Select(findIndex, richTextBoxDissasembly.Text.IndexOf('<', findIndex) - findIndex);
                    richTextBoxDissasembly.SelectionColor = Color.Blue;
                    richTextBoxDissasembly.SetSelectionLink(true);
                    richTextBoxDissasembly.Select(richTextBoxDissasembly.Text.IndexOf('<', findIndex), "</span>".Length);
                    richTextBoxDissasembly.SelectedText = "";
                    richTextBoxDissasembly.ReadOnly = true;

                    findIndex = richTextBoxDissasembly.Text.IndexOf(searchText);
                }

                // Finish line (no pun intended)
                richTextBoxDissasembly.AppendText(Environment.NewLine);
            }

            richTextBoxDissasembly.Visible = true;
        }
    }
}
