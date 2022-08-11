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
        public MParameters Parameters { get; private set; }
        public MExpression Expression { get; private set; }
        public MDataType ReturnType { get; private set; } 
        public string Name { get; set; }

        /// <summary>
        /// User-only description field. Does not affect execution or interpretation of function in any way
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Constructs a new function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="returnType">The return type of the function</param>
        /// <param name="expression">The string expression to evaluate, using the provided paramters</param>
        /// <param name="parameters">The function's parameters</param>
        public MFunction(string name, MDataType returnType, string expression, MParameters parameters, string desc)
        {
            Parameters = parameters;
            Expression = new MExpression(expression);
            ReturnType = returnType;
            Name = name;
            Description = desc;
        }
        /// <summary>
        /// Constructs a new function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="returnType">The return type of the function</param>
        /// <param name="expression">Delegate for native code to execute, taking in the arguments. Argument types have already been resolved.</param>
        /// <param name="parameters">The function's parameters</param>
        public MFunction(string name, MDataType returnType, NativeExpression expression, MParameters parameters, string desc)
        {
            Parameters = parameters;
            Expression = new MExpression(expression);
            ReturnType = returnType;
            Name = name;
            Description = desc;
        }

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
                        "Expected argument \"" +  Parameters.Get(i).Name + "\" to be of type '" + Parameters.Get(i).DataType + "' but received type '" + args.Get(i).Value.DataType + "'.", 
                        MList.FromOne(MValue.Number(i)));
                }
                else if (args.Get(i).Value.DataType == MDataType.Error)
                {
                    // An error was passed as an argument, so simply need to return it
                    // TODO: Allow a flag that prevents this from happening and allows errors to be fed to functions
                    return args.Get(i).Value;
                }
            }
            // It appears that the arguments passed to this function are valid, so time to run the evaluation
            return evaluator.Evaluate(Expression, args);
        }
    }
}
