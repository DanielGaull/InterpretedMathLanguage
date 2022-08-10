using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Structure.FunctionTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Structure
{
    public class MFunction
    {
        public List<MParameter> Parameters { get; private set; }
        public MExpression Expression { get; private set; }
        public MDataType ReturnType { get; private set; } 
        public string Name { get; set; }

        public MFunction(string name, MDataType returnType, string expression, params MParameter[] parameters)
        {
            Parameters = parameters.ToList();
            Expression = new MExpression(expression);
            ReturnType = returnType;
            Name = name;
        }

        public MValue Evaluate(IEvaluator evaluator, params MArgument[] args)
        {
            // Need to check that we've been provided the right number of arguments
            if (args.Length != Parameters.Count)
            {
                return MValue.Error(ErrorCodes.WRONG_ARG_COUNT, "Expected " + Parameters.Count + " arguments but received " + args.Length + ".", MList.Empty);
            }
            // Now check the types of the arguments to ensure they match. If any errors appear in the arguments, return that immediately
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Value.DataType != Parameters[i].DataType)
                {
                    // Improper data type!
                    return MValue.Error(ErrorCodes.INVALID_TYPE, 
                        "Expected argument \"" +  Parameters[i].Name + "\" to be of type '" + Parameters[i].DataType + "' but received type '" + args[i].Value.DataType + "'.", 
                        MList.FromOne(MValue.Number(i)));
                }
                else if (args[i].Value.DataType == MDataType.Error)
                {
                    // An error was passed as an argument, so simply need to return it
                    return args[i].Value;
                }
            }
            // It appears that the arguments passed to this function are valid, so time to run the evaluation
            return evaluator.Evaluate(Expression, args);
        }
    }
}
