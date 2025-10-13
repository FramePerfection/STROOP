using STROOP.Models;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public class WatchVariableObjectWrapper : WatchVariableAddressWrapper
    {
        static WatchVariableSetting DisplayAsObjectSetting = new WatchVariableSetting(
            "Display as Object",
            (ctrl, obj) =>
            {
                if (ctrl.WatchVarWrapper is WatchVariableObjectWrapper objectWrapper)
                    if (obj is bool doDisplayAsObject)
                        objectWrapper._displayAsObject = doDisplayAsObject;
                    else
                        return false;
                return true;
            },
            ("Object", () => true, WrapperProperty<WatchVariableObjectWrapper>(o => o._displayAsObject)),
            ("Address", () => true, WrapperProperty<WatchVariableObjectWrapper>(o => !o._displayAsObject))
        );

        static WatchVariableSetting SelectObjectSetting = new WatchVariableSetting(
            "Select Object",
            (ctrl, obj) =>
            {
                if (ctrl.WatchVarWrapper is WatchVariableObjectWrapper objectWrapper)
                {
                    var value = objectWrapper.CombineValues();
                    if (value.meaning == CombinedValuesMeaning.SameValue)
                        Config.ObjectSlotsManager.SelectSlotByAddress((uint)value.value);
                }

                return false;
            });

        private bool _displayAsObject;

        public WatchVariableObjectWrapper(IVariableView<uint> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            _displayAsObject = true;

            AddObjectContextMenuStripItems();
        }

        private void AddObjectContextMenuStripItems()
        {
            _watchVarControl.AddSetting(DisplayAsObjectSetting);
            _watchVarControl.AddSetting(SelectObjectSetting);
        }

        public override string GetClass() => "Object";

        public override string DisplayValue(uint value)
        {
            if (_displayAsObject)
            {
                uint? uintValueNullable = ParsingUtilities.ParseUIntNullable(value);
                if (uintValueNullable.HasValue)
                    return Config.ObjectSlotsManager.GetDescriptiveSlotLabelFromAddress(uintValueNullable.Value, false);
            }

            return base.DisplayValue(value);
        }

        public override bool TryParseValue(string value, out uint result)
        {
            string slotName = value.ToLower();

            if (slotName == "(no object)" || slotName == "no object")
            {
                result = 0;
                return true;
            }

            if (slotName == "(unused object)" || slotName == "unused object")
            {
                result = ObjectSlotsConfig.UnusedSlotAddress;
                return true;
            }

            if (slotName.StartsWith("slot"))
            {
                slotName = slotName.Remove(0, "slot".Length);
                slotName = slotName.Trim();
                ObjectDataModel obj = Config.ObjectSlotsManager.GetObjectFromLabel(slotName);
                if (obj != null)
                {
                    result = obj.Address;
                    return true;
                }
            }

            return base.TryParseValue(value, out result);
        }
    }
}
