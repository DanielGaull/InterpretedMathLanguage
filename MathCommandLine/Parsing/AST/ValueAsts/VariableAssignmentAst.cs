using System;
using System.Collections.Generic;
using System.Text;
using IML.Parsing.AST;

namespace IML.Parsing.AST.ValueAsts
{
    public class VariableAssignmentAst : Ast
    {
        public IdentifierAst Identifier { get; private set; }
        public Ast Value { get; private set; }

        public VariableAssignmentAst(IdentifierAst identifier, Ast value)
            : base(AstTypes.VariableAssignment)
        {
            Identifier = identifier;
            Value = value;
        }
    }
}
