using STROOP.Structs;
using System;

namespace STROOP.Utilities
{
    public static class MathOperationUtilities
    {
        public static string GetSymbol(BinaryOperationName operationName, bool useX = true, bool useSlash = true)
        {
            switch (operationName)
            {
                case BinaryOperationName.Add:
                    return "+";
                case BinaryOperationName.Subtract:
                    return "-";
                case BinaryOperationName.Multiply:
                    return useX ? "×" : "*";
                case BinaryOperationName.Divide:
                    return useSlash ? "/" : "÷";
                case BinaryOperationName.Modulo:
                    return "%";
                case BinaryOperationName.NonNegativeModulo:
                    return "%%";
                case BinaryOperationName.Exponent:
                    return "^";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetNoun(BinaryOperationName operationName)
        {
            switch (operationName)
            {
                case BinaryOperationName.Add:
                    return "Addition";
                case BinaryOperationName.Subtract:
                    return "Subtraction";
                case BinaryOperationName.Multiply:
                    return "Multiplication";
                case BinaryOperationName.Divide:
                    return "Division";
                case BinaryOperationName.Modulo:
                    return "Modulo";
                case BinaryOperationName.NonNegativeModulo:
                    return "Non-Negative Modulo";
                case BinaryOperationName.Exponent:
                    return "Exponent";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetResultName(BinaryOperationName operationName)
        {
            switch (operationName)
            {
                case BinaryOperationName.Add:
                    return "Sum";
                case BinaryOperationName.Subtract:
                    return "Difference";
                case BinaryOperationName.Multiply:
                    return "Product";
                case BinaryOperationName.Divide:
                    return "Quotient";
                case BinaryOperationName.Modulo:
                    return "Modulo";
                case BinaryOperationName.NonNegativeModulo:
                    return "Non-Negative Modulo";
                case BinaryOperationName.Exponent:
                    return "Exponent";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
