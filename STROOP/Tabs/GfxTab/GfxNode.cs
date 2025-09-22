using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.GfxTab
{
    public class GfxNode
    {
        private const int _maxSiblings = 1000; //Siblings are stored as a circular list. This limit prevent infinite loops on malformed memory.

        public virtual string Name
        {
            get { return "GFX node"; }
        } //This name is overridden by all the sub classes corresponding

        public uint Address;
        public List<GfxNode> Children;

        public static GfxNode ReadGfxNode(uint address)
        {
            if (address < 0x80000000u || address > 0x80800000u)
            {
                return null;
            }

            ushort type = Config.Stream.GetUInt16(address + 0x00);
            GfxNode res;

            switch (type)
            {
                case 0x001: res = new GfxRootnode(); break;
                case 0x002: res = new GfxScreenSpace(); break;
                case 0x004: res = new GfxMasterList(); break;
                case 0x00A: res = new GfxGroupParent(); break;
                case 0x00B: res = new GfxLevelOfDetail(); break;
                case 0x015: res = new GfxDebugTransformation(); break;
                case 0x016: res = new GfxTranslatedModel(); break;
                case 0x017: res = new GfxRotationNode(); break;
                case 0x018: res = new GfxGameObject(); break;
                case 0x019: res = new GfxAnimationNode(); break;
                case 0x01A: res = new GfxBillboard(); break;
                case 0x01B: res = new GfxDisplayList(); break;
                case 0x01C: res = new GfxScalingNode(); break;
                case 0x028: res = new GfxShadowNode(); break;
                case 0x029: res = new GfxObjectParent(); break;
                //Todo: add 0x2F
                case 0x103: res = new GfxProjection3D(); break;
                case 0x10C: res = new GfxChildSelector(); break;
                case 0x114: res = new GfxCamera(); break;
                case 0x12A: res = new GfxGeoLayoutScript(); break;
                case 0x12C: res = new GfxBackgroundImage(); break;
                case 0x12E: res = new GfxHeldObject(); break;
                default: res = new GfxNode(); break;
            }

            res.Address = address;
            res.Children = new List<GfxNode>();

            uint childAddress;

            if (type == 0x018 || type == 0x029)
            {
                // For some reason, the object parent has a null pointer as a child inbetween frames,
                // but during updatng it temporarily sets it to the pointer at offset 0x14
                // Object nodes also do something like that
                childAddress = Config.Stream.GetUInt32(address + 0x14);
            }
            else
            {
                childAddress = Config.Stream.GetUInt32(address + 0x10);  //offset 0x10 = child pointer
            }

            if (childAddress != 0)
            {
                //Traverse the circularly linked list of siblings until the first child is seen again
                uint currentAddress = childAddress;
                for (int i = 0; i < _maxSiblings; i++)
                {
                    res.Children.Add(ReadGfxNode(currentAddress));
                    currentAddress = Config.Stream.GetUInt32(currentAddress + 0x08); //offset 0x08 = next pointer
                    if (currentAddress == childAddress) break;
                }
            }

            return res;
        }

        public static List<NamedVariableCollection.IView> GetCommonVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<ushort>("Type", 0x00));
            precursors.Add(gfxProperty<ushort>("Active", 0x02, WatchVariableSubclass.Boolean, 0x01));
            precursors.Add(gfxProperty<ushort>("Bit 1", 0x02, WatchVariableSubclass.Boolean, 0x02));
            precursors.Add(gfxProperty<ushort>("Billboard object", 0x02, WatchVariableSubclass.Boolean, 0x04));
            precursors.Add(gfxProperty<ushort>("Bit 3", 0x02, WatchVariableSubclass.Boolean, 0x08));
            precursors.Add(gfxProperty<ushort>("Invisible object", 0x02, WatchVariableSubclass.Boolean, 0x10));
            precursors.Add(gfxProperty<ushort>("Is animated", 0x02, WatchVariableSubclass.Boolean, 0x20));
            precursors.Add(gfxProperty<byte>("List index", 0x02));   //note: not actually a byte, but the result of (short>>8)
            precursors.Add(gfxProperty<uint>("Previous", 0x04, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Next", 0x08, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Parent", 0x0C, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Child", 0x10, WatchVariableSubclass.Address));
            return precursors;
        }

        // Wrapper to make defining variables easier
        protected static NamedVariableCollection.IView<T> gfxProperty<T>(
            string name,
            uint offset,
            string? subclass = null,
            uint? mask = null)
            where T : struct, IConvertible
        {
            subclass ??= WatchVariableSubclass.Number;
            mask = mask ?? 0xFFFFFFFF;
            Color color = (offset <= 0x13)
                ? ColorUtilities.GetColorFromString("Yellow")
                : ColorUtilities.GetColorFromString("LightBlue");

            var descriptor = new MemoryDescriptor(typeof(T), BaseAddressType.GfxNode, offset, mask);
            var view = descriptor.CreateView(subclass.ToString());
            view.Name = name;
            return (NamedVariableCollection.MemoryDescriptorView<T>)view;
        }

        // If there are type specific variables, this should be overridden
        public virtual IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables() => Array.Empty<NamedVariableCollection.IView>();
    }

    internal class GfxChildSelector : GfxNode
    {
        public static readonly List<(uint, uint, string)> functionNameList = new List<(uint, uint, string)>
        {
            ( 0x80277150, 0x80276BA0, "Mario standing or moving" ),
            ( 0x802776D8, 0x80277128, "Vanish / metal cap" ),
            ( 0x80277740, 0x80277190, "Lost cap" ),
            ( 0x802771BC, 0x80276C0C, "Mario eyes" ),
            ( 0x802774F4, 0x80276F44, "Mario hand" ),
            ( 0x8029DBD4, 0x8029D458, "Current room" ),
            ( 0x8029DB48, 0x8029D3CC, "Fully opaque" ),
        };

        private static string GetFunctionName(List<(uint, uint, string)> functionNameList, uint functionAddress)
        {
            foreach ((uint addressUS, uint addressJP, string functionName) in functionNameList)
            {
                uint address = RomVersionConfig.SwitchMap(addressUS, addressJP);
                if (address == functionAddress) return functionName;
            }

            return null;
        }

        public override string Name
        {
            get
            {
                var functionAddress = Config.Stream.GetUInt32(Address + 0x14);
                string functionName = GetFunctionName(functionNameList, functionAddress);
                return "Switch" + (functionName == null ? "" : ": " + functionName);
            }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Selection function", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<ushort>("Selected child", 0x1E));
            return precursors;
        }
    }

    internal class GfxBackgroundImage : GfxNode
    {
        public override string Name
        {
            get { return "Background image"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            var precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Draw function", 0x14, WatchVariableSubclass.Address));
            return precursors;
        }
    }

    internal class GfxHeldObject : GfxNode
    {
        public override string Name
        {
            get { return "Held object"; }
        }

        //function gfxFunction  0x14
        //int marioOffset  0x18        memory offset from marioData to check
        //void* heldObj      0x1c        another struct
        //short[3] position     0x20,2,4
        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Function pointer", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<int>("Mario offset", 0x18));
            precursors.Add(gfxProperty<uint>("Held object", 0x1C, WatchVariableSubclass.Object));
            precursors.Add(gfxProperty<short>("Position x", 0x20));
            precursors.Add(gfxProperty<short>("Position y", 0x22));
            precursors.Add(gfxProperty<short>("Position z", 0x24));
            return precursors;
        }
    }

    internal class GfxGeoLayoutScript : GfxNode
    {
        // Todo: put these in external files and expand them
        public static readonly List<(uint, uint, string)> functionNameList = new List<(uint, uint, string)>
        {
            ( 0x802D01E0, 0x802CF700, "Water flow pause controller" ),
            ( 0x802D1B70, 0x802D1090, "Waterfall drawer" ),
            ( 0x8029D924, 0x8029D194, "Transparency controller" ),  //makes peach / toad / dust particles transparent
            ( 0x802D104C, 0x802D11FC, "Water rectangle drawer" ),
            ( 0x802D1CDC, 0x802D056C, "SSL Pyramid sand flow" ),
            ( 0x802761D0, 0x80275C20, "Snow controller" ),
            ( 0x802CD1E8, 0x802CC708, "Overlay?" ),
            ( 0x802D5D0C, 0x802D522C, "Painting wobble controller" ),
            ( 0x802D5B98, 0x802D50B8, "Painting drawer" ),
            ( 0x80277B14, 0x80277564, "Mirror Mario drawer" ),
            ( 0x802775CC, 0x8027701C, "Mario hand / foot scaler" ),
            ( 0x80277D6C, 0x802777BC, "Mirror Mario inside out" ),
            ( 0x80277294, 0x80276CE4, "Mario torso tilter" ),
            ( 0x802773A4, 0x80276DF4, "C-up head rotation" ),
        };

        private static string GetFunctionName(List<(uint, uint, string)> functionNameList, uint functionAddress)
        {
            foreach ((uint addressUS, uint addressJP, string functionName) in functionNameList)
            {
                uint address = RomVersionConfig.SwitchMap(addressUS, addressJP);
                if (address == functionAddress) return functionName;
            }

            return null;
        }

        public override string Name
        {
            get
            {
                var functionAddress = Config.Stream.GetUInt32(Address + 0x14);
                string functionName = GetFunctionName(functionNameList, functionAddress);
                return "Script" + (functionName == null ? "" : ": " + functionName);
            }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            var precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Function pointer", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<ushort>("Parameter 1", 0x18));
            precursors.Add(gfxProperty<ushort>("Parameter 2", 0x1A));
            return precursors;
        }
    }

    internal class GfxCamera : GfxNode
    {
        public override string Name
        {
            get { return "Camera"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Update function", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<float>("X from", 0x1C));
            precursors.Add(gfxProperty<float>("Y from", 0x20));
            precursors.Add(gfxProperty<float>("Z from", 0x24));
            precursors.Add(gfxProperty<float>("X to", 0x28));
            precursors.Add(gfxProperty<float>("Y to", 0x2C));
            precursors.Add(gfxProperty<float>("Z to", 0x30));
            return precursors;
        }
    }

    internal class GfxProjection3D : GfxNode
    {
        public override string Name
        {
            get { return "Projection 3D"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Update function", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<float>("Fov", 0x1C));
            precursors.Add(gfxProperty<short>("Z clip near", 0x20));
            precursors.Add(gfxProperty<short>("Z clip far", 0x22));
            return precursors;
        }
    }

    internal class GfxObjectParent : GfxNode
    {
        public override string Name
        {
            get { return "Object parent"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Temp child", 0x14, WatchVariableSubclass.Address));
            return precursors;
        }
    }

    internal class GfxShadowNode : GfxNode
    {
        public override string Name
        {
            get { return "Shadow"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<short>("Radius", 0x14));
            precursors.Add(gfxProperty<byte>("Opacity", 0x16));
            precursors.Add(gfxProperty<byte>("Type", 0x17));
            return precursors;
        }
    }

    internal class GfxScalingNode : GfxNode
    {
        public override string Name
        {
            get { return "Scaling node"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<float>("Scale", 0x18));
            return precursors;
        }
    }

    //For example Goomba body
    internal class GfxBillboard : GfxNode
    {
        public override string Name
        {
            get { return "Billboard"; }
        }
    }

    internal class GfxAnimationNode : GfxNode
    {
        public override string Name
        {
            get { return "Animated node"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Display list", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<short>("X offset", 0x18));
            precursors.Add(gfxProperty<short>("Y offset", 0x1A));
            precursors.Add(gfxProperty<short>("Z offset", 0x1C));
            return precursors;
        }
    }

    internal class GfxGameObject : GfxNode
    {
        public override string Name
        {
            get { return "Game object"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Shared child", 0x14, WatchVariableSubclass.Address));
            return precursors;
        }
    }

    internal class GfxRotationNode : GfxNode
    {
        public override string Name
        {
            get { return "Rotation"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Segmented address", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<short>("Angle x", 0x18)); //Todo: make these angle types
            precursors.Add(gfxProperty<short>("Angle y", 0x1A));
            precursors.Add(gfxProperty<short>("Angle z", 0x1C));
            return precursors;
        }
    }

    // This is used to draw the "S U P E R M A R I O" in debug level select
    internal class GfxTranslatedModel : GfxNode
    {
        public override string Name
        {
            get { return "Menu model"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Segmented address", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<short>("X offset", 0x18));
            precursors.Add(gfxProperty<short>("Y offset", 0x1A));
            precursors.Add(gfxProperty<short>("Z offset", 0x1C));
            return precursors;
        }
    }

    internal class GfxDebugTransformation : GfxNode
    {
        public override string Name
        {
            get { return "Debug transformation"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            var precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<short>("X translation", 0x18));
            precursors.Add(gfxProperty<short>("Y translation", 0x1A));
            precursors.Add(gfxProperty<short>("Z translation", 0x1C));
            precursors.Add(gfxProperty<short>("X rotation", 0x1E));
            precursors.Add(gfxProperty<short>("Y rotation", 0x20));
            precursors.Add(gfxProperty<short>("Z rotation", 0x22));
            return precursors;
        }
    }

    internal class GfxLevelOfDetail : GfxNode
    {
        public override string Name
        {
            get { return "Level of detail"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            var precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<short>("Min cam distance", 0x14));
            precursors.Add(gfxProperty<short>("Max cam distance", 0x16));
            precursors.Add(gfxProperty<uint>("Pointer 1", 0x18, WatchVariableSubclass.Address));

            return precursors;
        }
    }

    internal class GfxMasterList : GfxNode
    {
        public override string Name
        {
            get { return "Master list"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            var precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Pointer 0", 0x14, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 1", 0x18, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 2", 0x1C, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 3", 0x20, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 4", 0x24, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 5", 0x28, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 6", 0x2C, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 7", 0x30, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 8", 0x34, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 9", 0x3C, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 10", 0x40, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 11", 0x44, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 12", 0x48, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 13", 0x4C, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 14", 0x50, WatchVariableSubclass.Address));
            precursors.Add(gfxProperty<uint>("Pointer 15", 0x54, WatchVariableSubclass.Address));
            return precursors;
        }
    }

    // Possibly some extra things?
    internal class GfxGroupParent : GfxNode
    {
        public override string Name
        {
            get { return "Group"; }
        }
    }

    internal class GfxScreenSpace : GfxNode
    {
        public override string Name
        {
            get { return "Screenspace"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            var precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<float>("??? 0x14", 0x14));
            precursors.Add(gfxProperty<uint>("??? 0x18", 0x18));
            return precursors;
        }
    }

    internal class GfxRootnode : GfxNode
    {
        public override string Name
        {
            get { return "Root"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<short>("Some short", 0x14));
            precursors.Add(gfxProperty<short>("Screen xoffset", 0x16));
            precursors.Add(gfxProperty<short>("Screen yoffset", 0x18));
            precursors.Add(gfxProperty<short>("Screen half width", 0x1A));
            precursors.Add(gfxProperty<short>("Screen half height", 0x1C));
            return precursors;
        }
    }

    internal class GfxDisplayList : GfxNode
    {
        public override string Name
        {
            get { return "Display List"; }
        }

        public override IEnumerable<NamedVariableCollection.IView> GetTypeSpecificVariables()
        {
            List<NamedVariableCollection.IView> precursors = new List<NamedVariableCollection.IView>();
            precursors.Add(gfxProperty<uint>("Segmented address", 0x14, WatchVariableSubclass.Address));
            return precursors;
        }
    }
}
