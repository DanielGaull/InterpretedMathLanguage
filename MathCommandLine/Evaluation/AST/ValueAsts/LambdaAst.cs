using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation.AST.ValueAsts
{
    public class LambdaAst : Ast
    {

        public LambdaAst()
            : base(AstTypes.LambdaLiteral)
        {

        }
    }
}
