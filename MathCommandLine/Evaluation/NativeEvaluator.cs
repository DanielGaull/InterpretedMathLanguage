using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    class NativeEvaluator : IEvaluator
    {
        public MValue Evaluate(MExpression expression, MArguments arguments)
        {
            if (expression.IsNativeExpression)
            {
                return expression.NativeEvaluator(arguments);
            }
            throw new NotImplementedException();
        }
    }
}
