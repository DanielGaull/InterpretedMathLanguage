using MathCommandLine.Structure.FunctionTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure
{
    public delegate MValue NativeEvaluator(MArguments args);
    public class MExpression
    {
        public string Expression { get; private set; }
        public NativeEvaluator NativeEvaluator { get; private set; }
        public bool IsNativeExpression { get; private set; }

        public MExpression(string expression)
        {
            Expression = expression;
            IsNativeExpression = false;
        }

        public MExpression(NativeEvaluator nativeEvaluator)
        {
            NativeEvaluator = nativeEvaluator;
            IsNativeExpression = true;
        }
    }
}
