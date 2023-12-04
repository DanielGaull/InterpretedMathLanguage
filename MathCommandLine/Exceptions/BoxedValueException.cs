using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Exceptions
{
    public class BoxedValueException : Exception
    {
        public BoxedValueException(string message)
            : base(message)
        {

        }
    }
}
