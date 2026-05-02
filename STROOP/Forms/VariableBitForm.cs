using STROOP.Structs;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Models;
using STROOP.Variables;
using STROOP.Variables.Formatting;
using STROOP.Variables.Utilities;

namespace STROOP.Forms
{
    // TODO: fix UI inconsistencies
    public partial class VariableBitForm : Form, IUpdatableForm
    {
        private readonly string _varName;
        private readonly BindingList<ByteModel> _bytes;
        private readonly List<ByteModel> _reversedBytes;
        private readonly MemoryDescriptor _memoryDescriptor;

        private bool _hasDoneColoring = false;
        private bool _showFloatComponents = false;

        private Func<IEnumerable<uint>> _addressGetter;

        public VariableBitForm(string varName, MemoryDescriptor memoryDescriptor, bool fixAddresses)
        {
            _varName = varName;
            _memoryDescriptor = memoryDescriptor;

            if (fixAddresses)
            {
                var fixedAddresses = memoryDescriptor.GetAddressList();
                _addressGetter = () => fixedAddresses;
            }
            else
                _addressGetter = () => memoryDescriptor.GetAddressList();

            InitializeComponent();
            FormManager.AddForm(this);
            FormClosing += (sender, e) => FormManager.RemoveForm(this);

            _textBoxVarName.Text = _varName;
            _bytes = new BindingList<ByteModel>();
            for (int i = 0; i < _memoryDescriptor.ByteCount.Value; i++)
            {
                _bytes.Add(new ByteModel(_memoryDescriptor.ByteCount.Value - 1 - i, 0, _dataGridViewBits, this));
            }

            _dataGridViewBits.DataSource = _bytes;
            _dataGridViewBits.CellContentClick += (sender, e) =>
                _dataGridViewBits.CommitEdit(new DataGridViewDataErrorContexts());
            ControlUtilities.SetTableDoubleBuffered(_dataGridViewBits, true);

            _reversedBytes = _bytes.ToList();
            _reversedBytes.Reverse();

            int effectiveTableHeight = ControlUtilities.GetTableEffectiveHeight(_dataGridViewBits);
            int totalTableHeight = _dataGridViewBits.Height;
            int emptyHeight = totalTableHeight - effectiveTableHeight + 3;
            Height -= emptyHeight;

            ControlUtilities.AddCheckableContextMenuStripItems(
                this,
                new List<string>() { "Show Value", "Show Float Components" },
                new List<bool>() { false, true },
                boolValue => _showFloatComponents = boolValue,
                false);
        }

        public void UpdateForm()
        {
            if (!_hasDoneColoring)
            {
                DoColoring();
                _hasDoneColoring = true;
            }

            List<object> values = _addressGetter().Select(address => Config.Stream.GetValue(
                    _memoryDescriptor.ClrType,
                    address,
                    false,
                    _memoryDescriptor.Mask,
                    _memoryDescriptor.Shift
                )
            ).ToList();
            if (values.Count == 0) return;
            object value = values[0];
            if (!TypeUtilities.IsNumber(value))
                throw new ArgumentOutOfRangeException();

            byte[] bytes = TypeUtilities.GetBytes(value);
            if (bytes.Length != _bytes.Count)
                throw new ArgumentOutOfRangeException();

            for (int i = 0; i < _bytes.Count; i++)
            {
                _bytes[i].SetByteValue(bytes[bytes.Length - 1 - i], false);
            }

            if (_showFloatComponents && value is float floatValue)
            {
                _textBoxDecValue.Text = STROOPMath.GetFloatSign(floatValue).ToString();
                _textBoxHexValue.Text = STROOPMath.GetFloatExponent(floatValue).ToString();
                _textBoxBinaryValue.Text = STROOPMath.GetFloatMantissa(floatValue).ToString();
            }
            else
            {
                _textBoxDecValue.Text = value.ToString();
                _textBoxHexValue.Text = HexUtilities.FormatMemory(value, _memoryDescriptor.NibbleCount.Value);
                _textBoxBinaryValue.Text = String.Join(" ", _bytes.ToList().ConvertAll(b => b.GetBinary()));
            }
        }

        public void SetValueInMemory()
        {
            byte[] bytes = _reversedBytes.ConvertAll(b => b.GetByteValue()).ToArray();
            if (TypeUtilities.ConvertBytes(_memoryDescriptor.ClrType, bytes) is IConvertible validValue)
                foreach (var address in _addressGetter())
                    Config.Stream.SetValue(_memoryDescriptor.ClrType, validValue, address, false, _memoryDescriptor.Mask, _memoryDescriptor.Shift);
        }

        private void DoColoring()
        {
            // Color specially the differents parts of a float
            if (_memoryDescriptor.ClrType == typeof(float))
            {
                Color signColor = Color.LightBlue;
                Color exponentColor = Color.Pink;
                Color mantissaColor = Color.LightGreen.Lighten(0.5);

                for (int i = 0; i < 32; i++)
                {
                    Color color;
                    if (i < 1) color = signColor;
                    else if (i < 9) color = exponentColor;
                    else color = mantissaColor;

                    int rowIndex = i / 8;
                    int colIndex = i % 8 + 4;
                    DataGridViewCell cell = _dataGridViewBits.Rows[rowIndex].Cells[colIndex];
                    cell.Style.BackColor = color;
                }
            }
        }
    }
}
