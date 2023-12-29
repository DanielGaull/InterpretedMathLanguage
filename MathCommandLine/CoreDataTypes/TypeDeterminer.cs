using IML.Evaluation;
using IML.Evaluation.AST.ValueAsts;
using IML.Exceptions;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public class TypeDeterminer
    {
        // Takes in an expression (AST) and determines the type of it
        public AstType DetermineDataType(Ast expression, VariableAstTypeMap currentMap)
        {
            switch (expression.Type)
            {
                case AstTypes.Return:
                    // Doesn't really matter what we put here, since assigning to a return expression
                    // will not work, we'll return from the function before we can resolve the return statement
                    // i.e. "var x = return 5;" will return 5 before it finishes declaring x
                    return new AstType(MDataType.VOID_TYPE_NAME);
                case AstTypes.NumberLiteral:
                    return new AstType(MDataType.NUMBER_TYPE_NAME);
                case AstTypes.StringLiteral:
                    return new AstType(MDataType.STRING_TYPE_NAME);
                case AstTypes.ListLiteral:
                    {
                        // Combine the types of the items within the list
                        ListAst list = (ListAst)expression;
                        if (list.Elements.Count == 0)
                        {
                            // Empty list, can hold any type then
                            return AstType.Any;
                        }
                        AstType listType = AstType.UNION_BASE;
                        foreach (Ast entry in list.Elements)
                        {
                            listType = listType.Union(DetermineDataType(entry, currentMap));
                        }
                        return new AstType(new AstTypeEntry(MDataType.LIST_TYPE_NAME, new List<AstType>() { listType }));
                    }
                case AstTypes.LambdaLiteral:
                    {
                        LambdaAst lambda = (LambdaAst)expression;
                        List<AstType> paramTypes = new List<AstType>();
                        foreach (AstParameter p in lambda.Parameters)
                        {
                            paramTypes.Add(p.Type);
                        }
                        LambdaAstTypeEntry lambdaTypeEntry = new LambdaAstTypeEntry(lambda.ReturnType, paramTypes,
                            lambda.CreatesEnv ? LambdaEnvironmentType.ForceEnvironment : LambdaEnvironmentType.ForceNoEnvironment,
                            lambda.IsPure, lambda.IsLastVarArgs, lambda.GenericNames);
                        return new AstType(lambdaTypeEntry);
                    }
                case AstTypes.Call:
                    {
                        CallAst callAst = (CallAst)expression;
                        AstType callerType = DetermineDataType(callAst.CalledAst, currentMap);
                        // This better be a lambda type or union of lambda types
                        AstType finalReturnType = AstType.UNION_BASE;
                        if (callerType.Entries.Count <= 0)
                        {
                            throw new TypeDeterminationException("Caller has no type entries", expression);
                        }
                        foreach (AstTypeEntry entry in callerType.Entries)
                        {
                            if (entry is LambdaAstTypeEntry)
                            {
                                LambdaAstTypeEntry l = (LambdaAstTypeEntry)entry;
                                finalReturnType = finalReturnType.Union(l.ReturnType);
                            }
                            else
                            {
                                throw new TypeDeterminationException("Cannot call a non-function", expression);
                            }
                        }
                        return finalReturnType;
                    }
                case AstTypes.VariableDeclaration:
                    {
                        VariableDeclarationAst vast = (VariableDeclarationAst)expression;
                        currentMap.Add(vast.Name, vast.VariableValueType);
                        return new AstType(MDataType.VOID_TYPE_NAME);
                    }
                case AstTypes.VariableAssignment:
                    {
                        // Simply returns void
                        return new AstType(MDataType.VOID_TYPE_NAME);
                    }
                case AstTypes.Variable:
                    {
                        VariableAst vast = (VariableAst)expression;
                        AstTypeOrEmpty aOrE = currentMap.Get(vast.Name);
                        if (aOrE.IsEmpty)
                        {
                            throw new TypeDeterminationException($"Variable {vast.Name} is not defined", expression);
                        }
                        else
                        {
                            return aOrE.Type;
                        }
                    }
                case AstTypes.ReferenceLiteral:
                    {
                        ReferenceAst rast = (ReferenceAst)expression;
                        AstTypeOrEmpty aOrE = currentMap.Get(rast.RefName);
                        if (aOrE.IsEmpty)
                        {
                            throw new TypeDeterminationException($"Variable {rast.RefName} is not defined", expression);
                        }
                        else
                        {
                            // Return a reference of this type
                            return new AstType(MDataType.REF_TYPE_NAME, aOrE.Type);
                        }
                    }
                case AstTypes.Invalid:
                    throw new TypeDeterminationException("Expression given was Invalid", expression);
            }
            return null;
        }

        public AstType DetermineDataType(List<Ast> body, VariableAstTypeMap currentMap)
        {
            // If one entry, return just that. Otherwise, look for return ASTs and union them
            if (body.Count == 1)
            {
                return DetermineDataType(body[body.Count - 1], currentMap);
            }
            else
            {
                // Find all return ASTs and union them
                AstType returnType = AstType.UNION_BASE;
                bool foundReturn = false;
                foreach (Ast ast in body)
                {
                    // Call this simply so that it updates the variable type map properly
                    // We only care about the type of the expression if it's a return, but this will ensure
                    // that our map is properly updated
                    DetermineDataType(ast, currentMap);
                    // If return, we want to include it in our return type
                    if (ast.Type == AstTypes.Return)
                    {
                        ReturnAst rast = ast as ReturnAst;
                        returnType = returnType.Union(DetermineDataType(rast.Body, currentMap));
                        foundReturn = true;
                    }
                }
                // If still union base, return type is void
                // TODO: could still have a void even if we've found "return" statements
                if (!foundReturn)
                {
                    return new AstType(MDataType.VOID_TYPE_NAME);
                }
                return returnType;
            }
        }
    }
}
