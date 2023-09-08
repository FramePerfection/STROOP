﻿using STROOP.Models;
using STROOP.Utilities;
using System;
using System.Collections.Generic;

namespace STROOP.Structs
{
    public static class AirMovementCalculator
    {
        public static MarioState ApplyInput(MarioState marioState, Input input, int numQSteps = 4, List<TriangleDataModel> wallTris = null, List<MarioState> quarterSteps = null, bool resetHSpeedOnWalls = false)
        {
            MarioState withHSpeed = ComputeAirHSpeed(marioState, input);
            MarioState moved = AirMove(withHSpeed, numQSteps, wallTris, quarterSteps, resetHSpeedOnWalls);
            MarioState withYSpeed = ComputeAirYSpeed(moved);
            return withYSpeed;
        }

        public static MarioState ApplyInput(MarioState marioState, int angleDiff, int numQSteps = 4, List<TriangleDataModel> wallTris = null, List<MarioState> quarterSteps = null, bool resetHSpeedOnWalls = false)
        {
            MarioState withHSpeed = ComputeAirHSpeed(marioState, angleDiff);
            MarioState moved = AirMove(withHSpeed, numQSteps, wallTris, quarterSteps, resetHSpeedOnWalls);
            MarioState withYSpeed = ComputeAirYSpeed(moved);
            return withYSpeed;
        }

        public static MarioState ApplyInput(MarioState marioState, RelativeDirection direction, int numQSteps = 4)
        {
            MarioState withHSpeed = ComputeAirHSpeed(marioState, direction);
            MarioState moved = AirMove(withHSpeed, numQSteps);
            MarioState withYSpeed = ComputeAirYSpeed(moved);
            return withYSpeed;
        }

        public static MarioState ApplyInputRepeatedly(MarioState marioState, RelativeDirection direction, int numQSteps)
        {
            int numFrames = numQSteps / 4;
            int remainderQSteps = numQSteps % 4;
            for (int i = 0; i < numFrames; i++)
            {
                marioState = ApplyInput(marioState, direction);
            }
            return remainderQSteps == 0 ? marioState : ApplyInput(marioState, direction, remainderQSteps);
        }

        public static MarioState AirMove(MarioState initialState, int numQSteps = 4, List<TriangleDataModel> wallTris = null, List<MarioState> quarterSteps = null, bool resetHSpeedOnWalls = false)
        {
            bool resetHSpeed = false;

            float newX = initialState.X;
            float newY = initialState.Y;
            float newZ = initialState.Z;

            if (wallTris != null)
            {
                bool collidedWithWall;
                (newX, newZ, collidedWithWall) = WallDisplacementCalculator.HandleWallDisplacement2(newX, newY, newZ, wallTris, 50, 60);
                if (collidedWithWall && resetHSpeedOnWalls) resetHSpeed = true;
            }

            for (int i = 0; i < numQSteps; i++)
            {
                newX += initialState.XSpeed / 4;
                newY += initialState.YSpeed / 4;
                newZ += initialState.ZSpeed / 4;

                if (wallTris != null)
                {
                    bool collidedWithWall1;
                    bool collidedWithWall2;
                    (newX, newZ, collidedWithWall1) = WallDisplacementCalculator.HandleWallDisplacement2(newX, newY, newZ, wallTris, 50, 150);
                    (newX, newZ, collidedWithWall2) = WallDisplacementCalculator.HandleWallDisplacement2(newX, newY, newZ, wallTris, 50, 30);
                    if (collidedWithWall1 && resetHSpeedOnWalls) resetHSpeed = true;
                    if (collidedWithWall2 && resetHSpeedOnWalls) resetHSpeed = true;
                }

                if (quarterSteps != null)
                {
                    quarterSteps.Add(
                        new MarioState(
                            newX,
                            newY,
                            newZ,
                            initialState.XSpeed,
                            initialState.YSpeed,
                            initialState.ZSpeed,
                            initialState.HSpeed,
                            initialState.SlidingSpeedX,
                            initialState.SlidingSpeedZ,
                            initialState.SlidingAngle,
                            initialState.MarioAngle,
                            initialState.CameraAngle,
                            initialState.PreviousState,
                            initialState.LastInput,
                            initialState.Index));
                }
            }

            return new MarioState(
                newX,
                newY,
                newZ,
                initialState.XSpeed,
                initialState.YSpeed,
                initialState.ZSpeed,
                resetHSpeed ? 0 : initialState.HSpeed,
                initialState.SlidingSpeedX,
                initialState.SlidingSpeedZ,
                initialState.SlidingAngle,
                initialState.MarioAngle,
                initialState.CameraAngle,
                initialState.PreviousState,
                initialState.LastInput,
                initialState.Index);
        }

