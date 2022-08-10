using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure.FunctionTypes
{
    public struct MParameter
    {
        public MDataType DataType;
        public string Name;

        public MParameter(MDataType dataType, string name)
        {
            DataType = dataType;
            Name = name;
        }
    }
}
