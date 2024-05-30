using System;
using System.Collections.Generic;
using System.Text;
using IML.Parsing.AST;

namespace IML.Parsing.AST.ValueAsts
{
    public class CallAst : Ast
    {
        public Ast CalledAst { get; private set; }
        public List<Ast> Arguments { get; private set; }
        public List<AstType> ProvidedGenerics { get; private set; }

        public CallAst(Ast called, List<Ast> args, List<AstType> providedGenerics)
            : base(AstTypes.Call)
        {
            CalledAst = called;
            Arguments = args;
            ProvidedGenerics = providedGenerics;
        }
    }
}
