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
            Initialize();
        }

        private void Initialize()
        {
            nativeEvaluator = new NativeEvaluator();

            List<MFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions(this);
            FunctionDict funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Lambda, MDataType.Type, MDataType.Error);
            Parser parser = new Parser();
            stringEvaluator = new StringEvaluator(this, parser, funcDict, dtDict);
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
