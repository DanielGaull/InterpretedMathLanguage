using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Util;
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
        Action exitAction;

        private static readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");

        public Interpreter()
        {
        }

        public void Initialize(DataTypeDict dtDict, Parser parser, Action exitAction)
        {
            this.dtDict = dtDict;
            this.parser = parser;
            this.exitAction = exitAction;
        }

        public void Exit()
        {
            exitAction?.Invoke();
        }

        public MValue Evaluate(string expression, MEnvironment env)
        {
            // TODO: Intelligently handle whitespace in the parser, rather than removing it here
            //expression = CleanWhitespace(expression);
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
            Ast tree = parser.Parse(expression);
            EnsureValidity(tree);
            return EvaluateAst(tree, env);
        }

        // Throws an invalid syntax exception if it finds any invalid syntax
        private void EnsureValidity(Ast ast)
        {
            switch (ast.Type)
            {
                case AstTypes.Call:
                    EnsureValidity(ast.ParentAst);
                    foreach (var child in ast.AstCollectionArg)
                    {
                        EnsureValidity(child);
                    }
                    break;
                case AstTypes.Invalid:
                    throw new InvalidParseException(ast.Expression);
                case AstTypes.NumberLiteral:
                case AstTypes.StringLiteral:
                case AstTypes.ReferenceLiteral:
                case AstTypes.Variable:
                    // Do nothing, this is perfectly legal always
                    break;
                case AstTypes.ListLiteral:
                    foreach (var child in ast.AstCollectionArg)
                    {
                        EnsureValidity(child);
                    }
                    break;
                case AstTypes.LambdaLiteral:
                    EnsureValidity(ast.Body);
                    break;
                case AstTypes.MemberAccess:
                    EnsureValidity(ast.ParentAst);
                    break;
                case AstTypes.VariableAssignment:
                case AstTypes.VariableDeclaration:
                    EnsureValidity(ast.Body);
                    break;

            }
        }

        private MValue EvaluateAst(Ast ast, MEnvironment env)
        {
            switch (ast.Type)
            {
                case AstTypes.Call:
                    MValue evaluatedCaller = EvaluateAst(ast.ParentAst, env);
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
                case AstTypes.StringLiteral:
                    return MValue.String(ast.StringArg);
                case AstTypes.ReferenceLiteral:
                    MBoxedValue box = env.GetBox(ast.Name);
                    if (box != null)
                    {
                        return MValue.Reference(box);
                    }
                    else
                    {
                        return MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST,
                                $"Variable \"{ast.Name}\" does not exist.", MList.Empty);
                    }
                case AstTypes.ListLiteral:
                    // Need to evaluate each element of the list
                    List<MValue> elements = new List<MValue>();
                    foreach (Ast elem in ast.AstCollectionArg)
                    {
                        elements.Add(EvaluateAst(elem, env));
                    }
                    return MValue.List(new MList(elements));
                case AstTypes.LambdaLiteral:
                    // Immediately check to make sure we aren't allowing any parameters if the closure
                    // doesn't create an environment
                    // Environment-less closures (i.e. ()~>{...}) cannot have parameters
                    if (ast.Parameters.Length > 0 && !ast.CreatesEnv)
                    {
                        return MValue.Error(ErrorCodes.ILLEGAL_LAMBDA,
                            "Lambdas that don't create environments (~>) cannot have parameters", MList.Empty);
                    }
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
                    // Make sure the body is valid
                    if (ast.Body.Type == AstTypes.Invalid)
                    {
                        throw new InvalidParseException(ast.Body.Expression);
                    }
                    // Create a closure with this current environment
                    return MValue.Closure(new MClosure(parameters, env, ast.Body, ast.CreatesEnv));
                // Var declaration & assignment both return the value of the variable after the operation
                // This means that assignments such as "x += 5" will return the new value of x
                case AstTypes.VariableDeclaration:
                    string newVarName = ast.Name;
                    MValue newVarValue = EvaluateAst(ast.Body, env);
                    bool canSet = ast.VariableType == VariableType.Variable;
                    Regex reg = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
                    if (env.Has(newVarName))
                    {
                        return MValue.Error(ErrorCodes.CANNOT_DECLARE, $"Named value \"{newVarName}\" already exists.");
                    }
                    else if (!reg.IsMatch(newVarName))
                    {
                        return MValue.Error(ErrorCodes.CANNOT_DECLARE, $"The name \"{newVarName}\" is an invalid name.");
                    }
                    else
                    {
                        env.AddValue(newVarName, newVarValue, true, canSet, null);
                        return newVarValue;
                    }
                case AstTypes.VariableAssignment:
                    IdentifierAst identifier = ast.Identifier;
                    MValue assignValue = EvaluateAst(ast.Body, env);
                    switch (identifier.Type)
                    {
                        case IdentifierAstTypes.RawVar:
                            string varName = identifier.Name;
                            if (!env.Has(varName))
                            {
                                return MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST, $"Variable \"{varName}\" does not exist.");
                            }
                            MBoxedValue boxToAssign = env.GetBox(varName);
                            if (!boxToAssign.CanSet)
                            {
                                return MValue.Error(ErrorCodes.CANNOT_ASSIGN, $"Cannot assign value to constant \"{varName}\"");
                            }
                            boxToAssign.SetValue(assignValue);
                            break;
                        case IdentifierAstTypes.MemberAccess:
                            MValue assignParent = EvaluateAst(identifier.Parent, env);
                            return assignParent.SetValueByName(identifier.Name, assignValue, false);
                        case IdentifierAstTypes.Dereference:
                            // TODO
                            break;
                    }
                    return assignValue;
                case AstTypes.MemberAccess:
                    MValue original = EvaluateAst(ast.ParentAst, env);
                    string key = ast.Name;
                    return original.GetValueByName(key, false);
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
                // Step 1: Create the new environment (if the closure creates a new one)
                MEnvironment envToUse = closure.CreatesEnv ? new MEnvironment(closure.Environment) : currentEnv;
                // Step 2: Evaluate the body with that new environment
                // Only add args if the closure creates a new env; closures that don't create envs can't have params
                if (closure.CreatesEnv)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        envToUse.AddVariable(args[i].Name, args[i].Value);
                    }
                }
                return EvaluateAst(closure.AstBody, envToUse);
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

        public MDataType AddDataType(string typeName)
        {
            if (dtDict.Contains(typeName))
            {
                return MDataType.Empty;
            }
            MDataType t = dtDict.CreateAndRegisterType(typeName);
            return t;
        }
    }
}
