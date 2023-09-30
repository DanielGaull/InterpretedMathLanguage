using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Exceptions
{
    class OperatorException : Exception
    {
        public OperatorException(string message)
            : base(message)
        {

        }
    }
}
