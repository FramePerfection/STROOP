﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace STROOP.Structs
{
    public class TriangleInfoTable
    {
        public struct TriangleInfoReference
        {
            public short Type;
            public string Description;
            public short Slipperiness;
            public bool Exertion;

            public override int GetHashCode()
            {
                return Type;
            }
        }

        Dictionary<short, TriangleInfoReference> _typeTable = new Dictionary<short, TriangleInfoReference>();
        Dictionary<string, TriangleInfoReference> _descriptionTable = new Dictionary<string, TriangleInfoReference>();

        public TriangleInfoTable()
        {
        }

        public void Add(TriangleInfoReference triangleInfoRef)
        {
            _typeTable.Add(triangleInfoRef.Type, triangleInfoRef);
            if (!_descriptionTable.ContainsKey(triangleInfoRef.Description))
                _descriptionTable.Add(triangleInfoRef.Description, triangleInfoRef);
        }

        public string GetDescription(short type)
        {
            if (!_typeTable.ContainsKey(type))
                return "Unknown Type";
            return _typeTable[type].Description;
        }

        public short? GetType(string description)
        {
            if (!_descriptionTable.ContainsKey(description))
                return null;
            return _descriptionTable[description].Type;
        }

        public short? GetSlipperiness(short type)
        {
            if (!_typeTable.ContainsKey(type))
                return null;
            return _typeTable[type].Slipperiness;
        }

        public string GetSlipperinessDescription(short type)
        {
            short? slipperiness = GetSlipperiness(type);
            switch (slipperiness)
            {
                case 0x13:
                    return "Slide";
                case 0x14:
                    return "Slippery";
                case 0x00:
                    return "Default";
                case 0x15:
                    return "Non-Slippery";
                default:
                    return "Unknown Slipperiness";
            }
        }

        public double GetFrictionMultiplier(short type)
        {
            short? slipperiness = GetSlipperiness(type);
            switch (slipperiness)
            {
                case 0x13:
                    return 0.98;
                case 0x14:
                    return 0.96;
                case 0x00:
                    return 0.92;
                case 0x15:
                    return 0.92;
                default:
                    return Double.NaN;
            }
        }

        public double GetSlopeAccel(short type)
        {
            short? slipperiness = GetSlipperiness(type);
            switch (slipperiness)
            {
                case 0x13:
                    return 5.3;
                case 0x14:
                    return 2.7;
                case 0x00:
                    return 1.7;
                case 0x15:
                    return 0.0;
                default:
                    return Double.NaN;
            }
        }

        public double GetSlopeDecelValue(short type)
        {
            short? slipperiness = GetSlipperiness(type);
            switch (slipperiness)
            {
                case 0x13:
                    return 0.2;
                case 0x14:
                    return 0.7;
                case 0x00:
                    return 2.0;
                case 0x15:
                    return 3.0;
                default:
                    return Double.NaN;
            }
        }

        public bool? GetExertion(short type)
        {
            if (!_typeTable.ContainsKey(type))
                return null;
            return _typeTable[type].Exertion;
        }

        public List<string> GetAllDescriptions()
        {
            List<string> descriptions = _descriptionTable.Keys.ToList();
            descriptions.Sort();
            return descriptions;
        }
    }
}
