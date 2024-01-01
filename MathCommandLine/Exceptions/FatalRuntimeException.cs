using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Exceptions
{
    public class FatalRuntimeException : Exception
    {
        public FatalRuntimeException(string message)
            : base(message)
        {

        }
    }
}
