using MathCommandLine.Evaluation;
using MathCommandLine.Structure.FunctionTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure.Functions
{
    /**
     * Static class providing the core functions of the language
     */
    public static class CoreFunctions
    {
        // Numerical Functions
        public static readonly MFunction Add = new MFunction(
            "_add", MDataType.Number, 
            (args) => 
            {
                return MValue.Number(args.Get(0).Value.NumberValue + args.Get(1).Value.NumberValue);
            },
            new MParameters(
                new MParameter(MDataType.Number, "a"),
                new MParameter(MDataType.Number, "b")
            ),
            "Returns the sum of 'a' and 'b' (a + b)."
        );
        public static readonly MFunction Multiply = new MFunction(
            "_mul", MDataType.Number,
            (args) =>
            {
                return MValue.Number(args.Get(0).Value.NumberValue * args.Get(1).Value.NumberValue);
            },
            new MParameters(
                new MParameter(MDataType.Number, "a"),
                new MParameter(MDataType.Number, "b")
            ),
            "Returns the product of 'a' and 'b' (a * b)."
        );
        public static readonly MFunction Negate = new MFunction(
            "_add_inv", MDataType.Number,
            (args) =>
            {
                return MValue.Number(-args.Get(0).Value.NumberValue);
            },
            new MParameters(
                new MParameter(MDataType.Number, "a")
            ),
            "Returns the additive inverse of 'a'."
        );
        public static readonly MFunction MulInverse = new MFunction(
            "_mul_inv", MDataType.Number,
            (args) =>
            {
                double arg = args.Get(0).Value.NumberValue;
                if (arg == 0)
                {
                    return MValue.Error(ErrorCodes.DIV_BY_ZERO);
                }
                return MValue.Number(1 / arg);
            },
            new MParameters(
                new MParameter(MDataType.Number, "a")
            ),
            "Returns the multiplicative inverse of 'a'."
        );
        public static readonly MFunction Pow = new MFunction(
            "_pow", MDataType.Number,
            (args) =>
            {
                return MValue.Number(Math.Pow(args.Get(0).Value.NumberValue, args.Get(1).Value.NumberValue));
            },
            new MParameters(
                new MParameter(MDataType.Number, "base"), 
                new MParameter(MDataType.Number, "exponent")

            ),
            "Returns the value of 'base' raised to the power of 'exponent'."
        );
        public static readonly MFunction GetIntPart = new MFunction(
            "_i", MDataType.Number,
            (args) =>
            {
                return MValue.Number(Math.Floor(args.Get(0).Value.NumberValue));
            },
            new MParameters(
                new MParameter(MDataType.Number, "a")
            ),
            "Returns the integer part of 'a'."
        );
        public static readonly MFunction TrigFunction = new MFunction(
            // TODO: Add parameter requirements, and make the op here have to be a number from 0-5
            // TODO: Hyperbolic trig functions
            "_trig", MDataType.Number,
            (args) =>
            {
                double op = args.Get(1).Value.NumberValue;
                double arg = args.Get(0).Value.NumberValue;
                double result = 0;
                switch (op)
                {
                    case 0:
                        result = Math.Sin(arg);
                        break;
                    case 1:
                        result = Math.Cos(arg);
                        break;
                    case 2:
                        result = Math.Tan(arg);
                        break;
                    case 3:
                        result = Math.Asin(arg);
                        break;
                    case 4:
                        result = Math.Acos(arg);
                        break;
                    case 5:
                        result = Math.Atan(arg);
                        break;
                }
                return MValue.Number(result);
            },
            new MParameters(
                new MParameter(MDataType.Number, "arg"),
                new MParameter(MDataType.Number, "op")
            ),
            "Performs a trigonometric function. (Op Codes: 0-sin, 1-cos, 2-tan, 3-arcsin, 4-arccos, 5-arctan)"
        );
        public static readonly MFunction NaturalLog = new MFunction(
            "_ln", MDataType.Number,
            (args) =>
            {
                return MValue.Number(Math.Log(args.Get(0).Value.NumberValue));
            },
            new MParameters(
                new MParameter(MDataType.Number, "arg")
            ),
            "Returns the natural logarithm of 'arg'."
        );

    }
}
