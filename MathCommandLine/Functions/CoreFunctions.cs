﻿using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Functions
{
    /**
     * Static class providing the core functions of the language
     */
    public static class CoreFunctions
    {
        public static List<MFunction> GenerateCoreFunctions(IEvaluator evaluator)
        {
            return new List<MFunction>()
            {
                Add(evaluator),
                Multiply(evaluator),
                Negate(evaluator),
                MulInverse(evaluator),
                Pow(evaluator),
                GetIntPart(evaluator),
                TrigFunction(evaluator),
                NaturalLog(evaluator),

                ListLength(evaluator),
                GetFromList(evaluator),
                InsertInList(evaluator),
                RemoveFromList(evaluator),
                IndexOfList(evaluator),
                IndexOfListCustom(evaluator),
                MapList(evaluator),
                ReduceList(evaluator),
                CreateRangeList(evaluator),
                JoinLists(evaluator),

                TypeOf(evaluator),
                CompareFunction(evaluator),
                CaseFunction(evaluator),
                GetValue(evaluator),
                CastFunction(evaluator),
                CreateErrorFunction(evaluator)
            };
        }

        // Numerical Functions
        public static MFunction Add(IEvaluator evaluator) {
            return new MFunction(
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
        }
        public static MFunction Multiply(IEvaluator evaluator)
        {
            return new MFunction(
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
        }
        public static MFunction Negate(IEvaluator evaluator)
        {
            return new MFunction(
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
        }
        public static MFunction MulInverse(IEvaluator evaluator)
        {
            return new MFunction(
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
        }
        public static MFunction Pow(IEvaluator evaluator)
        {
            return new MFunction(
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
        }
        public static MFunction GetIntPart(IEvaluator evaluator)
        {
            return new MFunction(
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
        }
        public static MFunction TrigFunction(IEvaluator evaluator)
        {
            // TODO: Add parameter requirements, and make the op here have to be a number from 0-5
            // TODO: Hyperbolic trig functions
            return new MFunction(
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
        }
        public static MFunction NaturalLog(IEvaluator evaluator)
        {
            return new MFunction(
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

        // List Functions
        public static MFunction ListLength(IEvaluator evaluator)
        {
            return new MFunction(
                "_len", MDataType.Number,
                (args) =>
                {
                    return MValue.Number(MList.Length(args.Get(0).Value.ListValue));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list")
                ),
                "Returns the number of elements in 'list'."
            );
        }
        public static MFunction GetFromList(IEvaluator evaluator)
        {
            return new MFunction(
                "_getl", MDataType.Any,
                (args) =>
                {
                    // TODO: Handle index out of range errors
                    return MList.Get(args.Get(0).Value.ListValue, (int)args.Get(1).Value.NumberValue);
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Number, "index")
                ),
                "Returns the element at index 'index' in 'list'. May return error if 'index' is >= the length of 'list'."
            );
        }
        public static MFunction InsertInList(IEvaluator evaluator)
        {
            return new MFunction(
                "_insertl", MDataType.List,
                (args) =>
                {
                    // TODO: Handle index out of range errors
                    return MValue.List(MList.Insert(args.Get(0).Value.ListValue, (int)args.Get(1).Value.NumberValue, args.Get(2).Value));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Number, "index"),
                    new MParameter(MDataType.Any, "value")
                ),
                "Returns a new list with the elements of 'list' containing 'value' inserted at 'index'. May return error if 'index' is >= the length of 'list'."
            );
        }
        public static MFunction RemoveFromList(IEvaluator evaluator)
        {
            return new MFunction(
                "_removel", MDataType.List,
                (args) =>
                {
                    // TODO: Handle index out of range errors
                    return MValue.List(MList.Remove(args.Get(0).Value.ListValue, (int)args.Get(1).Value.NumberValue));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Number, "index")
                ),
                "Returns a new list with the elements of 'list' without the value at 'index'. May return error if 'index' is >= the length of 'list'."
            );
        }
        public static MFunction IndexOfList(IEvaluator evaluator)
        { 
            return new MFunction(
                "_indexl", MDataType.Number,
                (args) =>
                {
                    return MValue.Number(MList.IndexOf(args.Get(0).Value.ListValue, args.Get(1).Value));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Any, "element")
                ),
                "Returns the index of 'element' in 'list', or -1 if 'element' does not appear in 'list'."
            );
        }
        public static MFunction IndexOfListCustom(IEvaluator evaluator)
        {
            // TODO: Attempt to cast result of equality evaluation to number
            return new MFunction(
                "_indexlc", MDataType.Number,
                (args) =>
                {
                    return MValue.Number(MList.IndexOfCustom(args.Get(0).Value.ListValue, args.Get(1).Value, args.Get(2).Value.LambdaValue, evaluator));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Any, "element"),
                    new MParameter(MDataType.Lambda, "equality_evaluator")
                ),
                "Returns the index of 'element' in 'list', or -1 if 'element' does not appear in 'list'. Two elements are considered equal if 'equality_evaluator' " + 
                "returns a non-zero value when passed in those two elements."
            );
        }
        public static MFunction MapList(IEvaluator evaluator)
        {
            return new MFunction(
                "_map", MDataType.List,
                (args) =>
                {
                    return MValue.List(MList.Map(args.Get(0).Value.ListValue, args.Get(1).Value.LambdaValue, evaluator));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Lambda, "evaluator")
                ),
                "Executes 'evaluator' on each element of 'list', passing in the element and index, putting the results into a new list and returning it."
            );
        }
        public static MFunction ReduceList(IEvaluator evaluator)
        {
            return new MFunction(
                "_reduce", MDataType.Any,
                (args) =>
                {
                    return MList.Reduce(args.Get(0).Value.ListValue, args.Get(1).Value.LambdaValue, args.Get(2).Value, evaluator);
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Lambda, "reducer"),
                    new MParameter(MDataType.Any, "init_value")
                ),
                "Runs 'reducer' on each element of 'list', passing in (previous, current). 'previous' is the result of the previous iteration (for first element, " +
                "'previous' is 'init_value'. 'current' is the current element."
            );
        }
        public static MFunction CreateRangeList(IEvaluator evaluator)
        {
            return new MFunction(
                "_crange", MDataType.List,
                (args) =>
                {
                    return MValue.List(MList.CreateRange((int) args.Get(0).Value.NumberValue));
                },
                new MParameters(
                    new MParameter(MDataType.Number, "max")
                ),
                "Creates a list of integers from 0 to 'max', exclusive (i.e. 'max' is not included in the result list)."
            );
        }
        public static MFunction JoinLists(IEvaluator evaluator)
        {
            return new MFunction(
                "_join", MDataType.List,
                (args) =>
                {
                    return MValue.List(MList.Join(args.Get(0).Value.ListValue, args.Get(1).Value.ListValue));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list1"),
                    new MParameter(MDataType.List, "list2")
                ),
                "Concatenates the elements of 'list1' and 'list2' into a new list, which is returned."
            );
        }

        // Calculation Functions
        // TODO: derivatives/integrals/solve

        // Utility Functions
        public static MFunction TypeOf(IEvaluator evaluator)
        {
            return new MFunction(
                "_type_of", MDataType.Type,
                (args) =>
                {
                    return MValue.Type(args.Get(0).Value.DataType);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "value")
                ),
                "Returns the type of 'value'."
            );
        }
        public static MFunction CompareFunction(IEvaluator evaluator)
        {
            return new MFunction(
                "_cmp", MDataType.Number,
                (args) =>
                {
                    double first = args.Get(0).Value.NumberValue;
                    double second = args.Get(0).Value.NumberValue;
                    return MValue.Number((first < second) ? -1 : (first == second ? 0 : 1));
                },
                new MParameters(
                    new MParameter(MDataType.Number, "first"),
                    new MParameter(MDataType.Number, "second")
                ),
                "If 'first' is less than 'second', returns -1. If 'first' == 'second', returns 0. If 'first' > 'second', returns 1."
            );
        }
        public static MFunction CaseFunction(IEvaluator evaluator)
        {
            return new MFunction(
                "_c", MDataType.Any,
                (args) =>
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
                new MParameters(
                    new MParameter(MDataType.Any, "value"),
                    new MParameter(MDataType.List, "cases"),
                    new MParameter(MDataType.List, "results"),
                    new MParameter(MDataType.Any, "default")
                ),
                "If 'value' appears in 'cases', returns the corresponding value in 'results' (the one with the same index). Otherwise, returns 'default'."
            );
        }
        public static MFunction GetValue(IEvaluator evaluator)
        {
            return new MFunction(
                "_gv", MDataType.Any,
                (args) =>
                {
                    string key = Utilities.MListToString(args[0].Value.ListValue);
                    MValue original = args[1].Value;
                    return original.GetValueByName(key);
                },
                new MParameters(
                    new MParameter(MDataType.List, "key"),
                    new MParameter(MDataType.Any, "original")
                ),
                "Interprets 'key' as a string, attempting to get that value from 'original'. Returns NOT_COMPOSITE if 'original' isn't composite, or " + 
                "KEY_DOES_NOT_EXIST if the key does not exist in 'original'."
            );
        }
        public static MFunction CastFunction(IEvaluator evaluator)
        {
            return new MFunction(
                "_cast", MDataType.Any,
                (args) =>
                {
                    // TODO: Write this once casts are added to the evaluator
                    return MValue.Error(ErrorCodes.INVALID_CAST);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "value"),
                    new MParameter(MDataType.Type, "type")
                ),
                "Attempts to cast 'value' to 'type'. If it cannot be casted, an INVALID_CAST error is returned. The shortest cast path will be chosen."
            );
        }
        public static MFunction CreateErrorFunction(IEvaluator evaluator)
        {
            // TODO: Apply arg requirements
            return new MFunction(
                "_error", MDataType.Error,
                (args) =>
                {
                    int code = (int)args[0].Value.NumberValue;
                    MList msg = args[1].Value.ListValue;
                    MList info = args[2].Value.ListValue;
                    return MValue.Error((ErrorCodes)code, msg, info);
                },
                new MParameters(
                    new MParameter(MDataType.Number, "code"),
                    new MParameter(MDataType.List, "message"),
                    new MParameter(MDataType.List, "info")
                ),
                "Creates and returns an error with the specified code, message, and info list."
            );
        }
    }
}