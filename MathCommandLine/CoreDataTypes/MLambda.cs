using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class MLambda
    {
        Ast body;
        NativeExpression nativeBody;
        MParameters parameters;

        public static MLambda Empty = new MLambda();

        private MLambda()
        {

        }
        public MLambda(MParameters parameters, Ast body)
        {
            this.parameters = parameters;
            this.body = body;
        }
        public MLambda(MParameters parameters, NativeExpression nativeBody)
        {
            this.parameters = parameters;
            this.nativeBody = nativeBody;
        }

        public static bool operator ==(MLambda l1, MLambda l2)
        {
            return (l1.body == l2.body) && (l1.parameters == l2.parameters);
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
