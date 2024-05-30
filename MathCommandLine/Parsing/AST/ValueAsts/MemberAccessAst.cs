using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
{
    public class MemberAccessAst : Ast
    {
        public Ast Parent { get; private set; }
        public string Name { get; private set; }

        public MemberAccessAst(Ast parent, string name)
            : base(AstTypes.MemberAccess)
        {
            Parent = parent;
            Name = name;
        }
    }
}
