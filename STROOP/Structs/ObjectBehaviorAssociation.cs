﻿using STROOP.Controls;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STROOP.Structs
{
    public class ObjectBehaviorAssociation
    {
        public BehaviorCriteria Criteria;

        public string Name;
        public bool RotatesOnMap;
        public string ImagePath = "";
        public string MapImagePath = "";
        public Lazy<Image> Image;
        public Lazy<Image> TransparentImage;
        public Lazy<Image> MapImage;
        public PushHitbox PushHitbox;
        public List<WatchVariable> Precursors = new List<WatchVariable>();

        private List<WatchVariableControl> _watchVariableControls = null;
        public List<WatchVariableControl> WatchVariableControls
        {
            get
            {
                if (_watchVariableControls == null)
                    _watchVariableControls = Precursors.ConvertAll(precursor => new WatchVariableControl(precursor));
                return _watchVariableControls;
            }
        }

        public bool MeetsCriteria(BehaviorCriteria behaviorCriteria)
        {
            return Criteria.CongruentTo(behaviorCriteria);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectBehaviorAssociation))
                return false;

            var otherBehavior = (ObjectBehaviorAssociation)obj;
            return otherBehavior == this;
        }

        public override int GetHashCode()
        {
            return Criteria.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator ==(ObjectBehaviorAssociation a, ObjectBehaviorAssociation b)
        {
            if (object.ReferenceEquals(a, null))
                return object.ReferenceEquals(b, null);
            else if (object.ReferenceEquals(b, null))
                return false;

            return a.Criteria == b.Criteria;
        }

        public static bool operator !=(ObjectBehaviorAssociation a, ObjectBehaviorAssociation b)
        {
            if (object.ReferenceEquals(a, null))
                return !object.ReferenceEquals(b, null);
            else if (object.ReferenceEquals(b, null))
                return true;

            return a.Criteria != b.Criteria;
        }
    }
}
