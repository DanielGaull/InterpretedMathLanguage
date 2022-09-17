using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    class NativeEvaluator : IInterpreter
    {
        public MValue Evaluate(MExpression expression, MArguments arguments)
        {
            if (expression.IsNativeExpression)
            {
                return expression.NativeExpression(arguments);
            }
            throw new NotImplementedException();
        }
    }
}
