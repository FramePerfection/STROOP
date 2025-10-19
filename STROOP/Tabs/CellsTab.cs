using STROOP.Core;
using STROOP.Core.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System.Collections.Generic;
using System.Windows.Forms;
using STROOP.Structs;
using STROOP.Variables;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class CellsTab : STROOPTab
    {
        static IEnumerable<uint> GetTriangleAddresses()
        {
            var addr = AccessScope<StroopMainForm>.content.GetTab<CellsTab>().TriangleAddress;
            return addr != 0 ? new List<uint> { addr } : VariableUtilities.BaseAddressListEmpty;
        }

        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            VariableUtilities.baseAddressGetters["CellsTriangle"] = GetTriangleAddresses;
            VariableUtilities.baseAddressGetters["CellsTriangleExertionForceTable"] = () =>
                GetTriangleAddresses().ConvertAll(triangleAddress =>
                {
                    uint exertionForceIndex = Config.Stream.GetByte(triangleAddress + TriangleOffsetsConfig.ExertionForceIndex);
                    return TriangleConfig.ExertionForceTableAddress + 2 * exertionForceIndex;
                });
        }

        public uint TriangleAddress;

        public CellsTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Cells";

        public override void InitializeTab()
        {
            base.InitializeTab();
            TriangleAddress = 0;

            buttonCellsBuildTree.Click += (sender, e) => BuildTree();
            treeViewCells.AfterSelect += (sender, e) => SetTriangleAddress();
        }

        private void SetTriangleAddress()
        {
            object tag = treeViewCells.SelectedNode.Tag;
            TriangleAddress = tag is uint uintTag ? uintTag : 0;
        }

        private void BuildTree()
        {
            treeViewCells.BeginUpdate();
            treeViewCells.Nodes.Clear();
            treeViewCells.Nodes.Add(GetTreeNodeForPartition(true));
            treeViewCells.Nodes.Add(GetTreeNodeForPartition(false));
            treeViewCells.EndUpdate();
        }

        private TreeNode GetTreeNodeForPartition(bool staticPartition)
        {
            uint partitionAddress = staticPartition ? TriangleConfig.StaticTrianglePartitionAddress : TriangleConfig.DynamicTrianglePartitionAddress;

            List<TreeNode> nodes = new List<TreeNode>();
            int sum = 0;
            for (int z = 0; z < 16; z++)
            {
                TreeNode subNode = GetTreeNodeForZ(partitionAddress, z);
                nodes.Add(subNode);
                sum += (int)subNode.Tag;
            }

            string name = (staticPartition ? "Static Triangles" : "Dynamic Triangles") + " [" + sum + "]";
            TreeNode node = new TreeNode(name);
            node.Tag = sum;
            node.Nodes.AddRange(nodes.ToArray());
            return node;
        }

        private TreeNode GetTreeNodeForZ(uint partitionAddress, int z)
        {
            int lowerBound = -8192 + z * 1024;
            int upperBound = lowerBound + 1024;

            List<TreeNode> nodes = new List<TreeNode>();
            int sum = 0;
            for (int x = 0; x < 16; x++)
            {
                TreeNode subNode = GetTreeNodeForX(partitionAddress, z, x);
                nodes.Add(subNode);
                sum += (int)subNode.Tag;
            }

            string name = "Z:" + z + " (" + lowerBound + " < z < " + upperBound + ") [" + sum + "]";
            TreeNode node = new TreeNode(name);
            node.Tag = sum;
            node.Nodes.AddRange(nodes.ToArray());
            return node;
        }

        private TreeNode GetTreeNodeForX(uint partitionAddress, int z, int x)
        {
            int lowerBound = -8192 + x * 1024;
            int upperBound = lowerBound + 1024;

            List<TreeNode> nodes = new List<TreeNode>();
            int sum = 0;
            for (int type = 0; type < 3; type++)
            {
                TreeNode subNode = GetTreeNodeForType(partitionAddress, z, x, type);
                nodes.Add(subNode);
                sum += (int)subNode.Tag;
            }

            string name = "X:" + x + " (" + lowerBound + " < x < " + upperBound + ") [" + sum + "]";
            TreeNode node = new TreeNode(name);
            node.Tag = sum;
            node.Nodes.AddRange(nodes.ToArray());
            return node;
        }

        private TreeNode GetTreeNodeForType(uint partitionAddress, int z, int x, int type)
        {
            int typeSize = 2 * 4;
            int xSize = 3 * typeSize;
            int zSize = 16 * xSize;
            uint address = (uint)(partitionAddress + z * zSize + x * xSize + type * typeSize);
            address = Config.Stream.GetUInt32(address);

            List<TreeNode> nodes = new List<TreeNode>();
            while (address != 0)
            {
                uint triAddress = Config.Stream.GetUInt32(address + 4);
                short y1 = TriangleOffsetsConfig.GetY1(triAddress);
                string triAddressString = HexUtilities.FormatValue(triAddress) + " (y1 = " + y1 + ")";
                TreeNode subNode = new TreeNode(triAddressString);
                subNode.Tag = triAddress;
                nodes.Add(subNode);
                address = Config.Stream.GetUInt32(address);
            }

            string name = (type == 0 ? "Floors" : type == 1 ? "Ceilings" : "Walls") + " [" + nodes.Count + "]";
            TreeNode node = new TreeNode(name);
            node.Tag = nodes.Count;
            node.Nodes.AddRange(nodes.ToArray());
            return node;
        }
    }
}
