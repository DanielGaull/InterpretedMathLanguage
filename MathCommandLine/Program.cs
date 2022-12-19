using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Syntax;
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

            // Add core constants
            varManager.AddConstant("null", MValue.Null(), false);
            varManager.AddConstant("void", MValue.Void(), false);
            varManager.AddConstant("TRUE", MValue.Bool(true), false);
            varManager.AddConstant("FALSE", MValue.Bool(false), false);
            List<MFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions(evaluator);
            for (int i = 0; i < coreFuncs.Count; i++)
            {
                varManager.AddConstant(coreFuncs[i].Name, 
                    MValue.Lambda(new MLambda(coreFuncs[i].Parameters, coreFuncs[i].Expression)), false);
            }
            funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Lambda,
                MDataType.Type, MDataType.Error, MDataType.Reference, MDataType.String, MDataType.Void,
                MDataType.Boolean, MDataType.Null, MDataType.Any);
            evaluator.Initialize(dtDict, varManager);

            // SYNTAX TESTING
            SyntaxDef def = new SyntaxDef(new List<SyntaxSymbol> {
                new SyntaxSymbol(new SyntaxParameter(new MParameter(MDataType.Number, "a"))),
                new SyntaxSymbol("+"),
                new SyntaxSymbol(new SyntaxParameter(new MParameter(MDataType.Number, "b")))
            }, "_add(a,b)");
            SyntaxHandler sh = new SyntaxHandler(evaluator);
            var m = sh.Match(def, "1+2");

            Console.ReadLine();
            return;

            // Simple reading for now
            while (true)
            {
                Console.Write("Enter Expression: ");
                string input = Console.ReadLine();
                MValue result = evaluator.Evaluate(new MExpression(input), MArguments.Empty);
                if (result.DataType != MDataType.Void)
                {
                    // Never output void as a result, since we're typically running a function
                    string resultString = result.ToLongString();
                    Console.WriteLine(resultString);
                }
            }
        }
    }
}
