using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MathCommandLine
{
    class Program
    {
        static Interpreter evaluator;
        static VariableManager varManager;
        static FunctionDict funcDict;
        static void Main(string[] args)
        {
            evaluator = new Interpreter();
            varManager = new VariableManager();

            List<MFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions(evaluator);
            for (int i = 0; i < coreFuncs.Count; i++)
            {
                varManager.AddConstant(coreFuncs[i].Name, 
                    MValue.Lambda(new MLambda(coreFuncs[i].Parameters, coreFuncs[i].Expression)));
            }
            // Add core constants
            // TODO: Add core constant for TIME
            varManager.AddConstant("void", MValue.Void());
            funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Lambda,
                MDataType.Type, MDataType.Error, MDataType.Reference, MDataType.String, MDataType.Void, MDataType.Any);
            evaluator.Initialize(dtDict, varManager);

            // Simple reading for now
            while (true)
            {
                Console.Write("Enter Expression: ");
                string input = Console.ReadLine();
                MValue result = evaluator.Evaluate(new MExpression(input), VarsToArgs());
                if (result.DataType != MDataType.Void)
                {
                    // Never output void as a result, since we're typically running a function
                    string resultString = result.ToLongString();
                    Console.WriteLine(resultString);
                }
            }
        }

        static MArguments VarsToArgs()
        {
            List<MArgument> argsList = new List<MArgument>();
            argsList.AddRange(varManager.NamedValues
                .Where((value) => value.CanGetValue())
                .Select((value) => new MArgument(value.Name, value.GetValue())));
            argsList.AddRange(funcDict.Functions.Select((func) =>
                new MArgument(func.Name, MValue.Lambda(new MLambda(func.Parameters, func.Expression)))));
            return new MArguments(argsList);
        }
    }
}
