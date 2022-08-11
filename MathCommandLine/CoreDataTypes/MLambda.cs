using MathCommandLine.Evaluation;
using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public struct MLambda
    {
        public MParameters Parameters { get; set; }
        public MExpression Expression { get; set; }

        public MLambda(MParameters parameters, MExpression expression)
        {
            Parameters = parameters;
            Expression = expression;
        }

        public static MLambda Empty = new MLambda();

        // TODO: Duplicate of MFunction's code. Make the two inherit from a common abstract class that stores parameters, an expression, and this function
        public MValue Evaluate(MArguments args, IEvaluator evaluator)
        {
            // Need to check that we've been provided the right number of arguments
            if (args.Length != Parameters.Length)
            {
                return MValue.Error(ErrorCodes.WRONG_ARG_COUNT, "Expected " + Parameters.Length + " arguments but received " + args.Length + ".", MList.Empty);
            }
            // Now check the types of the arguments to ensure they match. If any errors appear in the arguments, return that immediately
            for (int i = 0; i < args.Length; i++)
            {
                if (args.Get(i).Value.DataType != Parameters.Get(i).DataType)
                {
                    // Improper data type!
                    return MValue.Error(ErrorCodes.INVALID_TYPE,
                        "Expected argument \"" + Parameters.Get(i).Name + "\" to be of type '" + Parameters.Get(i).DataType + "' but received type '" + args.Get(i).Value.DataType + "'.",
                        MList.FromOne(MValue.Number(i)));
                }
                else if (args.Get(i).Value.DataType == MDataType.Error)
                {
                    // An error was passed as an argument, so simply need to return it
                    return args.Get(i).Value;
                }
            }
            // It appears that the arguments passed to this lambda are valid, so time to run the evaluation
            return evaluator.Evaluate(Expression, args);
        }

        // TODO
        public static bool operator ==(MLambda l1, MLambda l2)
        {
            return (l1.Expression == l2.Expression) && (l1.Parameters == l2.Parameters);
        }
        public static bool operator !=(MLambda l1, MLambda l2)
        {
            return !(l1 == l2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MLambda)
            {
                MLambda lambda = (MLambda)obj;
                return lambda == this;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
