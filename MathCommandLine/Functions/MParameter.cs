using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Functions
{
    public struct MParameter
    {
        public List<MDataType> DataTypes;
        public List<ValueRestriction> Restrictions;
        public string Name;

        public MParameter(MDataType dataType, string name)
        {
            DataTypes = new List<MDataType> { dataType };
            Name = name;
            Restrictions = new List<ValueRestriction>();
        }
        public MParameter(string name, params MDataType[] dataTypes)
        {
            DataTypes = new List<MDataType>(dataTypes);
            Name = name;
            Restrictions = new List<ValueRestriction>();
        }
        public MParameter(string name, List<MDataType> dataTypes, List<ValueRestriction> restrictions)
        {
            DataTypes = dataTypes;
            Name = name;
            Restrictions = restrictions;
        }

        public bool ContainsType(MDataType type)
        {
            return DataTypes.Contains(type) || DataTypes.Contains(MDataType.Any);
        }
        public bool PassesRestrictions(MValue value)
        {
            if (value.DataType == MDataType.Number)
            {
                foreach (ValueRestriction restriction in Restrictions)
                {
                    if (!restriction.SatisfiesNumRestriction(value.NumberValue))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public string DataTypeString()
        {
            return string.Join('|', DataTypes);
        }

        public static bool operator ==(MParameter p1, MParameter p2)
        {
            // Two parameters are equal as long as their data types are equal, their names don't matter
            if (p1.DataTypes.Count != p2.DataTypes.Count)
            {
                return false;
            }
            for (int i = 0; i < p1.DataTypes.Count; i++)
            {
                if (p1.DataTypes[i] != p2.DataTypes[i])
                {
                    return false;
                }
            }
            return true;
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
