﻿using System;
using System.Windows.Forms;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;

namespace STROOP
{
    public abstract class FilePictureBox : PictureBox
    {
        protected uint _addressOffset;
        protected byte _mask;
        protected byte _currentValue;
        protected bool _hasUpdated;

        public FilePictureBox()
        {
        }

        protected void Initialize(uint addressOffset, byte mask)
        {
            _addressOffset = addressOffset;
            _mask = mask;
            _hasUpdated = false;

            this.Click += ClickAction;
            this.MouseEnter += (s, e) => this.Cursor = Cursors.Hand;
            this.MouseLeave += (s, e) => this.Cursor = Cursors.Arrow;
        }

        private void SetValue(bool boolValue)
        {
            if (boolValue)
                SetValue((byte)0xFF);
            else
                SetValue((byte)0x00);
        }

        private void SetValue(byte value)
        {
            byte oldByte = Config.Stream.GetByte(FileConfig.CurrentFileAddress + _addressOffset);
            byte newByte = MoreMath.ApplyValueToMaskedByte(oldByte, _mask, value);
            Config.Stream.SetValue(newByte, FileConfig.CurrentFileAddress + _addressOffset);
        }

        private byte GetValue()
        {
            byte currentByte = Config.Stream.GetByte(FileConfig.CurrentFileAddress + _addressOffset);
            byte maskedCurrentByte = (byte)(currentByte & _mask);
            return maskedCurrentByte;
        }

        protected virtual Image GetImageForValue(byte value)
        {
            return null;
        }

        protected virtual byte GetNewValueForValue(byte oldValue)
        {
            return oldValue == 0 ? _mask : (byte)0;
        }

        protected virtual void ClickAction(object sender, EventArgs e)
        {
            byte oldValue = GetValue();
            byte newValue = GetNewValueForValue(oldValue);
            SetValue(newValue);
        }

        public virtual void UpdateImage()
        {
            byte value = GetValue();
            if (_currentValue != value || !_hasUpdated)
            {
                this.Image = GetImageForValue(value);
                _currentValue = value;
                Invalidate();
            }
            _hasUpdated = true;
        }
    }
}
