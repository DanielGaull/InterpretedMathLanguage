using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Exceptions
{
    public class MException : Exception
    {
        public MException(string message)
            : base(message)
        {
        }
    }
}
