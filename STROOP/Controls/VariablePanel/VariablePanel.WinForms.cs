using STROOP.Controls.VariablePanel.Cells;
using STROOP.Forms;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Utilities;
using STROOP.Variables.VariablePanel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace STROOP.Controls.VariablePanel;

public partial class VariablePanel
{
    public List<ToolStripItem> customContextMenuItems = new List<ToolStripItem>();

    public override bool Focused => renderer.Focused;

    private string _varFilePath;
    string _dataPath;

    [Category("Data"), Browsable(true)]
    public string DataPath
    {
        get { return _dataPath; }
        set { Initialize(_dataPath = value); }
    }

    [Category("Layout"), Browsable(true)] public int? elementNameWidth { get; set; } = null;
    [Category("Layout"), Browsable(true)] public int? elementValueWidth { get; set; } = null;

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        renderer.Draw();
    }

    private void ShowVarContextMenu()
    {
        ContextMenuStrip ctx = new ContextMenuStrip();

        var uniqueSettings = GetSelectedVars().SelectMany(x => x.control.AvailableSettings()).ToHashSet();
        var sortedOptions = uniqueSettings.ToList();
        sortedOptions.Sort((a, b) => string.Compare(a.Name, b.Name));
        foreach (var setting in sortedOptions)
            setting.CreateContextMenuEntry(ctx.Items, GetSelectedVars);

        ctx.Items.Add(new ToolStripSeparator());
        foreach (var item in VariableSelectionUtilities.CreateSelectionToolStripItems(GetSelectedVars(), this))
            ctx.Items.Add(item);

        ctx.Show(Cursor.Position);
    }

    private void ShowContextMenu()
    {
        ToolStripMenuItem resetVariablesItem = new ToolStripMenuItem("Reset Variables");
        resetVariablesItem.Click += (sender, e) => ResetVariables();

        ToolStripMenuItem clearAllButHighlightedItem = new ToolStripMenuItem("Clear All But Highlighted");
        clearAllButHighlightedItem.Click += (sender, e) => ClearAllButHighlightedVariables();

        ToolStripMenuItem addCustomVariablesItem = new ToolStripMenuItem("Add Custom Variables");
        addCustomVariablesItem.Click += (sender, e) =>
        {
            VariableCreationForm form = new VariableCreationForm();
            form.Initialize(this);
            form.Show();
        };

        ToolStripMenuItem addDummyVariableItem = new ToolStripMenuItem("Add Dummy Variable...");
        foreach (string typeString in TypeUtilities.InGameTypeList)
        {
            ToolStripMenuItem typeItem = new ToolStripMenuItem(typeString);
            addDummyVariableItem.DropDownItems.Add(typeItem);
            typeItem.Click += (sender, e) =>
            {
                int numEntries = 1;
                if (GlobalKeyboard.IsCtrlDown())
                {
                    string numEntriesString = DialogUtilities.GetStringFromDialog(labelText: "Enter Num Vars:");
                    if (numEntriesString == null) return;
                    int parsed = ParsingUtilities.ParseInt(numEntriesString);
                    parsed = Math.Max(parsed, 0);
                    numEntries = parsed;
                }

                for (int i = 0; i < numEntries; i++)
                {
                    Type type = TypeUtilities.StringToType[typeString];
                    var view = (CustomVariable)typeof(VariablePanel)
                        .GetMethod(nameof(CreateDummyVariable), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(type)
                        .Invoke(null, []);
                    this.AddVariable(($"Dummy {i + 1}", view));
                }
            };
        }

        ToolStripMenuItem addRelativeVariablesItem = null, removePointVariableItem = null;
        var getSpecialFuncVars = getSpecialFuncVariables?.Invoke() ?? null;
        var specificsCount = getSpecialFuncVars?.Count() ?? 0;
        if (PositionAngle.HybridPositionAngle.pointPAs.Count > 0)
        {
            if (getSpecialFuncVars != null && specificsCount > 0)
            {
                void BindHandler(ToolStripMenuItem menuItem, PositionAngle.HybridPositionAngle targetPA, SpecialFuncVariables generator) =>
                    menuItem.Click += (_, __) => this.AddVariables(generator(targetPA));

                addRelativeVariablesItem = new ToolStripMenuItem("Add relative variables for...");
                if (specificsCount == 1)
                    addRelativeVariablesItem.Text = $"Add {getSpecialFuncVars.First().name} for...";
                foreach (var pa in PositionAngle.HybridPositionAngle.pointPAs)
                {
                    var paItem = new ToolStripMenuItem(pa.name);
                    if (specificsCount == 1)
                        BindHandler(paItem, pa, getSpecialFuncVars.First().generateVariables);
                    else
                        foreach (var specialFunc in getSpecialFuncVars)
                        {
                            var specificsItem = new ToolStripMenuItem(specialFunc.name);
                            BindHandler(specificsItem, pa, specialFunc.generateVariables);
                            paItem.DropDownItems.Add(specificsItem);
                        }

                    addRelativeVariablesItem.DropDownItems.Add(paItem);
                }
            }

            removePointVariableItem = new ToolStripMenuItem("Remove custom point ...");
            foreach (var customPA in PositionAngle.HybridPositionAngle.pointPAs)
            {
                var capture = customPA;
                var subElement = new ToolStripMenuItem(customPA.name);
                subElement.Click += (_, __) =>
                {
                    capture.first = () => PositionAngle.NaN;
                    capture.second = () => PositionAngle.NaN;
                    capture.OnDelete();
                    PositionAngle.HybridPositionAngle.pointPAs.Remove(capture);
                };
                removePointVariableItem.DropDownItems.Add(subElement);
            }
        }

        var addPointVariableItem = new ToolStripMenuItem("Add custom point...");
        addPointVariableItem.Click += (_, __) =>
        {
            var ptCount = 1;
            while (PositionAngle.HybridPositionAngle.pointPAs.Any(pa => pa.name.ToLower() == $"point{ptCount}"))
                ptCount++;
            var newName = DialogUtilities.GetStringFromDialog($"Point{ptCount}", "Enter name of new custom point", "Add custom point");
            if (newName?.Trim() != null)
                PositionAngle.HybridPositionAngle.pointPAs.Add(
                    new PositionAngle.HybridPositionAngle(() => PositionAngle.Mario, () => PositionAngle.Mario, newName));
        };

        ToolStripMenuItem openSaveClearItem = new ToolStripMenuItem("Open / Save / Clear ...");
        ControlUtilities.AddDropDownItems(
            openSaveClearItem,
            new List<string>() { "Restore", "Open", "Open as Pop Out", "Save in Place", "Save As", "Clear" },
            new List<Action>()
            {
                () => OpenVariables(DialogUtilities.OpenXmlElements(FileType.StroopVariables, _dataPath)),
                () => OpenVariables(),
                () => OpenVariablesAsPopOut(),
                () => SaveVariablesInPlace(),
                () => SaveVariables(),
                () => ClearVariables(),
            });

        ToolStripMenuItem doToAllVariablesItem = new ToolStripMenuItem("Do to all variables...");
        VariableSelectionUtilities.CreateSelectionToolStripItems(GetCurrentlyVisibleCells(), this)
            .ForEach(item => doToAllVariablesItem.DropDownItems.Add(item));

        filterVariablesItem.DropDown.MouseEnter += (sender, e) => { filterVariablesItem.DropDown.AutoClose = false; };
        filterVariablesItem.DropDown.MouseLeave += (sender, e) =>
        {
            filterVariablesItem.DropDown.AutoClose = true;
            filterVariablesItem.DropDown.Close();
        };

        ToolStripItem searchVariablesItem = new ToolStripMenuItem("Search variables...");
        searchVariablesItem.Click += (_, __) => (FindForm() as StroopMainForm)?.ShowSearchDialog();

        var strip = new ContextMenuStrip();
        strip.Items.Add(resetVariablesItem);
        strip.Items.Add(clearAllButHighlightedItem);
        strip.Items.Add(new ToolStripSeparator());
        if (addRelativeVariablesItem != null)
            strip.Items.Add(addRelativeVariablesItem);
        strip.Items.Add(addPointVariableItem);
        strip.Items.Add(removePointVariableItem);
        strip.Items.Add(addCustomVariablesItem);
        strip.Items.Add(addDummyVariableItem);
        strip.Items.Add(new ToolStripSeparator());
        strip.Items.Add(openSaveClearItem);
        strip.Items.Add(doToAllVariablesItem);
        strip.Items.Add(filterVariablesItem);
        strip.Items.Add(searchVariablesItem);
        if (customContextMenuItems.Count > 0)
        {
            strip.Items.Add(new ToolStripSeparator());
            foreach (var item in customContextMenuItems)
                strip.Items.Add(item);
        }

        strip.Show(System.Windows.Forms.Cursor.Position);
    }

    // Prevent unwanted scrolling to the top left of the renderer child control
    // For details, see https://stackoverflow.com/questions/419774/how-can-you-stop-a-winforms-panel-from-scrolling
    protected override Point ScrollToControl(Control activeControl)
        => DisplayRectangle.Location;
}
