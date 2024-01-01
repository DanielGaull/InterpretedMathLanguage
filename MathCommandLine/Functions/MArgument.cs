using IML.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Functions
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

        public void SetValue(MValue newValue)
        {
            Value = newValue;
        }
        public void SetName(string newName)
        {
            Name = newName;
        }
    }
}
