using MathCommandLine.Functions;
using MathCommandLine.Structure;
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
