using STROOP.Core;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using System.Xml.Linq;

namespace STROOP
{
    static class Program
    {
        public const string CONFIG_FILE_NAME = "Config/Config.xml";
        public static XDocument config { get; private set; }

        public static bool IsVisualStudioHostProcess()
        {
            return (System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToUpper() == "DEVENV");
        }

        static ScriptParser _scriptParser;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // This is necessary to keep the scale of controls in check after migrating from .NET Framework WinForms to .NET8 WinForms LOL
            Application.SetDefaultFont(new Font(new FontFamily("Microsoft Sans Serif"), 8.25F));

            typeof(System.Globalization.CultureInfo).GetField("s_userDefaultCulture", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, System.Globalization.CultureInfo.InvariantCulture);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoadingHandler.ShowLoadingForm();


            StroopMainForm mainForm;
            Initialize(out mainForm);

            LoadingHandler.CloseForm();
            Application.Run(mainForm);
        }

        static void Initialize(out StroopMainForm mainForm)
        {
            StroopMainForm tmpMainForm = null;
            LoadingHandler.LoadingForm.RunLoadingTasks(
                ("Loading Main Configuration",
                    () =>
                    {
                        config = XmlConfigParser.OpenConfig(@"Config/Config.xml");
                        SavedSettingsConfig.Load(@"Config/SavedSettings.xml");
                    }
            ),
                ("Loading Object Associations",
                    () => Config.ObjectAssociations = XmlConfigParser.OpenObjectAssoc(@"Config/ObjectAssociations.xml")
                ),
                ("Loading File Image Associations",
                    () => XmlConfigParser.OpenFileImageAssoc(@"Config/FileImageAssociations.xml", Config.FileImageGui)
                ),
                ("Loading Map Associations",
                    () => Tabs.MapTab.MapTab.MapAssociations = XmlConfigParser.OpenMapAssoc(@"Config/MapAssociations.xml")
                ),
                ("Loading Scripts",
                    () => _scriptParser = XmlConfigParser.OpenScripts(@"Config/Scripts.xml")
                ),
                ("Opening Tables",
                    () =>
                    {
                        TableConfig.MarioActions = XmlConfigParser.OpenActionTable(@"Config/MarioActions.xml");
                        TableConfig.MarioAnimations = XmlConfigParser.OpenAnimationTable(@"Config/MarioAnimations.xml");
                        TableConfig.TriangleInfo = XmlConfigParser.OpenTriangleInfoTable(@"Config/TriangleInfo.xml");
                        TableConfig.PendulumSwings = XmlConfigParser.OpenPendulumSwingTable(@"Config/PendulumSwings.xml");
                        TableConfig.RacingPenguinWaypoints = XmlConfigParser.OpenWaypointTable(@"Config/RacingPenguinWaypoints.xml");
                        TableConfig.KoopaTheQuick1Waypoints = XmlConfigParser.OpenWaypointTable(@"Config/KoopaTheQuick1Waypoints.xml");
                        TableConfig.KoopaTheQuick2Waypoints = XmlConfigParser.OpenWaypointTable(@"Config/KoopaTheQuick2Waypoints.xml");
                        TableConfig.TtmBowlingBallPoints = XmlConfigParser.OpenPointTable(@"Config/TtmBowlingBallPoints.xml");
                        TableConfig.Missions = XmlConfigParser.OpenMissionTable(@"Config/Missions.xml");
                        TableConfig.CourseData = XmlConfigParser.OpenCourseDataTable(@"Config/CourseData.xml");
                        TableConfig.FlyGuyData = new FlyGuyDataTable();
                        TableConfig.WdwRotatingPlatformTable = new ObjectAngleTable(1120);
                        TableConfig.ElevatorAxleTable = new ObjectAngleTable(400);
                    }
            ),
                ("Initialize Main Form",
                    () => tmpMainForm = new StroopMainForm(true)
                ),
                ("Creating Process Stream",
                    () => ProcessStream.Instance = new ProcessStream(tmpMainForm.OnUpdate)
                )
            );
            mainForm = tmpMainForm;
        }
    }
}
