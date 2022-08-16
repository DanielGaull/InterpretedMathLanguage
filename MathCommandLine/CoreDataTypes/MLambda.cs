using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class MLambda : Callable
    {
        public static MLambda Empty = new MLambda();

        public MLambda() : base(new MParameters(), null)
        {

        }
        public MLambda(MParameters parameters, MExpression expression) : base(parameters, expression)
        {
        }

        public static bool operator ==(MLambda l1, MLambda l2)
        {
            return (l1.Expression == l2.Expression) && (l1.Parameters == l2.Parameters);
        }
        public static bool operator !=(MLambda l1, MLambda l2)
        {
            return !(l1 == l2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MLambda)
            {
                MLambda lambda = (MLambda)obj;
                return lambda == this;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
