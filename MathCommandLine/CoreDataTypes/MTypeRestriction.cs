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

        public static bool operator ==(MTypeRestriction d1, MTypeRestriction d2)
        {
            // Just compare on pure equality of the definitions of the type restrictions (& args)
            if (d1.Definition != d2.Definition)
            {
                return false;
            }
            if (d1.ArgumentValues.Count != d2.ArgumentValues.Count)
            {
                return false;
            }
            for (int i = 0; i < d1.ArgumentValues.Count; i++)
            {
                if (d1.ArgumentValues[i] != d2.ArgumentValues[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(MTypeRestriction d1, MTypeRestriction d2)
        {
            return !(d1 == d2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MTypeRestriction other)
            {
                return this == other;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
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

            public static bool operator ==(Argument a1, Argument a2)
            {
                if (a1.ArgumentType != a2.ArgumentType)
                {
                    return false;
                }
                switch (a1.ArgumentType)
                {
                    case RestrictionArgumentType.Number:
                        return a1.NumberValue == a2.NumberValue;
                    case RestrictionArgumentType.Type:
                        return a1.TypeValue == a2.TypeValue;
                    case RestrictionArgumentType.String:
                        return a1.StringValue == a2.StringValue;
                }
                return false;
            }
            public static bool operator !=(Argument a1, Argument a2)
            {
                return !(a1 == a2);
            }

            public override bool Equals(object obj)
            {
                if (obj is Argument arg)
                {
                    return this == arg;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
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
