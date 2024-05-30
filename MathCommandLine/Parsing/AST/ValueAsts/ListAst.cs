using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
{
    public class ListAst : Ast
    {
        public List<Ast> Elements { get; private set; }

        public ListAst(List<Ast> elements)
            : base(AstTypes.ListLiteral)
        {
            Elements = elements;
        }
    }
}
