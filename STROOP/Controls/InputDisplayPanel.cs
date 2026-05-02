using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using STROOP.Structs;
using STROOP.Extensions;
using System.Drawing.Drawing2D;
using STROOP.Structs.Configurations;

namespace STROOP
{
    public class InputDisplayPanel : Panel
    {
        List<InputImageGui> _guiList;
        Dictionary<InputDisplayTypeEnum, InputImageGui> _guiDictionary;
        InputDisplayTypeEnum _inputDisplayType;
        InputFrame _currentInputs = null;

        public InputDisplayPanel()
        {
            this.DoubleBuffered = true;
        }

        public void SetInputDisplayGui(List<InputImageGui> guiList)
        {
            _guiList = guiList;
            _guiDictionary = new Dictionary<InputDisplayTypeEnum, InputImageGui>();
            _guiList.ForEach(gui => _guiDictionary.Add(gui.InputDisplayType, gui));
            _inputDisplayType = InputDisplayTypeEnum.Classic;

            List<ToolStripMenuItem> items = _guiList.ConvertAll(
                gui => new ToolStripMenuItem(gui.InputDisplayType.ToString()));
            for (int i = 0; i < items.Count; i++)
            {
                ToolStripMenuItem item = items[i];
                InputImageGui gui = _guiList[i];
                InputDisplayTypeEnum inputDisplayType = gui.InputDisplayType;

                item.Click += (sender, e) =>
                {
                    BackColor = GetBackColor(inputDisplayType);
                    _inputDisplayType = inputDisplayType;
                    items.ForEach(item2 => item2.Checked = item2 == item);
                };

                item.Checked = inputDisplayType == _inputDisplayType;
            }

            ContextMenuStrip = new ContextMenuStrip() { };
            items.ForEach(item => ContextMenuStrip.Items.Add(item));
        }

        public void UpdateInputs()
        {
            _currentInputs = InputFrame.GetCurrent();
        }


        private Color GetBackColor(InputDisplayTypeEnum inputDisplayType)
        {
            switch (inputDisplayType)
            {
                case InputDisplayTypeEnum.Classic:
                    return SystemColors.Control;
                case InputDisplayTypeEnum.Sleek:
                    return Color.Black;
                case InputDisplayTypeEnum.Vertical:
                    return Color.Black;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float GetScale(InputDisplayTypeEnum inputDisplayType)
        {
            switch (inputDisplayType)
            {
                case InputDisplayTypeEnum.Classic:
                    return 0.0003f;
                case InputDisplayTypeEnum.Sleek:
                    return 0.0007f;
                case InputDisplayTypeEnum.Vertical:
                    return 0.0014f;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_guiDictionary == null) return;
            InputImageGui gui = _guiDictionary[_inputDisplayType];

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            Rectangle scaledRect = new Rectangle(new Point(), Size).Zoom(gui.ControllerImage.Value.Size);
            e.Graphics.DrawImage(gui.ControllerImage.Value, scaledRect);

            InputFrame inputs = _currentInputs;
            if (inputs == null) return;

            foreach (var input in Enum.GetValues(typeof(InputConfig.ButtonMask)))
                if (inputs.IsButtonPressed((InputConfig.ButtonMask)input) &&
                    gui.ButtonImages.TryGetValue((InputConfig.ButtonMask)input, out Lazy<Image> img))
                    e.Graphics.DrawImage(img.Value, scaledRect);

            float controlStickOffsetScale = GetScale(_inputDisplayType);
            float hOffset = inputs.ControlStickH * controlStickOffsetScale * scaledRect.Width;
            float vOffset = inputs.ControlStickV * controlStickOffsetScale * scaledRect.Width;

            RectangleF controlStickRectange = new RectangleF(scaledRect.X + hOffset, scaledRect.Y - vOffset, scaledRect.Width, scaledRect.Height);
            e.Graphics.DrawImage(gui.ControlStickImage.Value, controlStickRectange);
        }
    }
}
