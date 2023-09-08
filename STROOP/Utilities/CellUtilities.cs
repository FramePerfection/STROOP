﻿using STROOP.Models;
using System.Collections.Generic;

namespace STROOP.Structs
{
    public static class CellUtilities
    {
        /**
         * Returns the lowest of three values.
         */
        private static short min_3(short a0, short a1, short a2) {
            if (a1 < a0) {
                a0 = a1;
            }

            if (a2 < a0) {
                a0 = a2;
            }

            return a0;
        }

        /**
         * Returns the highest of three values.
         */
        private static short max_3(short a0, short a1, short a2) {
            if (a1 > a0) {
                a0 = a1;
            }

            if (a2 > a0) {
                a0 = a2;
            }

            return a0;
        }

        /**
         * Every level is split into 16 * 16 cells of surfaces (to limit computing
         * time). This function determines the lower cell for a given x/z position.
         * @param coord The coordinate to test
         */
        private static short lower_cell_index(short coord, bool buffer = true) {
            short index;

            // Move from range [-0x2000, 0x2000) to [0, 0x4000)
            coord += 0x2000;
            if (coord < 0) {
                coord = 0;
            }

            // [0, 16)
            index = (short)(coord / 0x400);

            if (buffer)
            {
                // Include extra cell if close to boundary
                //! Some wall checks are larger than the buffer, meaning wall checks can
                //  miss walls that are near a cell border.
                if (coord % 0x400 < 50)
                {
                    index -= 1;
                }
            }

            if (index < 0) {
                index = 0;
            }

            // Potentially > 15, but since the upper index is <= 15, not exploitable
            return index;
        }

        /**
         * Every level is split into 16 * 16 cells of surfaces (to limit computing
         * time). This function determines the upper cell for a given x/z position.
         * @param coord The coordinate to test
         */
        private static short upper_cell_index(short coord, bool buffer = true) {
            short index;

            // Move from range [-0x2000, 0x2000) to [0, 0x4000)
            coord += 0x2000;
            if (coord < 0) {
                coord = 0;
            }

            // [0, 16)
            index = (short)(coord / 0x400);

            if (buffer)
            {
                // Include extra cell if close to boundary
                //! Some wall checks are larger than the buffer, meaning wall checks can
                //  miss walls that are near a cell border.
                if (coord % 0x400 > 0x400 - 50)
                {
                    index += 1;
                }
            }

            if (index > 15) {
                index = 15;
            }

            // Potentially < 0, but since lower index is >= 0, not exploitable
            return index;
        }

        /**
         * Every level is split into 16x16 cells, this takes a surface, finds
         * the appropriate cells (with a buffer), and adds the surface to those
         * cells.
         * @param surface The surface to check
         * @param dynamic Boolean determining whether the surface is static or dynamic
         */
        public static List<(int x, int z)> GetCells(TriangleDataModel tri) {
            // minY/maxY maybe? s32 instead of s16, though.
            short minX, minZ, maxX, maxZ;

            short minCellX, minCellZ, maxCellX, maxCellZ;

            short cellZ, cellX;

            minX = min_3(tri.X1, tri.X2, tri.X3);
            minZ = min_3(tri.Z1, tri.Z2, tri.Z3);
            maxX = max_3(tri.X1, tri.X2, tri.X3);
            maxZ = max_3(tri.Z1, tri.Z2, tri.Z3);

            minCellX = lower_cell_index(minX);
            maxCellX = upper_cell_index(maxX);
            minCellZ = lower_cell_index(minZ);
            maxCellZ = upper_cell_index(maxZ);

            List<(int x, int z)> cells = new List<(int x, int z)>();
            for (cellZ = minCellZ; cellZ <= maxCellZ; cellZ++) {
                for (cellX = minCellX; cellX <= maxCellX; cellX++) {
                    cells.Add((cellX, cellZ));
                }
            }
            return cells;
        }

        public static List<(int x, int z)> GetCells(short minX, short maxX, short minZ, short maxZ)
        {
            short minCellX, minCellZ, maxCellX, maxCellZ;

            short cellZ, cellX;

            minCellX = lower_cell_index(minX, false);
            maxCellX = upper_cell_index(maxX, false);
            minCellZ = lower_cell_index(minZ, false);
            maxCellZ = upper_cell_index(maxZ, false);

            List<(int x, int z)> cells = new List<(int x, int z)>();
            for (cellZ = minCellZ; cellZ <= maxCellZ; cellZ++)
            {
                for (cellX = minCellX; cellX <= maxCellX; cellX++)
                {
                    cells.Add((cellX, cellZ));
                }
            }
            return cells;
        }
    }
}
