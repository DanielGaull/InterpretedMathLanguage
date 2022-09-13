﻿using MathCommandLine.CoreDataTypes;
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
        static GenericEvaluator evaluator;
        static VariableManager varManager;
        static FunctionDict funcDict;
        static void Main(string[] args)
        {
            evaluator = new GenericEvaluator();
            varManager = new VariableManager();
            VariableReader varReader = varManager.GetReader();

            List<MFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions(evaluator);
            funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Lambda,
                MDataType.Type, MDataType.Error, MDataType.Any);
            evaluator.Initialize(dtDict, varReader);

            // Simple reading for now
            while (true)
            {
                Console.Write("Enter Expression: ");
                string input = Console.ReadLine();
                MValue result = evaluator.Evaluate(new MExpression(input), VarsToArgs());
                string resultString = result.ToLongString();
                Console.WriteLine(resultString);
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
