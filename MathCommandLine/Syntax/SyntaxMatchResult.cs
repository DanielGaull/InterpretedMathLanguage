using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Syntax
{
    public class SyntaxMatchResult
    {
        public bool IsMatch { get; private set; }
        Dictionary<string, MValue> vars;

        public SyntaxMatchResult(bool isMatch)
            : this(isMatch, null)
        {}
        public SyntaxMatchResult(bool isMatch, Dictionary<string, MValue> vars)
        {
            this.vars = vars;
            this.IsMatch = isMatch;
        }

        public MValue GetValue(string name)
        {
            if (vars == null)
            {
                return MValue.Empty;
            }
            return vars[name];
        }
    }
}
