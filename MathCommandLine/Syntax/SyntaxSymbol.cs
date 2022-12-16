using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Syntax
{
    // Represents a syntax symbol in an input expression to match
    // A symbol can either be a syntax parameter or a literal string
    public class SyntaxSymbol
    {
        public SyntaxSymbolTypes Type { get; private set; }
        public string StringArg { get; private set; }
        public SyntaxParameter ParameterArg { get; private set; }

        public SyntaxSymbol(string literal)
        {
            Type = SyntaxSymbolTypes.LiteralString;
            StringArg = literal;
        }
        public SyntaxSymbol(SyntaxParameter param)
        {
            Type = SyntaxSymbolTypes.SyntaxParam;
            ParameterArg = param;
        }
    }

    public enum SyntaxSymbolTypes
    {
        LiteralString,
        SyntaxParam,
    }
}
