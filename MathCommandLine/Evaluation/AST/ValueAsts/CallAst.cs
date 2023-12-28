using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation.AST.ValueAsts
{
    public class CallAst : Ast
    {
        public Ast CalledAst { get; private set; }
        public List<Ast> Arguments { get; private set; }

        public CallAst(Ast called, List<Ast> args)
            : base(AstTypes.Call)
        {
            CalledAst = called;
            Arguments = args;
        }
    }
}
