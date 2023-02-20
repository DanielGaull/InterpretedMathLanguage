using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class Interpreter : IInterpreter
    {
        NativeEvaluator nativeEvaluator;
        StringEvaluator stringEvaluator;

        DataTypeDict dtDict;
        MEnvironment initEnv;

        public Interpreter()
        {
        }

        public void Initialize(DataTypeDict dtDict, Parser parser, MEnvironment initEnv)
        {
            this.dtDict = dtDict;

            nativeEvaluator = new NativeEvaluator();

            stringEvaluator = new StringEvaluator(this, parser, dtDict);

            this.initEnv = initEnv;
        }

        public MValue Evaluate(MExpression expression, MArguments args)
        {
            if (expression.IsNativeExpression)
            {
                return nativeEvaluator.Evaluate(expression, args);
            }
            else
            {
                return stringEvaluator.Evaluate(expression, initEnv);
            }
        }

        public MDataType GetDataType(string typeName)
        {
            if (dtDict.Contains(typeName))
            {
                return dtDict.GetType(typeName);
            }
            return MDataType.Empty;
        }
    }
}
