using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using MathCommandLine.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Evaluation
{
    public class Interpreter : IInterpreter
    {
        DataTypeDict dtDict;
        Parser parser;

        private static readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");

        public Interpreter()
        {
        }

        public void Initialize(DataTypeDict dtDict, Parser parser)
        {
            this.dtDict = dtDict;
            this.parser = parser;
        }

        public MValue Evaluate(string expression, MEnvironment env)
        {
            expression = parser.ConvertStringsToLists(expression);
            expression = CleanWhitespace(expression);
            return FinalStageEvaluate(expression, env);
        }

        private string CleanWhitespace(string expression)
        {
            return WHITESPACE_REGEX.Replace(expression, "");
        }

        // For the "final stage" in evaluation, when the expression has been whittled down to only
        // functions, variables (i.e. arguments), and literal core values
        private MValue FinalStageEvaluate(string expression, MEnvironment env)
        {
            Ast tree = parser.ParseExpression(expression);
            return EvaluateAst(tree, env);
        }

        private MValue EvaluateAst(Ast ast, MEnvironment env)
        {
            switch (ast.Type)
            {
                case AstTypes.Call:
                    MValue evaluatedCaller = EvaluateAst(ast.CalledAst, env);
                    // If an error, return that error instead of attempting to call it
                    if (evaluatedCaller.DataType == MDataType.Error)
                    {
                        return evaluatedCaller;
                    }
                    if (evaluatedCaller.DataType != MDataType.Closure)
                    {
                        // Not a callable object
                        return MValue.Error(ErrorCodes.NOT_CALLABLE,
                            "Cannot invoke because \"" + evaluatedCaller.DataType.Name + "\" is not a callable data type.",
                            MList.Empty);
                    }
                    MClosure closure = evaluatedCaller.ClosureValue;
                    List<MArgument> argsList = new List<MArgument>();
                    for (int i = 0; i < ast.AstCollectionArg.Length; i++)
                    {
                        // Don't worry; names are added to values later
                        argsList.Add(new MArgument(EvaluateAst(ast.AstCollectionArg[i], env)));
                    }
                    return PerformCall(closure, new MArguments(argsList), env);
                case AstTypes.Variable:
                    // Return the value of the variable with this name
                    return env.Get(ast.Name);
                case AstTypes.NumberLiteral:
                    return MValue.Number(ast.NumberArg);
                case AstTypes.ListLiteral:
                    // Need to evaluate each element of the list
                    List<MValue> elements = new List<MValue>();
                    foreach (Ast elem in ast.AstCollectionArg)
                    {
                        elements.Add(EvaluateAst(elem, env));
                    }
                    return MValue.List(new MList(elements));
                case AstTypes.LambdaLiteral:
                    MParameter[] paramArray = ast.Parameters.Select((astParam) =>
                    {
                        string name = astParam.Name;
                        MTypeRestrictionsEntry[] entries = astParam.TypeEntries.Select((entry) =>
                        {
                            if (!dtDict.Contains(entry.DataTypeName))
                            {
                                return new MTypeRestrictionsEntry();
                            }
                            return new MTypeRestrictionsEntry(dtDict.GetType(entry.DataTypeName), entry.ValueRestrictions);
                        }).ToArray();
                        if (entries.Any((entry) => entry.IsEmpty))
                        {
                            return MParameter.Empty;
                        }
                        // If any type entries are empty, then return an error (type doesn't exist)
                        return new MParameter(name, entries);
                    }).ToArray();
                    if (paramArray.Any((param) => param.IsEmpty))
                    {
                        return MValue.Error(ErrorCodes.TYPE_DOES_NOT_EXIST,
                            "Type \"" + ast.Name + "\" is not defined.", MList.Empty);
                    }
                    MParameters parameters = new MParameters(paramArray);
                    // Create a closure with this current environment
                    return MValue.Closure(new MClosure(parameters, env, ast.Body));
                case AstTypes.Invalid:
                    throw new InvalidParseException(ast.Expression);
            }
            return MValue.Empty;
        }

        public MValue PerformCall(MClosure closure, MArguments args, MEnvironment currentEnv)
        {
            // We have a callable type!
            // Verify that everything is good to go before we actually call it
            // Need to check that we've been provided the right number of arguments
            MParameters parameters = closure.Parameters;
            if (args.Length != parameters.Length)
            {
                return MValue.Error(ErrorCodes.WRONG_ARG_COUNT, "Expected " + parameters.Length +
                    " arguments but received " + args.Length + ".", MList.Empty);
            }
            // Now check the types of the arguments to ensure they match. If any errors appear in the arguments, return that immediately
            for (int i = 0; i < args.Length; i++)
            {
                if (!parameters[i].ContainsType(args[i].Value.DataType))
                {
                    // Improper data type!
                    return MValue.Error(ErrorCodes.INVALID_TYPE,
                        "Expected argument \"" + parameters.Get(i).Name + "\" to be of type '" +
                            parameters.Get(i).DataTypeString() + "' but received type '" + args[i].Value.DataType + "'.",
                        MList.FromOne(MValue.Number(i)));
                }
                else if (!parameters[i].PassesRestrictions(args[i].Value))
                {
                    // Fails restrictions!
                    return MValue.Error(ErrorCodes.FAILS_RESTRICTION,
                        "Argument \"" + parameters.Get(i).Name + "\" fails one or more parameter restrictions.",
                        MList.FromOne(MValue.Number(i)));
                }
                else if (args[i].Value.DataType == MDataType.Error)
                {
                    // An error was passed as an argument, so simply need to return it
                    // TODO: Allow a flag that prevents this from happening and allows errors to be fed to functions
                    return args[i].Value;
                }
                else
                {
                    // Arg passes! But we need to make sure it's properly named
                    MArgument newArg = new MArgument(parameters[i].Name, args[i].Value);
                    args[i] = newArg;
                }
            }

            // Evaluate the closure
            // Determine type
            // - If a native closure, then hand it our current environment and call directly
            // - Otherwise, need to create a new environment and evaluate the body w/ that environment
            if (closure.IsNativeBody)
            {
                return closure.NativeBody(args, currentEnv);
            }
            else
            {
                // Step 1: Create the new environment
                // Step 2: Evaluate the body with that new environment
                MEnvironment newEnv = new MEnvironment(closure.Environment);
                for (int i = 0; i < args.Length; i++)
                {
                    newEnv.AddVariable(args[i].Name, args[i].Value);
                }
                return EvaluateAst(closure.AstBody, newEnv);
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
