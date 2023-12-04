using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Exceptions
{
    public class InvalidParseException : Exception
    {
        public InvalidParseException(string expression)
            : base("Invalid Syntax: \"" + expression + "\".")
        {

        }
        public InvalidParseException(string message, string expression)
            : base("Invalid Syntax: \"" + expression + "\" (" + message + ").")
        {

        }
    }
}
