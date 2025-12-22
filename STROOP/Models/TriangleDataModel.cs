using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Structs;

namespace STROOP.Models
{
    public class TriangleDataModel
    {
        public readonly uint Address;

        public readonly short SurfaceType;
        public readonly byte ExertionForceIndex;
        public readonly byte ExertionAngle;
        public readonly byte Flags;
        public readonly byte Room;

        public readonly short YMinMinus5;
        public readonly short YMaxPlus5;

        public readonly short X1;
        public readonly short Y1;
        public readonly short Z1;
        public readonly short X2;
        public readonly short Y2;
        public readonly short Z2;
        public readonly short X3;
        public readonly short Y3;
        public readonly short Z3;

        public readonly float NormX;
        public readonly float NormY;
        public readonly float NormZ;
        public readonly float NormOffset;

        public Vector3 p1 => new Vector3(X1, Y1, Z1);
        public Vector3 p2 => new Vector3(X2, Y2, Z2);
        public Vector3 p3 => new Vector3(X3, Y3, Z3);

        public readonly uint AssociatedObject;

        public readonly TriangleClassification Classification;

        public readonly bool XProjection;
        public readonly bool BelongsToObject;
        public readonly bool NoCamCollision;

        public readonly string Description;
        public readonly short Slipperiness;
        public readonly string SlipperinessDescription;
        public readonly double FrictionMultiplier;
        public readonly double SlopeAccel;
        public readonly double SlopeDecelValue;
        public readonly bool Exertion;

        public readonly static List<string> FieldNameList = new List<string>
        {
            "Address",
            "Classification",
            "SurfaceType",
            "Description",
            "Slipperiness",
            "SlipperinessDescription",
            "Exertion",
            "ExertionForceIndex",
            "ExertionAngle",
            "Flags",
            "XProjection",
            "BelongsToObject",
            "NoCamCollision",
            "Room",
            "YMin-5",
            "YMax+5",
            "X1",
            "Y1",
            "Z1",
            "X2",
            "Y2",
            "Z2",
            "X3",
            "Y3",
            "Z3",
            "NormX",
            "NormY",
            "NormZ",
            "NormOffset",
            "AssociatedObject",
        };

        private readonly List<Object> FieldValueList;

        private static Dictionary<uint, TriangleDataModel> _cache = new Dictionary<uint, TriangleDataModel>();

        public static void ClearCache()
        {
            _cache.Clear();
        }

        public static TriangleDataModel Create(uint triangleAddress)
        {
            if (!_cache.ContainsKey(triangleAddress))
            {
                TriangleDataModel tri = new TriangleDataModel(triangleAddress);
                _cache[triangleAddress] = tri;
            }

            return _cache[triangleAddress];
        }

        protected TriangleDataModel(uint virtualAddress, int x1, int y1, int z1, int x2, int y2, int z2, int x3, int y3, int z3)
            : this(x1, y1, z1, x2, y2, z2, x3, y3, z3)
        {
            Address = virtualAddress;
        }

