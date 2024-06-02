using IML.Parsing.AST.ValueAsts;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Exceptions
{
    public class TypeDeterminationException : Exception
    {
        public Ast Ast { get; private set; }

        public TypeDeterminationException(string message, Ast ast)
            : base("Type verification error: \"" + message + "\".")
        {
            Ast = ast;
        }

        public TypeDeterminationException(string message)
            : base("Type verification error: \"" + message + "\".")
        {
        }
    }
}
