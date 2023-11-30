using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    // This represents a single Type Argument/Type Restriction
    // It has a definition where it is defined for a particular data type
    // It also stores the value of its arguments
    // Can be thought of as the Type Argument definition being the "class", with this being an "instance"
    public class MTypeRestriction
    {
        public MTypeRestrictionDefinition Definition { get; private set; }
        public List<Argument> ArgumentValues { get; private set; }

        public MTypeRestriction(MTypeRestrictionDefinition def)
            : this(def, new List<Argument>())
        {
        }
        public MTypeRestriction(MTypeRestrictionDefinition def, List<Argument> args)
        {
            Definition = def;
            ArgumentValues = args;
        }
        public MTypeRestriction(MTypeRestrictionDefinition def, params Argument[] args)
            : this(def, new List<Argument>(args))
        {
        }

        public class Argument
        {
            public RestrictionArgumentType ArgumentType { get; private set; }
            public double NumberValue { get; private set; }
            public string StringValue { get; private set; }
            public MType TypeValue { get; private set; }

            private Argument(RestrictionArgumentType argType, double num, string str, MType type)
            {
                ArgumentType = argType;
                NumberValue = num;
                StringValue = str;
                TypeValue = type;
            }
            public static Argument Number(double value)
            {
                return new Argument(RestrictionArgumentType.Number, value, null, null);
            }
            public static Argument String(string value)
            {
                return new Argument(RestrictionArgumentType.String, 0, value, null);
            }
            public static Argument Type(MType value)
            {
                return new Argument(RestrictionArgumentType.String, 0, null, value);
            }
        }
    }

    public enum RestrictionArgumentType
    {
        Number,
        String,
        Type,
    }
}
