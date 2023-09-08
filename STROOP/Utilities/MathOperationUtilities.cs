﻿using STROOP.Structs;
using System;

namespace STROOP.Utilities
{
    public static class MathOperationUtilities
    {
        public static string GetSymbol(BinaryMathOperation operation, bool useX = true, bool useSlash = true)
        {
            switch (operation)
            {
                case BinaryMathOperation.Add:
                    return "+";
                case BinaryMathOperation.Subtract:
                    return "-";
                case BinaryMathOperation.Multiply:
                    return useX ? "×" : "*";
                case BinaryMathOperation.Divide:
                    return useSlash ? "/" : "÷";
                case BinaryMathOperation.Modulo:
                    return "%";
                case BinaryMathOperation.NonNegativeModulo:
                    return "%%";
                case BinaryMathOperation.Exponent:
                    return "^";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetNoun(BinaryMathOperation operation)
        {
            switch (operation)
            {
                case BinaryMathOperation.Add:
                    return "Addition";
                case BinaryMathOperation.Subtract:
                    return "Subtraction";
                case BinaryMathOperation.Multiply:
                    return "Multiplication";
                case BinaryMathOperation.Divide:
                    return "Division";
                case BinaryMathOperation.Modulo:
                    return "Modulo";
                case BinaryMathOperation.NonNegativeModulo:
                    return "Non-Negative Modulo";
                case BinaryMathOperation.Exponent:
                    return "Exponent";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetResultName(BinaryMathOperation operation)
        {
            switch (operation)
            {
                case BinaryMathOperation.Add:
                    return "Sum";
                case BinaryMathOperation.Subtract:
                    return "Difference";
                case BinaryMathOperation.Multiply:
                    return "Product";
                case BinaryMathOperation.Divide:
                    return "Quotient";
                case BinaryMathOperation.Modulo:
                    return "Modulo";
                case BinaryMathOperation.NonNegativeModulo:
                    return "Non-Negative Modulo";
                case BinaryMathOperation.Exponent:
                    return "Exponent";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
