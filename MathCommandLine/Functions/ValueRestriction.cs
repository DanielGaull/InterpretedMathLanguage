using MathCommandLine.CoreDataTypes;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Functions
{
    public struct ValueRestriction
    {
        public ValueRestrictionTypes Type;
        public double DoubleArg;
        public List<MDataType> DataTypesArg;

        public ValueRestriction(ValueRestrictionTypes type, double doubleArg, List<MDataType> dataTypesArg)
        {
            Type = type;
            DoubleArg = doubleArg;
            DataTypesArg = dataTypesArg;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ValueRestrictionTypes.Integer:
                    return "%";
                case ValueRestrictionTypes.LessThan:
                    return "<(" + DoubleArg + ")";
                case ValueRestrictionTypes.LessThanOrEqualTo:
                    return "<=(" + DoubleArg + ")";
                case ValueRestrictionTypes.GreaterThan:
                    return ">(" + DoubleArg + ")";
                case ValueRestrictionTypes.GreaterThanOrEqualTo:
                    return ">=(" + DoubleArg + ")";
                    // TODO: Others

            }
            return base.ToString();
        }

        public bool SatisfiesNumRestriction(double value)
        {
            return Type switch
            {
                ValueRestrictionTypes.Integer => Math.Abs(value % 1) <= (double.Epsilon * 100),
                ValueRestrictionTypes.LessThan => value < DoubleArg,
                ValueRestrictionTypes.GreaterThan => value > DoubleArg,
                ValueRestrictionTypes.LessThanOrEqualTo => value <= DoubleArg,
                ValueRestrictionTypes.GreaterThanOrEqualTo => value >= DoubleArg,
                _ => false,
            };
        }
        public bool SatisfiesListRestriction(MList value)
        {
            return Type switch
            {
                ValueRestrictionTypes.LengthEqual => MList.Length(value) == DoubleArg,
                ValueRestrictionTypes.LTypesAllowed => CheckTypeRequirement(value, DataTypesArg),
                _ => false
            };
        }
        private static bool CheckTypeRequirement(MList list, List<MDataType> typesAllowed)
        {
            List<MValue> entries = list.InternalList;
            for (int i = 0; i < entries.Count; i++)
            {
                if (!typesAllowed.Contains(entries[i].DataType))
                {
                    return false;
                }
            }
            return true;
        }

        public enum ValueRestrictionTypes
        {
            // Number restrictions
            Integer,
            LessThan,
            GreaterThan,
            LessThanOrEqualTo,
            GreaterThanOrEqualTo,
            // List restrictions
            LengthEqual,
            LTypesAllowed,
        }

        public static ValueRestriction Integer()
        {
            return new ValueRestriction(ValueRestrictionTypes.Integer, 0, new List<MDataType>());
        }
        public static ValueRestriction LessThan(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.LessThan, value, new List<MDataType>());
        }
        public static ValueRestriction GreaterThan(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.GreaterThan, value, new List<MDataType>());
        }
        public static ValueRestriction LessThanOrEqualTo(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.LessThanOrEqualTo, value, new List<MDataType>());
        }
        public static ValueRestriction GreaterThanOrEqualTo(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.GreaterThanOrEqualTo, value, new List<MDataType>());
        }
        public static ValueRestriction Positive()
        {
            return GreaterThan(0);
        }
        public static ValueRestriction Negative()
        {
            return LessThan(0);
        }

        public static ValueRestriction LengthEqual(int length)
        {
            return new ValueRestriction(ValueRestrictionTypes.LengthEqual, length, new List<MDataType>());
        }
        public static ValueRestriction TypesAllowed(params MDataType[] types)
        {
            return new ValueRestriction(ValueRestrictionTypes.LTypesAllowed, 0, new List<MDataType>(types));
        }

        public static bool operator ==(ValueRestriction p1, ValueRestriction p2)
        {
            return p1.Type == p2.Type && p1.DoubleArg == p2.DoubleArg;
        }
        public static bool operator !=(ValueRestriction p1, ValueRestriction p2)
        {
            return !(p1 == p2);
        }
        public override bool Equals(object obj)
        {
            if (obj is ValueRestriction)
            {
                ValueRestriction value = (ValueRestriction)obj;
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