        // update_air_without_turn
        private static MarioState ComputeAirHSpeed(MarioState initialState, int angleDiff)
        {
            bool longJump = false;
            int maxSpeed = longJump ? 48 : 32;

            ushort marioAngle = initialState.MarioAngle;
            int deltaAngleIntendedFacing = angleDiff;
            float inputScaledMagnitude = 32;

            float perpSpeed = 0;
            float newHSpeed = ApproachHSpeed(initialState.HSpeed, 0, 0.35f, 0.35f);
            if (inputScaledMagnitude > 0)
            {
                newHSpeed += (inputScaledMagnitude / 32) * 1.5f * InGameTrigUtilities.InGameCosine(deltaAngleIntendedFacing);
                perpSpeed = InGameTrigUtilities.InGameSine(deltaAngleIntendedFacing) * (inputScaledMagnitude / 32) * 10;
            }

            if (newHSpeed > maxSpeed) newHSpeed -= 1;
            if (newHSpeed < -16) newHSpeed += 2;

            float newSlidingXSpeed = InGameTrigUtilities.InGameSine(marioAngle) * newHSpeed;
            float newSlidingZSpeed = InGameTrigUtilities.InGameCosine(marioAngle) * newHSpeed;
            newSlidingXSpeed += perpSpeed * InGameTrigUtilities.InGameSine(marioAngle + 0x4000);
            newSlidingZSpeed += perpSpeed * InGameTrigUtilities.InGameCosine(marioAngle + 0x4000);
            float newXSpeed = newSlidingXSpeed;
            float newZSpeed = newSlidingZSpeed;

            return new MarioState(
                initialState.X,
                initialState.Y,
                initialState.Z,
                newXSpeed,
                initialState.YSpeed,
                newZSpeed,
                newHSpeed,
                initialState.SlidingSpeedX,
                initialState.SlidingSpeedZ,
                initialState.SlidingAngle,
                initialState.MarioAngle,
                initialState.CameraAngle,
                initialState,
                new Input(angleDiff, 0),
                initialState.Index + 1);
        }

        // update_air_without_turn
        private static MarioState ComputeAirHSpeed(MarioState initialState, Input input)
        {
            bool longJump = false;
            int maxSpeed = longJump ? 48 : 32;

            ushort marioAngle = initialState.MarioAngle;
            ushort yawIntended = MoreMath.CalculateAngleFromInputs(input.X, input.Y, initialState.CameraAngle);
            int deltaAngleIntendedFacing = yawIntended - marioAngle;
            float inputScaledMagnitude = input.GetScaledMagnitude();

            float perpSpeed = 0;
            float newHSpeed = ApproachHSpeed(initialState.HSpeed, 0, 0.35f, 0.35f);
            if (inputScaledMagnitude > 0)
            {
                newHSpeed += (inputScaledMagnitude / 32) * 1.5f * InGameTrigUtilities.InGameCosine(deltaAngleIntendedFacing);
                perpSpeed = InGameTrigUtilities.InGameSine(deltaAngleIntendedFacing) * (inputScaledMagnitude / 32) * 10;
            }

            if (newHSpeed > maxSpeed) newHSpeed -= 1;
            if (newHSpeed < -16) newHSpeed += 2;

            float newSlidingXSpeed = InGameTrigUtilities.InGameSine(marioAngle) * newHSpeed;
            float newSlidingZSpeed = InGameTrigUtilities.InGameCosine(marioAngle) * newHSpeed;
            newSlidingXSpeed += perpSpeed * InGameTrigUtilities.InGameSine(marioAngle + 0x4000);
            newSlidingZSpeed += perpSpeed * InGameTrigUtilities.InGameCosine(marioAngle + 0x4000);
            float newXSpeed = newSlidingXSpeed;
            float newZSpeed = newSlidingZSpeed;

            return new MarioState(
                initialState.X,
                initialState.Y,
                initialState.Z,
                newXSpeed,
                initialState.YSpeed,
                newZSpeed,
                newHSpeed,
                initialState.SlidingSpeedX,
                initialState.SlidingSpeedZ,
                initialState.SlidingAngle,
                initialState.MarioAngle,
                initialState.CameraAngle,
                initialState,
                input,
                initialState.Index + 1);
        }

