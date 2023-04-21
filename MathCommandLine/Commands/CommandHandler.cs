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
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Commands
{
    public class CommandHandler
    {
        static readonly string SYNTAX_FILES_PATH =
            SYNTAX_FILES_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IML\\syntax_files";

        const string HELP_STRING = "== IML CLI Help ==\n" +
            "eval - Start evaluation cycle. Evaluated expressions as they are entered. Run the \"_exit()\" function " +
            "to exit the evaluation cycle.\n" +
            "help - Display this help list\n" +
            "exit - Exit the interpreter command reader, if in a multiple-enter mode\n";

        public CommandHandler()
        {

        }

        public void Run(string[] args)
        {
            if (args.Length <= 0)
            {
                bool runningProgram = true;
                while (runningProgram)
                {
                    Console.Write("$ ");
                    string cmd = Console.ReadLine();
                    int result = RunCommand(cmd);
                    if (result == 1)
                    {
                        runningProgram = false;
                    }
                }
            }
            else
            {
                RunCommand(string.Join(' ', args));
            }
        }

        // Returns 0 if execution is normal
        // Returns 1 if entire program should exit
        private int RunCommand(string cmd)
        {
            int spaceIndex = cmd.IndexOf(' ');
            string cmdStarter = spaceIndex >= 0 ? cmd.Substring(0, spaceIndex) : cmd;
            switch (cmdStarter)
            {
                case "eval":
                case "e":
                    RunInterpreter();
                    break;
                case "run":
                case "r":
                    string fp = cmd.Substring(spaceIndex + 1);
                    if (fp.Length <= 0 || spaceIndex < 0)
                    {
                        PrintError("Must provide a file path to run");
                        return 0;
                    }
                    RunFile(fp);
                    break;
                case "help":
                case "h":
                case "?":
                    Console.WriteLine(HELP_STRING);
                    break;
                case "exit":
                case "quit":
                case "close":
                case "q":
                    return 1;
                case "test":
                case "tests":
                case "t":
                    RunTests();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Command not recognized");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            return 0;
        }

        private Interpreter CreateInterpreter(Parser parser, Action exitAction)
        {
            Interpreter evaluator = new Interpreter();

            //FunctionDict funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Closure,
                MDataType.Type, MDataType.Error, MDataType.Reference, MDataType.String, MDataType.Void,
                MDataType.Boolean, MDataType.Null, MDataType.Any);
            evaluator.Initialize(dtDict, parser, exitAction);
            return evaluator;
        }
        private MEnvironment CreateBaseEnv(Interpreter evaluator)
        {
            // Add core constants
            List<MFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions(evaluator);
            MEnvironment baseEnv = new MEnvironment(MEnvironment.Empty);
            baseEnv.AddConstant("null", MValue.Null());
            baseEnv.AddConstant("void", MValue.Void());
            baseEnv.AddConstant("true", MValue.Bool(true));
            baseEnv.AddConstant("false", MValue.Bool(false));
            for (int i = 0; i < coreFuncs.Count; i++)
            {
                MValue closure = MValue.Closure(
                    new MClosure(coreFuncs[i].Parameters, MEnvironment.Empty, coreFuncs[i].Expression));
                baseEnv.AddConstant(coreFuncs[i].Name, closure, coreFuncs[i].Description);
            }
            return baseEnv;
        }

        private MValue RunLine(MEnvironment env, SyntaxHandler handler,
            List<SyntaxDef> syntaxDefs, Interpreter evaluator, string line)
        {
            string syntaxHandled = handler.FullConvert(syntaxDefs, line);
            MValue result = evaluator.Evaluate(syntaxHandled, env);
            return result;
        }

        private void RunFile(string filePath)
        {
            string text = File.ReadAllText(filePath);

            Parser parser = new Parser();

            bool running = true;
            Interpreter evaluator = CreateInterpreter(parser, () =>
            {
                running = false;
            });

            MEnvironment baseEnv = CreateBaseEnv(evaluator);

            // Syntax loading; go to the syntax files path, and load in all the files it lists
            SyntaxHandler sh = new SyntaxHandler(parser, "{}(),".ToCharArray().ToList());
            List<SyntaxDef> syntaxDefinitions = ImportSyntax(sh);

            // Simple reading for now
            string[] lines = Regex.Split(text, "(?:\r\n|\r|\n)+");
            foreach (string line in lines)
            {
                if (line.Length <= 0)
                {
                    continue;
                }
                if (!running)
                {
                    return;
                }
                RunLine(baseEnv, sh, syntaxDefinitions, evaluator, line);
            }
        }

        private void RunInterpreter()
        {
            Parser parser = new Parser();

            bool running = true;
            Interpreter evaluator = CreateInterpreter(parser, () =>
            {
                running = false;
            });

            MEnvironment baseEnv = CreateBaseEnv(evaluator);

            // Syntax loading; go to the syntax files path, and load in all the files it lists
            SyntaxHandler sh = new SyntaxHandler(parser, "{}(),".ToCharArray().ToList());
            List<SyntaxDef> syntaxDefinitions = ImportSyntax(sh);

            // Simple reading for now
            while (running)
            {
                Console.Write(" Enter Expression: ");
                string input = Console.ReadLine();
                if (input.Length <= 0)
                {
                    continue;
                }
                try
                {
                    MValue result = RunLine(baseEnv, sh, syntaxDefinitions, evaluator, input);
                    if (result.DataType != MDataType.Void)
                    {
                        // Never output void as a result, since we're typically running a function
                        string resultString = "  " + result.ToLongString();
                        Console.WriteLine(resultString);

                        // TEST: output the unparsed exp
                        Console.WriteLine(parser.Unparse(parser.ParseExpression(input)));
                    }
                }
                catch (InvalidParseException ex)
                {
                    PrintError(ex.Message);
                }
            }
        }

        private void RunTests()
        {
            // TODO: This is duplicated with the RunInterpreter code
            Parser parser = new Parser();

            bool stopped = false;
            Interpreter evaluator = CreateInterpreter(parser, () =>
            {
                // On exit we just flag that we've exited; shouldn't really be used
                stopped = true;
            });

            MEnvironment baseEnv = CreateBaseEnv(evaluator);

            SyntaxHandler sh = new SyntaxHandler(parser, "{}(),".ToCharArray().ToList());
            List<SyntaxDef> syntaxDefinitions = ImportSyntax(sh);

            List<Tuple<string, string>> tests = GetTests();
            int passed = 0;
            foreach (var test in tests)
            {
                string input = test.Item1;
                string expected = test.Item2;
                string syntaxHandled = sh.FullConvert(syntaxDefinitions, input);
                string output;
                bool success;
                try
                {
                    MValue result = evaluator.Evaluate(syntaxHandled, baseEnv);
                    output = result.ToLongString();
                    success = expected == output;
                }
                catch (Exception ex)
                {
                    success = false;
                    output = "Exception";
                }
                if (success)
                {
                    passed++;
                }
                PrintTestResult(input, output, expected, success);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Passed " + passed + "/" + tests.Count + " tests.");
        }

        private List<SyntaxDef> ImportSyntax(SyntaxHandler sh)
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

        private void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void PrintTestResult(string input, string output, string expected, bool success)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(input);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" --> ");
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write(output);
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!success)
            {
                Console.Write(" (Expected: '" + expected + "')\n");
            }
            Console.Write("\n");
        }

        private List<Tuple<string, string>> GetTests()
        {
            return new List<Tuple<string, string>>() { 
                new Tuple<string, string>("_c({{()=>{TRUE},()=>{1}},{()=>{TRUE},()=>{2}}})", "(number) 1"),
                new Tuple<string, string>("_c({{()=>{null},()=>{1}},{()=>{TRUE},()=>{2}}})", "(number) 2"),
                new Tuple<string, string>("_add(1,2)", "(number) 3"),
                new Tuple<string, string>("(()=>{(()=>{1})()})()", "(number) 1"),
                new Tuple<string, string>("(()=>{2})()", "(number) 2"),
                new Tuple<string, string>("(()=>{()=>{3}})()()", "(number) 3"),
                new Tuple<string, string>("_map({1,2,3},(x)=>{_add(x,1)})", "(list) { 2, 3, 4 }"),
                new Tuple<string, string>("_reduce({1,2,3,4,5},(prev,current)=>{_add(prev,current)},0)", "(number) 15"),
                new Tuple<string, string>("_map(_crange(5),(x)=>{_add(x,5)})", "(list) { 5, 6, 7, 8, 9 }"),
                new Tuple<string, string>("_or_e(()=>{1},()=>{4})", "(number) 1"),
                new Tuple<string, string>("_or_e(()=>{null},()=>{4})", "(number) 4"),
                new Tuple<string, string>("_and_e(()=>{null},()=>{4})", "(null) null"),
                new Tuple<string, string>("_and_e(()=>{1},()=>{4})", "(number) 4"),
                new Tuple<string, string>("_do({()=>{var x = 5},()=>{var y = &x},()=>{_set(y,3)},()=>{x}})", "(number) 3"),
                new Tuple<string, string>("_map({1,2},(x)=>{_exit})", "(list) { ()~>{<function>}, ()~>{<function>} }"),
                new Tuple<string, string>("5*(2+3)", "(number) 25"),
                new Tuple<string, string>("(2+3)*5", "(number) 25")
            };
        }
    }
}
