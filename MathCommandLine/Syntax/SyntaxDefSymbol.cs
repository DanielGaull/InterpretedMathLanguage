using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Syntax
{
    // Represents a syntax symbol in an input expression to match
    // A symbol can either be a syntax parameter or a literal string
    public class SyntaxDefSymbol
    {
        public SyntaxDefSymbolTypes Type { get; private set; }
        public string StringArg { get; private set; }
        public SyntaxParameter ParameterArg { get; private set; }

        public SyntaxDefSymbol(string literal)
        {
            Type = SyntaxDefSymbolTypes.LiteralString;
            StringArg = literal;
        }
        public SyntaxDefSymbol(SyntaxParameter param)
        {
            Type = SyntaxDefSymbolTypes.SyntaxParam;
            ParameterArg = param;
        }
        public SyntaxDefSymbol()
        {
            Type = SyntaxDefSymbolTypes.Whitespace;
        }
    }

    public enum SyntaxDefSymbolTypes
    {
        LiteralString,
        SyntaxParam,
        Whitespace,
    }
}
