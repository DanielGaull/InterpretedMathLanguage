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

        public static bool operator ==(MExpression ex1, MExpression ex2)
        {
            if (ex1.IsNativeExpression != ex2.IsNativeExpression)
            {
                return false;
            }
            if (ex1.IsNativeExpression)
            {
                return ex1.Expression == ex2.Expression;
            }
            else
            {
                // TODO
                return false;
            }
        }
        public static bool operator !=(MExpression ex1, MExpression ex2)
        {
            return !(ex1 == ex2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MExpression)
            {
                MExpression exp = (MExpression)obj;
                return this == exp;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
