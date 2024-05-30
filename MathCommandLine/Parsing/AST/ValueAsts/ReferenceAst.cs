using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
{
    public class ReferenceAst : Ast
    {
        public string RefName { get; private set; }

        public ReferenceAst(string refName)
            : base(AstTypes.ReferenceLiteral)
        {
            RefName = refName;
        }
    }
}
