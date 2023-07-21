using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Exceptions
{
    public class BoxedValueException : Exception
    {
        public BoxedValueException(string message)
            : base(message)
        {

        }
    }
}
