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

                    break;
                case AstTypes.Call:
                    CallAst callAst = (CallAst)expression;
                    AstType callerType = DetermineDataType(callAst.CalledAst);
                    // This better be a lambda type or union of lambda types
                    foreach (AstTypeEntry entry in callerType.Entries) 
                    {
                        if (entry is LambdaAstTypeEntry)
                        {
                            LambdaAstTypeEntry l = (LambdaAstTypeEntry)entry;
                            // TODO: Union return types
                            // l.ReturnType
                        }
                        else
                        {
                            throw new TypeDeterminationException("Cannot call a non-function", expression);
                        }
                    }
                    
                    break;
                // Todo... the rest
            }
            return null;
        }

        public AstType DetermineDataType(List<Ast> body)
        {
            // TODO
            return DetermineDataType(body[body.Count - 1]);
        }
    }
}
