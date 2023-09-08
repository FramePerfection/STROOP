﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using STROOP.Utilities;

namespace STROOP.Controls
{
    public partial class ColorSelector : UserControl
    {
        public Color SelectedColor
        {
            get
            {
                return panelColorSelector.BackColor;
            }
            set
            {
                Color originalColor = SelectedColor;
                panelColorSelector.BackColor = value;
                textBoxColorSelector.SubmitText(ColorUtilities.ConvertColorToDecimal(value));
                if (value != originalColor)
                {
                    _colorChangeActions.ForEach(action => action(value));
                }
            }
        }

        private List<Action<Color>> _colorChangeActions = new List<Action<Color>>();

        public ColorSelector()
        {
            InitializeComponent();

            textBoxColorSelector.AddEnterAction(() => SubmitColorText());
            textBoxColorSelector.AddLostFocusAction(() => SubmitColorText());

            panelColorSelector.Click += (sender, e) =>
            {
                Color? newColor = ColorUtilities.GetColorFromDialog(SelectedColor);
                if (newColor.HasValue) SelectedColor = newColor.Value;
            };
        }

        public void AddColorChangeAction(Action<Color> action)
        {
            _colorChangeActions.Add(action);
        }

        private void SubmitColorText()
        {
            Color? newColor = ColorUtilities.ConvertDecimalToColor(textBoxColorSelector.Text);
            if (newColor.HasValue)
            {
                SelectedColor = newColor.Value;
            }
            else
            {
                textBoxColorSelector.Text = ColorUtilities.ConvertColorToDecimal(panelColorSelector.BackColor);
            }
        }
    }
}
