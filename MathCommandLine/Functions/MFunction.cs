using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure.FunctionTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Structure
{
    public class MFunction : Callable
    {
        // TODO: Something should be done with the return type other than just storing it
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
        public MFunction(string name, MDataType returnType, string expression, MParameters parameters, string desc) : base(parameters, new MExpression(expression))
        {
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
        public MFunction(string name, MDataType returnType, NativeExpression expression, MParameters parameters, string desc) : base(parameters, new MExpression(expression))
        {
            ReturnType = returnType;
            Name = name;
            Description = desc;
        }
    }
}
