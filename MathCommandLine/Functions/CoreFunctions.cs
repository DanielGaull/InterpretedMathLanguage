using IML.CoreDataTypes;
using IML.Environments;
using IML.Evaluation;
using IML.Structure;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IML.Functions
{
    /**
     * Static class providing the core functions of the language
     */
    public static class CoreFunctions
    {
        public static List<MNativeFunction> GenerateCoreFunctions()
        {
            return new List<MNativeFunction>()
            {
                Add(),
                Multiply(),
                Negate(),
                MulInverse(),
                Pow(),
                FloorFunction(),
                TrigFunction(),
                NaturalLog(),
                CreateRangeList(),

                Get(),
                Set(),
                GetDesc(),
                SetDesc(),

                TypeOf(),
                CompareFunction(),
                CaseFunction(),
                CreateErrorFunction(),
                CreateStringFunction(),
                DisplayFunction(),
                TimeFunction(),
                CheckFunction(),
                ExitFunction(),
                ReadFunction(),
                DoFunction(),

                //RegisterDataType(),

                AndFunction(),
                AndeFunction(),
                OrFunction(),
                OreFunction(),
                NotFunction(),
                EqFunction(),
                LtFunction()
            };
        }

        // Numerical Functions
        public static MNativeFunction Add() {
            return new MNativeFunction(
                "_add", 
                (args, env, interpreter) =>
                {
                    return MValue.Number(args.Get(0).Value.NumberValue + args.Get(1).Value.NumberValue);
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "a"),
                    new MParameter(MType.Number, "b")
                ),
                "Returns the sum of 'a' and 'b' (a + b)."
            );
        }
        public static MNativeFunction Multiply()
        {
            return new MNativeFunction(
                "_mul", 
                (args, env, interpreter) =>
                {
                    return MValue.Number(args.Get(0).Value.NumberValue * args.Get(1).Value.NumberValue);
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "a"),
                    new MParameter(MType.Number, "b")
                ),
                "Returns the product of 'a' and 'b' (a * b)."
            );
        }
        public static MNativeFunction Negate()
        {
            return new MNativeFunction(
                "_add_inv", 
                (args, env, interpreter) =>
                {
                    return MValue.Number(-args.Get(0).Value.NumberValue);
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "a")
                ),
                "Returns the additive inverse of 'a'."
            );
        }
        public static MNativeFunction MulInverse()
        {
            return new MNativeFunction(
                "_mul_inv", 
                (args, env, interpreter) =>
                {
                    double arg = args.Get(0).Value.NumberValue;
                    if (arg == 0)
                    {
                        return MValue.Error(ErrorCodes.DIV_BY_ZERO);
                    }
                    return MValue.Number(1 / arg);
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "a")
                ),
                "Returns the multiplicative inverse of 'a'."
            );
        }
        public static MNativeFunction Pow()
        {
            return new MNativeFunction(
                "_pow", 
                (args, env, interpreter) =>
                {
                    return MValue.Number(Math.Pow(args.Get(0).Value.NumberValue, args.Get(1).Value.NumberValue));
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "base"),
                    new MParameter(MType.Number, "exponent")

                ),
                "Returns the value of 'base' raised to the power of 'exponent'."
            );
        }
        public static MNativeFunction FloorFunction()
        {
            return new MNativeFunction(
                "_flr", 
                (args, env, interpreter) =>
                {
                    return MValue.Number(Math.Floor(args.Get(0).Value.NumberValue));
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "a")
                ),
                "Returns the floor of 'a'."
            );
        }
        public static MNativeFunction TrigFunction()
        {
            // TODO: Hyperbolic trig functions
            return new MNativeFunction(
                "_trig", 
                (args, env, interpreter) =>
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
                        case 6:
                            result = Math.Sinh(arg);
                            break;
                        case 7:
                            result = Math.Cosh(arg);
                            break;
                        case 8:
                            result = Math.Tanh(arg);
                            break;
                        case 9:
                            result = Math.Asinh(arg);
                            break;
                        case 10:
                            result = Math.Acosh(arg);
                            break;
                        case 11:
                            result = Math.Atanh(arg);
                            break;
                        default:
                            return MValue.Error(ErrorCodes.INVALID_ARGUMENT, "Trig operation must be between 0 and 11.");
                    }
                    return MValue.Number(result);
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "arg"),
                    new MParameter(MType.Number, "op")
                ),
                "Performs a trigonometric function. (Op Codes: 0-sin, 1-cos, 2-tan, 3-arcsin, 4-arccos, 5-arctan, " + 
                    "6-sinh, 7-cosh, 8-tanh, 9-asinh, 10-acosh, 11-atanh)"
            );
        }
        public static MNativeFunction NaturalLog()
        {
            return new MNativeFunction(
                "_ln", 
                (args, env, interpreter) =>
                {
                    return MValue.Number(Math.Log(args.Get(0).Value.NumberValue));
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "arg")
                ),
                "Returns the natural logarithm of 'arg'."
            );
        }

        // List Functions
        public static MNativeFunction CreateRangeList()
        {
            return new MNativeFunction(
                "_crange", 
                (args, env, interpreter) =>
                {
                    return MValue.List(MList.CreateRange((int) args.Get(0).Value.NumberValue));
                },
                MType.List(MType.Number),
                new MParameters(
                    new MParameter(MType.Number, "max")
                ),
                "Creates a list of integers from 0 to 'max', exclusive (i.e. 'max' is not included in the result list)."
            );
        }

        // Reference/var manipulation functions
        public static MNativeFunction Get() 
        {
            return new MNativeFunction(
                "_get", 
                (args, env, interpreter) =>
                {
                    MBoxedValue box = args[0].Value.RefValue;
                    return box.GetValue();
                },
                new MType(new MGenericDataTypeEntry("T")),
                new MParameters(
                    new MParameter(MType.Reference(new MType(new MGenericDataTypeEntry("T"))), "ref")
                ),
                "Returns the value stored in 'ref'.",
                new List<string>() // Generics
                {
                    "T"
                }
            );
        }
        public static MNativeFunction Set()
        {
            return new MNativeFunction(
                "_set", 
                (args, env, interpreter) =>
                {
                    MBoxedValue refAddr = args[0].Value.RefValue;
                    MValue value = args[1].Value;
                    return refAddr.SetValue(value);
                },
                MType.Void,
                new MParameters(
                    new MParameter(MType.Reference(new MType(new MGenericDataTypeEntry("T"))), "ref"),
                    new MParameter(new MType(new MGenericDataTypeEntry("T")), "value")
                ),
                "Assigns the value for 'ref' to 'value'. Throws CAN_NOT_ASSIGN if reference variable is constant.",
                new List<string>() // Generics
                {
                    "T"
                }
            );
        }
        public static MNativeFunction GetDesc()
        {
            return new MNativeFunction(
                "_gd",
                (args, env, interpreter) =>
                {
                    MBoxedValue refAddr = args[0].Value.RefValue;
                    if (refAddr.Description == null)
                    {
                        return MValue.Null();
                    }
                    else
                    {
                        return MValue.String(refAddr.Description);
                    }
                },
                MType.String,
                new MParameters(
                    new MParameter(MType.Reference(MType.Any), "ref")
                ),
                "Gets and returns the description for 'ref'. If there is no description, returns null."
            );
        }
        public static MNativeFunction SetDesc()
        {
            return new MNativeFunction(
                "_sd",
                (args, env, interpreter) =>
                {
                    MBoxedValue refAddr = args[0].Value.RefValue;
                    string newDesc = args[1].Value.GetStringValue();
                    refAddr.Description = newDesc;
                    return MValue.Void();
                },
                MType.Void,
                new MParameters(
                    new MParameter(MType.Reference(MType.Any), "ref"),
                    new MParameter(MType.String, "desc")
                ),
                "Assigns a new description to 'ref'."
            );
        }

        // Calculation Functions
        // TODO: derivatives/integrals/solve

        // Utility Functions
        public static MNativeFunction TypeOf()
        {
            return new MNativeFunction(
                "_type_of", 
                (args, env, interpreter) =>
                {
                    return MValue.Type(new MType(args[0].Value.DataType));
                },
                MType.Type,
                new MParameters(
                    new MParameter(MType.Any, "value")
                ),
                "Returns the type of 'value'."
            );
        }
        public static MNativeFunction CompareFunction()
        {
            return new MNativeFunction(
                "_cmp", 
                (args, env, interpreter) =>
                {
                    double first = args.Get(0).Value.NumberValue;
                    double second = args.Get(0).Value.NumberValue;
                    return MValue.Number((first < second) ? -1 : (first == second ? 0 : 1));
                },
                MType.Number,
                new MParameters(
                    new MParameter(MType.Number, "first"),
                    new MParameter(MType.Number, "second")
                ),
                "If 'first' is less than 'second', returns -1. If 'first' == 'second', returns 0. " +
                "If 'first' > 'second', returns 1."
            );
        }
        public static MNativeFunction CaseFunction()
        {
            return new MNativeFunction(
                "_case", 
                (args, env, interpreter) =>
                {
                    MValue value = args[0].Value;
                    MValue defaultValue = args[3].Value;
                    List<MValue> cases = args[1].Value.ListValue.GetInternalList();
                    List<MValue> results = args[2].Value.ListValue.GetInternalList();
                    for (int i = 0; i < cases.Count; i++)
                    {
                        if (cases[i] == value)
                        {
                            return results[i];
                        }
                    }
                    return defaultValue;
                },
                new MType(new MGenericDataTypeEntry("R")),
                new MParameters(
                    new MParameter(new MType(new MGenericDataTypeEntry("T")), "value"),
                    new MParameter(MType.List(new MType(new MGenericDataTypeEntry("T"))), "cases"),
                    new MParameter(MType.List(new MType(new MGenericDataTypeEntry("R"))), "results"),
                    new MParameter(new MType(new MGenericDataTypeEntry("R")), "default")
                ),
                "If 'value' appears in 'cases', returns the corresponding value in 'results' " +
                    "(the one with the same index). Otherwise, returns 'default'.",
                new List<string>() // Generics
                {
                    "T", "R"
                }
            );
        }
        public static MNativeFunction CreateErrorFunction()
        {
            // TODO: Apply arg restrictions
            return new MNativeFunction(
                "_error", 
                (args, env, interpreter) =>
                {
                    int code = (int)args[0].Value.NumberValue;
                    if (!Enum.IsDefined(typeof(ErrorCodes), code))
                    {
                        return MValue.Error(ErrorCodes.INVALID_ARGUMENT,
                                $"The value \"{code}\" is not a valid error code.");
                    }
                    string msg = args[1].Value.GetStringValue();
                    MList info = args[2].Value.ListValue;
                    return MValue.Error((ErrorCodes)code, msg, info);
                },
                MType.Error,
                new MParameters(
                    new MParameter(MType.Number, "code"),
                    new MParameter(MType.String, "message"),
                    new MParameter(MType.List(MType.Number), "info")
                ),
                "Creates and returns an error with the specified code, message, and info list."
            );
        }

        public static MNativeFunction CreateStringFunction()
        {
            return new MNativeFunction(
                "_str", 
                (args, env, interpreter) =>
                {
                    return MValue.String(args[0].Value.ListValue);
                },
                MType.String,
                new MParameters(
                    new MParameter(MType.List(MType.Number), "chars")
                ),
                "Creates a string with the specified character stream."
            );
        }

        public static MNativeFunction DisplayFunction()
        {
            return new MNativeFunction(
                "_display", 
                (args, env, interpreter) =>
                {
                    Console.WriteLine(args[0].Value.GetStringValue());
                    return MValue.Void();
                },
                MType.Void,
                new MParameters(
                    new MParameter(MType.String, "str")
                ),
                "Prints the specified string to the standard output stream"
            );
        }

        public static MNativeFunction TimeFunction()
        {

            return new MNativeFunction(
                "_time", 
                (args, env, interpreter) =>
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                    return MValue.Number((double)unixTimeMilliseconds);
                },
                MType.Number,
                new MParameters(),
                "Returns the number of milliseconds since the Unix epoch"
            );
        }

        public static MNativeFunction CheckFunction()
        {
            return new MNativeFunction(
                "_c", 
                (args, env, interpreter) =>
                {
                    List<MValue> pairs = args[0].Value.ListValue.InternalList;
                    for (int i = 0; i < pairs.Count; i++)
                    {
                        List<MValue> pair = pairs[i].ListValue.InternalList;
                        if (pair.Count != 2)
                        {
                            return MValue.Error(ErrorCodes.INVALID_ARGUMENT, 
                                "Expected list length of 2 but found " + pair.Count + ".");
                        }
                        MFunction cond = pair[0].FunctionValue;
                        MValue condValue = interpreter.PerformCall(cond, MArguments.Empty, env);
                        if (condValue.DataType.DataType.MatchesTypeExactly(MDataType.Error))
                        {
                            return condValue;
                        }
                        // Everything is truthy except the "false" value, "void", and "null"
                        if (condValue.IsTruthy())
                        {
                            MFunction outputFunc = pair[1].FunctionValue;
                            MValue output = interpreter.PerformCall(outputFunc, MArguments.Empty, env);
                            return output;
                        }
                    }
                    // Return void if we didn't evaluate any of the conditions
                    return MValue.Void();
                },
                MType.Any,
                new MParameters(
                    new MParameter(MType.List(MType.Function(MType.Any)), "pairs")
                ),
                "Takes in a list of 2-lists of functions with no arguments, evaluating the each element. " +
                "Once an element returns true, then the corresponding code is run and returned, " +
                "with no other code being run."
            );
        }
        public static MNativeFunction ExitFunction()
        {
            return new MNativeFunction(
                "_exit",
                (args, env, interpreter) =>
                {
                    interpreter.Exit();
                    return MValue.Void();
                },
                MType.Void,
                new MParameters(),
                "Exits the program immediately (returns null)"
            );
        }
        public static MNativeFunction ReadFunction()
        {
            return new MNativeFunction(
                "_read",
                (args, env, interpreter) =>
                {
                    string prompt = args[0].Value.GetStringValue();
                    Console.Write(prompt);
                    string input = Console.ReadLine();
                    return MValue.String(input);
                },
                MType.String,
                new MParameters(new MParameter(MType.String, "prompt")),
                "Reads in and returns a single string line from the user using the standard input stream"
            );
        }
        public static MNativeFunction DoFunction()
        {

            return new MNativeFunction(
                "_do",
                (args, env, interpreter) =>
                {
                    List<MValue> funcs = args[0].Value.ListValue.InternalList;
                    MValue returnValue = MValue.Void();
                    for (int i = 0; i < funcs.Count; i++)
                    {
                        MFunction function = funcs[i].FunctionValue;
                        returnValue = interpreter.PerformCall(function, MArguments.Empty, env);
                    }
                    return returnValue;
                },
                MType.Any,
                new MParameters(
                    new MParameter(MType.List(MType.Function(MType.Any)), "code")
                ),
                "Executes the series of functions in 'code'. These functions should take no parameters. " +
                "Functions are executed in order. Returns the return value of the last function."
            );
        }

        // Boolean functions
        public static MNativeFunction AndFunction()
        {

            return new MNativeFunction(
                "_and", 
                (args, env, interpreter) =>
                {
                    if (args[0].Value.IsTruthy())
                    {
                        return args[1].Value;
                    }
                    return args[0].Value;
                },
                MType.Union(new MType(new MGenericDataTypeEntry("T1")), new MType(new MGenericDataTypeEntry("T2"))),
                new MParameters(
                    new MParameter(new MType(new MGenericDataTypeEntry("T1")), "input1"),
                    new MParameter(new MType(new MGenericDataTypeEntry("T2")), "input2")
                ),
                "If 'input1' is truthy, returns 'input2'. Otherwise, returns 'input1'",
                new List<string>()
                {
                    "T1", "T2"
                }
            );
        }
        public static MNativeFunction OrFunction()
        {

            return new MNativeFunction(
                "_or", 
                (args, env, interpreter) =>
                {
                    if (args[0].Value.IsTruthy())
                    {
                        return args[0].Value;
                    }
                    return args[1].Value;
                },
                MType.Union(new MType(new MGenericDataTypeEntry("T1")), new MType(new MGenericDataTypeEntry("T2"))),
                new MParameters(
                    new MParameter(new MType(new MGenericDataTypeEntry("T1")), "input1"),
                    new MParameter(new MType(new MGenericDataTypeEntry("T2")), "input2")
                ),
                "If 'input1' is truthy, returns 'input1'. Otherwise, returns 'input2'",
                new List<string>()
                {
                    "T1", "T2"
                }
            );
        }
        public static MNativeFunction NotFunction()
        {

            return new MNativeFunction(
                "_not", 
                (args, env, interpreter) =>
                {
                    bool b = args[0].Value.IsTruthy();
                    return MValue.Bool(!b);
                },
                MType.Boolean,
                new MParameters(
                    new MParameter(MType.Any, "input")
                ),
                "Inverts the input"
            );
        }
        public static MNativeFunction AndeFunction()
        {
            return new MNativeFunction(
                "_and_e", 
                (args, env, interpreter) =>
                {
                    MFunction b1 = args[0].Value.FunctionValue;
                    MFunction b2 = args[1].Value.FunctionValue;
                    MValue result1 = interpreter.PerformCall(b1, MArguments.Empty, env);
                    if (result1.DataType.DataType.MatchesTypeExactly(MDataType.Error))
                    {
                        return result1;
                    }
                    if (result1.IsTruthy())
                    {
                        // Simply return the second value
                        return interpreter.PerformCall(b2, MArguments.Empty, env);
                    }
                    return result1;
                },
                MType.Union(new MType(new MGenericDataTypeEntry("T1")), new MType(new MGenericDataTypeEntry("T2"))),
                new MParameters(
                    new MParameter(MType.Function(new MType(new MGenericDataTypeEntry("T1"))), "eval1"),
                    new MParameter(MType.Function(new MType(new MGenericDataTypeEntry("T2"))), "eval2")
                ),
                "If 'eval1' returns truthy, returns the result of 'eval2'. Otherwise, returns result of 'eval1'",
                new List<string>()
                {
                    "T1", "T2"
                }
            );
        }
        public static MNativeFunction OreFunction()
        {

            return new MNativeFunction(
                "_or_e", 
                (args, env, interpreter) =>
                {
                    MFunction b1 = args[0].Value.FunctionValue;
                    MFunction b2 = args[1].Value.FunctionValue;
                    MValue result1 = interpreter.PerformCall(b1, MArguments.Empty, env);
                    if (result1.DataType.DataType.MatchesTypeExactly(MDataType.Error))
                    {
                        return result1;
                    }
                    if (!result1.IsTruthy())
                    {
                        // Simply return the second value
                        return interpreter.PerformCall(b2, MArguments.Empty, env);
                    }
                    return result1;
                },
                MType.Union(new MType(new MGenericDataTypeEntry("T1")), new MType(new MGenericDataTypeEntry("T2"))),
                new MParameters(
                    new MParameter(MType.Function(new MType(new MGenericDataTypeEntry("T1"))), "eval1"),
                    new MParameter(MType.Function(new MType(new MGenericDataTypeEntry("T2"))), "eval2")
                ),
                "If 'eval1' returns truthy, returns result. Otherwise, returns result of 'eval2'",
                new List<string>()
                {
                    "T1", "T2"
                }
            );
        }

        public static MNativeFunction EqFunction()
        {

            return new MNativeFunction(
                "_eq", 
                (args, env, interpreter) =>
                {
                    MValue item1 = args[0].Value;
                    MValue item2 = args[1].Value;
                    return MValue.Bool(item1 == item2);
                },
                MType.Boolean,
                new MParameters(
                    new MParameter(MType.Any, "item1"),
                    new MParameter(MType.Any, "item2")
                ),
                "Returns whether or not both items are equal"
            );
        }
        public static MNativeFunction LtFunction()
        {

            return new MNativeFunction(
                "_lt", 
                (args, env, interpreter) =>
                {
                    double item1 = args[0].Value.NumberValue;
                    double item2 = args[1].Value.NumberValue;
                    return MValue.Bool(item1 < item2);
                },
                MType.Boolean,
                new MParameters(
                    new MParameter(MType.Number, "num1"),
                    new MParameter(MType.Number, "num2")
                ),
                "Returns true if num1 is less than num2"
            );
        }
    }
}
