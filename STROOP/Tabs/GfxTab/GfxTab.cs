using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;

namespace STROOP.Tabs.GfxTab
{
    public partial class GfxTab : STROOPTab
    {
        /**
        * The Gfx tree is responsible for drawing everything in SM64 except HUD text
        * Nodes that actually draw things are 'DisplayLists' for static things (like level geometry) or 'GeoLayout scripts'
        * for more complex things (like water rectangles, snow, painting wobble).
        * Other nodes affect everything below it. There is a child selector that ensures only one room in the castle / BBH / HMC is drawn at a time,
        * there are nodes setting up a camera, rotationg / scaling models, handling animation, all kinds of stuff
        * This manager makes it easy to browse all the nodes and edit them
        */
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            WatchVariableUtilities.baseAddressGetters[BaseAddressType.GfxNode] =
                () => AccessScope<GfxTab>.content?.SelectedNode?.Address.Yield() ?? Array.Empty<uint>();
        }

        public GfxNode SelectedNode;
        IEnumerable<WatchVariableControl> SpecificVariables;

        public GfxTab()
        {
            InitializeComponent();

            richTextBoxGfx.Font = new Font("Courier New", 8);
            richTextBoxGfx.ForeColor = Color.Black;

            treeViewGfx.AfterSelect += _treeView_AfterSelect;
            buttonGfxRefresh.Click += RefreshButton_Click;
            buttonGfxRefreshObject.Click += RefreshButtonObject_Click;
            buttonGfxDumpDisplayList.Click += DumpButton_Click;
            buttonGfxHitboxHack.Click += (sender, e) => InjectHitboxViewCode();

            SpecificVariables = new List<WatchVariableControl>();
        }

        public override void InitializeTab()
        {
            base.InitializeTab();
            SuspendLayout();
            foreach (var precursor in GfxNode.GetCommonVariables())
                watchVariablePanelGfx.AddVariable(precursor);
            ResumeLayout();
        }

        public override string GetDisplayName() => "Gfx";

        // Inject code that shows hitboxes in-game
        // Note: a bit ugly at the moment. Hack folder is hardcoded instead of taken from Config file,
        // and it's put here in the GFX tab by a lack of a better place. The hacks in the hack tab are
        // constantly reapplied when memory is changed, which doesn't work with this hack which initializes 
        // variables that are later changed.
        public void InjectHitboxViewCode()
        {
            RomHack hck = null;
            try
            {
                if (RomVersionConfig.Version == Structs.RomVersion.US)
                {
                    hck = new RomHack("Resources\\Hacks\\HitboxViewU.hck", "HitboxView");
                }
                else if (RomVersionConfig.Version == Structs.RomVersion.JP)
                {
                    hck = new RomHack("Resources\\Hacks\\HitboxViewJ.hck", "HitboxView");
                }
                else
                {
                    MessageBox.Show("Hitbox view hack only available on US and JP versions");
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Hack files are missing in Resources\\Hacks folder");
            }

            hck?.LoadPayload();
        }

        public override void Update(bool active)
        {
            // allow calls made in the update scope to retrieve this tab's selected node address
            using (new AccessScope<GfxTab>(this))
                base.Update(active);
        }

        // Dump the display list of the currently selected gfx node (if applicable)
        // This can contain vertices and triangles, but also draw settings like lighting and fog
        private void DumpButton_Click(object sender, EventArgs e)
        {
            if (SelectedNode != null && (SelectedNode is GfxDisplayList || SelectedNode is GfxAnimationNode
                                                                        || SelectedNode is GfxTranslatedModel || SelectedNode is GfxRotationNode))
            {
                uint address = Config.Stream.GetUInt32(SelectedNode.Address + 0x14);
                richTextBoxGfx.Text = Fast3DDecoder.DecodeList(SegmentationUtilities.DecodeSegmentedAddress(address));
            }
            else
            {
                MessageBox.Show("Select a display list node first");
            }
        }

        // The variables in the first 0x14 bytes in a GFX node are common, but after that there are type-specific variables
        void UpdateSpecificVariables(GfxNode node)
        {
            watchVariablePanelGfx.RemoveVariables(SpecificVariables);
            if (node != null)
                SpecificVariables = watchVariablePanelGfx.AddVariables(node.GetTypeSpecificVariables());
            else
                SpecificVariables = new WatchVariableControl[0];
        }

        // Build a GFX tree for every object that is selected in the object slot view
        private void RefreshButtonObject_Click(object sender, EventArgs e)
        {
            HashSet<uint> list = Config.ObjectSlotsManager.SelectedSlotsAddresses;
            if (list != null && list.Count > 0)
            {
                treeViewGfx.Nodes.Clear();
                foreach (uint address in list)
                {
                    AddToTreeView(address);
                }

                ExpandNodesUpTo(treeViewGfx.Nodes, 4);
            }
            else
            {
                MessageBox.Show("Select at least one object slot.");
            }
        }

        /**
         * When selecting a node, ensure the variable containers are related to that node
         */
        private void _treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            GfxNode node = (GfxNode)e.Node.Tag;
            SelectedNode = node;
            UpdateSpecificVariables(SelectedNode);
        }

        /**
         * When refresh is clicked, the old GFX tree is discarded and a new one is read
         */
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            treeViewGfx.Nodes.Clear();

            // A pointer to the root node of the GFX tree is stored at offset 0x04 in a certain struct
            var StructWithGfxRoot = Config.Stream.GetUInt32(RomVersionConfig.SwitchMap(0x8032DDCC, 0x8032CE6C));

            if (StructWithGfxRoot > 0x80000000u)
            {
                AddToTreeView(Config.Stream.GetUInt32(StructWithGfxRoot + 0x04));
            }

            ExpandNodesUpTo(treeViewGfx.Nodes, 4);
        }

        // By default, a new TreeNode is collapsed. If you expand all, then the treeview will be overwhelmed with 240 object nodes
        // This function allows to expand only the nodes a certain amount of levels deep while keeping the deeper ones collapsed
        private void ExpandNodesUpTo(TreeNodeCollection nodes, int level)
        {
            if (level <= 0) return;

            foreach (TreeNode node in nodes)
            {
                node.Expand();
                ExpandNodesUpTo(node.Nodes, level - 1);
            }
        }

        public void AddToTreeView(uint rootAddress)
        {
            GfxNode root = GfxNode.ReadGfxNode(rootAddress);
            treeViewGfx.Nodes.Add(GfxToTreeNode(root));
        }

        /*
         * Recursively converts a tree of GfxNodes to a tree of TreeNodes so that they can be displayed in the tree viewer
         */
        public TreeNode GfxToTreeNode(GfxNode node)
        {
            // Should only happen when memory is invalid (for example when the US setting is used on a JP ROM)
            if (node == null) return new TreeNode("Invalid Gfx Node");

            TreeNode res = new TreeNode(node.Name, node.Children.Select(x => GfxToTreeNode(x)).ToArray());
            res.Tag = node;
            return res;
        }
    }
}
