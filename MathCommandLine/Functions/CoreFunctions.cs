using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using MathCommandLine.Variables;
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
        public static List<MFunction> GenerateCoreFunctions(IInterpreter evaluator)
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

                Get(evaluator),
                Set(evaluator),
                Declare(evaluator),
                Delete(evaluator),

                TypeOf(evaluator),
                CompareFunction(evaluator),
                CaseFunction(evaluator),
                GetValue(evaluator),
                CastFunction(evaluator),
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
            };
        }

        // Numerical Functions
        public static MFunction Add(IInterpreter evaluator) {
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
        public static MFunction Multiply(IInterpreter evaluator)
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
        public static MFunction Negate(IInterpreter evaluator)
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
        public static MFunction MulInverse(IInterpreter evaluator)
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
        public static MFunction Pow(IInterpreter evaluator)
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
        public static MFunction GetIntPart(IInterpreter evaluator)
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
        public static MFunction TrigFunction(IInterpreter evaluator)
        {
            // TODO: Add parameter restrictions, and make the op here have to be a number from 0-5
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
        public static MFunction NaturalLog(IInterpreter evaluator)
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
        public static MFunction ListLength(IInterpreter evaluator)
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
        public static MFunction GetFromList(IInterpreter evaluator)
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
        public static MFunction InsertInList(IInterpreter evaluator)
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
        public static MFunction RemoveFromList(IInterpreter evaluator)
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
        public static MFunction IndexOfList(IInterpreter evaluator)
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
        public static MFunction IndexOfListCustom(IInterpreter evaluator)
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
        public static MFunction MapList(IInterpreter evaluator)
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
        public static MFunction ReduceList(IInterpreter evaluator)
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
        public static MFunction CreateRangeList(IInterpreter evaluator)
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
        public static MFunction JoinLists(IInterpreter evaluator)
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

        // Reference/var manipulation functions
        public static MFunction Get(IInterpreter interpreter) 
        {
            return new MFunction(
                "_get", MDataType.Any,
                (args) =>
                {
                    int refAddr = args[0].Value.AddrValue;
                    VariableManager varManager = interpreter.GetVariableManager();
                    if (varManager.HasValue(refAddr))
                    {
                        return varManager.GetValue(refAddr);
                    }
                    else
                    {
                        return MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST,
                            $"Variable or argument @ \"{refAddr}\" does not exist.", MList.Empty);
                    }
                },
                new MParameters(
                    new MParameter(MDataType.Reference, "ref")
                ),
                "Returns the value stored in 'ref'. Returns VAR_DOES_NOT_EXIST if reference variable does not exist."
            );
        }
        public static MFunction Set(IInterpreter interpreter)
        {
            return new MFunction(
                "_set", MDataType.Void,
                (args) =>
                {
                    int refAddr = args[0].Value.AddrValue;
                    MValue value = args[1].Value;
                    VariableManager varManager = interpreter.GetVariableManager();
                    if (varManager.HasValue(refAddr))
                    {
                        if (varManager.CanModifyValue(refAddr, value))
                        {
                            varManager.SetValue(refAddr, value);
                            return MValue.Void();
                        }
                        else
                        {
                            return MValue.Error(ErrorCodes.CANNOT_ASSIGN, $"Cannot assign {value} to @\"{refAddr}\".");
                        }
                    }
                    else
                    {
                        return MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST,
                            $"Variable or argument @\"{refAddr}\" does not exist.", MList.Empty);
                    }
                },
                new MParameters(
                    new MParameter(MDataType.Reference, "ref"),
                    new MParameter(MDataType.Any, "value")
                ),
                "Assigns the value for 'ref' to 'value'. Returns VAR_DOES_NOT_EXIST if reference variable does not exist, and" + 
                " CAN_NOT_ASSIGN if reference variable is constant."
            );
        }
        public static MFunction Declare(IInterpreter interpreter)
        {
            return new MFunction(
                "_declare", MDataType.Reference,
                (args) =>
                {
                    string refName = args[0].Value.GetStringValue();
                    bool can_get = args[2].Value.BoolValue;
                    bool can_set = args[3].Value.BoolValue;
                    bool can_delete = args[4].Value.BoolValue;
                    MValue value = args[1].Value;
                    VariableManager varManager = interpreter.GetVariableManager();
                    if (varManager.HasValue(refName))
                    {
                        return MValue.Error(ErrorCodes.CANNOT_DECLARE, $"Named value \"{refName}\" already exists.");
                    }
                    else
                    {
                        varManager.AddNamedValue(refName, new MReferencedValue(value, can_get, can_set, can_delete));
                        return MValue.Void();
                    }
                },
                new MParameters(
                    new MParameter(MDataType.String, "nv_name"),
                    new MParameter(MDataType.Any, "value"),
                    new MParameter(MDataType.Boolean, "can_get"),
                    new MParameter(MDataType.Boolean, "can_set"),
                    new MParameter(MDataType.Boolean, "can_delete")
                ),
                "Declares the named value 'nv_name' and assigns 'value' to it. If 'can_get' is false, then 'nv_name' " +
                "cannot be accessed. If 'can_get' is false, then 'nv_name' cannot be modified. " +
                "If 'can_delete' is false, then 'nv_name' cannot be deleted. " +
                "Returns CANNOT_DECLARE error if the named value has already been declared."
            );
        }
        public static MFunction Delete(IInterpreter interpreter)
        {
            return new MFunction(
                "_delete", MDataType.Void,
                (args) =>
                {
                    int refValue = args[0].Value.AddrValue;
                    VariableManager varManager = interpreter.GetVariableManager();
                    if (varManager.HasValue(refValue))
                    {
                        if (varManager.CanDelete(refValue))
                        {
                            varManager.Delete(refValue);
                        }
                        else
                        {
                            return MValue.Error(ErrorCodes.CANNOT_DELETE,
                                $"Variable or argument @\"{refValue}\" cannot be deleted.", MList.Empty);
                        }
                        return MValue.Void();
                    }
                    else
                    {
                        return MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST,
                            $"Variable or argument @\"{refValue}\" does not exist.", MList.Empty);
                    }
                },
                new MParameters(
                    new MParameter(MDataType.Reference, "ref")
                ),
                "Deletes the named value pointed to by 'ref'. Returns VAR_DOES_NOT_EXIST if the named value does not exist."
            );
        }

        // Calculation Functions
        // TODO: derivatives/integrals/solve

        // Utility Functions
        public static MFunction TypeOf(IInterpreter evaluator)
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
        public static MFunction CompareFunction(IInterpreter evaluator)
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
        public static MFunction CaseFunction(IInterpreter evaluator)
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
        public static MFunction GetValue(IInterpreter evaluator)
        {
            return new MFunction(
                "_gv", MDataType.Any,
                (args) =>
                {
                    MValue original = args[0].Value;
                    string key = args[1].Value.GetStringValue();
                    return original.GetValueByName(key);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "original"),
                    new MParameter(MDataType.String, "key")
                ),
                "Attempts to get the value for 'key' from 'original'. Returns NOT_COMPOSITE if 'original' isn't composite, or " + 
                "KEY_DOES_NOT_EXIST if the key does not exist in 'original'."
            );
        }
        public static MFunction CastFunction(IInterpreter evaluator)
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
        public static MFunction CreateErrorFunction(IInterpreter evaluator)
        {
            // TODO: Apply arg restrictions
            return new MFunction(
                "_error", MDataType.Error,
                (args) =>
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
                "_str", MDataType.String,
                (args) =>
                {
                    return MValue.String(args[0].Value.ListValue);
                },
                new MParameters(
                    new MParameter(MDataType.List, "chars")
                ),
                "Creates a string with the specified char stream."
            );
        }

        public static MFunction DisplayFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_display", MDataType.Void,
                (args) =>
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

        public static MFunction CreateReferenceFunction(IInterpreter evaluator)
        {
            return new MFunction(
                "_ref", MDataType.Reference,
                (args) =>
                {
                    VariableManager varManager = evaluator.GetVariableManager();
                    string name = args[0].Value.GetStringValue();
                    return MValue.Reference(varManager.AddressForName(name));
                },
                new MParameters(
                    new MParameter(MDataType.String, "var_name")
                ),
                "Returns a reference to the variable whose name is specified"
            );
        }

        public static MFunction CreateTypeFunction(IInterpreter interpreter)
        {
            return new MFunction(
                "_type", MDataType.Type,
                (args) =>
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
                    new MParameter(MDataType.String, "var_name")
                ),
                "Returns a reference to the variable whose name is specified"
            );
        }

        public static MFunction TimeFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_time", MDataType.Number,
                (args) =>
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
                "_chk", MDataType.Any,
                (args) =>
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
                        MLambda cond = pair[0].LambdaValue;
                        MValue condValue = cond.Evaluate(MArguments.Empty(), interpreter);
                        // Everything is truthy except the "false" value, "void", and "null"
                        if (condValue.IsTruthy())
                        {
                            MLambda outputFunc = pair[1].LambdaValue;
                            MValue output = outputFunc.Evaluate(MArguments.Empty(), interpreter);
                            return output;
                        }
                    }
                    // Return void if we didn't evaluate any of the conditions
                    return MValue.Void();
                },
                new MParameters(
                    // TODO: Add restrictions here
                    new MParameter(MDataType.List, "pairs")
                ),
                "Takes in a list of 2-lists of functions with no arguments, evaluating the each element. Once an element returns " +
                "true, then the corresponding code is run and returned, with no other code being run."
            );
        }

        // Boolean functions
        public static MFunction AndFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_and", MDataType.Boolean,
                (args) =>
                {
                    bool b1 = args[0].Value.IsTruthy();
                    bool b2 = args[1].Value.IsTruthy();
                    return MValue.Bool(b1 && b2);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "input1"),
                    new MParameter(MDataType.Any, "input2")
                ),
                "Returns true iff both inputs are true"
            );
        }
        public static MFunction OrFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_or", MDataType.Boolean,
                (args) =>
                {
                    bool b1 = args[0].Value.IsTruthy();
                    bool b2 = args[1].Value.IsTruthy();
                    return MValue.Bool(b1 || b2);
                },
                new MParameters(
                    new MParameter(MDataType.Any, "input1"),
                    new MParameter(MDataType.Any, "input2")
                ),
                "Returns true if either input is true"
            );
        }
        public static MFunction NotFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_not", MDataType.Boolean,
                (args) =>
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
                "_ande", MDataType.Boolean,
                (args) =>
                {
                    MLambda b1 = args[0].Value.LambdaValue;
                    MLambda b2 = args[1].Value.LambdaValue;
                    MValue result1 = b1.Evaluate(MArguments.Empty(), interpreter);
                    if (result1.IsTruthy())
                    {
                        // Simply return the second value
                        return b2.Evaluate(MArguments.Empty(), interpreter);
                    }
                    return MValue.Bool(false);
                },
                new MParameters(
                    new MParameter(MDataType.Lambda, "eval1"),
                    new MParameter(MDataType.Lambda, "eval2")
                ),
                "If 'eval1' returns truthy, returns the result of 'eval2'. Otherwise, returns false"
            );
        }
        public static MFunction OreFunction(IInterpreter interpreter)
        {

            return new MFunction(
                "_ore", MDataType.Boolean,
                (args) =>
                {
                    MLambda b1 = args[0].Value.LambdaValue;
                    MLambda b2 = args[1].Value.LambdaValue;
                    MValue result1 = b1.Evaluate(MArguments.Empty(), interpreter);
                    if (!result1.IsTruthy())
                    {
                        // Simply return the second value
                        return b2.Evaluate(MArguments.Empty(), interpreter);
                    }
                    return result1;
                },
                new MParameters(
                    new MParameter(MDataType.Lambda, "eval1"),
                    new MParameter(MDataType.Lambda, "eval2")
                ),
                "If 'eval1' returns truthy, returns result. Otherwise, returns result of 'eval2'"
            );
        }
    }
}
