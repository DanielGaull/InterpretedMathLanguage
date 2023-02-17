using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Functions
{
    public class MFunction
    {
        /// <summary>
        /// User-only description field. Does not affect execution or interpretation of function in any way
        /// </summary>
        public string Description { get; set; }

        public string Name { get; private set; }
        public MParameters Parameters { get; private set; }
        public NativeExpression Expression { get; private set; }

        /// <summary>
        /// Constructs a new function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="returnType">The return type of the function</param>
        /// <param name="expression">Delegate for native code to execute, taking in the arguments. Argument types have already been resolved.</param>
        /// <param name="parameters">The function's parameters</param>
        public MFunction(string name, NativeExpression expression, MParameters parameters, string desc)
        {
            Name = name;
            Description = desc;
            Parameters = parameters;
            Expression = expression;
        }
    }
}
