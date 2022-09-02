﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Functions
{
    public struct ValueRestriction
    {
        public ValueRestrictionTypes Type;
        public double DoubleArg;

        public ValueRestriction(ValueRestrictionTypes type, double doubleArg)
        {
            Type = type;
            DoubleArg = doubleArg;
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

        public enum ValueRestrictionTypes
        {
            Integer,
            LessThan,
            GreaterThan,
            LessThanOrEqualTo,
            GreaterThanOrEqualTo,
        }

        public static ValueRestriction Integer()
        {
            return new ValueRestriction(ValueRestrictionTypes.Integer, 0);
        }
        public static ValueRestriction LessThan(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.LessThan, value);
        }
        public static ValueRestriction GreaterThan(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.GreaterThan, value);
        }
        public static ValueRestriction LessThanOrEqualTo(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.LessThanOrEqualTo, value);
        }
        public static ValueRestriction GreaterThanOrEqualTo(double value)
        {
            return new ValueRestriction(ValueRestrictionTypes.GreaterThanOrEqualTo, value);
        }
        public static ValueRestriction Positive()
        {
            return GreaterThan(0);
        }
        public static ValueRestriction Negative()
        {
            return LessThan(0);
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