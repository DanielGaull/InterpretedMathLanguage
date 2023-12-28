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
        public MType DetermineDataType(Ast expression)
        {
            switch (expression.Type)
            {
                case AstTypes.NumberLiteral:
                    return MType.Number;
                case AstTypes.StringLiteral:
                    return MType.String;
                case AstTypes.ReferenceLiteral:
                    return MType.Reference(MType.Any);

            }
            return null;
        }
    }
}
