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

        // NOTE: When a call happens, the generics are not always resolved
        // This just stores the provided generics if there are any
        public CallAst(Ast called, List<Ast> args, List<AstType> providedGenerics)
            : base(AstTypes.Call)
        {
            CalledAst = called;
            Arguments = args;
            ProvidedGenerics = providedGenerics;
        }
    }
}
