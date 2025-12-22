using System;
using System.Windows.Forms;

namespace STROOP
{
    public class BetterTextbox : TextBox
    {
        public string LastSubmittedText;

        public override string Text
        {
            get { return base.Text; }
            set
            {
                if (LastSubmittedText == null)
                {
                    LastSubmittedText = value;
                }

                base.Text = value;
            }
        }

        public BetterTextbox()
        {
            AddLostFocusAction(() => LastSubmittedText = this.Text);
            AddDoubleClickAction(() => this.SelectAll());
            AddEnterAction(() => Parent.Focus());
            AddEscapeAction(() =>
            {
                this.Reset();
                this.Parent.Focus();
            });
        }

        public void SubmitText(string text)
        {
            Text = text;
            LastSubmittedText = text;
        }

        /** The same as SubmitText, but is a NOOP if the text is already submitted. */
        public void SubmitTextLoosely(string text)
        {
            if (text != LastSubmittedText) SubmitText(text);
        }

        public void Reset()
        {
            this.Text = LastSubmittedText;
        }

        public void AddEnterAction(Action enterAction)
        {
            this.KeyDown += (sender, e) =>
            {
                if (e.KeyData == Keys.Enter)
                {
                    enterAction();
                }
            };
        }

        public void AddEscapeAction(Action escapeAction)
        {
            this.KeyDown += (sender, e) =>
            {
                if (e.KeyData == Keys.Escape)
                {
                    escapeAction();
                }
            };
        }

        public void AddLostFocusAction(Action lostFocusAction)
        {
            this.LostFocus += (sender, e) => lostFocusAction();
        }

        public void AddDoubleClickAction(Action doubleClickAction)
        {
            this.DoubleClick += (sender, e) => doubleClickAction();
        }
    }
}
