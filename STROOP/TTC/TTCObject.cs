﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace STROOP.Ttc
{
    /** RNGObject is an abstract class that applies to
     *  every object that calls RNG. An RNGObject has an
     *  update method that updates the object's state
     *  exactly 1 frame forward, during which it may call RNG.
     */
    public abstract class TtcObject
    {

        //variables to tweak how objects are printed
        public static readonly string OPENER = "(";
	    public static readonly string SEPARATOR = ",";
	    public static readonly string CLOSER = ")";
	
	    //id is an identifier for this object (e.g. "Cog 2")
	    public string id;

        //the frame that the objects are updating to
        public int currentFrame;

        /** id begins as the object's class name by default.
         */
        public TtcObject()
        {
            id = GetType().Name.ToString();
        }

        /** Update the object exactly one frame.
         */
        public abstract void update();

        /** Return a string representation of the object, used for
         *  debugging purposes and to see the internal state.
         */
        public override abstract string ToString();

        /** Set the index of the object. This helps distinguish
         *  objects when printing them (e.g. helps make the distinction
         *  between "Bobomb 1" and "Bobomb 2").
         */
        public TtcObject setIndex(int index)
        {
            id = GetType().Name.ToString() + index;
            return this;
        }

        /** Poll RNG from the RNG manager.
         */
        protected int pollRNG()
        {
            return TtcRng.pollRNG();
        }

        /** Sets the frame that the object should be updating to.
         */
        public void setFrame(int currentFrame)
        {
            this.currentFrame = currentFrame;
        }

        /** Returns an angle between 0 and 65535 inclusive
         *  by using mods.
         */
        protected int normalize(int angle)
        {
            return ((angle % 65536) + 65536) % 65536;
        }


        /** Returns a new number that is the current number moved towards
         * the target number by at most max displacement.
         */
        protected int moveNumberTowards(int currentNumber, int targetNumber, int maxDisplacement)
        {
            if (currentNumber == targetNumber)
            { //exactly equal to target
                return currentNumber;
            }
            else if (currentNumber < targetNumber)
            { //lower than target
                int diff = targetNumber - currentNumber;
                int newNumber = currentNumber + Math.Min(diff, maxDisplacement);
                return newNumber;
            }
            else
            { //higher than target
                int diff = currentNumber - targetNumber;
                int newNumber = currentNumber - Math.Min(diff, maxDisplacement);
                return newNumber;
            }
        }

        /** Returns a new angle that is the current angle moved towards the target angle
         *  in the closer direction by at most max displacement. Normalization is included.
         */
        protected int moveAngleTowards(int currentAngle, int targetAngle, int maxDisplacement)
        {
            if (currentAngle == targetAngle) return currentAngle;
            int diff = targetAngle - currentAngle;
            diff = (diff + 65536) % 65536;

            int newAngle;
            if (diff < 32768)
            { //target is slightly above current
                newAngle = currentAngle + Math.Min(diff, maxDisplacement);
            }
            else
            { //target is slightly below current
                newAngle = currentAngle - Math.Min(65536 - diff, maxDisplacement);
            }
            return normalize(newAngle);
        }
    }
}
