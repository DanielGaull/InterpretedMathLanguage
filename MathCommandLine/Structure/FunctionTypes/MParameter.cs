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

        public static bool operator ==(MParameter p1, MParameter p2)
        {
            // Two parameters are equal as long as their data types are equal, their names don't matter
            return (p1.DataType == p2.DataType);
        }
        public static bool operator !=(MParameter p1, MParameter p2)
        {
            return !(p1 == p2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MParameter)
            {
                MParameter value = (MParameter)obj;
                return value == this;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
