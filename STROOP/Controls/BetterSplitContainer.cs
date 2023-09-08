﻿using System;
using System.Windows.Forms;

namespace STROOP
{
    public class BetterSplitContainer : SplitContainer
    {
        private int? _initialSplitterDistance;

        public BetterSplitContainer()
        {
            _initialSplitterDistance = null;

            MouseDown += splitCont_MouseDown;
            MouseUp += splitCont_MouseUp;
            MouseMove += splitCont_MouseMove;
            DoubleClick += splitCont_DoubleClick;
        }

        //assign this to the SplitContainer's MouseDown event
        private void splitCont_DoubleClick(object sender, EventArgs e)
        {
            if (_initialSplitterDistance.HasValue)
            {
                SplitterDistance = _initialSplitterDistance.Value;
            }
        }

        //assign this to the SplitContainer's MouseDown event
        private void splitCont_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_initialSplitterDistance.HasValue)
            {
                _initialSplitterDistance = SplitterDistance;
            }

            // This disables the normal move behavior
            ((SplitContainer)sender).IsSplitterFixed = true;
        }

        //assign this to the SplitContainer's MouseUp event
        private void splitCont_MouseUp(object sender, MouseEventArgs e)
        {
            // This allows the splitter to be moved normally again
            ((SplitContainer)sender).IsSplitterFixed = false;
        }

        //assign this to the SplitContainer's MouseMove event
        private void splitCont_MouseMove(object sender, MouseEventArgs e)
        {
            // Check to make sure the splitter won't be updated by the
            // normal move behavior also
            if (((SplitContainer)sender).IsSplitterFixed)
            {
                // Make sure that the button used to move the splitter
                // is the left mouse button
                if (e.Button.Equals(MouseButtons.Left))
                {
                    // Checks to see if the splitter is aligned Vertically
                    if (((SplitContainer)sender).Orientation.Equals(Orientation.Vertical))
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.X > 0 && e.X < ((SplitContainer)sender).Width)
                        {
                            // Move the splitter & force a visual refresh
                            ((SplitContainer)sender).SplitterDistance = e.X;
                            ((SplitContainer)sender).Refresh();
                        }
                    }
                    // If it isn't aligned vertically then it must be
                    // horizontal
                    else
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.Y > 0 && e.Y < ((SplitContainer)sender).Height)
                        {
                            // Move the splitter & force a visual refresh
                            ((SplitContainer)sender).SplitterDistance = e.Y;
                            ((SplitContainer)sender).Refresh();
                        }
                    }
                }
                // If a button other than left is pressed or no button
                // at all
                else
                {
                    // This allows the splitter to be moved normally again
                    ((SplitContainer)sender).IsSplitterFixed = false;
                }
            }
        }
    }
}
