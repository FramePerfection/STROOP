using System.Windows.Forms;

namespace STROOP
{
    public class TabControlEx : TabControl
    {
        private TabPage _currentTab;

        private TabPage _previousTab;

        public TabPage PreviousTab
        {
            get => _previousTab ?? SelectedTab;
            private set => _previousTab = value;
        }

        public TabControlEx()
        {
            SelectedIndexChanged += (sender, e) =>
            {
                PreviousTab = _currentTab ?? TabPages[0];
                _currentTab = SelectedTab;
            };
        }
    }
}
