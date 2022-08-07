using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Structure
{
    struct MDataType
    {
        // The name of this data type. This is the name that is used in-code to refer to this type
        public string Name;
        // True for primitive types (number, list, lambda, big_int, big_decimal, type)
        public bool IsPrimitive;
        // List of tuples containing the name of the key and the type of the value stored in that key
        public List<Tuple<string, MDataType>> keysAndValues;

        public MDataType(string name, bool isPrimitive, List<Tuple<string, MDataType>> keysAndValues)
        {
            Name = name;
            IsPrimitive = isPrimitive;
            this.keysAndValues = keysAndValues;
        }

        // Creates a primitive data type
        public MDataType(string name)
            : this(name, true, null)
        {}

        // Creates a composite data type
        public MDataType(string name, params Tuple<string, MDataType>[] keysAndValues)
            : this(name, false, keysAndValues.ToList())
        {}

        public static MDataType Empty = new MDataType();

        // Core data types
        public static MDataType Number = new MDataType("number");
        public static MDataType List = new MDataType("list");
        public static MDataType Lambda = new MDataType("lambda");
        public static MDataType Type = new MDataType("type");
        public static MDataType BigInt = new MDataType("big_int");
        public static MDataType BigDecimal = new MDataType("big_decimal");
        public static MDataType Error = new MDataType("error",
            Tuple.Create("code", Number),
            Tuple.Create("message", List),
            Tuple.Create("data", List));
    }
}
