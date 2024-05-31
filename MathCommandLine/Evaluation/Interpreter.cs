using IML.CoreDataTypes;
using IML.Environments;
using IML.Parsing.AST;
using IML.Exceptions;
using IML.Functions;
using IML.Parsing;
using IML.Parsing.AST.ValueAsts;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IML.Evaluation
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
            return EvaluateAst(tree, env).Value;
        }

        // Throws an invalid syntax exception if it finds any invalid syntax
        private void EnsureValidity(Ast baseAst)
        {
            switch (baseAst.Type)
            {
                case AstTypes.Call:
                    {
                        CallAst ast = (CallAst)baseAst;
                        EnsureValidity(ast.CalledAst);
                        foreach (var child in ast.Arguments)
                        {
                            EnsureValidity(child);
                        }
                    }
                    break;
                case AstTypes.Invalid:
                    throw new InvalidParseException(((InvalidAst)baseAst).Expression);
                case AstTypes.NumberLiteral:
                case AstTypes.StringLiteral:
                case AstTypes.ReferenceLiteral:
                case AstTypes.Variable:
                    // Do nothing, this is perfectly legal always
                    break;
                case AstTypes.ListLiteral:
                    {
                        ListAst ast = (ListAst)baseAst;
                        foreach (var child in ast.Elements)
                        {
                            EnsureValidity(child);
                        }
                    }
                    break;
                case AstTypes.LambdaLiteral:
                    {
                        LambdaAst ast = (LambdaAst)baseAst;
                        foreach (Ast line in ast.Body)
                        {
                            EnsureValidity(line);
                        }
                    }
                    break;
                case AstTypes.MemberAccess:
                    EnsureValidity(((MemberAccessAst)baseAst).Parent);
                    break;
                case AstTypes.VariableAssignment:
                    EnsureValidity(((VariableAssignmentAst)baseAst).Value);
                    break;
                case AstTypes.VariableDeclaration:
                    EnsureValidity(((VariableDeclarationAst)baseAst).Value);
                    break;

            }
        }

        private ValueOrReturn EvaluateAst(Ast baseAst, MEnvironment env)
        {
            switch (baseAst.Type)
            {
                case AstTypes.Call:
                    {
                        CallAst ast = (CallAst)baseAst;
                        ValueOrReturn evaluatedCallerVR = EvaluateAst(ast.CalledAst, env);
                        if (evaluatedCallerVR.IsReturn)
                        {
                            return evaluatedCallerVR;
                        }
                        MValue evaluatedCaller = evaluatedCallerVR.Value;
                        // If an error, return that error instead of attempting to call it
                        if (evaluatedCaller.DataType.DataType == MDataType.Error)
                        {
                            return new ValueOrReturn(evaluatedCaller);
                        }
                        if (evaluatedCaller.DataType.DataType != MDataType.Function)
                        {
                            // Not a callable object
                            return new ValueOrReturn(MValue.Error(ErrorCodes.NOT_CALLABLE,
                                "Cannot invoke because \"" + evaluatedCaller.DataType.DataType.Name + "\" is not a callable data type.",
                                MList.Empty));
                        }
                        MFunction function = evaluatedCaller.FunctionValue;
                        List<MArgument> argsList = new List<MArgument>();
                        for (int i = 0; i < ast.Arguments.Count; i++)
                        {
                            // Don't worry; names are added to values later
                            ValueOrReturn argVr = EvaluateAst(ast.Arguments[i], env);
                            if (argVr.IsReturn)
                            {
                                return argVr;
                            }
                            argsList.Add(new MArgument(argVr.Value));
                        }
                        return PerformCall(function, new MArguments(argsList), env);
                    }
                case AstTypes.Variable:
                    // Return the value of the variable with this name
                    return new ValueOrReturn(env.Get(((VariableAst)baseAst).Name));
                case AstTypes.NumberLiteral:
                    return new ValueOrReturn(MValue.Number(((NumberAst)baseAst).Value));
                case AstTypes.StringLiteral:
                    return new ValueOrReturn(MValue.String(((StringAst)baseAst).Value));
                case AstTypes.ReferenceLiteral:
                    {
                        string name = ((ReferenceAst)baseAst).RefName;
                        MBoxedValue box = env.GetBox(name);
                        if (box != null)
                        {
                            return new ValueOrReturn(MValue.Reference(box));
                        }
                        else
                        {
                            return new ValueOrReturn(MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST,
                                    $"Variable \"{name}\" does not exist.", MList.Empty));
                        }
                    }
                case AstTypes.ListLiteral:
                    {
                        ListAst ast = (ListAst)baseAst;
                        // Need to evaluate each element of the list
                        List<MValue> elements = new List<MValue>();
                        MType listType = MType.UNION_BASE;
                        foreach (Ast elem in ast.Elements)
                        {
                            ValueOrReturn valueVr = EvaluateAst(elem, env);
                            if (valueVr.IsReturn)
                            {
                                return valueVr;
                            }
                            MValue value = valueVr.Value;
                            elements.Add(value);
                            listType = listType.Union(new MType(value.DataType));
                        }
                        return new ValueOrReturn(MValue.List(new MList(elements, listType)));
                    }
                case AstTypes.LambdaLiteral:
                    {
                        LambdaAst ast = (LambdaAst)baseAst;
                        // Immediately check to make sure we aren't allowing any parameters if the function
                        // doesn't create an environment
                        // Environment-less functions (i.e. ()~>{...}) cannot have parameters
                        if (ast.Parameters.Count > 0 && !ast.CreatesEnv)
                        {
                            return new ValueOrReturn(MValue.Error(ErrorCodes.ILLEGAL_LAMBDA,
                                "Lambdas that don't create environments (~>) cannot have parameters", MList.Empty));
                        }
                        int indexOfInvalidBody = ast.Body.FindIndex(x => x.Type == AstTypes.Invalid);
                        if (indexOfInvalidBody >= 0)
                        {
                            throw new InvalidParseException(((InvalidAst)ast.Body[indexOfInvalidBody]).Expression);
                        }

                        List<string> anonymousGenerics = new List<string>(ast.GenericNames);

                        MType returnType = ResolveType(ast.ReturnType, anonymousGenerics);
                        List<MType> paramTypes = new List<MType>();
                        List<string> paramNames = new List<string>();
                        for (int i = 0; i < ast.Parameters.Count; i++)
                        {
                            paramTypes.Add(ResolveType(ast.Parameters[i].Type, anonymousGenerics));
                            paramNames.Add(ast.Parameters[i].Name);
                        }
                        LambdaEnvironmentType envType =
                            ast.CreatesEnv ?
                            LambdaEnvironmentType.ForceEnvironment :
                            LambdaEnvironmentType.ForceNoEnvironment;

                        MFunctionDataTypeEntry funcType = MDataTypeEntry.Function(returnType, paramTypes,
                            ast.GenericNames, ast.IsPure, ast.IsLastVarArgs, envType);

                        MFunction function = new MFunction(funcType, paramNames, env, ast.Body);
                        // Create a function with this current environment
                        return new ValueOrReturn(MValue.Function(function));
                    }
                // Var declaration & assignment both return the value of the variable after the operation
                // This means that assignments such as "x += 5" will return the new value of x
                case AstTypes.VariableDeclaration:
                    {
                        VariableDeclarationAst ast = (VariableDeclarationAst)baseAst;
                        string newVarName = ast.Name;
                        ValueOrReturn newVarVr = EvaluateAst(ast.Value, env);
                        if (newVarVr.IsReturn)
                        {
                            return newVarVr;
                        }
                        MValue newVarValue = newVarVr.Value;
                        bool canSet = ast.VariableType == VariableType.Variable;
                        Regex reg = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
                        if (env.Has(newVarName))
                        {
                            return new ValueOrReturn(MValue.Error(ErrorCodes.CANNOT_DECLARE, 
                                $"Named value \"{newVarName}\" already exists."));
                        }
                        else if (!reg.IsMatch(newVarName))
                        {
                            return new ValueOrReturn(MValue.Error(ErrorCodes.CANNOT_DECLARE, 
                                $"The name \"{newVarName}\" is an invalid name."));
                        }
                        else
                        {
                            env.AddValue(newVarName, newVarValue, true, canSet, null);
                            return new ValueOrReturn(MValue.Void());
                        }
                    }
                case AstTypes.VariableAssignment:
                    {
                        VariableAssignmentAst ast = (VariableAssignmentAst)baseAst;
                        IdentifierAst identifier = ast.Identifier;
                        ValueOrReturn assignVr = EvaluateAst(ast.Value, env);
                        if (assignVr.IsReturn)
                        {
                            return assignVr;
                        }
                        MValue assignValue = assignVr.Value;
                        switch (identifier.Type)
                        {
                            case IdentifierAstTypes.RawVar:
                                string varName = identifier.Name;
                                if (!env.Has(varName))
                                {
                                    return new ValueOrReturn(MValue.Error(ErrorCodes.VAR_DOES_NOT_EXIST, 
                                        $"Variable \"{varName}\" does not exist."));
                                }
                                MBoxedValue boxToAssign = env.GetBox(varName);
                                if (!boxToAssign.CanSet)
                                {
                                    return new ValueOrReturn(MValue.Error(ErrorCodes.CANNOT_ASSIGN, 
                                        $"Cannot assign value to constant \"{varName}\""));
                                }
                                boxToAssign.SetValue(assignValue);
                                break;
                            case IdentifierAstTypes.MemberAccess:
                                ValueOrReturn assignParentVr = EvaluateAst(identifier.Parent, env); ;
                                if (assignParentVr.IsReturn)
                                {
                                    return assignParentVr;
                                }
                                MValue assignParent = assignParentVr.Value;
                                return new ValueOrReturn(assignParent.SetValueByName(identifier.Name, assignValue, false));
                            case IdentifierAstTypes.Dereference:
                                // TODO
                                break;
                        }
                        return new ValueOrReturn(MValue.Void());
                    }
                case AstTypes.MemberAccess:
                    {
                        MemberAccessAst ast = (MemberAccessAst)baseAst;
                        ValueOrReturn origVr = EvaluateAst(ast.Parent, env);
                        if (origVr.IsReturn)
                        {
                            return origVr;
                        }
                        MValue original = origVr.Value;
                        string key = ast.Name;
                        return new ValueOrReturn(original.GetValueByName(key, false));
                    }
                case AstTypes.Invalid:
                    throw new InvalidParseException(((InvalidAst)baseAst).Expression);
            }
            return null;
        }
        // If in an environment-less function, we "pass returns to parent"
        // i.e., if return is called, we want to go up
        // So something like this (assuming "if" is defined)
        // var func = ()=>{
        //  if(true, ()~>{return 5})
        // }
        // Should make "func" return 5, not return 5 from the environmentless lambda
        private ValueOrReturn EvaluateBody(List<Ast> body, MEnvironment env, bool passReturnsToParent)
        {
            // If single-line body, return what the body evaluates to
            if (body.Count == 1)
            {
                return EvaluateAst(body.First(), env);
            }

            for (int i = 0; i < body.Count; i++)
            {
                // If we find a return, check if we pass returns to parent
                // Otherwise, we'll ignore it
                if (body[i].Type == AstTypes.Return)
                {
                    Ast returnAstValue = ((ReturnAst)body[i]).Body;
                    ValueOrReturn retValVr = EvaluateAst(returnAstValue, env);
                    if (retValVr.IsReturn)
                    {
                        return retValVr;
                    }
                    MValue returnValue = retValVr.Value;
                    if (passReturnsToParent)
                    {
                        return new ValueOrReturn(true, returnValue);
                    }
                    else
                    {
                        return new ValueOrReturn(returnValue);
                    }
                }
                else
                {
                    // Simply run the line
                    EvaluateAst(body[i], env);
                }
            }
            // If we got down here, we didn't encounter a single return the entire time
            // Therefore, we return void
            return new ValueOrReturn(MValue.Void());
        }

        private MType ResolveType(AstType astType, List<string> anonymousGenerics)
        {
            if (astType.Entries.Count <= 0)
            {
                throw new FatalRuntimeException("Type has no entries when being resolved");
            }
            List<MDataTypeEntry> entries = new List<MDataTypeEntry>();
            for (int i = 0; i < astType.Entries.Count; i++)
            {
                MDataTypeEntry entry = ResolveTypeEntry(astType.Entries[i], anonymousGenerics);
                entries.Add(entry);
            }
            return new MType(entries);
        }
        private MDataTypeEntry ResolveTypeEntry(AstTypeEntry astTypeEntry, List<string> anonymousGenerics)
        {
            if (astTypeEntry is LambdaAstTypeEntry)
            {
                LambdaAstTypeEntry funcEntry = (LambdaAstTypeEntry)astTypeEntry;
                MType returnType = ResolveType(funcEntry.ReturnType, anonymousGenerics);
                List<MType> paramTypes = new List<MType>();
                for (int i = 0; i < funcEntry.ParamTypes.Count; i++)
                {
                    paramTypes.Add(ResolveType(funcEntry.ParamTypes[i], anonymousGenerics));
                }
                return new MFunctionDataTypeEntry(returnType, paramTypes, funcEntry.GenericNames,
                    funcEntry.IsPure, funcEntry.EnvironmentType, funcEntry.IsLastVarArgs);
            }
            else
            {
                if (dtDict.Contains(astTypeEntry.DataTypeName))
                {
                    MDataType dt = dtDict.GetType(astTypeEntry.DataTypeName);
                    if (dt.NumberOfGenerics != astTypeEntry.Generics.Count)
                    {
                        throw new InvalidTypeException(
                            $"The type \"{dt.Name}\" requires {dt.NumberOfGenerics} generic(s), but " +
                                $"{astTypeEntry.Generics.Count} were provided",
                            astTypeEntry);
                    }
                    List<MType> generics = new List<MType>();
                    for (int i = 0; i < astTypeEntry.Generics.Count; i++)
                    {
                        generics.Add(ResolveType(astTypeEntry.Generics[i], anonymousGenerics));
                    }
                    return new MConcreteDataTypeEntry(dt, generics);
                }
                else if (anonymousGenerics.Contains(astTypeEntry.DataTypeName))
                {
                    if (astTypeEntry.Generics.Count > 0)
                    {
                        // Generic entries cannot have generics themselves
                        throw new InvalidTypeException($"Generic entry \"{astTypeEntry.DataTypeName}\" cannot have generics",
                            astTypeEntry);
                    }
                    return new MGenericDataTypeEntry(astTypeEntry.DataTypeName);
                }
                else
                {
                    throw new InvalidTypeException($"Data type \"{astTypeEntry.DataTypeName}\" does not exist", 
                        astTypeEntry);
                }
            }
        }

        public ValueOrReturn PerformCall(MFunction function, MArguments args, MEnvironment currentEnv)
        {
            // We have a callable type!
            // Verify that everything is good to go before we actually call it
            // Need to check that we've been provided the right number of arguments
            MParameters parameters = function.Parameters;
            if (args.Length != parameters.Length)
            {
                return new ValueOrReturn(MValue.Error(ErrorCodes.WRONG_ARG_COUNT, "Expected " + parameters.Length +
                    " arguments but received " + args.Length + ".", MList.Empty));
            }
            // Now check the types of the arguments to ensure they match. If any errors appear in the arguments, return that immediately
            for (int i = 0; i < args.Length; i++)
            {
                if (!parameters[i].Type.ValueMatches(args[i].Value))
                {
                    // Improper data type!
                    return new ValueOrReturn(MValue.Error(ErrorCodes.INVALID_TYPE,
                        "Expected argument \"" + parameters.Get(i).Name + "\" to be of type '" +
                            parameters.Get(i).DataTypeString() + "' but received type '" + args[i].Value.DataType + "'.",
                        MList.FromOne(MValue.Number(i))));
                }
                else if (args[i].Value.DataType.DataType == MDataType.Error)
                {
                    // An error was passed as an argument, so simply need to return it
                    return new ValueOrReturn(args[i].Value);
                }
                else
                {
                    // Arg passes! But we need to make sure it's properly named
                    MArgument newArg = new MArgument(parameters[i].Name, args[i].Value);
                    args[i] = newArg;
                }
            }

            // Evaluate the function
            // Determine type
            // - If a native function, then hand it our current environment and call directly
            // - Otherwise, need to create a new environment and evaluate the body w/ that environment
            if (function.IsNativeBody)
            {
                return function.NativeBody(args, currentEnv, this);
            }
            else
            {
                // Step 1: Create the new environment (if the function creates a new one)
                MEnvironment envToUse = function.CreatesEnv ? new MEnvironment(function.Environment) : currentEnv;
                // Step 2: Evaluate the body with that new environment
                // Only add args if the function creates a new env; functions that don't create envs can't have params
                if (function.CreatesEnv)
                {
                    // Add the params and defined generics to the new environment; get them from the param values
                    for (int i = 0; i < args.Length; i++)
                    {
                        envToUse.AddVariable(args[i].Name, args[i].Value);
                        // TODO
                        // CANNOT simply set every generic to a var's type, ex.
                        // if we've got a (x:T|R, y:R)=>void called with (5, "hello")
                        // we can't set T and R to number, because then y is invalid, even though
                        // this is a valid call since T = number, R = string
                        // Need an algorithm to match these up best
                    }
                }

                return EvaluateBody(function.AstBody, envToUse, !function.CreatesEnv);
            }
        }
        
        public MDataType GetDataType(string typeName)
        {
            if (dtDict.Contains(typeName))
            {
                return dtDict.GetType(typeName);
            }
            return null;
        }
    }
}
