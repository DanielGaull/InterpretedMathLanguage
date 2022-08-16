using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Functions
{
    public struct MArgument
    {
        public string Name;
        public MValue Value;

        public MArgument(string name, MValue value)
        {
            Name = name;
            Value = value;
        }
        public MArgument(MValue value)
            : this("", value)
        {}
    }
}
