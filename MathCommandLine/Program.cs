using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
using MathCommandLine.Evaluation;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MathCommandLine
{
    class Program
    {
        static Interpreter evaluator;
        static FunctionDict funcDict;

        static readonly string SYNTAX_FILES_PATH =
            SYNTAX_FILES_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IML\\syntax_files";

        static void Main(string[] args)
        {
            evaluator = new Interpreter();

            // Add core constants
            List<MFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions(evaluator);
            MEnvironment baseEnv = new MEnvironment(MEnvironment.Empty);
            baseEnv.AddConstant("null", MValue.Null());
            baseEnv.AddConstant("void", MValue.Void());
            baseEnv.AddConstant("TRUE", MValue.Bool(true));
            baseEnv.AddConstant("FALSE", MValue.Bool(false));
            for (int i = 0; i < coreFuncs.Count; i++)
            {
                MValue closure = MValue.Closure(
                    new MClosure(coreFuncs[i].Parameters, MEnvironment.Empty, coreFuncs[i].Expression));
                baseEnv.AddConstant(coreFuncs[i].Name, closure);
            }
            funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Closure,
                MDataType.Type, MDataType.Error, MDataType.Reference, MDataType.String, MDataType.Void,
                MDataType.Boolean, MDataType.Null, MDataType.Any);
            Parser parser = new Parser();
            evaluator.Initialize(dtDict, parser);

            // Syntax loading; go to the syntax files path, and load in all the files it lists
            
            SyntaxHandler sh = new SyntaxHandler(parser, "{}(),".ToCharArray().ToList());
            List<SyntaxDef> syntaxDefinitions = ImportSyntax(sh);

            // SYNTAX TESTING
            //SyntaxDef def = new SyntaxDef(new List<SyntaxDefSymbol> {
            //    new SyntaxDefSymbol(new SyntaxParameter("a")),
            //    new SyntaxDefSymbol("+"),
            //    new SyntaxDefSymbol(new SyntaxParameter("b"))
            //}, new List<SyntaxResultSymbol>() {
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, "_add("),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "a"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ","),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "b"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ")")
            //});
            //SyntaxDef mulDef = new SyntaxDef(new List<SyntaxDefSymbol> {
            //    new SyntaxDefSymbol(new SyntaxParameter("a")),
            //    new SyntaxDefSymbol("*"),
            //    new SyntaxDefSymbol(new SyntaxParameter("b"))
            //}, new List<SyntaxResultSymbol>() {
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, "_mul("),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "a"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ","),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "b"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ")")
            //});
            //// Syntax definition for variable declarations
            //SyntaxDef def2 = new SyntaxDef(new List<SyntaxDefSymbol> {
            //    new SyntaxDefSymbol("var"),
            //    new SyntaxDefSymbol(),
            //    new SyntaxDefSymbol(new SyntaxParameter("name", true, false)),
            //    new SyntaxDefSymbol("="),
            //    new SyntaxDefSymbol(new SyntaxParameter("value"))
            //}, new List<SyntaxResultSymbol>() {
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, "_declare("),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "name"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ","),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "value"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ",TRUE,TRUE,TRUE)")
            //});
            //SyntaxDef def3 = new SyntaxDef(new List<SyntaxDefSymbol> {
            //    new SyntaxDefSymbol(new SyntaxParameter("name", true, false)),
            //    new SyntaxDefSymbol("="),
            //    new SyntaxDefSymbol(new SyntaxParameter("value"))
            //}, new List<SyntaxResultSymbol>() {
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, "_assign(_ref("),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "name"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, "),"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "value"),
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.ExpressionPiece, ")")
            //});
            //SyntaxDef def4 = new SyntaxDef(new List<SyntaxDefSymbol> {
            //    new SyntaxDefSymbol("["),
            //    new SyntaxDefSymbol(new SyntaxParameter("code", false, true)),
            //    new SyntaxDefSymbol("]")
            //}, new List<SyntaxResultSymbol>() {
            //    new SyntaxResultSymbol(SyntaxResultSymbolTypes.Argument, "code")
            //});
            //List<SyntaxDef> definitions = new List<SyntaxDef>() {
            //    def4, def, mulDef, def2, def3
            //};
            ////var m = sh.Match(def, "1+2");
            //var x = sh.Convert(def, "1+2");
            //var z = sh.Convert(def2, "var x=7");
            //var w = sh.Convert(def3, "x=7");
            //var a = sh.Convert(def4, "[_add(5,2)]");
            //Console.WriteLine(x);
            //Console.WriteLine(z);
            //Console.WriteLine(w);
            //Console.WriteLine(a);

            //var b = sh.FullConvert(definitions, "[5+2]");
            //Console.WriteLine(b);

            //sh.ParseSyntaxDefinitionLine("syntax \"{a}+{b}\" \"_add(\"a\",\"b\")\"");

            // Simple reading for now
            while (true)
            {
                Console.Write("Enter Expression: ");
                string input = Console.ReadLine();
                if (input.Length <= 0)
                {
                    continue;
                }
                MValue result;
                try
                {
                    string syntaxHandled = sh.FullConvert(syntaxDefinitions, input);
                    result = evaluator.Evaluate(syntaxHandled, baseEnv);
                    if (result.DataType != MDataType.Void)
                    {
                        // Never output void as a result, since we're typically running a function
                        string resultString = result.ToLongString();
                        Console.WriteLine(resultString);
                    }
                }
                catch (InvalidParseException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        static List<SyntaxDef> ImportSyntax(SyntaxHandler sh)
        {
            string syntaxFiles = File.ReadAllText(SYNTAX_FILES_PATH);
            string[] syntaxFilesLines = syntaxFiles.Split(new string[] { Environment.NewLine },
                StringSplitOptions.None);
            List<SyntaxDef> defs = new List<SyntaxDef>();
            foreach (string filepath in syntaxFilesLines)
            {
                if (filepath.Length <= 0)
                {
                    continue;
                }
                // Open the corresponding file
                string fileContents = File.ReadAllText(filepath);
                string[] lines = fileContents.Split(new string[] { Environment.NewLine },
                    StringSplitOptions.None);
                foreach (string line in lines)
                {
                    if (line.Length <= 0)
                    {
                        continue;
                    }
                    if (line.StartsWith(";"))
                    {
                        // Used for comments, ignore this line
                        continue;
                    }
                    SyntaxDef def = sh.ParseSyntaxDefinitionLine(line);
                    defs.Add(def);
                }
            }
            return defs;
        }
    }
}
