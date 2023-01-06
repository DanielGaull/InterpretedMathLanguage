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
            SyntaxDef def = new SyntaxDef(new List<SyntaxDefSymbol> {
                new SyntaxDefSymbol(new SyntaxParameter(new MParameter(MDataType.Number, "a"))),
                new SyntaxDefSymbol("+"),
                new SyntaxDefSymbol(new SyntaxParameter(new MParameter(MDataType.Number, "b")))
            }, new List<SyntaxResultSymbol>() {
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, "_add("),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "a"),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ","),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "b"),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ")")
            });
            // Syntax definition for variable declarations
            SyntaxDef def2 = new SyntaxDef(new List<SyntaxDefSymbol> {
                new SyntaxDefSymbol("var"),
                new SyntaxDefSymbol(),
                new SyntaxDefSymbol(new SyntaxParameter(new MParameter(MDataType.String, "name"), true, false)),
                new SyntaxDefSymbol("="),
                new SyntaxDefSymbol(new SyntaxParameter(new MParameter(MDataType.Any, "value")))
            }, new List<SyntaxResultSymbol>() {
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, "_declare("),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "name"),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ","),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "value"),
                new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ", TRUE, TRUE, TRUE)")
            });
            SyntaxHandler sh = new SyntaxHandler(evaluator);
            var m = sh.Match(def, "1+2");
            var x = sh.Convert(def, "1+2");
            var z = sh.Convert(def2, "var x=7"); // z is null right now

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
