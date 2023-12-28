using IML.Evaluation;
using IML.Evaluation.AST.ValueAsts;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    class TypeDeterminer
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
