using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation.AST.ValueAsts
{
    public class VariableAst : Ast
    {
        public string Name { get; private set; }

        public VariableAst(string name)
            : base(AstTypes.Variable)
        {
            Name = name;
        }
    }
}
