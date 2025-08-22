using System;
using System.Collections.Generic;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Models;
using STROOP.Tabs.MapTab.MapObjects;
using STROOP.Utilities;
using System.Windows.Forms;
using System.Linq;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.DataUtil
{
    public class ObjectTrianglePrediction
    {
        //each collisionData entry is s16
        const uint collisionDataSize = 2;

        bool showingPredictions = false;
        ToolStripMenuItem itemIncludePrediction = new ToolStripMenuItem("Show far triangles (prediction)");

        List<TriangleDataModel> bufferedTris;
        MapObject.PositionAngleProvider positionAngleProvider;
        Predicate<TriangleDataModel> filter = null;
        uint lastGlobalTimer;
        Dictionary<uint, Dictionary<uint, (int, int, int)>> cachedFaceAngles = new Dictionary<uint, Dictionary<uint, (int, int, int)>>();

        public ObjectTrianglePrediction(MapObject.PositionAngleProvider positionAngleProvider, Predicate<TriangleDataModel> filter)
        {
            this.positionAngleProvider = positionAngleProvider;
            this.filter = filter;
            itemIncludePrediction.Checked = true;
            Update();
        }

        public void AddContextMenuItems(ToolStripItemCollection target)
        {
            itemIncludePrediction.Click += (_, __) => itemIncludePrediction.Checked = !itemIncludePrediction.Checked;
            target.Add(itemIncludePrediction);
        }

        public void Update()
        {
            var globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);

            if (globalTimer != lastGlobalTimer || bufferedTris == null || showingPredictions != itemIncludePrediction.Checked)
            {
                showingPredictions = itemIncludePrediction.Checked;
                try
                {
                    uint globalTimerMinus1 = (uint)(globalTimer - 1);

                    var loadedObjTriangles = new List<TriangleDataModel>();
                    var faceAngles = new Dictionary<uint, (int, int, int)>();
                    foreach (var obj in positionAngleProvider())
                    {
                        var objAddress = PositionAngle.GetObjectAddress(obj);
                        if (objAddress == 0)
                            continue;
                        loadedObjTriangles.AddRange(TriangleUtilities.GetObjectTrianglesForObject(objAddress).Where(t => filter(t)));

                        if (showingPredictions)
                        {
                            var oFlags = Config.Stream.GetInt32(objAddress + 0x8C);
                            if ((oFlags & (1 << 4)) != 0) //OBJ_FLAG_SET_FACE_ANGLE_TO_MOVE_ANGLE
                                faceAngles[objAddress] = (
                                    Config.Stream.GetInt32(objAddress + 0xD0),
                                    Config.Stream.GetInt32(objAddress + 0xD4),
                                    Config.Stream.GetInt32(objAddress + 0xD8));
                        }
                    }
                    if (faceAngles.Count > 0)
                        this.cachedFaceAngles[globalTimer] = faceAngles;

                    var predictions = ComputeNewTriangles(positionAngleProvider, globalTimerMinus1);

                    bufferedTris = new List<TriangleDataModel>(loadedObjTriangles);
                    if (showingPredictions)
                        foreach (var tri in predictions)
                        {
                            if (filter != null && !filter(tri))
                                continue;
                            if (loadedObjTriangles.Any(loaded =>
                                loaded.p1 == tri.p1 &&
                                loaded.p2 == tri.p2 &&
                                loaded.p3 == tri.p3
                            ))
                                continue;
                            bufferedTris.Add(tri);
                        }

                    lastGlobalTimer = globalTimer;
                }
                catch { /*inconsistent game states can fail to read predictions*/ }
            }
        }
        public List<TriangleDataModel> GetTriangles() => bufferedTris;

        List<TriangleDataModel> ComputeNewTriangles(MapObject.PositionAngleProvider positionAngleProvider, uint gTimerMinus1)
        {
            const short TERRAIN_LOAD_CONTINUE = 0x0041;
            var triangleList = new List<TriangleDataModel>();

            foreach (var pa in positionAngleProvider())
            {
                var objAddress = PositionAngle.GetObjectAddress(pa);

                var objCollisionPointerOffset = 0x218u;
                var collisionPointer = Config.Stream.GetInt32(objAddress + objCollisionPointerOffset);
                if (collisionPointer == 0)
                    continue;

                uint collisionData = (uint)(collisionPointer + collisionDataSize);
                short readCollisionData;

                var oFlags = Config.Stream.GetInt32(objAddress + 0x8C);
                int angleX, angleY, angleZ;


                if ((oFlags & (1 << 4)) != 0 //OBJ_FLAG_SET_FACE_ANGLE_TO_MOVE_ANGLE
                    && cachedFaceAngles.TryGetValue(gTimerMinus1, out var cacheDictionary)
                    && cacheDictionary.TryGetValue(objAddress, out var cached))
                {
                    angleX = cached.Item1;
                    angleY = cached.Item2;
                    angleZ = cached.Item3;
                }
                else
                {
                    //Still better than nothing if the object isn't moving (awkwardly, e.g. Spindel)
                    angleX = Config.Stream.GetInt32(objAddress + 0xD0);
                    angleY = Config.Stream.GetInt32(objAddress + 0xD4);
                    angleZ = Config.Stream.GetInt32(objAddress + 0xD8);
                }

                var vertexData = transform_object_vertices(ref collisionData, objAddress, (short)angleX, (short)angleY, (short)angleZ);
                // TERRAIN_LOAD_CONTINUE acts as an "end" to the terrain data.
                while ((readCollisionData = Config.Stream.GetInt16(collisionData)) != TERRAIN_LOAD_CONTINUE)
                    load_object_surfaces(triangleList, ref collisionData, vertexData);
            }
            return triangleList;
        }

        short ReadCollisionData(ref uint dataPtr)
        {
            var result = Config.Stream.GetInt16(dataPtr);
            dataPtr += collisionDataSize;
            return result;
        }

        bool surface_has_force(short surfaceType)
        {
            switch (surfaceType)
            {
                case 0x0004: // SURFACE_0004: // Unused
                case 0x000E: // SURFACE_FLOWING_WATER:
                case 0x0024: // SURFACE_DEEP_MOVING_QUICKSAND:
                case 0x0025: // SURFACE_SHALLOW_MOVING_QUICKSAND:
                case 0x0027: // SURFACE_MOVING_QUICKSAND:
                case 0x002C: // SURFACE_HORIZONTAL_WIND:
                case 0x002D: // SURFACE_INSTANT_MOVING_QUICKSAND:
                    return true;
                default:
                    return false;
            }
        }

        /**
         * Initialize a surface from reading its data and putting it into a surface
         * stuct.
         */
        TriangleDataModel read_surface_data(short[] vertexData, uint vertexIndicesPtr)
        {
            int x1, y1, z1;
            int x2, y2, z2;
            int x3, y3, z3;
            int maxY, minY;
            float nx, ny, nz;
            float mag;
            int offset1, offset2, offset3;

            offset1 = 3 * ReadCollisionData(ref vertexIndicesPtr);
            offset2 = 3 * ReadCollisionData(ref vertexIndicesPtr);
            offset3 = 3 * ReadCollisionData(ref vertexIndicesPtr);

            x1 = vertexData[offset1 + 0];
            y1 = vertexData[offset1 + 1];
            z1 = vertexData[offset1 + 2];

            x2 = vertexData[offset2 + 0];
            y2 = vertexData[offset2 + 1];
            z2 = vertexData[offset2 + 2];

            x3 = vertexData[offset3 + 0];
            y3 = vertexData[offset3 + 1];
            z3 = vertexData[offset3 + 2];

            // (v2 - v1) x (v3 - v2)
            nx = (y2 - y1) * (z3 - z2) - (z2 - z1) * (y3 - y2);
            ny = (z2 - z1) * (x3 - x2) - (x2 - x1) * (z3 - z2);
            nz = (x2 - x1) * (y3 - y2) - (y2 - y1) * (x3 - x2);
            mag = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);

            // Could have used min_3 and max_3 for this...
            minY = y1;
            if (y2 < minY)
                minY = y2;
            if (y3 < minY)
                minY = y3;

            maxY = y1;
            if (y2 > maxY)
                maxY = y2;
            if (y3 > maxY)
                maxY = y3;

            // Checking to make sure no DIV/0
            if (mag < 0.0001)
                return null;

            mag = (float)(1.0 / mag);
            nx *= mag;
            ny *= mag;
            nz *= mag;

            return new VirtualTriangleDataModel(x1, y1, z1, x2, y2, z2, x3, y3, z3);
        }

        class VirtualTriangleDataModel : TriangleDataModel
        {
            static uint virtualTriangleAddrIndex = 1;
            public VirtualTriangleDataModel(int x1, int y1, int z1, int x2, int y2, int z2, int x3, int y3, int z3)
            : base(0xFF000000 | virtualTriangleAddrIndex++, x1, y1, z1, x2, y2, z2, x3, y3, z3) { }
        }

        /**
         * Load in the surfaces for the gCurrentObject. This includes setting the flags,
         * exertion, and room.
         */
        void load_object_surfaces(List<TriangleDataModel> targetList, ref uint dataPtr, short[] vertexData)
        {
            short surfaceType;
            int i;
            int numSurfaces;
            bool hasForce;

            surfaceType = ReadCollisionData(ref dataPtr);

            numSurfaces = ReadCollisionData(ref dataPtr);

            hasForce = surface_has_force(surfaceType);

            for (i = 0; i < numSurfaces; i++)
            {
                TriangleDataModel surface = read_surface_data(vertexData, dataPtr);
                if (surface != null)
                    targetList.Add(surface);

                if (hasForce)
                    dataPtr += 4 * collisionDataSize;
                else
                    dataPtr += 3 * collisionDataSize;
            }
        }

        /**
         * Applies an object's tranformation to the object's vertices.
         */
        short[] transform_object_vertices(ref uint dataPtr, uint gCurrentObject, short objAngleX, short objAngleY, short objAngleZ)
        {
            const uint objGfxScaleOffset = 0x2C;
            const uint objPosOffset = 0xA0;
            const uint objTransformOffset = 0x21C;
            const uint objThrowMatrixOffset = 0x50;

            float objScaleX = Config.Stream.GetSingle(gCurrentObject + objGfxScaleOffset);
            float objScaleY = Config.Stream.GetSingle(gCurrentObject + objGfxScaleOffset + 0x4);
            float objScaleZ = Config.Stream.GetSingle(gCurrentObject + objGfxScaleOffset + 0x8);

            float objPosX = Config.Stream.GetSingle(gCurrentObject + objPosOffset);
            float objPosY = Config.Stream.GetSingle(gCurrentObject + objPosOffset + 0x4);
            float objPosZ = Config.Stream.GetSingle(gCurrentObject + objPosOffset + 0x8);

            int numVertices;

            float vx, vy, vz;

            numVertices = Config.Stream.GetInt16(dataPtr);
            dataPtr += collisionDataSize;

            short[] result = new short[numVertices * 3];
            int i = 0;

            Matrix4 m;
            uint throwMtxPtr = Config.Stream.GetUInt32(gCurrentObject + objThrowMatrixOffset);
            if (throwMtxPtr != 0)
            {
                uint transformPtr = gCurrentObject + objTransformOffset;
                uint idx = 0;
                float F() => Config.Stream.GetSingle(transformPtr + idx++ * 4);
                m = new Matrix4(F(), F(), F(), F(), F(), F(), F(), F(), F(), F(), F(), F(), F(), F(), F(), F());
            }
            else
                m = mtxf_rotate_zxy_and_translate(new Vector3(objPosX, objPosY, objPosZ), objAngleX, objAngleY, objAngleZ);

            //apply_object_scale_to_matrix(gCurrentObject, m, *objectTransform);
            m.M11 *= objScaleX;
            m.M12 *= objScaleY;
            m.M13 *= objScaleZ;
            m.M21 *= objScaleX;
            m.M22 *= objScaleY;
            m.M23 *= objScaleZ;
            m.M31 *= objScaleX;
            m.M32 *= objScaleY;
            m.M33 *= objScaleZ;

            // Go through all vertices, rotating and translating them to transform the object.
            while (numVertices-- > 0)
            {
                vx = Config.Stream.GetInt16(dataPtr); dataPtr += collisionDataSize;
                vy = Config.Stream.GetInt16(dataPtr); dataPtr += collisionDataSize;
                vz = Config.Stream.GetInt16(dataPtr); dataPtr += collisionDataSize;

                //! No bounds check on vertex data
                result[i++] = (short)(vx * m.M11 + vy * m.M21 + vz * m.M31 + m.M41);
                result[i++] = (short)(vx * m.M12 + vy * m.M22 + vz * m.M32 + m.M42);
                result[i++] = (short)(vx * m.M13 + vy * m.M23 + vz * m.M33 + m.M43);
            }
            return result;
        }


        /**
         * Build a matrix that rotates around the z axis, then the x axis, then the y
         * axis, and then translates.
         */
        static Matrix4 mtxf_rotate_zxy_and_translate(Vector3 translate, short rotate0, short rotate1, short rotate2)
        {
            Matrix4 dest = new Matrix4();

            float sx = InGameTrigUtilities.InGameSine(rotate0);
            float cx = InGameTrigUtilities.InGameCosine(rotate0);

            float sy = InGameTrigUtilities.InGameSine(rotate1);
            float cy = InGameTrigUtilities.InGameCosine(rotate1);

            float sz = InGameTrigUtilities.InGameSine(rotate2);
            float cz = InGameTrigUtilities.InGameCosine(rotate2);

            dest.M11/*[0][0]*/ = cy * cz + sx * sy * sz;
            dest.M21/*[1][0]*/ = -cy * sz + sx * sy * cz;
            dest.M31/*[2][0]*/ = cx * sy;
            dest.M41/*[3][0]*/ = translate[0];

            dest.M12/*[0][1]*/ = cx * sz;
            dest.M22/*[1][1]*/ = cx * cz;
            dest.M32/*[2][1]*/ = -sx;
            dest.M42/*[3][1]*/ = translate[1];

            dest.M13/*[0][2]*/ = -sy * cz + sx * cy * sz;
            dest.M23/*[1][2]*/ = sy * sz + sx * cy * cz;
            dest.M33/*[2][2]*/ = cx * cy;
            dest.M43/*[3][2]*/ = translate[2];

            dest.M14/*[0][3]*/ = dest.M24/*[1][3]*/ = dest.M34/*[2][3]*/ = 0.0f;
            dest.M44/*[3][3]*/ = 1.0f;
            return dest;
        }
    }

}
