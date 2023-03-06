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
                    }
                }
                catch (InvalidParseException ex)
                {
                    PrintError(ex.Message);
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

        private void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
