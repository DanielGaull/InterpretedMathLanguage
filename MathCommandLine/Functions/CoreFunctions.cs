using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
using MathCommandLine.Evaluation;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MathCommandLine.Functions
{
    /**
     * Static class providing the core functions of the language
     */
    public static class CoreFunctions
    {
        public static List<MFunction> GenerateCoreFunctions(IInterpreter evaluator)
        {
            return new List<MFunction>()
            {
                Add(evaluator),
                Multiply(evaluator),
                Negate(evaluator),
                MulInverse(evaluator),
                Pow(evaluator),
                FloorFunction(evaluator),
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
                ConcatLists(evaluator),

                Get(evaluator),
                Set(evaluator),
                Declare(evaluator),
                //Delete(evaluator),

                TypeOf(evaluator),
                CompareFunction(evaluator),
                CaseFunction(evaluator),
                GetValue(evaluator),
                CreateErrorFunction(evaluator),
                CreateStringFunction(evaluator),
                DisplayFunction(evaluator),
                CreateTypeFunction(evaluator),
                TimeFunction(evaluator),
                CheckFunction(evaluator),

                CreateReferenceFunction(evaluator),

                AndFunction(evaluator),
                AndeFunction(evaluator),
                OrFunction(evaluator),
                OreFunction(evaluator),
                NotFunction(evaluator),
                EqFunction(evaluator),
                LtFunction(evaluator)
            };
        }

        // Numerical Functions
        public static MFunction Add(IInterpreter evaluator) {
            return new MFunction(
                "_add", 
                (args, env) =>
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
        public static MFunction Multiply(IInterpreter evaluator)
        {
            return new MFunction(
                "_mul", 
                (args, env) =>
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
        public static MFunction Negate(IInterpreter evaluator)
        {
            return new MFunction(
                "_add_inv", 
                (args, env) =>
                {
                    return MValue.Number(-args.Get(0).Value.NumberValue);
                },
                new MParameters(
                    new MParameter(MDataType.Number, "a")
                ),
                "Returns the additive inverse of 'a'."
            );
        }
        public static MFunction MulInverse(IInterpreter evaluator)
        {
            return new MFunction(
                "_mul_inv", 
                (args, env) =>
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
        public static MFunction Pow(IInterpreter evaluator)
        {
            return new MFunction(
                "_pow", 
                (args, env) =>
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
        public static MFunction FloorFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_flr", 
                (args, env) =>
                {
                    return MValue.Number(Math.Floor(args.Get(0).Value.NumberValue));
                },
                new MParameters(
                    new MParameter(MDataType.Number, "a")
                ),
                "Returns the floor of 'a'."
            );
        }
        public static MFunction TrigFunction(IInterpreter evaluator)
        {
            // TODO: Add parameter restrictions, and make the op here have to be a number from 0-5
            // TODO: Hyperbolic trig functions
            return new MFunction(
                "_trig", 
                (args, env) =>
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
        public static MFunction NaturalLog(IInterpreter evaluator)
        {
            return new MFunction(
                "_ln", 
                (args, env) =>
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
        public static MFunction ListLength(IInterpreter evaluator)
        {
            return new MFunction(
                "_len", 
                (args, env) =>
                {
                    return MValue.Number(MList.Length(args.Get(0).Value.ListValue));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list")
                ),
                "Returns the number of elements in 'list'."
            );
        }
        public static MFunction GetFromList(IInterpreter evaluator)
        {
            return new MFunction(
                "_getl", 
                (args, env) =>
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
        public static MFunction InsertInList(IInterpreter evaluator)
        {
            return new MFunction(
                "_insertl", 
                (args, env) =>
                {
                    // TODO: Handle index out of range errors
                    return MValue.List(MList.Insert(args.Get(0).Value.ListValue, (int)args.Get(1).Value.NumberValue, args.Get(2).Value));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Number, "index"),
                    new MParameter(MDataType.Any, "value")
                ),
                "Returns a new list with the elements of 'list' containing 'value' inserted at 'index'. " +
                "May return error if 'index' is >= the length of 'list'."
            );
        }
        public static MFunction RemoveFromList(IInterpreter evaluator)
        {
            return new MFunction(
                "_removel", 
                (args, env) =>
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
        public static MFunction IndexOfList(IInterpreter evaluator)
        { 
            return new MFunction(
                "_indexl", 
                (args, env) =>
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
        public static MFunction IndexOfListCustom(IInterpreter evaluator)
        {
            // TODO: Attempt to cast result of equality evaluation to number
            return new MFunction(
                "_indexlc", 
                (args, env) =>
                {
                    return MValue.Number(MList.IndexOfCustom(args.Get(0).Value.ListValue, args.Get(1).Value, 
                        args.Get(2).Value.ClosureValue, evaluator, env));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Any, "element"),
                    new MParameter(MDataType.Closure, "equality_evaluator")
                ),
                "Returns the index of 'element' in 'list', or -1 if 'element' does not appear in 'list'. Two elements are considered equal if 'equality_evaluator' " + 
                "returns a non-zero value when passed in those two elements."
            );
        }
        public static MFunction MapList(IInterpreter evaluator)
        {
            return new MFunction(
                "_map", 
                (args, env) =>
                {
                    return MValue.List(MList.Map(args.Get(0).Value.ListValue, 
                        args.Get(1).Value.ClosureValue, evaluator, env));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Closure, "mapper")
                ),
                "Executes 'mapper' on each element of 'list', passing in the element, " +
                "putting the results into a new list and returning it."
            );
        }
        public static MFunction ReduceList(IInterpreter evaluator)
        {
            return new MFunction(
                "_reduce", 
                (args, env) =>
                {
                    return MList.Reduce(args.Get(0).Value.ListValue, args.Get(1).Value.ClosureValue, 
                        args.Get(2).Value, evaluator, env);
                },
                new MParameters(
                    new MParameter(MDataType.List, "list"),
                    new MParameter(MDataType.Closure, "reducer"),
                    new MParameter(MDataType.Any, "init_value")
                ),
                "Runs 'reducer' on each element of 'list', passing in (previous, current). " +
                "'previous' is the result of the previous iteration (for first element, " +
                "'previous' is 'init_value'). 'current' is the current element."
            );
        }
        public static MFunction CreateRangeList(IInterpreter evaluator)
        {
            return new MFunction(
                "_crange", 
                (args, env) =>
                {
                    return MValue.List(MList.CreateRange((int) args.Get(0).Value.NumberValue));
                },
                new MParameters(
                    new MParameter(MDataType.Number, "max")
                ),
                "Creates a list of integers from 0 to 'max', exclusive (i.e. 'max' is not included in the result list)."
            );
        }
        public static MFunction ConcatLists(IInterpreter evaluator)
        {
            return new MFunction(
                "_concat", 
                (args, env) =>
                {
                    return MValue.List(MList.Concat(args.Get(0).Value.ListValue, args.Get(1).Value.ListValue));
                },
                new MParameters(
                    new MParameter(MDataType.List, "list1"),
                    new MParameter(MDataType.List, "list2")
                ),
                "Concatenates the elements of 'list1' and 'list2' into a new list, which is returned."
            );
        }

        // Reference/var manipulation functions
        public static MFunction Get(IInterpreter interpreter) 
        {
            return new MFunction(
                "_get", 
                (args, env) =>
                {
                    MBoxedValue box = args[0].Value.RefValue;
                    return box.GetValue();
                },
                new MParameters(
                    new MParameter(MDataType.Reference, "ref")
                ),
                "Returns the value stored in 'ref'."
            );
        }
        public static MFunction Set(IInterpreter interpreter)
        {
            return new MFunction(
                "_set", 
                (args, env) =>
                {
                    MBoxedValue refAddr = args[0].Value.RefValue;
                    MValue value = args[1].Value;
                    return refAddr.SetValue(value);
                },
                new MParameters(
                    new MParameter(MDataType.Reference, "ref"),
                    new MParameter(MDataType.Any, "value")
                ),
                "Assigns the value for 'ref' to 'value'. Returns CAN_NOT_ASSIGN if reference variable is constant."
            );
        }
        public static MFunction Declare(IInterpreter interpreter)
        {
            return new MFunction(
                "_declare", 
                (args, env) =>
                {
                    string refName = args[0].Value.GetStringValue();
                    bool can_get = args[2].Value.BoolValue;
                    bool can_set = args[3].Value.BoolValue;
                    MValue value = args[1].Value;
                    Regex reg = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
                    if (env.Has(refName))
                    {
                        return MValue.Error(ErrorCodes.CANNOT_DECLARE, $"Named value \"{refName}\" already exists.");
                    }
                    else if (!reg.IsMatch(refName))
                    {
                        return MValue.Error(ErrorCodes.CANNOT_DECLARE, $"The name \"{refName}\" is an invalid name.");
                    }
                    else
                    {
                        env.AddValue(refName, value, can_get, can_set);
                        return MValue.Void();
                    }
                },
                new MParameters(
                    new MParameter(MDataType.String, "nv_name"),
                    new MParameter(MDataType.Any, "value"),
                    new MParameter(MDataType.Boolean, "can_get"),
                    new MParameter(MDataType.Boolean, "can_set")
                ),
                "Declares the named value 'nv_name' and assigns 'value' to it. If 'can_get' is false, then 'nv_name' " +
                "cannot be accessed. If 'can_get' is false, then 'nv_name' cannot be modified. " +
                "Returns CANNOT_DECLARE error if the named value has already been declared, or the name is invalid."
            );
        }


        public static MFunction CreateReferenceFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_ref",
                (args, env) =>
                {
                    string name = args[0].Value.GetStringValue();
                    MBoxedValue box = env.GetBox(name);
                    if (box != null)
                    {
                        return MValue.Reference(box);
                    }
                    else
                    {
                        return MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST,
                                $"Variable \"{name}\" does not exist.", MList.Empty);
                    }
                },
                new MParameters(
                    new MParameter(MDataType.String, "var_name")
                ),
                "Returns a reference to the named value 'var_name'."
            );
        }

        // Calculation Functions
        // TODO: derivatives/integrals/solve

        // Utility Functions
        public static MFunction TypeOf(IInterpreter evaluator)
        {
            return new MFunction(
                "_type_of", 
                (args, env) =>
                {
                    return MValue.Type(args.Get(0).Value.DataType);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "value")
                ),
                "Returns the type of 'value'."
            );
        }
        public static MFunction CompareFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_cmp", 
                (args, env) =>
                {
                    double first = args.Get(0).Value.NumberValue;
                    double second = args.Get(0).Value.NumberValue;
                    return MValue.Number((first < second) ? -1 : (first == second ? 0 : 1));
                },
                new MParameters(
                    new MParameter(MDataType.Number, "first"),
                    new MParameter(MDataType.Number, "second")
                ),
                "If 'first' is less than 'second', returns -1. If 'first' == 'second', returns 0. " +
                "If 'first' > 'second', returns 1."
            );
        }
        public static MFunction CaseFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_case", 
                (args, env) =>
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
                "If 'value' appears in 'cases', returns the corresponding value in 'results' " +
                "(the one with the same index). Otherwise, returns 'default'."
            );
        }
        public static MFunction GetValue(IInterpreter evaluator)
        {
            return new MFunction(
                "_gv", 
                (args, env) =>
                {
                    MValue original = args[0].Value;
                    string key = args[1].Value.GetStringValue();
                    return original.GetValueByName(key);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "original"),
                    new MParameter(MDataType.String, "key")
                ),
                "Attempts to get the value for 'key' from 'original'. " +
                "Returns NOT_COMPOSITE if 'original' isn't composite, or " + 
                "KEY_DOES_NOT_EXIST if the key does not exist in 'original'."
            );
        }
        public static MFunction CreateErrorFunction(IInterpreter evaluator)
        {
            // TODO: Apply arg restrictions
            return new MFunction(
                "_error", 
                (args, env) =>
                {
                    int code = (int)args[0].Value.NumberValue;
                    string msg = args[1].Value.GetStringValue();
                    MList info = args[2].Value.ListValue;
                    return MValue.Error((ErrorCodes)code, msg, info);
                },
                new MParameters(
                    new MParameter(MDataType.Number, "code"),
                    new MParameter(MDataType.String, "message"),
                    new MParameter(MDataType.List, "info")
                ),
                "Creates and returns an error with the specified code, message, and info list."
            );
        }

        // TODO: Add restriction
        public static MFunction CreateStringFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_str", 
                (args, env) =>
                {
                    return MValue.String(args[0].Value.ListValue);
                },
                new MParameters(
                    new MParameter(MDataType.List, "chars")
                ),
                "Creates a string with the specified character stream."
            );
        }

        public static MFunction DisplayFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_display", 
                (args, env) =>
                {
                    Console.WriteLine(args[0].Value.GetStringValue());
                    return MValue.Void();
                },
                new MParameters(
                    new MParameter(MDataType.String, "str")
                ),
                "Prints the specified string to the standard output stream"
            );
        }

        public static MFunction CreateTypeFunction(IInterpreter interpreter)
        {
            return new MFunction(
                "_type", 
                (args, env) =>
                {
                    string typeName = args[0].Value.GetStringValue();
                    MDataType type = interpreter.GetDataType(typeName);
                    if (type.IsEmpty())
                    {
                        return MValue.Error(ErrorCodes.TYPE_DOES_NOT_EXIST, $"Type \"{typeName}\" does not exist.");
                    }
                    else
                    {
                        return MValue.Type(type);
                    }
                },
                new MParameters(
                    new MParameter(MDataType.String, "type_name")
                ),
                "Returns a type object for a type with the specified 'type_name'. Returns an error if the type does not exist."
            );
        }

        public static MFunction TimeFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_time", 
                (args, env) =>
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                    return MValue.Number((double)unixTimeMilliseconds);
                },
                new MParameters(),
                "Returns the number of milliseconds since the epoch"
            );
        }

        public static MFunction CheckFunction(IInterpreter interpreter)
        {
            return new MFunction(
                "_c", 
                (args, env) =>
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
                        MClosure cond = pair[0].ClosureValue;
                        MValue condValue = interpreter.PerformCall(cond, MArguments.Empty, env);
                        // Everything is truthy except the "false" value, "void", and "null"
                        if (condValue.IsTruthy())
                        {
                            MClosure outputFunc = pair[1].ClosureValue;
                            MValue output = interpreter.PerformCall(outputFunc, MArguments.Empty, env);
                            return output;
                        }
                    }
                    // Return void if we didn't evaluate any of the conditions
                    return MValue.Void();
                },
                new MParameters(
                    new MParameter("pairs", new MTypeRestrictionsEntry(MDataType.List, 
                        ValueRestriction.TypesAllowed(MDataType.List)))
                ),
                "Takes in a list of 2-lists of functions with no arguments, evaluating the each element. " +
                "Once an element returns true, then the corresponding code is run and returned, " +
                "with no other code being run."
            );
        }

        // Boolean functions
        public static MFunction AndFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_and", 
                (args, env) =>
                {
                    if (args[0].Value.IsTruthy())
                    {
                        return args[1].Value;
                    }
                    return args[0].Value;
                },
                new MParameters(
                    new MParameter(MDataType.Any, "input1"),
                    new MParameter(MDataType.Any, "input2")
                ),
                "If 'input1' is truthy, returns 'input2'. Otherwise, returns 'input1'"
            );
        }
        public static MFunction OrFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_or", 
                (args, env) =>
                {
                    if (args[0].Value.IsTruthy())
                    {
                        return args[0].Value;
                    }
                    return args[1].Value;
                },
                new MParameters(
                    new MParameter(MDataType.Any, "input1"),
                    new MParameter(MDataType.Any, "input2")
                ),
                "If 'input1' is truthy, returns 'input1'. Otherwise, returns 'input2'"
            );
        }
        public static MFunction NotFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_not", 
                (args, env) =>
                {
                    bool b = args[0].Value.IsTruthy();
                    return MValue.Bool(!b);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "input")
                ),
                "Inverts the input"
            );
        }
        public static MFunction AndeFunction(IInterpreter interpreter)
        {
            return new MFunction(
                "_and_e", 
                (args, env) =>
                {
                    MClosure b1 = args[0].Value.ClosureValue;
                    MClosure b2 = args[1].Value.ClosureValue;
                    MValue result1 = interpreter.PerformCall(b1, MArguments.Empty, env);
                    if (result1.IsTruthy())
                    {
                        // Simply return the second value
                        return interpreter.PerformCall(b2, MArguments.Empty, env);
                    }
                    return result1;
                },
                new MParameters(
                    new MParameter(MDataType.Closure, "eval1"),
                    new MParameter(MDataType.Closure, "eval2")
                ),
                "If 'eval1' returns truthy, returns the result of 'eval2'. Otherwise, returns result of 'eval1'"
            );
        }
        public static MFunction OreFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_or_e", 
                (args, env) =>
                {
                    MClosure b1 = args[0].Value.ClosureValue;
                    MClosure b2 = args[1].Value.ClosureValue;
                    MValue result1 = interpreter.PerformCall(b1, MArguments.Empty, env);
                    if (!result1.IsTruthy())
                    {
                        // Simply return the second value
                        return interpreter.PerformCall(b2, MArguments.Empty, env);
                    }
                    return result1;
                },
                new MParameters(
                    new MParameter(MDataType.Closure, "eval1"),
                    new MParameter(MDataType.Closure, "eval2")
                ),
                "If 'eval1' returns truthy, returns result. Otherwise, returns result of 'eval2'"
            );
        }

        public static MFunction EqFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_eq", 
                (args, env) =>
                {
                    MValue item1 = args[0].Value;
                    MValue item2 = args[1].Value;
                    return MValue.Bool(item1 == item2);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "item1"),
                    new MParameter(MDataType.Any, "item2")
                ),
                "Returns whether or not both items are equal"
            );
        }
        public static MFunction LtFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_lt", 
                (args, env) =>
                {
                    double item1 = args[0].Value.NumberValue;
                    double item2 = args[1].Value.NumberValue;
                    return MValue.Bool(item1 < item2);
                },
                new MParameters(
                    new MParameter(MDataType.Number, "num1"),
                    new MParameter(MDataType.Number, "num2")
                ),
                "Returns true if num1 is less than num2"
            );
        }
    }
}
