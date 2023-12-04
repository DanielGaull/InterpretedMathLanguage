using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Exceptions
{
    class OperatorException : Exception
    {
        public OperatorException(string message)
            : base(message)
        {

        }
    }
}
