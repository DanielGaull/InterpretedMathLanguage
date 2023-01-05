using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Syntax
{
    public class SyntaxArgument
    {
        public SyntaxParameter DefiningParameter { get; private set; }
        public string LiteralValue { get; private set; }

        public SyntaxArgument(SyntaxParameter param, string value)
        {
            DefiningParameter = param;
            LiteralValue = value;
        }
    }
}
