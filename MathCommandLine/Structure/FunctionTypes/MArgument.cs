using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure.FunctionTypes
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
    }
}
