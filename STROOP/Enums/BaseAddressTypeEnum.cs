﻿using STROOP.Managers;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STROOP.Structs
{
    // TODO add new offset types
    public enum BaseAddressTypeEnum
    {
        Invalid,
        None,

        Absolute,
        Relative,

        Mario,
        MarioObj,
        Camera,
        CameraSettings,
        File,
        MainSave,
        Object,
        ProcessGroup,
        Triangle,
        TriangleExertionForceTable,
        CellsTriangle,
        CellsTriangleExertionForceTable,
        Floor,
        Wall,
        Ceiling,
        InputCurrent,
        InputJustPressed,
        InputBuffered,
        Graphics,
        Animation,
        Waypoint,
        Water,
        Area,
        Painting,
        Music,
        LastCoin,
        WarpDestination,

        HackedArea,
        CamHack,
        GhostHack,
    };
}
