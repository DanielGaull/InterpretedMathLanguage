using MathCommandLine.CoreDataTypes;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
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
        }

        public void Initialize(DataTypeDict dtDict)
        {
            nativeEvaluator = new NativeEvaluator();

            Parser parser = new Parser();
            stringEvaluator = new StringEvaluator(this, parser, dtDict);
        }

        public MValue Evaluate(MExpression expression, MArguments arguments)
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
