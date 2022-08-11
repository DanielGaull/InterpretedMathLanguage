using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class StringEvaluator : IEvaluator
    {
        public MValue Evaluate(MExpression expression, MArguments arguments)
        {
            if (!expression.IsNativeExpression)
            {
                // TODO: Evaluate the string expression
            }
            throw new NotImplementedException();
        }
    }
}
