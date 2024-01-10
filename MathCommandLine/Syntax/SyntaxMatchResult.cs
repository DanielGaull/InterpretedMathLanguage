using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Syntax
{
    public class SyntaxMatchResult
    {
        public bool IsMatch { get; private set; }
        // Simply maps each parameter name to its string representation in the code
        Dictionary<string, SyntaxArgument> args;

        public SyntaxMatchResult(bool isMatch)
            : this(isMatch, null)
        {}
        public SyntaxMatchResult(bool isMatch, Dictionary<string, SyntaxArgument> args)
        {
            this.args = args;
            this.IsMatch = isMatch;
        }

        public SyntaxArgument GetValue(string name)
        {
            if (args == null)
            {
                return null;
            }
            return args[name];
        }
    }
}