        private TriangleDataModel(uint triangleAddress)
        {
            Address = triangleAddress;

            SurfaceType = Config.Stream.GetInt16(triangleAddress + TriangleOffsetsConfig.SurfaceType);
            ExertionForceIndex = Config.Stream.GetByte(triangleAddress + TriangleOffsetsConfig.ExertionForceIndex);
            ExertionAngle = Config.Stream.GetByte(triangleAddress + TriangleOffsetsConfig.ExertionAngle);
            Flags = Config.Stream.GetByte(triangleAddress + TriangleOffsetsConfig.Flags);
            Room = Config.Stream.GetByte(triangleAddress + TriangleOffsetsConfig.Room);

            YMinMinus5 = Config.Stream.GetInt16(triangleAddress + TriangleOffsetsConfig.YMinMinus5);
            YMaxPlus5 = Config.Stream.GetInt16(triangleAddress + TriangleOffsetsConfig.YMaxPlus5);

            X1 = TriangleOffsetsConfig.GetX1(triangleAddress);
            Y1 = TriangleOffsetsConfig.GetY1(triangleAddress);
            Z1 = TriangleOffsetsConfig.GetZ1(triangleAddress);
            X2 = TriangleOffsetsConfig.GetX2(triangleAddress);
            Y2 = TriangleOffsetsConfig.GetY2(triangleAddress);
            Z2 = TriangleOffsetsConfig.GetZ2(triangleAddress);
            X3 = TriangleOffsetsConfig.GetX3(triangleAddress);
            Y3 = TriangleOffsetsConfig.GetY3(triangleAddress);
            Z3 = TriangleOffsetsConfig.GetZ3(triangleAddress);

            NormX = Config.Stream.GetSingle(triangleAddress + TriangleOffsetsConfig.NormX);
            NormY = Config.Stream.GetSingle(triangleAddress + TriangleOffsetsConfig.NormY);
            NormZ = Config.Stream.GetSingle(triangleAddress + TriangleOffsetsConfig.NormZ);
            NormOffset = Config.Stream.GetSingle(triangleAddress + TriangleOffsetsConfig.NormOffset);

            AssociatedObject = Config.Stream.GetUInt32(triangleAddress + TriangleOffsetsConfig.AssociatedObject);

            Classification = TriangleUtilities.CalculateClassification(NormY);

            XProjection = (Flags & TriangleOffsetsConfig.XProjectionMask) != 0;
            BelongsToObject = (Flags & TriangleOffsetsConfig.BelongsToObjectMask) != 0;
            NoCamCollision = (Flags & TriangleOffsetsConfig.NoCamCollisionMask) != 0;

            Description = TableConfig.TriangleInfo.GetDescription(SurfaceType);
            Slipperiness = TableConfig.TriangleInfo.GetSlipperiness(SurfaceType) ?? 0;
            SlipperinessDescription = TableConfig.TriangleInfo.GetSlipperinessDescription(SurfaceType);
            FrictionMultiplier = TableConfig.TriangleInfo.GetFrictionMultiplier(SurfaceType);
            SlopeAccel = TableConfig.TriangleInfo.GetSlopeAccel(SurfaceType);
            SlopeDecelValue = TableConfig.TriangleInfo.GetSlopeDecelValue(SurfaceType);
            Exertion = TableConfig.TriangleInfo.GetExertion(SurfaceType) ?? false;

            FieldValueList = new List<object>
            {
                HexUtilities.FormatValue(Address, 8),
                Classification,
                HexUtilities.FormatValue(SurfaceType, 2),
                Description,
                HexUtilities.FormatValue(Slipperiness, 2),
                SlipperinessDescription,
                Exertion,
                ExertionForceIndex,
                ExertionAngle,
                HexUtilities.FormatValue(Flags, 2),
                XProjection,
                BelongsToObject,
                NoCamCollision,
                Room,
                YMinMinus5,
                YMaxPlus5,
                X1,
                Y1,
                Z1,
                X2,
                Y2,
                Z2,
                X3,
                Y3,
                Z3,
                NormX,
                NormY,
                NormZ,
                NormOffset,
                HexUtilities.FormatValue(AssociatedObject, 8),
            };
        }

        public TriangleDataModel((int, int, int) p1, (int, int, int) p2, (int, int, int) p3) :
            this(p1.Item1, p1.Item2, p1.Item3, p2.Item1, p2.Item2, p2.Item3, p3.Item1, p3.Item2, p3.Item3)
        {
        }

        public TriangleDataModel(int x1, int y1, int z1, int x2, int y2, int z2, int x3, int y3, int z3)
        {
            X1 = (short)x1;
            Y1 = (short)y1;
            Z1 = (short)z1;
            X2 = (short)x2;
            Y2 = (short)y2;
            Z2 = (short)z2;
            X3 = (short)x3;
            Y3 = (short)y3;
            Z3 = (short)z3;

            (NormX, NormY, NormZ, NormOffset) = TriangleUtilities.GetNorms(x1, y1, z1, x2, y2, z2, x3, y3, z3);

            YMinMinus5 = (short)(MoreMath.Min(y1, y2, y3) - 5);
            YMaxPlus5 = (short)(MoreMath.Max(y1, y2, y3) + 5);

            XProjection = NormX < -0.707 || NormX > 0.707;
            Classification = TriangleUtilities.CalculateClassification(NormY);
        }

