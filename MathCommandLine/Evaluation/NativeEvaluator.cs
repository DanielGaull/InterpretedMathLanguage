using MathCommandLine.Environments;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class NativeEvaluator
    {
        public MValue Evaluate(MExpression expression, MArguments arguments, MEnvironment env)
        {
            if (expression.IsNativeExpression)
            {
                return expression.NativeExpression(arguments, env);
            }
            throw new NotImplementedException();
        }
    }
}
