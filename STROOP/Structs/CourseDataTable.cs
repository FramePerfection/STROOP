﻿using System.Collections.Generic;

namespace STROOP.Structs
{
    public class CourseDataTable
    {
        public struct CourseDataReference
        {
            public int Index;
            public string FullName;
            public string ShortName;
            public byte MaxCoinsWithoutGlitches;
            public byte MaxCoinsWithGlitches;

            public override int GetHashCode()
            {
                return Index;
            }
        }

        Dictionary<int, CourseDataReference> _table = new Dictionary<int, CourseDataReference>();

        public CourseDataTable()
        {
        }

        public void Add(CourseDataReference courseDataRef)
        {
            _table.Add(courseDataRef.Index, courseDataRef);
        }

        public byte? GetMaxCoinsWithoutGlitches(int index)
        {
            if (!_table.ContainsKey(index))
                return null;

            return _table[index].MaxCoinsWithoutGlitches;
        }

        public byte? GetMaxCoinsWithGlitches(int index)
        {
            if (!_table.ContainsKey(index))
                return null;

            return _table[index].MaxCoinsWithGlitches;
        }

        public string GetFullName(int index)
        {
            if (!_table.ContainsKey(index))
                return null;

            return _table[index].FullName;
        }
    }
}
