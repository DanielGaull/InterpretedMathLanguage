﻿using IML.Evaluation;
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
        public AstType DetermineDataType(Ast expression)
        {
            switch (expression.Type)
            {
                case AstTypes.NumberLiteral:
                    return new AstType("number");
                case AstTypes.StringLiteral:
                    return new AstType("string");
                case AstTypes.ListLiteral:
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
                        listType = listType.Union(DetermineDataType(entry));
                    }
                    return listType;
                case AstTypes.LambdaLiteral:
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
                case AstTypes.Call:
                    CallAst callAst = (CallAst)expression;
                    AstType callerType = DetermineDataType(callAst.CalledAst);
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
                // Todo... the rest
            }
            return null;
        }

        public AstType DetermineDataType(List<Ast> body)
        {
            // TODO: If one entry, return just that. Otherwise, look for return ASTs and union them
            return DetermineDataType(body[body.Count - 1]);
        }
    }
}
