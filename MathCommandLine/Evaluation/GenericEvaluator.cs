using MathCommandLine.CoreDataTypes;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class GenericEvaluator : IInterpreter
    {
        NativeEvaluator nativeEvaluator;
        StringEvaluator stringEvaluator;

        DataTypeDict dtDict;
        VariableManager variableManager;

        public GenericEvaluator()
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
