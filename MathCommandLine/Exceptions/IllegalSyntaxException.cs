using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Exceptions
{
    public class IllegalSyntaxException : Exception
    {
        public IllegalSyntaxException(string line)
            : base("Invalid Syntax Definition: \"" + line + "\".")
        {

        }
        public IllegalSyntaxException(string message, string line)
            : base("Invalid Syntax Definition: \"" + line + "\" (" + message + ").")
        {

        }
    }
}
