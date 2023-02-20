using MathCommandLine.Environments;
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
        public MValue Evaluate(string expression, MArguments arguments, MEnvironment env);
        public MDataType GetDataType(string typeName);

    }
}
