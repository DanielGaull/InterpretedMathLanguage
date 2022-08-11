using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public interface IEvaluator
    {
        public MValue Evaluate(MExpression expression, MArguments arguments);
    }
}