        public bool Intersect(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersection, out Vector3 normal)
        {
            intersection = default(Vector3);
            normal = default(Vector3);
            bool result = IntersectTriangle(rayOrigin, rayDirection, p1, p2, p3, out float t, out float u, out float v, out Vector3 n);
            if (result)
            {
                intersection = rayOrigin + t * rayDirection;
                normal = Vector3.Normalize(n);
            }

            return result;
        }

        public static bool IntersectTriangle(Vector3 RayOrigin, Vector3 RayDirection, Vector3 A, Vector3 B, Vector3 C, out float t, out float u, out float v, out Vector3 N)
        {
            Vector3 E1 = B - A;
            Vector3 E2 = C - A;
            N = Vector3.Cross(E1, E2);
            float det = -Vector3.Dot(RayDirection, N);
            float invdet = 1.0f / det;
            Vector3 AO = RayOrigin - A;
            Vector3 DAO = Vector3.Cross(AO, RayDirection);
            u = Vector3.Dot(E2, DAO) * invdet;
            v = -Vector3.Dot(E1, DAO) * invdet;
            t = Vector3.Dot(AO, N) * invdet;
            return (Math.Abs(det) >= 1e-6 && t >= 0.0 && u >= 0.0 && v >= 0.0 && (u + v) <= 1.0);
        }

        public override string ToString()
        {
            return String.Join("\t", FieldValueList);
        }

        public static string GetFieldNameString()
        {
            return String.Join("\t", FieldNameList);
        }

        public bool IsWall()
        {
            return Classification == TriangleClassification.Wall;
        }

        public bool IsFloor()
        {
            return Classification == TriangleClassification.Floor;
        }

        public bool IsCeiling()
        {
            return Classification == TriangleClassification.Ceiling;
        }

        public short GetMinX()
        {
            return Math.Min(X1, Math.Min(X2, X3));
        }

        public short GetMaxX()
        {
            return Math.Max(X1, Math.Max(X2, X3));
        }

        public short GetMinY()
        {
            return Math.Min(Y1, Math.Min(Y2, Y3));
        }

        public short GetMaxY()
        {
            return Math.Max(Y1, Math.Max(Y2, Y3));
        }

        public short GetMinZ()
        {
            return Math.Min(Z1, Math.Min(Z2, Z3));
        }

        public short GetMaxZ()
        {
            return Math.Max(Z1, Math.Max(Z2, Z3));
        }

        public int GetRangeX()
        {
            return GetMaxX() - GetMinX();
        }

        public int GetRangeY()
        {
            return GetMaxY() - GetMinY();
        }

        public int GetRangeZ()
        {
            return GetMaxZ() - GetMinZ();
        }

        public double GetMidpointX()
        {
            return (GetMinX() + GetMaxX()) / 2.0;
        }

        public double GetMidpointY()
        {
            return (GetMinY() + GetMaxY()) / 2.0;
        }

        public double GetMidpointZ()
        {
            return (GetMinZ() + GetMaxZ()) / 2.0;
        }

        public List<(float x, float z)> Get2DVertices()
        {
            return new List<(float, float)>()
            {
                (X1, Z1), (X2, Z2), (X3, Z3)
            };
        }

        public List<(float x, float y, float z)> Get3DVertices()
        {
            return new List<(float, float, float)>()
            {
                (X1, Y1, Z1), (X2, Y2, Z2), (X3, Y3, Z3)
            };
        }

        public double GetDistToMidpoint()
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            return MoreMath.GetDistanceBetween(marioX, marioY, marioZ, GetMidpointX(), GetMidpointY(), GetMidpointZ());
        }

