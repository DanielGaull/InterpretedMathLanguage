using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class StringEvaluator : IEvaluator
    {
        public MValue Evaluate(MExpression expression, MArgument[] arguments)
        {
            if (!expression.IsNativeExpression)
            {
                // TODO: Evaluate the string expression
            }
            throw new NotImplementedException();
        }
    }
}
