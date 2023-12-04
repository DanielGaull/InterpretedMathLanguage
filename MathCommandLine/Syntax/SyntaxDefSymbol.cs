using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Syntax
{
    // Represents a syntax symbol in an input expression to match
    // A symbol can either be a syntax parameter or a literal string
    public class SyntaxDefSymbol
    {
        public SyntaxDefSymbolTypes Type { get; private set; }
        public string StringArg { get; private set; }
        public SyntaxParameter ParameterArg { get; private set; }

        public SyntaxDefSymbol(SyntaxDefSymbolTypes type, string literal, SyntaxParameter param)
        {
            Type = type;
            StringArg = literal;
            ParameterArg = param;
        }

        public static SyntaxDefSymbol Literal(string literal)
        {
            return new SyntaxDefSymbol(SyntaxDefSymbolTypes.LiteralString, literal, null);
        }
        public static SyntaxDefSymbol Parameter(SyntaxParameter param)
        {
            return new SyntaxDefSymbol(SyntaxDefSymbolTypes.SyntaxParam, null, param);
        }
        public static SyntaxDefSymbol Whitespace()
        {
            return new SyntaxDefSymbol(SyntaxDefSymbolTypes.Whitespace, null, null);
        }
        public static SyntaxDefSymbol OptionalWhitespace()
        {
            return new SyntaxDefSymbol(SyntaxDefSymbolTypes.OptionalWhitespace, null, null);
        }

    }

    public enum SyntaxDefSymbolTypes
    {
        LiteralString,
        SyntaxParam,
        Whitespace,
        OptionalWhitespace,
    }
}
