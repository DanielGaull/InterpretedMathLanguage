using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation.AST.ValueAsts
{
    public class ReturnAst : Ast
    {
        public Ast Body { get; private set; }

        public ReturnAst(Ast body)
            : base(AstTypes.Return)
        {
            Body = body;
        }
    }
}
