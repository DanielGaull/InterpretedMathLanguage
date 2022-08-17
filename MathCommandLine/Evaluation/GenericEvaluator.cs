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
            Initialize();
        }

        private void Initialize()
        {
            nativeEvaluator = new NativeEvaluator();

            List<MFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions(this);
            FunctionDict funcDict = new FunctionDict(coreFuncs);
            // TODO: Add user-defined functions here
            stringEvaluator = new StringEvaluator(this, funcDict);
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
