using MathCommandLine.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure
{
    public struct MVariable
    {
        public MDataType VarType;
        public MValue Value;
        public string Name;

        private MVariable(MDataType type, MValue value, string name)
        {
            VarType = type;
            Value = value;
            Name = name;
        }

        public static MVariable Number(string name, double value)
        {
            return new MVariable(MDataType.Number, MValue.Number(value), name);
        }
        public static MVariable List(string name, MList value)
        {
            return new MVariable(MDataType.List, MValue.List(value), name);
        }
        public static MVariable Lambda(string name, MLambda value)
        {
            return new MVariable(MDataType.Lambda, MValue.Lambda(value), name);
        }
        public static MVariable Type(string name, MDataType value)
        {
            return new MVariable(MDataType.Type, MValue.Type(value), name);
        }

        public static MVariable CreateVariable(MDataType type, MValue value, string name)
        {
            return new MVariable(type, value, name);
        }
    }
}
