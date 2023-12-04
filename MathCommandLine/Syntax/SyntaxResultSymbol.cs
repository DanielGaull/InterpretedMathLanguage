using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Syntax
{
    public class SyntaxResultSymbol
    {
        public SyntaxResultSymbolTypes Type { get; private set; }
        public string ExpressionArg { get; private set; }
        public string ArgName { get; private set; }

        public SyntaxResultSymbol(SyntaxResultSymbolTypes type, string arg)
        {
            this.Type = type;
            switch (type)
            {
                case SyntaxResultSymbolTypes.Argument:
                    ArgName = arg;
                    break;
                case SyntaxResultSymbolTypes.ExpressionPiece:
                    ExpressionArg = arg;
                    break;
            }
        }
    }

    public enum SyntaxResultSymbolTypes
    {
        ExpressionPiece,
        Argument
    }
}
