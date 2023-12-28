using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation.AST.ValueAsts
{
    public class StringAst : Ast
    {
        public string Value { get; private set; }

        public StringAst(string value)
            : base(AstTypes.StringLiteral)
        {
            Value = value;
        }
    }
}
