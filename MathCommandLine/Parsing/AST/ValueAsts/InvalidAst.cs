using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
{
    public class InvalidAst : Ast
    {
        public string Expression { get; private set; }

        public InvalidAst(string expr)
            : base(AstTypes.Invalid)
        {
            Expression = expr;
        }
    }
}
