using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Functions
{
    public abstract class Callable
    {
        public MParameters Parameters { get; set; }
        public MExpression Expression { get; set; }

        protected Callable(MParameters parameters, MExpression expression)
        {
            Parameters = parameters;
            Expression = expression;
        }

        public MValue Evaluate(MArguments args, IInterpreter evaluator)
        {
            // Need to check that we've been provided the right number of arguments
            if (args.Length != Parameters.Length)
            {
                return MValue.Error(ErrorCodes.WRONG_ARG_COUNT, "Expected " + Parameters.Length + 
                    " arguments but received " + args.Length + ".", MList.Empty);
            }
            // Now check the types of the arguments to ensure they match. If any errors appear in the arguments, return that immediately
            for (int i = 0; i < args.Length; i++)
            {
                if (!Parameters[i].ContainsType(args[i].Value.DataType))
                {
                    // Improper data type!
                    return MValue.Error(ErrorCodes.INVALID_TYPE,
                        "Expected argument \"" + Parameters.Get(i).Name + "\" to be of type '" + 
                            Parameters.Get(i).DataTypeString() + "' but received type '" + args.Get(i).Value.DataType + "'.",
                        MList.FromOne(MValue.Number(i)));
                }
                else if (!Parameters[i].PassesRestrictions(args[i].Value))
                {
                    // Fails restrictions!
                    return MValue.Error(ErrorCodes.FAILS_RESTRICTION,
                        "Argument \"" + Parameters.Get(i).Name + "\" fails one or more parameter restrictions.",
                        MList.FromOne(MValue.Number(i)));
                }
                else if (args.Get(i).Value.DataType == MDataType.Error)
                {
                    // An error was passed as an argument, so simply need to return it
                    // TODO: Allow a flag that prevents this from happening and allows errors to be fed to functions
                    return args.Get(i).Value;
                }
                else
                {
                    // Arg passes! But we need to make sure it's properly named
                    MArgument newArg = new MArgument(Parameters[i].Name, args[i].Value);
                    args.Set(i, newArg);
                }
            }
            // It appears that the arguments passed to this function are valid, so time to run the evaluation
            return evaluator.Evaluate(Expression, args);
        }
    }
}
