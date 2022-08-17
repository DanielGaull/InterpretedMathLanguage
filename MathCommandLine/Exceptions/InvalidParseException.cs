using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Exceptions
{
    public class InvalidParseException : Exception
    {
        public InvalidParseException(string expression)
            : base("Cannot parse expression: \"" + expression + "\".")
        {

        }
    }
}
