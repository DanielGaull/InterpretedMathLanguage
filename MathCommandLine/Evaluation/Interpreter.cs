using MathCommandLine.CoreDataTypes;
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
        VariableManager variableManager;

        public Interpreter()
        {
        }

        public void Initialize(DataTypeDict dtDict, VariableManager variableManager)
        {
            this.dtDict = dtDict;
            this.variableManager = variableManager;

            nativeEvaluator = new NativeEvaluator();

            Parser parser = new Parser();
            stringEvaluator = new StringEvaluator(this, parser, dtDict, variableManager.GetReader());
        }

        public MValue Evaluate(MExpression expression, MArguments args)
        {
            if (expression.IsNativeExpression)
            {
                return nativeEvaluator.Evaluate(expression, args);
            }
            else
            {
                return stringEvaluator.Evaluate(expression, args);
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

        public VariableManager GetVariableManager()
        {
            return variableManager;
        }
    }
}
