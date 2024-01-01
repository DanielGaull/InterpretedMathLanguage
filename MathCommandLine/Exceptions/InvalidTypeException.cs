using IML.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Exceptions
{
    public class InvalidTypeException : Exception
    {
        AstType type;
        AstTypeEntry entry;

        public InvalidTypeException(string message, AstType type)
            : base(message)
        {
            this.type = type;
        }
        public InvalidTypeException(string message, AstTypeEntry entry)
            : base(message)
        {
            this.entry = entry;
        }
    }
}
