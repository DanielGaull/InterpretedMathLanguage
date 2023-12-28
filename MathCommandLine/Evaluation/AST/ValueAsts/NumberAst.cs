using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation.AST.ValueAsts
{
    public class NumberAst : Ast
    {
        public double Value { get; private set; }

        public NumberAst(double number)
            : base(AstTypes.NumberLiteral)
        {
            Value = number;
        }
    }
}
