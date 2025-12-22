using System;
using System.Collections.Generic;
using System.Linq;
using STROOP.Structs;
using System.Windows.Forms;
using OpenTK.Windowing.Common;
using STROOP.Utilities;
using STROOP.Controls;
using STROOP.Structs.Configurations;
using STROOP.Models;

namespace STROOP.Tabs
{
    public partial class ModelTab : STROOPTab
    {
        private ModelGraphics _modelView;

        public uint ModelObjectAddress;

        public uint ModelPointer
        {
            get
            {
                uint modelObjectAddress = ModelObjectAddress;
                return modelObjectAddress == 0 ? 0 : Config.Stream.GetUInt32(modelObjectAddress + ObjectConfig.ModelPointerOffset);
            }
        }

        private uint _previousModelPointer = 0;

        /// <summary>
        /// Mode of camera movement in the view. ManualMode indicates the camera 
        /// should fly around with user input. Otherwise a value of false indicates
        /// the camera rotates around the model (automatically).
        /// </summary>
        public bool ManualMode
        {
            get
            {
                if (_modelView == null)
                    return false;

                return _modelView.ManualMode;
            }
            set
            {
                if (_modelView != null)
                    _modelView.ManualMode = value;
            }
        }

        public ModelTab()
        {
            InitializeComponent();
            glControlModelView.Profile = ContextProfile.Compatability;
        }

        public override string GetDisplayName() => "Model";

        public override void InitializeTab()
        {
            base.InitializeTab();
            dataGridViewVertices.SelectionChanged += _dataGridViewVertices_SelectionChanged;
            dataGridViewTriangles.SelectionChanged += _dataGridViewTriangles_SelectionChanged;

            checkBoxModelLevel.Click += CheckBoxLevel_CheckedChanged;

            UpdateModelPointer();

            _modelView = new ModelGraphics(glControlModelView);
            _modelView.Load();
        }

        public bool ShowsObject(uint address) => IsActiveTab && ModelObjectAddress == address;

        public override HashSet<uint> selection => null;

        public override Action<IEnumerable<ObjectSlot>> objectSlotsClicked => objs =>
        {
            var selectedSlot = objs.Last();
            uint currentModelObjectAddress = ModelObjectAddress;
            uint newModelObjectAddress = currentModelObjectAddress == selectedSlot.CurrentObject.Address
                ? 0
                : selectedSlot.CurrentObject.Address;
            ModelObjectAddress = newModelObjectAddress;
            ManualMode = false;
        };

        private void UpdateCounts()
        {
            labelModelVertices.Text = "Vertices: " + dataGridViewVertices.Rows.Count;
            labelModelTriangles.Text = "Triangles: " + dataGridViewTriangles.Rows.Count;
        }

        private void CheckBoxLevel_CheckedChanged(object sender, EventArgs e)
        {
            SwitchLevelModel();

            textBoxModelAddress.Text = "(Level)";
            UpdateCounts();
            checkBoxModelLevel.Checked = true;
        }

        private void SwitchLevelModel()
        {
            List<TriangleDataModel> triangleStructs = TriangleUtilities.GetLevelTriangles();

            // Build vertice and triangle list from triangle set
            List<int[]> triangles = new List<int[]>();
            List<short[]> vertices = new List<short[]>();
            List<int> surfaceTypes = new List<int>();
            triangleStructs.ForEach(t =>
            {
                var vIndex = vertices.Count;
                triangles.Add(new int[] { vIndex, vIndex + 1, vIndex + 2 });
                surfaceTypes.Add(t.SurfaceType);
                vertices.Add(new short[] { t.X1, t.Y1, t.Z1 });
                vertices.Add(new short[] { t.X2, t.Y2, t.Z2 });
                vertices.Add(new short[] { t.X3, t.Y3, t.Z3 });
            });

            _modelView?.ChangeModel(vertices, triangles);

            // Update tables
            dataGridViewVertices.Rows.Clear();
            for (int i = 0; i < vertices.Count; i++)
            {
                short[] v = vertices[i];
                dataGridViewVertices.Rows.Add(i, v[0], v[1], v[2]);
            }

            dataGridViewTriangles.Rows.Clear();
            for (int i = 0; i < triangles.Count; i++)
            {
                int[] t = triangles[i];
                dataGridViewTriangles.Rows.Add(0, surfaceTypes[i], t[0], t[1], t[2]);
            }

            dataGridViewTriangles.SelectAll();

            ModelObjectAddress = _previousModelPointer = 0;
        }

