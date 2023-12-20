using IML.CoreDataTypes;
using IML.Environments;
using IML.Evaluation;
using IML.Functions;
using IML.Structure;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Functions
{
    public class MFunction
    {
        /// <summary>
        /// User-only description field. Does not affect execution or interpretation of function in any way
        /// </summary>
        public string Description { get; set; }

        public string Name { get; private set; }
        public MType ReturnType { get; private set; }
        public List<string> GenericNames { get; private set; }
        public MParameters Parameters { get; private set; }
        public NativeExpression Expression { get; private set; }

        /// <summary>
        /// Constructs a new function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="returnType">The return type of the function</param>
        /// <param name="expression">Delegate for native code to execute, taking in the arguments. Argument types have already been resolved.</param>
        /// <param name="parameters">The function's parameters</param>
        public MFunction(string name, NativeExpression expression, MType returnType, MParameters parameters, string desc,
            List<string> generics)
        {
            Name = name;
            Description = desc;
            ReturnType = returnType;
            Parameters = parameters;
            Expression = expression;
            GenericNames = generics;
        }
        public MFunction(string name, NativeExpression expression, MType returnType, MParameters parameters, string desc)
            : this(name, expression, returnType, parameters, desc, new List<string>())
        {
        }

        public MClosure ToClosure()
        {
            List<MType> paramTypes = new List<MType>();
            List<string> paramNames = new List<string>();
            for (int i = 0; i < Parameters.Length; i++)
            {
                paramTypes.Add(Parameters[i].Type);
                paramNames.Add(Parameters[i].Name);
            }
            return new MClosure(MDataTypeEntry.Function(
                    ReturnType,
                    paramTypes,
                    GenericNames,
                    true,
                    false
                ), 
                paramNames,
                MEnvironment.Empty, 
                Expression);
        }
    }
}