        public int GetClosestVertex()
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            return GetClosestVertex(marioX, marioY, marioZ);
        }

        public int GetClosestVertex(double x, double y, double z)
        {
            double dist1 = MoreMath.GetDistanceBetween(X1, Y1, Z1, x, y, z);
            double dist2 = MoreMath.GetDistanceBetween(X2, Y2, Z2, x, y, z);
            double dist3 = MoreMath.GetDistanceBetween(X3, Y3, Z3, x, y, z);

            if (dist1 <= dist2 && dist1 <= dist3) return 1;
            if (dist2 <= dist3) return 2;
            return 3;
        }

        public double GetHeightOnTriangle(double x, double z)
        {
            return -(x * NormX + NormZ * z + NormOffset) / NormY;
        }

        public float GetTruncatedHeightOnTriangle(double doubleX, double doubleZ)
        {
            int x = (int)doubleX;
            int z = (int)doubleZ;
            return -(x * NormX + NormZ * z + NormOffset) / NormY;
        }

        public static double GetHeightOnTriangle(
            double x, double z, double normX, double normY, double normZ, double normOffset)
        {
            return (-x * normX - z * normZ - normOffset) / normY;
        }

        public bool IsPointInsideAndAboveTriangle(short shortX, short shortY, short shortZ, out float truncatedHeight)
        {
            truncatedHeight = float.NaN;
            if (!IsInsideTriangle(shortX, shortZ, X1, Z1, X2, Z2, X3, Z3)) return false;

            truncatedHeight = -(shortX * NormX + NormZ * shortZ + NormOffset) / NormY;

            if (shortY < truncatedHeight - 78) return false;

            return true;
        }

        static int Side(short pX, short pZ, short v1X, short v1Z, short v2X, short v2Z) => (v1Z - pZ) * (v2X - v1X) - (v1X - pX) * (v2Z - v1Z);

        static bool IsInsideTriangle(short pX, short pZ, short v1X, short v1Z, short v2X, short v2Z, short v3X, short v3Z)
        {
            int side12 = Side(pX, pZ, v1X, v1Z, v2X, v2Z);
            int side23 = Side(pX, pZ, v2X, v2Z, v3X, v3Z);
            int side31 = Side(pX, pZ, v3X, v3Z, v1X, v1Z);
            return (side12 <= 0 && side23 <= 0 && side31 <= 0) || (side12 >= 0 && side23 >= 0 && side31 >= 0);
        }

        public bool IsPointInsideAndBelowTriangle(short shortX, short shortY, short shortZ, out float truncatedHeight)
        {
            truncatedHeight = float.NaN;
            if (!IsInsideTriangle(shortX, shortZ, X1, Z1, X2, Z2, X3, Z3)) return false;

            truncatedHeight = -(shortX * NormX + NormZ * shortZ + NormOffset) / NormY;

            if (shortY > truncatedHeight + 78) return false;

            return true;
        }

        public bool IsPointInsideAndWithinTriangle(double doubleX, double doubleY, double doubleZ)
        {
            short shortX = (short)doubleX;
            short shortY = (short)doubleY;
            short shortZ = (short)doubleZ;

            if (!MoreMath.IsPointInsideTriangle(shortX, shortZ, X1, Z1, X2, Z2, X3, Z3)) return false;

            double heightOnTriangle = GetHeightOnTriangle(shortX, shortZ, NormX, NormY, NormZ, NormOffset);
            if (shortY < heightOnTriangle - 78 || shortY > heightOnTriangle) return false;

            return true;
        }

        public double GetVerticalDistAwayFromTriangleHitbox(double doubleX, double doubleY, double doubleZ)
        {
            short shortX = (short)doubleX;
            short shortY = (short)doubleY;
            short shortZ = (short)doubleZ;

            double heightOnTriangle = GetHeightOnTriangle(shortX, shortZ, NormX, NormY, NormZ, NormOffset);
            if (shortY < heightOnTriangle - 78) return shortY - (heightOnTriangle - 78);
            if (shortY > heightOnTriangle) return shortY - heightOnTriangle;

            return 0;
        }

        public float? GetTruncatedHeightOnTriangleIfInsideTriangle(double doubleX, double doubleZ)
        {
            short shortX = (short)doubleX;
            short shortZ = (short)doubleZ;
            if (!MoreMath.IsPointInsideTriangle(shortX, shortZ, X1, Z1, X2, Z2, X3, Z3)) return null;
            return GetTruncatedHeightOnTriangle(doubleX, doubleZ);
        }

        public bool IsTriWithinVerticalDistOfCenter(float? withinDistNullable, float centerY)
        {
            if (!withinDistNullable.HasValue) return true;
            float withinDist = withinDistNullable.Value;
            short minY = GetMinY();
            short maxY = GetMaxY();
            bool triTooFarDown = centerY - maxY > withinDist;
            bool triTooFarUp = minY - centerY > withinDist;
            return !triTooFarDown && !triTooFarUp;
        }
    }
}