        private void _dataGridViewVertices_SelectionChanged(object sender, EventArgs e)
        {
            bool[] selection = new bool[dataGridViewVertices.Rows.Count];

            foreach (DataGridViewRow row in dataGridViewVertices.SelectedRows)
            {
                selection[row.Index] = true;
            }

            _modelView.ChangeVertexSelection(selection);
        }

        private void _dataGridViewTriangles_SelectionChanged(object sender, EventArgs e)
        {
            bool[] selection = new bool[dataGridViewTriangles.Rows.Count];

            foreach (DataGridViewRow row in dataGridViewTriangles.SelectedRows)
            {
                selection[row.Index] = true;
            }

            _modelView.ChangeTriangleSelection(selection);
        }

        public List<short[]> GetVerticesFromModelPointer(ref uint modelPtr)
        {
            List<short[]> vertices = new List<short[]>();
            modelPtr += 2;
            int numberOfVertices = Math.Min(Config.Stream.GetUInt16(modelPtr), (ushort)500);
            modelPtr += 2;

            for (int i = 0; i < numberOfVertices; i++)
            {
                short x = Config.Stream.GetInt16(modelPtr);
                short y = Config.Stream.GetInt16(modelPtr + 0x02);
                short z = Config.Stream.GetInt16(modelPtr + 0x04);
                modelPtr += 0x06;
                vertices.Add(new short[3] { x, y, z });
            }

            return vertices;
        }

        public List<int[]> GetTrianglesFromContinuedModelPointer(uint contModelPtr)
        {
            var triangles = new List<int[]>();

            for (int totalVertices = 0, group = 0; totalVertices < 500 / 2; group++)
            {
                ushort type = Config.Stream.GetUInt16(contModelPtr); // Type (unused, but here anyway for doc.)

                if (contModelPtr > (0x80000000 | Config.RamSize))
                    return new List<int[]>();
                if (type == 0x41)
                    break;

                contModelPtr += 2;
                int numberOfTriangles = Config.Stream.GetUInt16(contModelPtr);
                contModelPtr += 2;

                totalVertices += numberOfTriangles;

                for (int i = 0; i < numberOfTriangles; i++)
                {
                    short v1 = Config.Stream.GetInt16(contModelPtr);
                    short v2 = Config.Stream.GetInt16(contModelPtr + 0x02);
                    short v3 = Config.Stream.GetInt16(contModelPtr + 0x04);
                    contModelPtr += 0x06;
                    triangles.Add(new int[] { v1, v2, v3, group, type });
                }
            }

            return triangles;
        }

        public void UpdateModelPointer()
        {
            if (ModelPointer == 0)
            {
                textBoxModelAddress.Text = "(None)";
                dataGridViewVertices.Rows.Clear();
                dataGridViewTriangles.Rows.Clear();
                _modelView?.ClearModel();
                return;
            }

            textBoxModelAddress.Text = HexUtilities.FormatValue(ModelPointer, 8);

            uint modelPtr = ModelPointer;
            List<short[]> vertices = GetVerticesFromModelPointer(ref modelPtr);
            List<int[]> triangles = GetTrianglesFromContinuedModelPointer(modelPtr);
            _modelView?.ChangeModel(vertices, triangles);

            // TODO: transformation

            dataGridViewVertices.Rows.Clear();
            for (int i = 0; i < vertices.Count; i++)
            {
                short[] v = vertices[i];
                dataGridViewVertices.Rows.Add(i, v[0], v[1], v[2]);
            }

            dataGridViewTriangles.Rows.Clear();
            for (int i = 0; i < triangles.Count; i++)
            {
                int[] t = triangles[i];
                dataGridViewTriangles.Rows.Add(t[3], t[4], t[0], t[1], t[2]);
            }

            dataGridViewTriangles.SelectAll();
            checkBoxModelLevel.Checked = false;
        }

        public override void Update(bool active)
        {
            if (!active)
                return;

            uint currentModelPointer = ModelPointer;
            if (currentModelPointer != _previousModelPointer)
            {
                _previousModelPointer = currentModelPointer;
                UpdateModelPointer();
            }

            UpdateCounts();

            _modelView.Control.Invalidate();
        }
    }
}
