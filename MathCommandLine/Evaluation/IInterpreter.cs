using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public interface IInterpreter
    {
        public MValue Evaluate(MExpression expression, MArguments arguments);
        public MDataType GetDataType(string typeName);
        public VariableManager GetVariableManager();

    }
}
