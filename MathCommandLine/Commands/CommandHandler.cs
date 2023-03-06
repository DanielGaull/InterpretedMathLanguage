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
                RunCommand(args[0]);
            }
        }

        // Returns 0 if execution is normal
        // Returns 1 if entire program should exit
        private int RunCommand(string cmd)
        {
            string cmdStarter = cmd.IndexOf(' ') >= 0 ? cmd.Substring(0, cmd.IndexOf(' ')) : cmd;
            switch (cmdStarter)
            {
                case "eval":
                case "e":
                    RunInterpreter();
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
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Command not recognized");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            return 0;
        }

        private void RunInterpreter()
        {
            Interpreter evaluator = new Interpreter();

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
            FunctionDict funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Closure,
                MDataType.Type, MDataType.Error, MDataType.Reference, MDataType.String, MDataType.Void,
                MDataType.Boolean, MDataType.Null, MDataType.Any);
            Parser parser = new Parser();
            bool running = true;
            evaluator.Initialize(dtDict, parser, () => {
                running = false;
            });

            // Syntax loading; go to the syntax files path, and load in all the files it lists
            SyntaxHandler sh = new SyntaxHandler(parser, "{}(),".ToCharArray().ToList());
            List<SyntaxDef> syntaxDefinitions = ImportSyntax(sh);

            // Simple reading for now
            while (running)
            {
                Console.Write("  Enter Expression: ");
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
                        string resultString = "  " + result.ToLongString();
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
    }
}
