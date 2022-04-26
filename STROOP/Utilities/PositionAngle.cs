﻿using STROOP.Models;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace STROOP.Utilities
{
    public class PositionAngle
    {
        public class CustomPositionAngle : PositionAngle
        {
            Vector3 customPos;
            ushort customAngle;
            public CustomPositionAngle(Vector3 pos, ushort angle = 0) { this.customPos = pos; this.customAngle = angle; }

            public override double X => customPos.X;
            public override double Y => customPos.Y;
            public override double Z => customPos.Z;
            public override bool SetX(double value) { customPos.X = (float)value; return true; }
            public override bool SetY(double value) { customPos.Y = (float)value; return true; }
            public override bool SetZ(double value) { customPos.Z = (float)value; return true; }
            public override bool SetAngle(double value) { customAngle = (ushort)value; return true; }
        }

        private readonly PositionAngleTypeEnum PosAngleType;
        private readonly uint? Address;
        private readonly int? Index;
        private readonly int? Index2;
        private readonly double? Frame;
        private readonly string Text;
        private double? ThisX;
        private double? ThisY;
        private double? ThisZ;
        private double? ThisAngle;
        private readonly PositionAngle PosAngle1;
        private readonly PositionAngle PosAngle2;
        private readonly List<Func<double>> Getters;
        private readonly List<Func<double, bool>> Setters;

        public static Dictionary<uint, (double, double, double, double, List<double>)> Schedule =
            new Dictionary<uint, (double, double, double, double, List<double>)>();
        public static int ScheduleOffset = 0;

        private static uint GetScheduleIndex()
        {
            uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
            return ParsingUtilities.ParseUIntRoundingCapping(globalTimer + ScheduleOffset);
        }

        private enum PositionAngleTypeEnum
        {
            Custom,
            Mario,
            Holp,
            Camera,
            CameraFocus,
            CamHackCamera,
            CamHackFocus,
            Obj,
            ObjHome,
            ObjGfx,
            ObjScale,
            Selected,
            First,
            Last,
            FirstHome,
            LastHome,
            GoombaProjection,
            KoopaTheQuick,
            Tri,
            ObjTri,
            Wall,
            Floor,
            Ceiling,
            Snow,
            QFrame,
            GFrame,
            Schedule,
            Hybrid,
            Trunc,
            Pos,
            Ang,
            Functions,
            Self,
            None,
        }

        public bool IsSelected
        {
            get => PosAngleType == PositionAngleTypeEnum.Selected;
        }

        public bool CompareType(PositionAngle other) => PosAngleType == other.PosAngleType;

        private bool ShouldHaveAddress(PositionAngleTypeEnum posAngleType)
        {
            return posAngleType == PositionAngleTypeEnum.Obj ||
                posAngleType == PositionAngleTypeEnum.ObjHome ||
                posAngleType == PositionAngleTypeEnum.ObjGfx ||
                posAngleType == PositionAngleTypeEnum.ObjScale ||
                posAngleType == PositionAngleTypeEnum.GoombaProjection ||
                posAngleType == PositionAngleTypeEnum.Tri ||
                posAngleType == PositionAngleTypeEnum.ObjTri;
        }

        private bool ShouldHaveIndex(PositionAngleTypeEnum posAngleType)
        {
            return posAngleType == PositionAngleTypeEnum.Tri ||
                posAngleType == PositionAngleTypeEnum.ObjTri ||
                posAngleType == PositionAngleTypeEnum.Wall ||
                posAngleType == PositionAngleTypeEnum.Floor ||
                posAngleType == PositionAngleTypeEnum.Ceiling ||
                posAngleType == PositionAngleTypeEnum.Snow;
        }

        private bool ShouldHaveFrame(PositionAngleTypeEnum posAngleType)
        {
            return posAngleType == PositionAngleTypeEnum.QFrame ||
                posAngleType == PositionAngleTypeEnum.GFrame;
        }

        private bool ShouldHaveText(PositionAngleTypeEnum posAngleType)
        {
            return posAngleType == PositionAngleTypeEnum.First ||
                posAngleType == PositionAngleTypeEnum.Last ||
                posAngleType == PositionAngleTypeEnum.FirstHome ||
                posAngleType == PositionAngleTypeEnum.LastHome;
        }

        protected PositionAngle() { }

        private PositionAngle(
            PositionAngleTypeEnum posAngleType,
            uint? address = null,
            int? index = null,
            int? index2 = null,
            double? frame = null,
            string text = null,
            double? thisX = null,
            double? thisY = null,
            double? thisZ = null,
            double? thisAngle = null,
            PositionAngle posAngle1 = null,
            PositionAngle posAngle2 = null,
            List<Func<double>> getters = null,
            List<Func<double, bool>> setters = null)
        {
            PosAngleType = posAngleType;
            Address = address;
            Index = index;
            Index2 = index2;
            Frame = frame;
            Text = text;
            ThisX = thisX;
            ThisY = thisY;
            ThisZ = thisZ;
            ThisAngle = thisAngle;
            PosAngle1 = posAngle1;
            PosAngle2 = posAngle2;
            Getters = getters;
            Setters = setters;

            bool shouldHaveAddress = ShouldHaveAddress(posAngleType);
            if (address.HasValue != shouldHaveAddress)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveIndex = ShouldHaveIndex(posAngleType);
            if (index.HasValue != shouldHaveIndex)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveIndex2 = PosAngleType == PositionAngleTypeEnum.ObjTri;
            if (index2.HasValue != shouldHaveIndex2)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveFrame = ShouldHaveFrame(posAngleType);
            if (frame.HasValue != shouldHaveFrame)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveText = ShouldHaveText(posAngleType);
            if ((text != null) != shouldHaveText)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveThisX = PosAngleType == PositionAngleTypeEnum.Pos;
            if (thisX.HasValue != shouldHaveThisX)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveThisY = PosAngleType == PositionAngleTypeEnum.Pos;
            if (thisY.HasValue != shouldHaveThisY)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveThisZ = PosAngleType == PositionAngleTypeEnum.Pos;
            if (thisZ.HasValue != shouldHaveThisZ)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveThisAngle =
                PosAngleType == PositionAngleTypeEnum.Pos ||
                PosAngleType == PositionAngleTypeEnum.Ang;
            if (thisAngle.HasValue != shouldHaveThisAngle)
                throw new ArgumentOutOfRangeException();

            bool shouldHavePosAngle1 =
                PosAngleType == PositionAngleTypeEnum.Hybrid ||
                PosAngleType == PositionAngleTypeEnum.Trunc;
            if ((posAngle1 != null) != shouldHavePosAngle1)
                throw new ArgumentOutOfRangeException();

            bool shouldHavePosAngle2 = PosAngleType == PositionAngleTypeEnum.Hybrid;
            if ((posAngle2 != null) != shouldHavePosAngle2)
                throw new ArgumentOutOfRangeException();

            bool shouldHaveGetters = PosAngleType == PositionAngleTypeEnum.Functions;
            if ((getters != null) != shouldHaveGetters)
                throw new ArgumentOutOfRangeException();
            if (getters != null && (getters.Count < 3 || getters.Count > 4)) // optional angle getter
                throw new ArgumentOutOfRangeException();

            bool shouldHaveSetters = PosAngleType == PositionAngleTypeEnum.Functions;
            if ((setters != null) != shouldHaveSetters)
                throw new ArgumentOutOfRangeException();
            if (setters != null && (setters.Count < 3 || setters.Count > 4)) // optional angle setter
                throw new ArgumentOutOfRangeException();
        }

        public static PositionAngle Mario = new PositionAngle(PositionAngleTypeEnum.Mario);
        public static PositionAngle Holp = new PositionAngle(PositionAngleTypeEnum.Holp);
        public static PositionAngle Selected = new PositionAngle(PositionAngleTypeEnum.Selected);
        public static PositionAngle KoopaTheQuick = new PositionAngle(PositionAngleTypeEnum.KoopaTheQuick);
        public static PositionAngle Camera = new PositionAngle(PositionAngleTypeEnum.Camera);
        public static PositionAngle CameraFocus = new PositionAngle(PositionAngleTypeEnum.CameraFocus);
        public static PositionAngle CamHackCamera = new PositionAngle(PositionAngleTypeEnum.CamHackCamera);
        public static PositionAngle CamHackFocus = new PositionAngle(PositionAngleTypeEnum.CamHackFocus);
        public static PositionAngle Scheduler = new PositionAngle(PositionAngleTypeEnum.Schedule);
        public static PositionAngle Self = new PositionAngle(PositionAngleTypeEnum.Self);
        public static PositionAngle None = new PositionAngle(PositionAngleTypeEnum.None);
        public static PositionAngle Custom(Vector3 position, ushort angle = 0) => new CustomPositionAngle(position, angle);

        public static PositionAngle Obj(uint address) =>
            new PositionAngle(PositionAngleTypeEnum.Obj, address: address);
        public static PositionAngle ObjHome(uint address) =>
            new PositionAngle(PositionAngleTypeEnum.ObjHome, address: address);
        public static PositionAngle MarioObj() => Obj(Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress));
        public static PositionAngle ObjGfx(uint address) =>
            new PositionAngle(PositionAngleTypeEnum.ObjGfx, address: address);
        public static PositionAngle ObjScale(uint address) =>
            new PositionAngle(PositionAngleTypeEnum.ObjScale, address: address);
        public static PositionAngle First(string text) =>
            new PositionAngle(PositionAngleTypeEnum.First, text: text);
        public static PositionAngle Last(string text) =>
            new PositionAngle(PositionAngleTypeEnum.Last, text: text);
        public static PositionAngle FirstHome(string text) =>
            new PositionAngle(PositionAngleTypeEnum.FirstHome, text: text);
        public static PositionAngle LastHome(string text) =>
            new PositionAngle(PositionAngleTypeEnum.LastHome, text: text);
        public static PositionAngle GoombaProjection(uint address) =>
            new PositionAngle(PositionAngleTypeEnum.GoombaProjection, address: address);
        public static PositionAngle Tri(uint address, int index) =>
            new PositionAngle(PositionAngleTypeEnum.Tri, address: address, index: index);
        public static PositionAngle ObjTri(uint address, int index, int index2) =>
            new PositionAngle(PositionAngleTypeEnum.ObjTri, address: address, index: index, index2: index2);
        public static PositionAngle Wall(int index) =>
            new PositionAngle(PositionAngleTypeEnum.Wall, index: index);
        public static PositionAngle Floor(int index) =>
            new PositionAngle(PositionAngleTypeEnum.Floor, index: index);
        public static PositionAngle Ceiling(int index) =>
            new PositionAngle(PositionAngleTypeEnum.Ceiling, index: index);
        public static PositionAngle Snow(int index) =>
            new PositionAngle(PositionAngleTypeEnum.Snow, index: index);
        public static PositionAngle QFrame(double frame) =>
            new PositionAngle(PositionAngleTypeEnum.QFrame, frame: frame);
        public static PositionAngle GFrame(double frame) =>
            new PositionAngle(PositionAngleTypeEnum.GFrame, frame: frame);
        public static PositionAngle Hybrid(PositionAngle posAngle1, PositionAngle posAngle2) =>
            new PositionAngle(PositionAngleTypeEnum.Hybrid, posAngle1: posAngle1, posAngle2: posAngle2);
        public static PositionAngle Trunc(PositionAngle posAngle) =>
            new PositionAngle(PositionAngleTypeEnum.Trunc, posAngle1: posAngle);
        public static PositionAngle Functions(List<Func<double>> getters, List<Func<double, bool>> setters) =>
            new PositionAngle(PositionAngleTypeEnum.Functions, getters: getters, setters: setters);
        public static PositionAngle Pos(double x, double y, double z, double angle = double.NaN) =>
            new PositionAngle(PositionAngleTypeEnum.Pos, thisX: x, thisY: y, thisZ: z, thisAngle: angle);
        public static PositionAngle Ang(double angle) =>
            new PositionAngle(PositionAngleTypeEnum.Ang, thisAngle: angle);

        public static PositionAngle FromString(string stringValue)
        {
            if (stringValue == null) return null;
            stringValue = stringValue.ToLower();
            List<string> parts = ParsingUtilities.ParseStringList(stringValue);

            if (parts.Count == 1 && parts[0] == "mario")
            {
                return Mario;
            }
            else if (parts.Count == 1 && parts[0] == "holp")
            {
                return Holp;
            }
            else if (parts.Count == 1 && (parts[0] == "cam" || parts[0] == "camera"))
            {
                return Camera;
            }
            else if (parts.Count == 1 && (parts[0] == "camfocus" || parts[0] == "camerafocus"))
            {
                return CameraFocus;
            }
            else if (parts.Count == 1 && (parts[0] == "camhackcam" || parts[0] == "camhackcamera"))
            {
                return CamHackCamera;
            }
            else if (parts.Count == 1 && parts[0] == "camhackfocus")
            {
                return CamHackFocus;
            }
            else if (parts.Count == 2 && (parts[0] == "obj" || parts[0] == "object"))
            {
                uint? address = ParsingUtilities.ParseHexNullable(parts[1]);
                if (!address.HasValue) return null;
                return Obj(address.Value);
            }
            else if (parts.Count == 2 && (parts[0] == "objhome" || parts[0] == "objecthome"))
            {
                uint? address = ParsingUtilities.ParseHexNullable(parts[1]);
                if (!address.HasValue) return null;
                return ObjHome(address.Value);
            }
            else if (parts.Count == 2 &&
                (parts[0] == "objgfx" || parts[0] == "objectgfx" || parts[0] == "objgraphics" || parts[0] == "objectgraphics"))
            {
                uint? address = ParsingUtilities.ParseHexNullable(parts[1]);
                if (!address.HasValue) return null;
                return ObjGfx(address.Value);
            }
            else if (parts.Count == 2 && (parts[0] == "objscale" || parts[0] == "objectscale"))
            {
                uint? address = ParsingUtilities.ParseHexNullable(parts[1]);
                if (!address.HasValue) return null;
                return ObjScale(address.Value);
            }
            else if (parts.Count == 1 && parts[0] == "selected")
            {
                return Selected;
            }
            else if (parts.Count >= 2 && parts[0] == "first")
            {
                return First(string.Join(" ", parts.Skip(1)));
            }
            else if (parts.Count >= 2 && parts[0] == "last")
            {
                return Last(string.Join(" ", parts.Skip(1)));
            }
            else if (parts.Count >= 2 && parts[0] == "firsthome")
            {
                return FirstHome(string.Join(" ", parts.Skip(1)));
            }
            else if (parts.Count >= 2 && parts[0] == "lasthome")
            {
                return LastHome(string.Join(" ", parts.Skip(1)));
            }
            else if (parts.Count == 2 && parts[0] == "goombaprojection")
            {
                uint? address = ParsingUtilities.ParseHexNullable(parts[1]);
                if (!address.HasValue) return null;
                return GoombaProjection(address.Value);
            }
            else if (parts.Count == 1 && parts[0] == "koopathequick")
            {
                return KoopaTheQuick;
            }
            else if (parts.Count == 3 && (parts[0] == "tri" || parts[0] == "triangle"))
            {
                uint? address = ParsingUtilities.ParseHexNullable(parts[1]);
                if (!address.HasValue) return null;
                int? index = ParsingUtilities.ParseIntNullable(parts[2]);
                if (!index.HasValue || index.Value < 1 || index.Value > 7) return null;
                // 1 = vertex 1
                // 2 = vertex 2
                // 3 = vertex 3
                // 4 = vertex closest to Mario
                // 5 = vertex closest to Self
                // 6 = point on triangle under Mario
                // 7 = point on triangle under Self
                return Tri(address.Value, index.Value);
            }
            else if (parts.Count == 4 && parts[0] == "objtri")
            {
                uint? address = ParsingUtilities.ParseHexNullable(parts[1]);
                if (!address.HasValue) return null;
                int? index = ParsingUtilities.ParseIntNullable(parts[2]);
                if (!index.HasValue) return null;
                int? index2 = ParsingUtilities.ParseIntNullable(parts[3]);
                if (!index2.HasValue || index2.Value < 0 || index2.Value > 4) return null;
                return ObjTri(address.Value, index.Value, index2.Value);
            }
            else if (parts.Count == 2 && parts[0] == "wall")
            {
                int? index = ParsingUtilities.ParseIntNullable(parts[1]);
                if (!index.HasValue || index.Value < 0 || index.Value > 4) return null;
                return Wall(index.Value);
            }
            else if (parts.Count == 2 && parts[0] == "floor")
            {
                int? index = ParsingUtilities.ParseIntNullable(parts[1]);
                if (!index.HasValue || index.Value < 0 || index.Value > 4) return null;
                return Floor(index.Value);
            }
            else if (parts.Count == 2 && parts[0] == "ceiling")
            {
                int? index = ParsingUtilities.ParseIntNullable(parts[1]);
                if (!index.HasValue || index.Value < 0 || index.Value > 4) return null;
                return Ceiling(index.Value);
            }
            else if (parts.Count == 2 && parts[0] == "snow")
            {
                int? index = ParsingUtilities.ParseIntNullable(parts[1]);
                if (!index.HasValue || index.Value < 0) return null;
                return Snow(index.Value);
            }
            else if (parts.Count == 2 && parts[0] == "qframe")
            {
                double? frame = ParsingUtilities.ParseDoubleNullable(parts[1]);
                if (!frame.HasValue) return null;
                return QFrame(frame.Value);
            }
            else if (parts.Count == 2 && parts[0] == "gframe")
            {
                double? frame = ParsingUtilities.ParseDoubleNullable(parts[1]);
                if (!frame.HasValue) return null;
                return GFrame(frame.Value);
            }
            else if (parts.Count >= 1 && parts[0] == "trunc")
            {
                PositionAngle posAngle = FromString(string.Join(" ", parts.Skip(1)));
                if (posAngle == null) return null;
                return Trunc(posAngle);
            }
            else if (parts.Count == 1 && parts[0] == "self")
            {
                return Self;
            }
            else if (parts.Count >= 1 && (parts[0] == "pos" || parts[0] == "position"))
            {
                double x = parts.Count >= 2 ? ParsingUtilities.ParseDoubleNullable(parts[1]) ?? double.NaN : double.NaN;
                double y = parts.Count >= 3 ? ParsingUtilities.ParseDoubleNullable(parts[2]) ?? double.NaN : double.NaN;
                double z = parts.Count >= 4 ? ParsingUtilities.ParseDoubleNullable(parts[3]) ?? double.NaN : double.NaN;
                double angle = parts.Count >= 5 ? ParsingUtilities.ParseDoubleNullable(parts[4]) ?? double.NaN : double.NaN;
                return Pos(x, y, z, angle);
            }
            else if (parts.Count == 2 && (parts[0] == "ang" || parts[0] == "angle"))
            {
                double angle = ParsingUtilities.ParseDoubleNullable(parts[1]) ?? double.NaN;
                return Ang(angle);
            }
            else if (parts.Count == 1 && parts[0] == "schedule")
            {
                return Scheduler;
            }
            else if (parts.Count == 1 && parts[0] == "none")
            {
                return None;
            }

            return null;
        }

        public override string ToString()
        {
            List<object> parts = new List<object>();
            if (IsObject())
                parts.Add(GetMapNameForObject(Address.Value));
            else
                parts.Add(PosAngleType);
            if (Address.HasValue) parts.Add(HexUtilities.FormatValue(Address.Value, 8));
            if (Index.HasValue) parts.Add(Index.Value);
            if (Index2.HasValue) parts.Add(Index2.Value);
            if (Frame.HasValue) parts.Add(Frame.Value);
            if (Text != null) parts.Add(Text);
            if (ThisX.HasValue) parts.Add(ThisX.Value);
            if (ThisY.HasValue) parts.Add(ThisY.Value);
            if (ThisZ.HasValue) parts.Add(ThisZ.Value);
            if (ThisAngle.HasValue) parts.Add(ThisAngle.Value);
            if (PosAngle1 != null) parts.Add("[" + PosAngle1 + "]");
            if (PosAngle2 != null) parts.Add("[" + PosAngle2 + "]");
            return string.Join(" ", parts);
        }

        public string GetMapName()
        {
            switch (PosAngleType)
            {
                case PositionAngleTypeEnum.Obj:
                    return GetMapNameForObject(Address.Value);
                case PositionAngleTypeEnum.ObjHome:
                    return "Home for " + GetMapNameForObject(Address.Value);
                default:
                    return ToString();
            }
        }

        public static string GetMapNameForObject(uint address)
        {
            ObjectDataModel obj = new ObjectDataModel(address, true);
            string objectName = Config.ObjectAssociations.GetObjectName(obj.BehaviorCriteria);
            string slotLabel = Config.ObjectSlotsManager.GetDescriptiveSlotLabelFromAddress(address, true);
            return string.Format("[{0}] {1}", slotLabel, objectName);
        }

        public static string NameOfMultiple(IEnumerable<PositionAngle> positionAngles)
        {
            int count = 0;
            string result = "None";
            bool objects = true;
            bool isHome = true;
            string singleObjectName = "None";
            foreach (var posAngle in positionAngles)
            {
                isHome &= posAngle.PosAngleType == PositionAngleTypeEnum.ObjHome;
                objects &= posAngle.IsObjectOrMario();
                string n;
                if (posAngle.IsObject())
                {
                    ObjectDataModel obj = new ObjectDataModel(posAngle.Address.Value, true);
                    n = Config.ObjectAssociations.GetObjectName(obj.BehaviorCriteria);
                }
                else
                    n = posAngle.GetMapName();
                if (count == 0)
                {
                    result = n;
                    singleObjectName = posAngle.GetMapName();
                }
                else
                {
                    if (result != n)
                        result = "Objects";
                }
                count++;
            }
            if (count == 1)
                result = singleObjectName;

            if (isHome)
                result = "Home of " + result;

            return count > 1 ? "Multiple " + result : result;
        }

        public bool IsObject()
        {
            return PosAngleType == PositionAngleTypeEnum.Obj;
        }

        public bool IsObjectDependent()
        {
            return PosAngleType == PositionAngleTypeEnum.Obj ||
                PosAngleType == PositionAngleTypeEnum.ObjHome ||
                PosAngleType == PositionAngleTypeEnum.ObjGfx ||
                PosAngleType == PositionAngleTypeEnum.ObjScale ||
                PosAngleType == PositionAngleTypeEnum.GoombaProjection ||
                PosAngleType == PositionAngleTypeEnum.ObjTri;
        }

        public bool IsObjectOrMario()
        {
            return PosAngleType == PositionAngleTypeEnum.Obj ||
                PosAngleType == PositionAngleTypeEnum.Mario;
        }

        public uint GetObjAddress()
        {
            switch (PosAngleType)
            {
                case PositionAngleTypeEnum.Obj:
                    return Address.Value;
                case PositionAngleTypeEnum.Mario:
                    return Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public uint? GetObjectAddressIfObjectDependent()
        {
            return IsObjectDependent() ? Address : null;
        }

        public bool IsSelf() => PosAngleType == PositionAngleTypeEnum.Self;

        public bool DependsOnSelf()
        {
            if (PosAngleType == PositionAngleTypeEnum.Tri)
            {
                return Index == 5 || Index == 7;
            }
            return false;
        }

        public bool IsNone()
        {
            return PosAngleType == PositionAngleTypeEnum.None;
        }




        public Vector3 position
        {
            get { return new Vector3((float)X, (float)Y, (float)Z); }
            set { SetX(value.X); SetY(value.Y); SetZ(value.Z); }
        }


        public virtual double X
        {
            get
            {
                if (ShouldHaveAddress(PosAngleType) && Address == 0) return Double.NaN;
                switch (PosAngleType)
                {
                    case PositionAngleTypeEnum.Mario:
                        return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                    case PositionAngleTypeEnum.Holp:
                        return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HolpXOffset);
                    case PositionAngleTypeEnum.Camera:
                        return Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.XOffset);
                    case PositionAngleTypeEnum.CameraFocus:
                        return Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.FocusXOffset);
                    case PositionAngleTypeEnum.CamHackCamera:
                        return Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                    case PositionAngleTypeEnum.CamHackFocus:
                        return Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    case PositionAngleTypeEnum.Obj:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.XOffset);
                    case PositionAngleTypeEnum.ObjHome:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.HomeXOffset);
                    case PositionAngleTypeEnum.ObjGfx:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.GraphicsXOffset);
                    case PositionAngleTypeEnum.ObjScale:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.ScaleWidthOffset);
                    case PositionAngleTypeEnum.Selected:
                        {
                            List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                            if (objAddresses.Count == 0) return Double.NaN;
                            uint objAddress = objAddresses[0];
                            return Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                        }
                    case PositionAngleTypeEnum.First:
                        return GetObjectValue(Text, true, CoordinateAngle.X);
                    case PositionAngleTypeEnum.Last:
                        return GetObjectValue(Text, false, CoordinateAngle.X);
                    case PositionAngleTypeEnum.FirstHome:
                        return GetObjectValue(Text, true, CoordinateAngle.X, home: true);
                    case PositionAngleTypeEnum.LastHome:
                        return GetObjectValue(Text, false, CoordinateAngle.X, home: true);
                    case PositionAngleTypeEnum.GoombaProjection:
                        return GetGoombaProjection(Address.Value).x;
                    case PositionAngleTypeEnum.KoopaTheQuick:
                        return PlushUtilities.GetX();
                    case PositionAngleTypeEnum.Tri:
                        return GetTriangleVertexComponent(Address.Value, Index.Value, Coordinate.X);
                    case PositionAngleTypeEnum.ObjTri:
                        {
                            uint? triAddress = TriangleUtilities.GetTriangleAddressOfObjectTriangleIndex(Address.Value, Index.Value);
                            if (!triAddress.HasValue) return double.NaN;
                            return GetTriangleVertexComponent(triAddress.Value, Index2.Value, Coordinate.X);
                        }
                    case PositionAngleTypeEnum.Wall:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset), Index.Value, Coordinate.X);
                    case PositionAngleTypeEnum.Floor:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset), Index.Value, Coordinate.X);
                    case PositionAngleTypeEnum.Ceiling:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset), Index.Value, Coordinate.X);
                    case PositionAngleTypeEnum.Snow:
                        return GetSnowComponent(Index.Value, Coordinate.X);
                    case PositionAngleTypeEnum.QFrame:
                        return GetQFrameComponent(Frame.Value, Coordinate.X);
                    case PositionAngleTypeEnum.GFrame:
                        return GetGFrameComponent(Frame.Value, Coordinate.X);
                    case PositionAngleTypeEnum.Schedule:
                        uint scheduleIndex = GetScheduleIndex();
                        if (Schedule.ContainsKey(scheduleIndex)) return Schedule[scheduleIndex].Item1;
                        return Double.NaN;
                    case PositionAngleTypeEnum.Hybrid:
                        return PosAngle1.X;
                    case PositionAngleTypeEnum.Functions:
                        return Getters[0]();
                    case PositionAngleTypeEnum.Pos:
                        return ThisX.Value;
                    case PositionAngleTypeEnum.Ang:
                        return double.NaN;
                    case PositionAngleTypeEnum.Trunc:
                        return (int)PosAngle1.X;
                    case PositionAngleTypeEnum.Self:
                        return SpecialConfig.SelfPA.X;
                    case PositionAngleTypeEnum.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public virtual double Y
        {
            get
            {
                if (ShouldHaveAddress(PosAngleType) && Address == 0) return Double.NaN;
                switch (PosAngleType)
                {
                    case PositionAngleTypeEnum.Mario:
                        return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    case PositionAngleTypeEnum.Holp:
                        return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HolpYOffset);
                    case PositionAngleTypeEnum.Camera:
                        return Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.YOffset);
                    case PositionAngleTypeEnum.CameraFocus:
                        return Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.FocusYOffset);
                    case PositionAngleTypeEnum.CamHackCamera:
                        return Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                    case PositionAngleTypeEnum.CamHackFocus:
                        return Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                    case PositionAngleTypeEnum.Obj:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.YOffset);
                    case PositionAngleTypeEnum.ObjHome:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.HomeYOffset);
                    case PositionAngleTypeEnum.ObjGfx:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.GraphicsYOffset);
                    case PositionAngleTypeEnum.ObjScale:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.ScaleHeightOffset);
                    case PositionAngleTypeEnum.Selected:
                        {
                            List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                            if (objAddresses.Count == 0) return Double.NaN;
                            uint objAddress = objAddresses[0];
                            return Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                        }
                    case PositionAngleTypeEnum.First:
                        return GetObjectValue(Text, true, CoordinateAngle.Y);
                    case PositionAngleTypeEnum.Last:
                        return GetObjectValue(Text, false, CoordinateAngle.Y);
                    case PositionAngleTypeEnum.FirstHome:
                        return GetObjectValue(Text, true, CoordinateAngle.Y, home: true);
                    case PositionAngleTypeEnum.LastHome:
                        return GetObjectValue(Text, false, CoordinateAngle.Y, home: true);
                    case PositionAngleTypeEnum.GoombaProjection:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.YOffset);
                    case PositionAngleTypeEnum.KoopaTheQuick:
                        return PlushUtilities.GetY();
                    case PositionAngleTypeEnum.Tri:
                        return GetTriangleVertexComponent(Address.Value, Index.Value, Coordinate.Y);
                    case PositionAngleTypeEnum.ObjTri:
                        {
                            uint? triAddress = TriangleUtilities.GetTriangleAddressOfObjectTriangleIndex(Address.Value, Index.Value);
                            if (!triAddress.HasValue) return double.NaN;
                            return GetTriangleVertexComponent(triAddress.Value, Index2.Value, Coordinate.Y);
                        }
                    case PositionAngleTypeEnum.Wall:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset), Index.Value, Coordinate.Y);
                    case PositionAngleTypeEnum.Floor:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset), Index.Value, Coordinate.Y);
                    case PositionAngleTypeEnum.Ceiling:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset), Index.Value, Coordinate.Y);
                    case PositionAngleTypeEnum.Snow:
                        return GetSnowComponent(Index.Value, Coordinate.Y);
                    case PositionAngleTypeEnum.QFrame:
                        return GetQFrameComponent(Frame.Value, Coordinate.Y);
                    case PositionAngleTypeEnum.GFrame:
                        return GetGFrameComponent(Frame.Value, Coordinate.Y);
                    case PositionAngleTypeEnum.Schedule:
                        uint scheduleIndex = GetScheduleIndex();
                        if (Schedule.ContainsKey(scheduleIndex)) return Schedule[scheduleIndex].Item2;
                        return Double.NaN;
                    case PositionAngleTypeEnum.Hybrid:
                        return PosAngle1.Y;
                    case PositionAngleTypeEnum.Functions:
                        return Getters[1]();
                    case PositionAngleTypeEnum.Pos:
                        return ThisY.Value;
                    case PositionAngleTypeEnum.Ang:
                        return double.NaN;
                    case PositionAngleTypeEnum.Trunc:
                        return (int)PosAngle1.Y;
                    case PositionAngleTypeEnum.Self:
                        return SpecialConfig.SelfPA.Y;
                    case PositionAngleTypeEnum.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public virtual double Z
        {
            get
            {
                if (ShouldHaveAddress(PosAngleType) && Address == 0) return Double.NaN;
                switch (PosAngleType)
                {
                    case PositionAngleTypeEnum.Mario:
                        return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                    case PositionAngleTypeEnum.Holp:
                        return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HolpZOffset);
                    case PositionAngleTypeEnum.Camera:
                        return Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.ZOffset);
                    case PositionAngleTypeEnum.CameraFocus:
                        return Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.FocusZOffset);
                    case PositionAngleTypeEnum.CamHackCamera:
                        return Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    case PositionAngleTypeEnum.CamHackFocus:
                        return Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                    case PositionAngleTypeEnum.Obj:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.ZOffset);
                    case PositionAngleTypeEnum.ObjHome:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.HomeZOffset);
                    case PositionAngleTypeEnum.ObjGfx:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.GraphicsZOffset);
                    case PositionAngleTypeEnum.ObjScale:
                        return Config.Stream.GetSingle(Address.Value + ObjectConfig.ScaleDepthOffset);
                    case PositionAngleTypeEnum.Selected:
                        {
                            List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                            if (objAddresses.Count == 0) return Double.NaN;
                            uint objAddress = objAddresses[0];
                            return Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                        }
                    case PositionAngleTypeEnum.First:
                        return GetObjectValue(Text, true, CoordinateAngle.Z);
                    case PositionAngleTypeEnum.Last:
                        return GetObjectValue(Text, false, CoordinateAngle.Z);
                    case PositionAngleTypeEnum.FirstHome:
                        return GetObjectValue(Text, true, CoordinateAngle.Z, home: true);
                    case PositionAngleTypeEnum.LastHome:
                        return GetObjectValue(Text, false, CoordinateAngle.Z, home: true);
                    case PositionAngleTypeEnum.GoombaProjection:
                        return GetGoombaProjection(Address.Value).z;
                    case PositionAngleTypeEnum.KoopaTheQuick:
                        return PlushUtilities.GetZ();
                    case PositionAngleTypeEnum.Tri:
                        return GetTriangleVertexComponent(Address.Value, Index.Value, Coordinate.Z);
                    case PositionAngleTypeEnum.ObjTri:
                        {
                            uint? triAddress = TriangleUtilities.GetTriangleAddressOfObjectTriangleIndex(Address.Value, Index.Value);
                            if (!triAddress.HasValue) return double.NaN;
                            return GetTriangleVertexComponent(triAddress.Value, Index2.Value, Coordinate.Z);
                        }
                    case PositionAngleTypeEnum.Wall:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset), Index.Value, Coordinate.Z);
                    case PositionAngleTypeEnum.Floor:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset), Index.Value, Coordinate.Z);
                    case PositionAngleTypeEnum.Ceiling:
                        return GetTriangleVertexComponent(
                            Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset), Index.Value, Coordinate.Z);
                    case PositionAngleTypeEnum.Snow:
                        return GetSnowComponent(Index.Value, Coordinate.Z);
                    case PositionAngleTypeEnum.QFrame:
                        return GetQFrameComponent(Frame.Value, Coordinate.Z);
                    case PositionAngleTypeEnum.GFrame:
                        return GetGFrameComponent(Frame.Value, Coordinate.Z);
                    case PositionAngleTypeEnum.Schedule:
                        uint scheduleIndex = GetScheduleIndex();
                        if (Schedule.ContainsKey(scheduleIndex)) return Schedule[scheduleIndex].Item3;
                        return Double.NaN;
                    case PositionAngleTypeEnum.Hybrid:
                        return PosAngle1.Z;
                    case PositionAngleTypeEnum.Functions:
                        return Getters[2]();
                    case PositionAngleTypeEnum.Pos:
                        return ThisZ.Value;
                    case PositionAngleTypeEnum.Ang:
                        return double.NaN;
                    case PositionAngleTypeEnum.Trunc:
                        return (int)PosAngle1.Z;
                    case PositionAngleTypeEnum.Self:
                        return SpecialConfig.SelfPA.Z;
                    case PositionAngleTypeEnum.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public virtual double Angle
        {
            get
            {
                if (ShouldHaveAddress(PosAngleType) && Address == 0) return Double.NaN;
                switch (PosAngleType)
                {
                    case PositionAngleTypeEnum.Mario:
                        return Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    case PositionAngleTypeEnum.Holp:
                        return Double.NaN;
                    case PositionAngleTypeEnum.Camera:
                        return Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.FacingYawOffset);
                    case PositionAngleTypeEnum.CameraFocus:
                        return Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.FacingYawOffset);
                    case PositionAngleTypeEnum.CamHackCamera:
                        return CamHackUtilities.GetCamHackYawFacing();
                    case PositionAngleTypeEnum.CamHackFocus:
                        return CamHackUtilities.GetCamHackYawFacing();
                    case PositionAngleTypeEnum.Obj:
                        return Config.Stream.GetUInt16(Address.Value + ObjectConfig.YawFacingOffset);
                    case PositionAngleTypeEnum.ObjHome:
                        return Double.NaN;
                    case PositionAngleTypeEnum.ObjGfx:
                        return Config.Stream.GetUInt16(Address.Value + ObjectConfig.GraphicsYawOffset);
                    case PositionAngleTypeEnum.ObjScale:
                        return Double.NaN;
                    case PositionAngleTypeEnum.Selected:
                        {
                            List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                            if (objAddresses.Count == 0) return Double.NaN;
                            uint objAddress = objAddresses[0];
                            return Config.Stream.GetUInt16(objAddress + ObjectConfig.YawFacingOffset);
                        }
                    case PositionAngleTypeEnum.First:
                        return GetObjectValue(Text, true, CoordinateAngle.Angle);
                    case PositionAngleTypeEnum.Last:
                        return GetObjectValue(Text, false, CoordinateAngle.Angle);
                    case PositionAngleTypeEnum.FirstHome:
                        return GetObjectValue(Text, true, CoordinateAngle.Angle, home: true);
                    case PositionAngleTypeEnum.LastHome:
                        return GetObjectValue(Text, false, CoordinateAngle.Angle, home: true);
                    case PositionAngleTypeEnum.GoombaProjection:
                        return MoreMath.NormalizeAngleUshort(Config.Stream.GetInt32(Address.Value + ObjectConfig.GoombaTargetAngleOffset));
                    case PositionAngleTypeEnum.KoopaTheQuick:
                        return PlushUtilities.GetAngle();
                    case PositionAngleTypeEnum.Tri:
                        return Double.NaN;
                    case PositionAngleTypeEnum.ObjTri:
                        return double.NaN;
                    case PositionAngleTypeEnum.Wall:
                        return Double.NaN;
                    case PositionAngleTypeEnum.Floor:
                        return Double.NaN;
                    case PositionAngleTypeEnum.Ceiling:
                        return Double.NaN;
                    case PositionAngleTypeEnum.Snow:
                        return Double.NaN;
                    case PositionAngleTypeEnum.QFrame:
                        return Double.NaN;
                    case PositionAngleTypeEnum.GFrame:
                        return Double.NaN;
                    case PositionAngleTypeEnum.Schedule:
                        uint scheduleIndex = GetScheduleIndex();
                        if (Schedule.ContainsKey(scheduleIndex)) return Schedule[scheduleIndex].Item4;
                        return Double.NaN;
                    case PositionAngleTypeEnum.Hybrid:
                        return PosAngle2.Angle;
                    case PositionAngleTypeEnum.Functions:
                        if (Getters.Count >= 4) return Getters[3]();
                        return Double.NaN;
                    case PositionAngleTypeEnum.Pos:
                        return ThisAngle.Value;
                    case PositionAngleTypeEnum.Ang:
                        return ThisAngle.Value;
                    case PositionAngleTypeEnum.Trunc:
                        return MoreMath.NormalizeAngleTruncated(PosAngle1.Angle);
                    case PositionAngleTypeEnum.Self:
                        return SpecialConfig.SelfPA.Angle;
                    case PositionAngleTypeEnum.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public (double x, double y, double z, double angle) GetValues()
        {
            return (X, Y, Z, Angle);
        }

        public double GetAdditionalValue(int index)
        {
            if (PosAngleType != PositionAngleTypeEnum.Schedule) return Double.NaN;
            uint scheduleIndex = GetScheduleIndex();
            if (!Schedule.ContainsKey(scheduleIndex)) return Double.NaN;
            List<double> doubleList = Schedule[scheduleIndex].Item5;
            if (index < 0 || index >= doubleList.Count) return Double.NaN;
            return doubleList[index];
        }

        private static double GetObjectValue(string name, bool first, CoordinateAngle coordAngle, bool home = false, bool gfx = false)
        {
            List<ObjectDataModel> objs = Config.ObjectSlotsManager.GetLoadedObjectsWithName(name);
            ObjectDataModel obj = first ? objs.FirstOrDefault() : objs.LastOrDefault();
            uint? objAddress = obj?.Address;
            if (!objAddress.HasValue) return Double.NaN;
            switch (coordAngle)
            {
                case CoordinateAngle.X:
                    uint xOffset = home ? ObjectConfig.HomeXOffset : gfx ? ObjectConfig.GraphicsXOffset : ObjectConfig.XOffset;
                    return Config.Stream.GetSingle(objAddress.Value + xOffset);
                case CoordinateAngle.Y:
                    uint yOffset = home ? ObjectConfig.HomeYOffset : gfx ? ObjectConfig.GraphicsYOffset : ObjectConfig.YOffset;
                    return Config.Stream.GetSingle(objAddress.Value + yOffset);
                case CoordinateAngle.Z:
                    uint zOffset = home ? ObjectConfig.HomeZOffset : gfx ? ObjectConfig.GraphicsZOffset : ObjectConfig.ZOffset;
                    return Config.Stream.GetSingle(objAddress.Value + zOffset);
                case CoordinateAngle.Angle:
                    if (home) return Double.NaN;
                    if (gfx) return Config.Stream.GetUInt16(objAddress.Value + ObjectConfig.GraphicsYawOffset);
                    return Config.Stream.GetUInt16(objAddress.Value + ObjectConfig.YawFacingOffset);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static (double x, double z) GetGoombaProjection(uint address)
        {
            double startX = Config.Stream.GetSingle(address + ObjectConfig.XOffset);
            double startZ = Config.Stream.GetSingle(address + ObjectConfig.ZOffset);
            double hSpeed = Config.Stream.GetSingle(address + ObjectConfig.HSpeedOffset);
            int countdown = Config.Stream.GetInt32(address + ObjectConfig.GoombaCountdownOffset);
            ushort targetAngle = MoreMath.NormalizeAngleUshort(Config.Stream.GetInt32(address + ObjectConfig.GoombaTargetAngleOffset));
            return MoreMath.AddVectorToPoint(hSpeed * countdown, targetAngle, startX, startZ);
        }

        private static double GetTriangleVertexComponent(uint address, int index, Coordinate coordinate)
        {
            if (address == 0) return Double.NaN;
            switch (index)
            {
                case 1:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return TriangleOffsetsConfig.GetX1(address);
                        case Coordinate.Y:
                            return TriangleOffsetsConfig.GetY1(address);
                        case Coordinate.Z:
                            return TriangleOffsetsConfig.GetZ1(address);
                    }
                    break;
                case 2:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return TriangleOffsetsConfig.GetX2(address);
                        case Coordinate.Y:
                            return TriangleOffsetsConfig.GetY2(address);
                        case Coordinate.Z:
                            return TriangleOffsetsConfig.GetZ2(address);
                    }
                    break;
                case 3:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return TriangleOffsetsConfig.GetX3(address);
                        case Coordinate.Y:
                            return TriangleOffsetsConfig.GetY3(address);
                        case Coordinate.Z:
                            return TriangleOffsetsConfig.GetZ3(address);
                    }
                    break;
                case 4:
                    int closestVertexToMario = TriangleDataModel.Create(address).GetClosestVertex(
                        Mario.X, Mario.Y, Mario.Z);
                    return GetTriangleVertexComponent(address, closestVertexToMario, coordinate);
                case 5:
                    int closestVertexToSelf = TriangleDataModel.Create(address).GetClosestVertex(
                        SpecialConfig.SelfX, SpecialConfig.SelfY, SpecialConfig.SelfZ);
                    return GetTriangleVertexComponent(address, closestVertexToSelf, coordinate);
                case 6:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return Mario.X;
                        case Coordinate.Y:
                            return TriangleDataModel.Create(address).GetHeightOnTriangle(Mario.X, Mario.Z);
                        case Coordinate.Z:
                            return Mario.Z;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case 7:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return SpecialConfig.SelfX;
                        case Coordinate.Y:
                            return TriangleDataModel.Create(address).GetHeightOnTriangle(SpecialConfig.SelfX, SpecialConfig.SelfZ);
                        case Coordinate.Z:
                            return SpecialConfig.SelfZ;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
            throw new ArgumentOutOfRangeException();
        }

        private static double GetSnowComponent(int index, Coordinate coordinate)
        {
            short numSnowParticles = Config.Stream.GetInt16(SnowConfig.CounterAddress);
            if (index < 0 || index >= numSnowParticles) return Double.NaN;
            uint snowStart = Config.Stream.GetUInt32(SnowConfig.SnowArrayPointerAddress);
            uint structOffset = (uint)index * SnowConfig.ParticleStructSize;
            switch (coordinate)
            {
                case Coordinate.X:
                    return Config.Stream.GetInt32(snowStart + structOffset + SnowConfig.XOffset);
                case Coordinate.Y:
                    return Config.Stream.GetInt32(snowStart + structOffset + SnowConfig.YOffset);
                case Coordinate.Z:
                    return Config.Stream.GetInt32(snowStart + structOffset + SnowConfig.ZOffset);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static double GetQFrameComponent(double frame, Coordinate coordinate)
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            ushort angle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);

            (double pointX, double pointZ) = MoreMath.AddVectorToPoint(hSpeed * frame, angle, marioX, marioZ);
            double pointY = marioY;

            switch (coordinate)
            {
                case Coordinate.X:
                    return pointX;
                case Coordinate.Y:
                    return pointY;
                case Coordinate.Z:
                    return pointZ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static double GetGFrameComponent(double gFrame, Coordinate coordinate)
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            ushort angle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
            uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);

            double frame = gFrame - globalTimer;
            (double pointX, double pointZ) = MoreMath.AddVectorToPoint(hSpeed * frame, angle, marioX, marioZ);
            double pointY = marioY;

            switch (coordinate)
            {
                case Coordinate.X:
                    return pointX;
                case Coordinate.Y:
                    return pointY;
                case Coordinate.Z:
                    return pointZ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }




        public virtual bool SetX(double value)
        {
            if (ShouldHaveAddress(PosAngleType) && Address == 0) return false;
            switch (PosAngleType)
            {
                case PositionAngleTypeEnum.Mario:
                    return SetMarioComponent((float)value, Coordinate.X);
                case PositionAngleTypeEnum.Holp:
                    return Config.Stream.SetValue((float)value, MarioConfig.StructAddress + MarioConfig.HolpXOffset);
                case PositionAngleTypeEnum.Camera:
                    return Config.Stream.SetValue((float)value, CameraConfig.StructAddress + CameraConfig.XOffset);
                case PositionAngleTypeEnum.CameraFocus:
                    return Config.Stream.SetValue((float)value, CameraConfig.StructAddress + CameraConfig.FocusXOffset);
                case PositionAngleTypeEnum.CamHackCamera:
                    return Config.Stream.SetValue((float)value, CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                case PositionAngleTypeEnum.CamHackFocus:
                    return Config.Stream.SetValue((float)value, CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                case PositionAngleTypeEnum.Obj:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.XOffset);
                case PositionAngleTypeEnum.ObjHome:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.HomeXOffset);
                case PositionAngleTypeEnum.ObjGfx:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.GraphicsXOffset);
                case PositionAngleTypeEnum.ObjScale:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.ScaleWidthOffset);
                case PositionAngleTypeEnum.Selected:
                    {
                        List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                        if (objAddresses.Count == 0) return false;
                        uint objAddress = objAddresses[0];
                        return Config.Stream.SetValue((float)value, objAddress + ObjectConfig.XOffset);
                    }
                case PositionAngleTypeEnum.First:
                    return SetObjectValue(value, Text, true, CoordinateAngle.X);
                case PositionAngleTypeEnum.Last:
                    return SetObjectValue(value, Text, false, CoordinateAngle.X);
                case PositionAngleTypeEnum.FirstHome:
                    return SetObjectValue(value, Text, true, CoordinateAngle.X, home: true);
                case PositionAngleTypeEnum.LastHome:
                    return SetObjectValue(value, Text, false, CoordinateAngle.X, home: true);
                case PositionAngleTypeEnum.GoombaProjection:
                    return false;
                case PositionAngleTypeEnum.KoopaTheQuick:
                    return false;
                case PositionAngleTypeEnum.Tri:
                    return SetTriangleVertexComponent((short)value, Address.Value, Index.Value, Coordinate.X);
                case PositionAngleTypeEnum.ObjTri:
                    {
                        uint? triAddress = TriangleUtilities.GetTriangleAddressOfObjectTriangleIndex(Address.Value, Index.Value);
                        if (!triAddress.HasValue) return false;
                        return SetTriangleVertexComponent((short)value, triAddress.Value, Index2.Value, Coordinate.X);
                    }
                case PositionAngleTypeEnum.Wall:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset), Index.Value, Coordinate.X);
                case PositionAngleTypeEnum.Floor:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset), Index.Value, Coordinate.X);
                case PositionAngleTypeEnum.Ceiling:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset), Index.Value, Coordinate.X);
                case PositionAngleTypeEnum.Snow:
                    return SetSnowComponent((int)value, Index.Value, Coordinate.X);
                case PositionAngleTypeEnum.QFrame:
                    return false;
                case PositionAngleTypeEnum.GFrame:
                    return false;
                case PositionAngleTypeEnum.Schedule:
                    return false;
                case PositionAngleTypeEnum.Hybrid:
                    return PosAngle1.SetX(value);
                case PositionAngleTypeEnum.Functions:
                    return Setters[0](value);
                case PositionAngleTypeEnum.Pos:
                    ThisX = value;
                    return true;
                case PositionAngleTypeEnum.Ang:
                    return false;
                case PositionAngleTypeEnum.Trunc:
                    return PosAngle1.SetX(value);
                case PositionAngleTypeEnum.Self:
                    return SpecialConfig.SelfPA.SetX(value);
                case PositionAngleTypeEnum.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual bool SetY(double value)
        {
            if (ShouldHaveAddress(PosAngleType) && Address == 0) return false;
            switch (PosAngleType)
            {
                case PositionAngleTypeEnum.Mario:
                    return SetMarioComponent((float)value, Coordinate.Y);
                case PositionAngleTypeEnum.Holp:
                    return Config.Stream.SetValue((float)value, MarioConfig.StructAddress + MarioConfig.HolpYOffset);
                case PositionAngleTypeEnum.Camera:
                    return Config.Stream.SetValue((float)value, CameraConfig.StructAddress + CameraConfig.YOffset);
                case PositionAngleTypeEnum.CameraFocus:
                    return Config.Stream.SetValue((float)value, CameraConfig.StructAddress + CameraConfig.FocusYOffset);
                case PositionAngleTypeEnum.CamHackCamera:
                    return Config.Stream.SetValue((float)value, CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                case PositionAngleTypeEnum.CamHackFocus:
                    return Config.Stream.SetValue((float)value, CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                case PositionAngleTypeEnum.Obj:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.YOffset);
                case PositionAngleTypeEnum.ObjHome:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.HomeYOffset);
                case PositionAngleTypeEnum.ObjGfx:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.GraphicsYOffset);
                case PositionAngleTypeEnum.ObjScale:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.ScaleHeightOffset);
                case PositionAngleTypeEnum.Selected:
                    {
                        List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                        if (objAddresses.Count == 0) return false;
                        uint objAddress = objAddresses[0];
                        return Config.Stream.SetValue((float)value, objAddress + ObjectConfig.YOffset);
                    }
                case PositionAngleTypeEnum.First:
                    return SetObjectValue(value, Text, true, CoordinateAngle.Y);
                case PositionAngleTypeEnum.Last:
                    return SetObjectValue(value, Text, false, CoordinateAngle.Y);
                case PositionAngleTypeEnum.FirstHome:
                    return SetObjectValue(value, Text, true, CoordinateAngle.Y, home: true);
                case PositionAngleTypeEnum.LastHome:
                    return SetObjectValue(value, Text, false, CoordinateAngle.Y, home: true);
                case PositionAngleTypeEnum.GoombaProjection:
                    return false;
                case PositionAngleTypeEnum.KoopaTheQuick:
                    return false;
                case PositionAngleTypeEnum.Tri:
                    return SetTriangleVertexComponent((short)value, Address.Value, Index.Value, Coordinate.Y);
                case PositionAngleTypeEnum.ObjTri:
                    {
                        uint? triAddress = TriangleUtilities.GetTriangleAddressOfObjectTriangleIndex(Address.Value, Index.Value);
                        if (!triAddress.HasValue) return false;
                        return SetTriangleVertexComponent((short)value, triAddress.Value, Index2.Value, Coordinate.Y);
                    }
                case PositionAngleTypeEnum.Wall:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset), Index.Value, Coordinate.Y);
                case PositionAngleTypeEnum.Floor:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset), Index.Value, Coordinate.Y);
                case PositionAngleTypeEnum.Ceiling:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset), Index.Value, Coordinate.Y);
                case PositionAngleTypeEnum.Snow:
                    return SetSnowComponent((int)value, Index.Value, Coordinate.Y);
                case PositionAngleTypeEnum.QFrame:
                    return false;
                case PositionAngleTypeEnum.GFrame:
                    return false;
                case PositionAngleTypeEnum.Schedule:
                    return false;
                case PositionAngleTypeEnum.Hybrid:
                    return PosAngle1.SetY(value);
                case PositionAngleTypeEnum.Functions:
                    return Setters[1](value);
                case PositionAngleTypeEnum.Pos:
                    ThisY = value;
                    return true;
                case PositionAngleTypeEnum.Ang:
                    return false;
                case PositionAngleTypeEnum.Trunc:
                    return PosAngle1.SetY(value);
                case PositionAngleTypeEnum.Self:
                    return SpecialConfig.SelfPA.SetY(value);
                case PositionAngleTypeEnum.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual bool SetZ(double value)
        {
            if (ShouldHaveAddress(PosAngleType) && Address == 0) return false;
            switch (PosAngleType)
            {
                case PositionAngleTypeEnum.Mario:
                    return SetMarioComponent((float)value, Coordinate.Z);
                case PositionAngleTypeEnum.Holp:
                    return Config.Stream.SetValue((float)value, MarioConfig.StructAddress + MarioConfig.HolpZOffset);
                case PositionAngleTypeEnum.Camera:
                    return Config.Stream.SetValue((float)value, CameraConfig.StructAddress + CameraConfig.ZOffset);
                case PositionAngleTypeEnum.CameraFocus:
                    return Config.Stream.SetValue((float)value, CameraConfig.StructAddress + CameraConfig.FocusZOffset);
                case PositionAngleTypeEnum.CamHackCamera:
                    return Config.Stream.SetValue((float)value, CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                case PositionAngleTypeEnum.CamHackFocus:
                    return Config.Stream.SetValue((float)value, CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                case PositionAngleTypeEnum.Obj:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.ZOffset);
                case PositionAngleTypeEnum.ObjHome:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.HomeZOffset);
                case PositionAngleTypeEnum.ObjGfx:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.GraphicsZOffset);
                case PositionAngleTypeEnum.ObjScale:
                    return Config.Stream.SetValue((float)value, Address.Value + ObjectConfig.ScaleDepthOffset);
                case PositionAngleTypeEnum.Selected:
                    {
                        List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                        if (objAddresses.Count == 0) return false;
                        uint objAddress = objAddresses[0];
                        return Config.Stream.SetValue((float)value, objAddress + ObjectConfig.ZOffset);
                    }
                case PositionAngleTypeEnum.First:
                    return SetObjectValue(value, Text, true, CoordinateAngle.Z);
                case PositionAngleTypeEnum.Last:
                    return SetObjectValue(value, Text, false, CoordinateAngle.Z);
                case PositionAngleTypeEnum.FirstHome:
                    return SetObjectValue(value, Text, true, CoordinateAngle.Z, home: true);
                case PositionAngleTypeEnum.LastHome:
                    return SetObjectValue(value, Text, false, CoordinateAngle.Z, home: true);
                case PositionAngleTypeEnum.GoombaProjection:
                    return false;
                case PositionAngleTypeEnum.KoopaTheQuick:
                    return false;
                case PositionAngleTypeEnum.Tri:
                    return SetTriangleVertexComponent((short)value, Address.Value, Index.Value, Coordinate.Z);
                case PositionAngleTypeEnum.ObjTri:
                    {
                        uint? triAddress = TriangleUtilities.GetTriangleAddressOfObjectTriangleIndex(Address.Value, Index.Value);
                        if (!triAddress.HasValue) return false;
                        return SetTriangleVertexComponent((short)value, triAddress.Value, Index2.Value, Coordinate.Z);
                    }
                case PositionAngleTypeEnum.Wall:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset), Index.Value, Coordinate.Z);
                case PositionAngleTypeEnum.Floor:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset), Index.Value, Coordinate.Z);
                case PositionAngleTypeEnum.Ceiling:
                    return SetTriangleVertexComponent(
                        (short)value, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset), Index.Value, Coordinate.Z);
                case PositionAngleTypeEnum.Snow:
                    return SetSnowComponent((int)value, Index.Value, Coordinate.Z);
                case PositionAngleTypeEnum.QFrame:
                    return false;
                case PositionAngleTypeEnum.GFrame:
                    return false;
                case PositionAngleTypeEnum.Schedule:
                    return false;
                case PositionAngleTypeEnum.Hybrid:
                    return PosAngle1.SetZ(value);
                case PositionAngleTypeEnum.Functions:
                    return Setters[2](value);
                case PositionAngleTypeEnum.Pos:
                    ThisZ = value;
                    return true;
                case PositionAngleTypeEnum.Ang:
                    return false;
                case PositionAngleTypeEnum.Trunc:
                    return PosAngle1.SetZ(value);
                case PositionAngleTypeEnum.Self:
                    return SpecialConfig.SelfPA.SetZ(value);
                case PositionAngleTypeEnum.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual bool SetAngle(double value)
        {
            if (ShouldHaveAddress(PosAngleType) && Address == 0) return false;
            ushort valueUShort = MoreMath.NormalizeAngleUshort(value);
            switch (PosAngleType)
            {
                case PositionAngleTypeEnum.Mario:
                    return Config.Stream.SetValue(valueUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                case PositionAngleTypeEnum.Holp:
                    return false;
                case PositionAngleTypeEnum.Camera:
                    return Config.Stream.SetValue(valueUShort, CameraConfig.StructAddress + CameraConfig.FacingYawOffset);
                case PositionAngleTypeEnum.CameraFocus:
                    return false;
                case PositionAngleTypeEnum.CamHackCamera:
                    return false;
                case PositionAngleTypeEnum.CamHackFocus:
                    return false;
                case PositionAngleTypeEnum.Obj:
                    {
                        bool success = true;
                        success &= Config.Stream.SetValue(valueUShort, Address.Value + ObjectConfig.YawFacingOffset);
                        success &= Config.Stream.SetValue(valueUShort, Address.Value + ObjectConfig.YawMovingOffset);
                        return success;
                    }
                case PositionAngleTypeEnum.ObjHome:
                    return false;
                case PositionAngleTypeEnum.ObjGfx:
                    return Config.Stream.SetValue(valueUShort, Address.Value + ObjectConfig.GraphicsYawOffset);
                case PositionAngleTypeEnum.ObjScale:
                    return false;
                case PositionAngleTypeEnum.Selected:
                    {
                        List<uint> objAddresses = Config.ObjectSlotsManager.SelectedSlotsAddresses;
                        if (objAddresses.Count == 0) return false;
                        uint objAddress = objAddresses[0];
                        bool success = true;
                        success &= Config.Stream.SetValue(valueUShort, objAddress + ObjectConfig.YawFacingOffset);
                        success &= Config.Stream.SetValue(valueUShort, objAddress + ObjectConfig.YawMovingOffset);
                        return success;
                    }
                case PositionAngleTypeEnum.First:
                    return SetObjectValue(value, Text, true, CoordinateAngle.Angle);
                case PositionAngleTypeEnum.Last:
                    return SetObjectValue(value, Text, false, CoordinateAngle.Angle);
                case PositionAngleTypeEnum.FirstHome:
                    return SetObjectValue(value, Text, true, CoordinateAngle.Angle, home: true);
                case PositionAngleTypeEnum.LastHome:
                    return SetObjectValue(value, Text, false, CoordinateAngle.Angle, home: true);
                case PositionAngleTypeEnum.GoombaProjection:
                    return false;
                case PositionAngleTypeEnum.KoopaTheQuick:
                    return false;
                case PositionAngleTypeEnum.Tri:
                    return false;
                case PositionAngleTypeEnum.ObjTri:
                    return false;
                case PositionAngleTypeEnum.Wall:
                    return false;
                case PositionAngleTypeEnum.Floor:
                    return false;
                case PositionAngleTypeEnum.Ceiling:
                    return false;
                case PositionAngleTypeEnum.Snow:
                    return false;
                case PositionAngleTypeEnum.QFrame:
                    return false;
                case PositionAngleTypeEnum.GFrame:
                    return false;
                case PositionAngleTypeEnum.Schedule:
                    return false;
                case PositionAngleTypeEnum.Hybrid:
                    return PosAngle2.SetAngle(value);
                case PositionAngleTypeEnum.Functions:
                    if (Setters.Count >= 4) return Setters[3](value);
                    return false;
                case PositionAngleTypeEnum.Pos:
                    ThisAngle = value;
                    return true;
                case PositionAngleTypeEnum.Ang:
                    ThisAngle = value;
                    return true;
                case PositionAngleTypeEnum.Trunc:
                    return PosAngle1.SetAngle(value);
                case PositionAngleTypeEnum.Self:
                    return SpecialConfig.SelfPA.SetAngle(value);
                case PositionAngleTypeEnum.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool SetMarioComponent(float value, Coordinate coordinate)
        {
            bool success = true;
            bool streamAlreadySuspended = Config.Stream.IsSuspended;
            if (!streamAlreadySuspended) Config.Stream.Suspend();

            switch (coordinate)
            {
                case Coordinate.X:
                    success &= Config.Stream.SetValue(value, MarioConfig.StructAddress + MarioConfig.XOffset);
                    break;
                case Coordinate.Y:
                    success &= Config.Stream.SetValue(value, MarioConfig.StructAddress + MarioConfig.YOffset);
                    break;
                case Coordinate.Z:
                    success &= Config.Stream.SetValue(value, MarioConfig.StructAddress + MarioConfig.ZOffset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (KeyboardUtilities.IsAltHeld())
            {
                uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                switch (coordinate)
                {
                    case Coordinate.X:
                        success &= Config.Stream.SetValue(value, marioObjRef + ObjectConfig.GraphicsXOffset);
                        break;
                    case Coordinate.Y:
                        success &= Config.Stream.SetValue(value, marioObjRef + ObjectConfig.GraphicsYOffset);
                        break;
                    case Coordinate.Z:
                        success &= Config.Stream.SetValue(value, marioObjRef + ObjectConfig.GraphicsZOffset);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!streamAlreadySuspended) Config.Stream.Resume();
            return success;
        }

        private static bool SetObjectValue(double value, string name, bool first, CoordinateAngle coordAngle, bool home = false, bool gfx = false)
        {
            List<ObjectDataModel> objs = Config.ObjectSlotsManager.GetLoadedObjectsWithName(name);
            ObjectDataModel obj = first ? objs.FirstOrDefault() : objs.LastOrDefault();
            uint? objAddress = obj?.Address;
            if (!objAddress.HasValue) return false;
            switch (coordAngle)
            {
                case CoordinateAngle.X:
                    uint xOffset = home ? ObjectConfig.HomeXOffset : gfx ? ObjectConfig.GraphicsXOffset : ObjectConfig.XOffset;
                    return Config.Stream.SetValue((float)value, objAddress.Value + xOffset);
                case CoordinateAngle.Y:
                    uint yOffset = home ? ObjectConfig.HomeYOffset : gfx ? ObjectConfig.GraphicsYOffset : ObjectConfig.YOffset;
                    return Config.Stream.SetValue((float)value, objAddress.Value + yOffset);
                case CoordinateAngle.Z:
                    uint zOffset = home ? ObjectConfig.HomeZOffset : gfx ? ObjectConfig.GraphicsZOffset : ObjectConfig.ZOffset;
                    return Config.Stream.SetValue((float)value, objAddress.Value + zOffset);
                case CoordinateAngle.Angle:
                    if (home) return false;
                    if (gfx) return Config.Stream.SetValue(MoreMath.NormalizeAngleUshort(value), objAddress.Value + ObjectConfig.GraphicsYawOffset);
                    bool success = true;
                    success &= Config.Stream.SetValue(MoreMath.NormalizeAngleUshort(value), objAddress.Value + ObjectConfig.YawFacingOffset);
                    success &= Config.Stream.SetValue(MoreMath.NormalizeAngleUshort(value), objAddress.Value + ObjectConfig.YawMovingOffset);
                    return success;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool SetTriangleVertexComponent(short value, uint address, int index, Coordinate coordinate)
        {
            if (address == 0) return false;
            switch (index)
            {
                case 1:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return TriangleOffsetsConfig.SetX1(value, address);
                        case Coordinate.Y:
                            return TriangleOffsetsConfig.SetY1(value, address);
                        case Coordinate.Z:
                            return TriangleOffsetsConfig.SetZ1(value, address);
                    }
                    break;
                case 2:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return TriangleOffsetsConfig.SetX2(value, address);
                        case Coordinate.Y:
                            return TriangleOffsetsConfig.SetY2(value, address);
                        case Coordinate.Z:
                            return TriangleOffsetsConfig.SetZ2(value, address);
                    }
                    break;
                case 3:
                    switch (coordinate)
                    {
                        case Coordinate.X:
                            return TriangleOffsetsConfig.SetX3(value, address);
                        case Coordinate.Y:
                            return TriangleOffsetsConfig.SetY3(value, address);
                        case Coordinate.Z:
                            return TriangleOffsetsConfig.SetZ3(value, address);
                    }
                    break;
                case 4:
                    int closestVertexToMario = TriangleDataModel.Create(address).GetClosestVertex(
                        Mario.X, Mario.Y, Mario.Z);
                    return SetTriangleVertexComponent(value, address, closestVertexToMario, coordinate);
                case 5:
                    int closestVertexToSelf = TriangleDataModel.Create(address).GetClosestVertex(
                        SpecialConfig.SelfX, SpecialConfig.SelfY, SpecialConfig.SelfZ);
                    return SetTriangleVertexComponent(value, address, closestVertexToSelf, coordinate);
                case 6:
                    return false;
                case 7:
                    return false;
            }
            throw new ArgumentOutOfRangeException();
        }

        private static bool SetSnowComponent(int value, int index, Coordinate coordinate)
        {
            short numSnowParticles = Config.Stream.GetInt16(SnowConfig.CounterAddress);
            if (index < 0 || index > numSnowParticles) return false;
            uint snowStart = Config.Stream.GetUInt32(SnowConfig.SnowArrayPointerAddress);
            uint structOffset = (uint)index * SnowConfig.ParticleStructSize;
            switch (coordinate)
            {
                case Coordinate.X:
                    return Config.Stream.SetValue(value, snowStart + structOffset + SnowConfig.XOffset);
                case Coordinate.Y:
                    return Config.Stream.SetValue(value, snowStart + structOffset + SnowConfig.YOffset);
                case Coordinate.Z:
                    return Config.Stream.SetValue(value, snowStart + structOffset + SnowConfig.ZOffset);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool SetValues(double? x = null, double? y = null, double? z = null, double? angle = null)
        {
            bool success = true;
            if (x.HasValue) success &= SetX(x.Value);
            if (y.HasValue) success &= SetY(y.Value);
            if (z.HasValue) success &= SetZ(z.Value);
            if (angle.HasValue) success &= SetAngle(angle.Value);
            return success;
        }






        public static double GetDistance(PositionAngle p1, PositionAngle p2)
        {
            return MoreMath.GetDistanceBetween(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z);
        }

        public static double GetHDistance(PositionAngle p1, PositionAngle p2)
        {
            return MoreMath.GetDistanceBetween(p1.X, p1.Z, p2.X, p2.Z);
        }

        public static double GetXDistance(PositionAngle p1, PositionAngle p2)
        {
            return p2.X - p1.X;
        }

        public static double GetYDistance(PositionAngle p1, PositionAngle p2)
        {
            return p2.Y - p1.Y;
        }

        public static double GetZDistance(PositionAngle p1, PositionAngle p2)
        {
            return p2.Z - p1.Z;
        }

        public static double GetFDistance(PositionAngle p1, PositionAngle p2)
        {
            double hDist = MoreMath.GetDistanceBetween(p1.X, p1.Z, p2.X, p2.Z);
            double angle = MoreMath.AngleTo_AngleUnits(p1.X, p1.Z, p2.X, p2.Z);
            (double sidewaysDist, double forwardsDist) =
                MoreMath.GetComponentsFromVectorRelatively(hDist, angle, p1.Angle);
            return forwardsDist;
        }

        public static double GetSDistance(PositionAngle p1, PositionAngle p2)
        {
            double hDist = MoreMath.GetDistanceBetween(p1.X, p1.Z, p2.X, p2.Z);
            double angle = MoreMath.AngleTo_AngleUnits(p1.X, p1.Z, p2.X, p2.Z);
            (double sidewaysDist, double forwardsDist) =
                MoreMath.GetComponentsFromVectorRelatively(hDist, angle, p1.Angle);
            return sidewaysDist;
        }

        private static double AngleTo(double x1, double z1, double x2, double z2, bool inGameAngle, bool truncate)
        {
            double angleTo = inGameAngle
                ? InGameTrigUtilities.InGameAngleTo((float)x1, (float)z1, (float)x2, (float)z2)
                : MoreMath.AngleTo_AngleUnits(x1, z1, x2, z2);
            if (truncate) angleTo = MoreMath.NormalizeAngleTruncated(angleTo);
            return angleTo;
        }

        public static double GetAngleTo(PositionAngle p1, PositionAngle p2, bool? inGameAngleNullable, bool truncate)
        {
            bool inGameAngle = inGameAngleNullable ?? SavedSettingsConfig.UseInGameTrigForAngleLogic;
            return AngleTo(p1.X, p1.Z, p2.X, p2.Z, inGameAngle, truncate);
        }

        public static double GetDAngleTo(PositionAngle p1, PositionAngle p2, bool? inGameAngleNullable, bool truncate)
        {
            bool inGameAngle = inGameAngleNullable ?? SavedSettingsConfig.UseInGameTrigForAngleLogic;
            double angleTo = AngleTo(p1.X, p1.Z, p2.X, p2.Z, inGameAngle, truncate);
            double angle = truncate ? MoreMath.NormalizeAngleTruncated(p1.Angle) : p1.Angle;
            double angleDiff = angle - angleTo;
            return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
        }

        public static double GetAngleDifference(PositionAngle p1, PositionAngle p2, bool truncate)
        {
            double angle1 = truncate ? MoreMath.NormalizeAngleTruncated(p1.Angle) : p1.Angle;
            double angle2 = truncate ? MoreMath.NormalizeAngleTruncated(p2.Angle) : p2.Angle;
            double angleDiff = angle1 - angle2;
            return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
        }





        private static bool GetToggle()
        {
            return KeyboardUtilities.IsCtrlHeld();
        }

        public static bool SetDistance(PositionAngle p1, PositionAngle p2, double distance, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                (double x, double y, double z) = MoreMath.ExtrapolateLine3D(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, distance);
                return p2.SetValues(x: x, y: y, z: z);
            }
            else
            {
                (double x, double y, double z) = MoreMath.ExtrapolateLine3D(p2.X, p2.Y, p2.Z, p1.X, p1.Y, p1.Z, distance);
                return p1.SetValues(x: x, y: y, z: z);
            }
        }

        public static bool SetHDistance(PositionAngle p1, PositionAngle p2, double distance, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                (double x, double z) = MoreMath.ExtrapolateLine2D(p1.X, p1.Z, p2.X, p2.Z, distance);
                return p2.SetValues(x: x, z: z);
            }
            else
            {
                (double x, double z) = MoreMath.ExtrapolateLine2D(p2.X, p2.Z, p1.X, p1.Z, distance);
                return p1.SetValues(x: x, z: z);
            }
        }

        public static bool SetXDistance(PositionAngle p1, PositionAngle p2, double distance, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                double x = p1.X + distance;
                return p2.SetValues(x: x);
            }
            else
            {
                double x = p2.X - distance;
                return p1.SetValues(x: x);
            }
        }

        public static bool SetYDistance(PositionAngle p1, PositionAngle p2, double distance, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                double y = p1.Y + distance;
                return p2.SetValues(y: y);
            }
            else
            {
                double y = p2.Y - distance;
                return p1.SetValues(y: y);
            }
        }

        public static bool SetZDistance(PositionAngle p1, PositionAngle p2, double distance, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                double z = p1.Z + distance;
                return p2.SetValues(z: z);
            }
            else
            {
                double z = p2.Z - distance;
                return p1.SetValues(z: z);
            }
        }

        public static bool SetFDistance(PositionAngle p1, PositionAngle p2, double distance, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                (double x, double z) =
                    MoreMath.GetRelativelyOffsettedPosition(
                        p1.X, p1.Z, p1.Angle, p2.X, p2.Z, null, distance);
                return p2.SetValues(x: x, z: z);
            }
            else
            {
                (double x, double z) =
                    MoreMath.GetRelativelyOffsettedPosition(
                        p2.X, p2.Z, p1.Angle, p1.X, p1.Z, null, -1 * distance);
                return p1.SetValues(x: x, z: z);
            }
        }

        public static bool SetSDistance(PositionAngle p1, PositionAngle p2, double distance, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                (double x, double z) =
                    MoreMath.GetRelativelyOffsettedPosition(
                        p1.X, p1.Z, p1.Angle, p2.X, p2.Z, distance, null);
                return p2.SetValues(x: x, z: z);
            }
            else
            {
                (double x, double z) =
                    MoreMath.GetRelativelyOffsettedPosition(
                        p2.X, p2.Z, p1.Angle, p1.X, p1.Z, -1 * distance, null);
                return p1.SetValues(x: x, z: z);
            }
        }

        public static bool SetAngleTo(PositionAngle p1, PositionAngle p2, double angle, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                (double x, double z) =
                    MoreMath.RotatePointAboutPointToAngle(
                        p2.X, p2.Z, p1.X, p1.Z, angle);
                return p2.SetValues(x: x, z: z);
            }
            else
            {
                (double x, double z) =
                    MoreMath.RotatePointAboutPointToAngle(
                        p1.X, p1.Z, p2.X, p2.Z, MoreMath.ReverseAngle(angle));
                return p1.SetValues(x: x, z: z);
            }
        }

        public static bool SetDAngleTo(PositionAngle p1, PositionAngle p2, double angleDiff, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                double currentAngle = MoreMath.AngleTo_AngleUnits(p1.X, p1.Z, p2.X, p2.Z);
                double newAngle = currentAngle + angleDiff;
                return p1.SetValues(angle: newAngle);
            }
            else
            {
                double newAngle = p1.Angle - angleDiff;
                (double x, double z) =
                    MoreMath.RotatePointAboutPointToAngle(
                        p2.X, p2.Z, p1.X, p1.Z, newAngle);
                return p2.SetValues(x: x, z: z);
            }
        }

        public static bool SetAngleDifference(PositionAngle p1, PositionAngle p2, double angleDiff, bool? toggleNullable = null)
        {
            bool toggle = toggleNullable ?? GetToggle();
            if (!toggle)
            {
                double newAngle = p2.Angle + angleDiff;
                return p1.SetValues(angle: newAngle);
            }
            else
            {
                double newAngle = p1.Angle - angleDiff;
                return p2.SetValues(angle: newAngle);
            }
        }
    }
}
