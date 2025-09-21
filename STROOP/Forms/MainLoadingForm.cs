using STROOP.Structs;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace STROOP.Forms
{
    public partial class MainLoadingForm : Form
    {
        int _maxStatus;
        Point lastclickedpoint;

        public MainLoadingForm()
        {
            InitializeComponent();
            textBoxLoadingHelpfulHint.Text = HelpfulHintUtilities.GetRandomHelpfulHint();
            ControlUtilities.AddContextMenuStripFunctions(
                textBoxLoadingHelpfulHint,
                new List<string>() { "Show All Helpful Hints" },
                new List<Action>() { () => HelpfulHintUtilities.ShowAllHelpfulHints() });
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            progressBarLoading.Maximum = _maxStatus;
            CenterToScreen();
        }

        public void RunLoadingTasks(params (string name, Action task)[] tasks)
        {
            _maxStatus = tasks.Length;
            for (int i = 0; i < tasks.Length; i++)
            {
                var taskNumber = i;
                Invoke(new Action(() =>
                {
                    progressBarLoading.Maximum = _maxStatus;
                    progressBarLoading.Value = taskNumber;
                    labelLoadingStatus.Text = $"{tasks[taskNumber].name} [{(taskNumber + 1)} / {_maxStatus}]";
                }));
                tasks[i].task();
            }

            Invoke(new Action(() => labelLoadingStatus.Text = "Finishing"));
        }

        private void MainLoadingForm_MouseDown(object sender, MouseEventArgs e)
        {
            lastclickedpoint.X = e.X;
            lastclickedpoint.Y = e.Y;
        }

        private void MainLoadingForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastclickedpoint.X;
                this.Top += e.Y - lastclickedpoint.Y;
            }
        }

        private void progressBarLoading_MouseDown(object sender, MouseEventArgs e)
        {
            // This has 4 references (each control)
            lastclickedpoint = new Point(e.X, e.Y);
        }

        private void progressBarLoading_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastclickedpoint.X;
                this.Top += e.Y - lastclickedpoint.Y;
            }
        }
    }
}
