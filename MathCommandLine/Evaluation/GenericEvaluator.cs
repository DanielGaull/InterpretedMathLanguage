using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class GenericEvaluator : IEvaluator
    {
        NativeEvaluator nativeEvaluator;
        StringEvaluator stringEvaluator;
        public GenericEvaluator()
        {
            nativeEvaluator = new NativeEvaluator();
            stringEvaluator = new StringEvaluator();
        }
        public MValue Evaluate(MExpression expression, MArgument[] arguments)
        {
            if (expression.IsNativeExpression)
            {
                return nativeEvaluator.Evaluate(expression, arguments);
            }
            else
            {
                return stringEvaluator.Evaluate(expression, arguments);
            }
        }
    }
}
