using System;
using System.Collections.Generic;
using STROOP.Models;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs.MapTab.DataUtil
{
    public class CollisionStructure
    {
        const int NUM_CELLS = 16;
        List<TriangleDataModel>[,] static_cells = new List<TriangleDataModel>[NUM_CELLS, NUM_CELLS];
        List<TriangleDataModel>[,] dynamic_cells = new List<TriangleDataModel>[NUM_CELLS, NUM_CELLS];

        // Range level area is 16384x16384 (-8192 to +8192 in x and z)
        const short LEVEL_BOUNDARY_MAX = 0x2000; // 8192
        const short CELL_SIZE = (1 << 10); // 0x400
        const short NUM_CELLS_INDEX = (NUM_CELLS - 1);


        short lower_cell_index(short coord)
        {
            // Move from range [-0x2000, 0x2000) to [0, 0x4000)
            coord += LEVEL_BOUNDARY_MAX;
            if (coord < 0)
            {
                coord = 0;
            }

            // [0, 16)
            var index = coord / CELL_SIZE;

            // Include extra cell if close to boundary
            //! Some wall checks are larger than the buffer, meaning wall checks can
            //  miss walls that are near a cell border.
            if (coord % CELL_SIZE < 50)
            {
                index -= 1;
            }

            if (index < 0)
            {
                index = 0;
            }

            // Potentially > 15, but since the upper index is <= 15, not exploitable
            return (short)index;
        }

        /**
         * Every level is split into 16 * 16 cells of surfaces (to limit computing
         * time). This function determines the upper cell for a given x/z position.
         * @param coord The coordinate to test
         */
        short upper_cell_index(short coord)
        {
            // Move from range [-0x2000, 0x2000) to [0, 0x4000)
            coord += LEVEL_BOUNDARY_MAX;
            if (coord < 0)
            {
                coord = 0;
            }

            // [0, 16)
            var index = coord / CELL_SIZE;

            // Include extra cell if close to boundary
            //! Some wall checks are larger than the buffer, meaning wall checks can
            //  miss walls that are near a cell border.
            if (coord % CELL_SIZE > CELL_SIZE - 50)
            {
                index += 1;
            }

            if (index > NUM_CELLS_INDEX)
            {
                index = NUM_CELLS_INDEX;
            }

            // Potentially < 0, but since lower index is >= 0, not exploitable
            return (short)index;
        }

        /**
         * Every level is split into 16x16 cells, this takes a surface, finds
         * the appropriate cells (with a buffer), and adds the surface to those
         * cells.
         * @param surface The surface to check
         * @param dynamic Boolean determining whether the surface is static or dynamic
         */
        void add_surface(TriangleDataModel tri, bool dynamic)
        {
            // minY/maxY maybe? s32 instead of s16, though.

            short minX, minZ, maxX, maxZ;

            short minCellX, minCellZ, maxCellX, maxCellZ;

            short cellZ, cellX;

            minX = Math.Min(Math.Min(tri.X1, tri.X2), tri.X3);
            minZ = Math.Min(Math.Min(tri.Z1, tri.Z2), tri.Z3);
            maxX = Math.Max(Math.Max(tri.X1, tri.X2), tri.X3);
            maxZ = Math.Max(Math.Max(tri.Z1, tri.Z2), tri.Z3);

            minCellX = lower_cell_index(minX);
            maxCellX = upper_cell_index(maxX);
            minCellZ = lower_cell_index(minZ);
            maxCellZ = upper_cell_index(maxZ);

            for (cellZ = minCellZ; cellZ <= maxCellZ; cellZ++)
                for (cellX = minCellX; cellX <= maxCellX; cellX++)
                    (dynamic ? dynamic_cells : static_cells)[cellX, cellZ].Add(tri);
        }

        public CollisionStructure(List<TriangleDataModel> static_tris, List<TriangleDataModel> dynamic_tris)
        {
            for (int y = 0; y < NUM_CELLS; y++)
                for (int x = 0; x < NUM_CELLS; x++)
                {
                    static_cells[x, y] = new List<TriangleDataModel>();
                    dynamic_cells[x, y] = new List<TriangleDataModel>();
                }

            foreach (var tri in static_tris)
                add_surface(tri, false);
            foreach (var tri in dynamic_tris)
                add_surface(tri, true);

            for (int y = 0; y < NUM_CELLS; y++)
                for (int x = 0; x < NUM_CELLS; x++)
                {
                    static_cells[x, y].Sort((a, b) => a.Y1 == b.Y1 ? 0 : (b.Y1 > a.Y1 ? 1 : -1) * (a.Classification == Structs.TriangleClassification.Floor ? 1 : -1));
                    dynamic_cells[x, y].Sort((a, b) => a.Y1 == b.Y1 ? 0 : (b.Y1 > a.Y1 ? 1 : -1) * (a.Classification == Structs.TriangleClassification.Floor ? 1 : -1));
                }
        }

        static List<TriangleDataModel> GetCellTriangles(uint baseAddress)
        {
            var lst = new List<TriangleDataModel>();
            baseAddress = Config.Stream.GetUInt32(baseAddress);
            while (baseAddress != 0)
            {
                lst.Add(TriangleDataModel.Create(Config.Stream.GetUInt32(baseAddress + 4)));
                baseAddress = Config.Stream.GetUInt32(baseAddress);
            }

            return lst;
        }

        public CollisionStructure(Structs.TriangleClassification classification)
        {
            int typeSize = 2 * 4;
            int xSize = 3 * typeSize;
            int zSize = 16 * xSize;
            int type = (int)classification;

            for (int y = 0; y < NUM_CELLS; y++)
                for (int x = 0; x < NUM_CELLS; x++)
                {
                    var offset = y * zSize + x * xSize + type * typeSize;
                    static_cells[x, y] = GetCellTriangles((uint)(TriangleConfig.StaticTrianglePartitionAddress + offset));
                    dynamic_cells[x, y] = GetCellTriangles((uint)(TriangleConfig.DynamicTrianglePartitionAddress + offset));
                }
        }

        public List<TriangleDataModel> GetNearbyTriangles(float x, float z, float size, bool dynamic)
        {
            size = Math.Min(size, 50); //SM64 is funny like that
            var cellX = (int)((x + LEVEL_BOUNDARY_MAX) / CELL_SIZE) & NUM_CELLS_INDEX;
            var cellZ = (int)((z + LEVEL_BOUNDARY_MAX) / CELL_SIZE) & NUM_CELLS_INDEX;
            return (dynamic ? dynamic_cells : static_cells)[cellX, cellZ];
        }

        (TriangleDataModel floor, float floorY) FindFloorAndY(float x, float y, float z, bool dynamic)
        {
            var tris = GetNearbyTriangles(x, z, 0, dynamic);
            foreach (var tri in tris)
                if (tri.IsPointInsideAndAboveTriangle((short)x, (short)y, (short)z, out var truncatedHeight))
                    return (tri, truncatedHeight);
            return (null, float.NaN);
        }

        (TriangleDataModel floor, float ceilY) FindCeilingAndY(float x, float y, float z, bool dynamic)
        {
            var tris = GetNearbyTriangles(x, z, 0, dynamic);
            foreach (var tri in tris)
                if (tri.IsPointInsideAndBelowTriangle((short)x, (short)y, (short)z, out var truncatedHeight))
                    return (tri, truncatedHeight);
            return (null, float.NaN);
        }

        public (TriangleDataModel staticFloor, float floorY) FindFloorAndY(float x, float y, float z)
        {
            var static_tri = FindFloorAndY(x, y, z, false);
            var dynamic_tri = FindFloorAndY(x, y, z, true);
            if (dynamic_tri.floorY > static_tri.floorY) return dynamic_tri;
            return static_tri;
        }

        public (TriangleDataModel staticFloor, float ceilY) FindCeilingAndY(float x, float y, float z)
        {
            var static_tri = FindCeilingAndY(x, y, z, false);
            var dynamic_tri = FindCeilingAndY(x, y, z, true);
            if (dynamic_tri.ceilY < static_tri.ceilY) return dynamic_tri;
            return static_tri;
        }

        public int GetNumWallCollisions(
            float marioX, float marioY, float marioZ, float radius, float offsetY, bool dynamic)
        {
            var surfs = GetNearbyTriangles(marioX, marioZ, radius, dynamic);

            int output = 0;

            float offset;
            float x = marioX;
            float y = marioY + offsetY;
            float z = marioZ;
            float px, pz;
            float w1, w2, w3;
            float y1, y2, y3;

            // Max collision radius = 200
            if (radius > 200.0f) radius = 200.0f;

            foreach (TriangleDataModel surf in surfs)
            {
                if (y < surf.YMinMinus5 || y > surf.YMaxPlus5)
                    continue;

                offset = surf.NormX * x + surf.NormY * y + surf.NormZ * z + surf.NormOffset;

                if (offset < -radius || offset > radius)
                    continue;

                px = x;
                pz = z;

                if (surf.XProjection)
                {
                    w1 = -surf.Z1;
                    w2 = -surf.Z2;
                    w3 = -surf.Z3;
                    y1 = surf.Y1;
                    y2 = surf.Y2;
                    y3 = surf.Y3;

                    if (surf.NormX > 0.0f)
                    {
                        if ((y1 - y) * (w2 - w1) - (w1 - -pz) * (y2 - y1) > 0.0f) continue;
                        if ((y2 - y) * (w3 - w2) - (w2 - -pz) * (y3 - y2) > 0.0f) continue;
                        if ((y3 - y) * (w1 - w3) - (w3 - -pz) * (y1 - y3) > 0.0f) continue;
                    }
                    else
                    {
                        if ((y1 - y) * (w2 - w1) - (w1 - -pz) * (y2 - y1) < 0.0f) continue;
                        if ((y2 - y) * (w3 - w2) - (w2 - -pz) * (y3 - y2) < 0.0f) continue;
                        if ((y3 - y) * (w1 - w3) - (w3 - -pz) * (y1 - y3) < 0.0f) continue;
                    }
                }
                else
                {
                    w1 = surf.X1;
                    w2 = surf.X2;
                    w3 = surf.X3;
                    y1 = surf.Y1;
                    y2 = surf.Y2;
                    y3 = surf.Y3;

                    if (surf.NormZ > 0.0f)
                    {
                        if ((y1 - y) * (w2 - w1) - (w1 - px) * (y2 - y1) > 0.0f) continue;
                        if ((y2 - y) * (w3 - w2) - (w2 - px) * (y3 - y2) > 0.0f) continue;
                        if ((y3 - y) * (w1 - w3) - (w3 - px) * (y1 - y3) > 0.0f) continue;
                    }
                    else
                    {
                        if ((y1 - y) * (w2 - w1) - (w1 - px) * (y2 - y1) < 0.0f) continue;
                        if ((y2 - y) * (w3 - w2) - (w2 - px) * (y3 - y2) < 0.0f) continue;
                        if ((y3 - y) * (w1 - w3) - (w3 - px) * (y1 - y3) < 0.0f) continue;
                    }
                }

                output++;
            }

            return output;
        }
    }
}
