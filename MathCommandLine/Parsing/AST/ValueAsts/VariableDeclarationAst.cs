using IML.Environments;
using IML.Parsing.AST;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
{
    public class VariableDeclarationAst : Ast
    {
        public string Name { get; private set; }
        public Ast Value { get; private set; }
        public VariableType VariableType { get; private set; }
        public AstType VariableValueType { get; private set; }

        public VariableDeclarationAst(string name, Ast value, VariableType varType, AstType variableValueType)
            : base(AstTypes.VariableDeclaration)
        {
            Name = name;
            Value = value;
            VariableType = varType;
            VariableValueType = variableValueType;
        }
    }
}
