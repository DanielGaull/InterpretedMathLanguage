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
        //NativeEvaluator nativeEvaluator;
        Evaluator evaluator;

        DataTypeDict dtDict;

        public Interpreter()
        {
        }

        public void Initialize(DataTypeDict dtDict, Parser parser)
        {
            this.dtDict = dtDict;

            //nativeEvaluator = new NativeEvaluator();

            evaluator = new Evaluator(parser, dtDict);
        }

        public MValue Evaluate(string expression, MArguments args, MEnvironment env)
        {
            return evaluator.Evaluate(expression, env);
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
