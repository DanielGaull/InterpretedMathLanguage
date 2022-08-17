using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Functions
{
    public struct ParamRequirement
    {
        public ParamRequirementTypes Type;
        public double DoubleArg;

        public ParamRequirement(ParamRequirementTypes type, double doubleArg)
        {
            Type = type;
            DoubleArg = doubleArg;
        }

        public bool SatisfiesNumRequirement(double value)
        {
            return Type switch
            {
                ParamRequirementTypes.Integer => Math.Abs(value % 1) <= (double.Epsilon * 100),
                ParamRequirementTypes.LessThan => value < DoubleArg,
                ParamRequirementTypes.GreaterThan => value > DoubleArg,
                ParamRequirementTypes.LessThanOrEqualTo => value <= DoubleArg,
                ParamRequirementTypes.GreaterThanOrEqualTo => value >= DoubleArg,
                _ => false,
            };
        }

        public enum ParamRequirementTypes
        {
            Integer,
            LessThan,
            GreaterThan,
            LessThanOrEqualTo,
            GreaterThanOrEqualTo,
        }

        public static ParamRequirement Integer()
        {
            return new ParamRequirement(ParamRequirementTypes.Integer, 0);
        }
        public static ParamRequirement LessThan(double value)
        {
            return new ParamRequirement(ParamRequirementTypes.LessThan, value);
        }
        public static ParamRequirement GreaterThan(double value)
        {
            return new ParamRequirement(ParamRequirementTypes.GreaterThan, value);
        }
        public static ParamRequirement LessThanOrEqualTo(double value)
        {
            return new ParamRequirement(ParamRequirementTypes.LessThanOrEqualTo, value);
        }
        public static ParamRequirement GreaterThanOrEqualTo(double value)
        {
            return new ParamRequirement(ParamRequirementTypes.GreaterThanOrEqualTo, value);
        }
        public static ParamRequirement Positive()
        {
            return GreaterThan(0);
        }
        public static ParamRequirement Negative()
        {
            return LessThan(0);
        }
    }
}