        // update_air_without_turn
        private static MarioState ComputeAirHSpeed(MarioState initialState, RelativeDirection direction)
        {
            bool longJump = false;
            int maxSpeed = longJump ? 48 : 32;

            ushort marioAngle = initialState.MarioAngle;
            int deltaAngleIntendedFacing;
            switch (direction)
            {
                case RelativeDirection.Forward:
                    deltaAngleIntendedFacing = 0;
                    break;
                case RelativeDirection.Backward:
                    deltaAngleIntendedFacing = 32768;
                    break;
                case RelativeDirection.Left:
                    deltaAngleIntendedFacing = 16384;
                    break;
                case RelativeDirection.Right:
                    deltaAngleIntendedFacing = 49152;
                    break;
                case RelativeDirection.Center:
                    deltaAngleIntendedFacing = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            float inputScaledMagnitude = direction == RelativeDirection.Center ? 0 : 32;

            float perpSpeed = 0;
            float newHSpeed = ApproachHSpeed(initialState.HSpeed, 0, 0.35f, 0.35f);
            if (inputScaledMagnitude > 0)
            {
                newHSpeed += (inputScaledMagnitude / 32) * 1.5f * InGameTrigUtilities.InGameCosine(deltaAngleIntendedFacing);
                perpSpeed = InGameTrigUtilities.InGameSine(deltaAngleIntendedFacing) * (inputScaledMagnitude / 32) * 10;
            }

            if (newHSpeed > maxSpeed) newHSpeed -= 1;
            if (newHSpeed < -16) newHSpeed += 2;

            float newSlidingXSpeed = InGameTrigUtilities.InGameSine(marioAngle) * newHSpeed;
            float newSlidingZSpeed = InGameTrigUtilities.InGameCosine(marioAngle) * newHSpeed;
            newSlidingXSpeed += perpSpeed * InGameTrigUtilities.InGameSine(marioAngle + 0x4000);
            newSlidingZSpeed += perpSpeed * InGameTrigUtilities.InGameCosine(marioAngle + 0x4000);
            float newXSpeed = newSlidingXSpeed;
            float newZSpeed = newSlidingZSpeed;

            return new MarioState(
                initialState.X,
                initialState.Y,
                initialState.Z,
                newXSpeed,
                initialState.YSpeed,
                newZSpeed,
                newHSpeed,
                initialState.SlidingSpeedX,
                initialState.SlidingSpeedZ,
                initialState.SlidingAngle,
                initialState.MarioAngle,
                initialState.CameraAngle,
                initialState,
                null,
                initialState.Index + 1);
        }

        private static float ComputeAirHSpeed(float initialHSpeed)
        {
            int maxSpeed = 32;
            float newHSpeed = ApproachHSpeed(initialHSpeed, 0, 0.35f, 0.35f);
            if (newHSpeed > maxSpeed) newHSpeed -= 1;
            if (newHSpeed < -16) newHSpeed += 2;
            return newHSpeed;
        }

        public static float ComputePosition(float position, float hSpeed, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                hSpeed = ComputeAirHSpeed(hSpeed);
                position += hSpeed;
            }
            return position;
        }

        private static MarioState ComputeAirYSpeed(MarioState initialState)
        {
            float newYSpeed = Math.Max(initialState.YSpeed - 4, -75);
            return new MarioState(
                initialState.X,
                initialState.Y,
                initialState.Z,
                initialState.XSpeed,
                newYSpeed,
                initialState.ZSpeed,
                initialState.HSpeed,
                initialState.SlidingSpeedX,
                initialState.SlidingSpeedZ,
                initialState.SlidingAngle,
                initialState.MarioAngle,
                initialState.CameraAngle,
                initialState.PreviousState,
                initialState.LastInput,
                initialState.Index);
        }

        private static float ApproachHSpeed(float speed, float maxSpeed, float increase, float decrease)
        {
            if (speed < maxSpeed)
                return Math.Min(maxSpeed, speed + increase);
            else
                return Math.Max(maxSpeed, speed - decrease);
        }
    }
}
