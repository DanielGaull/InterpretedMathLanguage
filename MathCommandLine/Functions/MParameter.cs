using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Functions
{
    public struct MParameter
    {
        public List<MTypeRestrictionsEntry> TypeEntries;
        public string Name;
        private bool isNotEmpty;

        public MParameter(MDataType dataType, string name)
        {
            TypeEntries = new List<MTypeRestrictionsEntry> { new MTypeRestrictionsEntry(dataType) };
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(string name, params MDataType[] dataTypes)
        {
            TypeEntries = new List<MTypeRestrictionsEntry>(dataTypes.Select((type) =>
            {
                return new MTypeRestrictionsEntry(type);
            }));
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(string name, List<MTypeRestrictionsEntry> entries)
        {
            TypeEntries = entries;
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(string name, params MTypeRestrictionsEntry[] entries)
        {
            TypeEntries = new List<MTypeRestrictionsEntry>(entries);
            Name = name;
            isNotEmpty = true;
        }

        public static MParameter Empty = new MParameter();

        public bool IsEmpty
        {
            get
            {
                return !isNotEmpty;
            }
        }

        public List<MDataType> GetDataTypes()
        {
            return TypeEntries.Select((entry) => entry.DataType).ToList();
        }

        public bool ContainsType(MDataType type)
        {
            List<MDataType> types = GetDataTypes();
            return types.Contains(type) || types.Contains(MDataType.Any);
        }
        public bool PassesRestrictions(MValue value)
        {
            if (value.DataType == MDataType.Number)
            {
                // Find the entry for the number type
                MTypeRestrictionsEntry entry = TypeEntries
                    .Where((entry) => entry.DataType == MDataType.Number)
                    .FirstOrDefault();
                if (entry.IsEmpty)
                {
                    // Technically passes all restrictions, since it's the wrong type (we don't allow for numbers here)
                    return true;
                }
                foreach (ValueRestriction restriction in entry.ValueRestrictions)
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
            return string.Join('|', GetDataTypes());
        }

        public static bool operator ==(MParameter p1, MParameter p2)
        {
            // Two parameters are equal as long as their data types and restrictions are equal, their names don't matter
            if (p1.TypeEntries.Count != p2.TypeEntries.Count)
            {
                return false;
            }
            for (int i = 0; i < p1.TypeEntries.Count; i++)
            {
                if (p1.TypeEntries[i] != p2.TypeEntries[i])
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

    public struct MTypeRestrictionsEntry
    {
        private bool isNotEmpty;
        public MDataType DataType { get; private set; }
        public ValueRestriction[] ValueRestrictions { get; private set; }

        public MTypeRestrictionsEntry(MDataType dataType, params ValueRestriction[] valueRestrictions)
        {
            DataType = dataType;
            ValueRestrictions = valueRestrictions;
            isNotEmpty = true;
        }

        public static MTypeRestrictionsEntry Empty = new MTypeRestrictionsEntry();

        public bool IsEmpty
        {
            get
            {
                return !isNotEmpty;
            }
        }

        public static bool operator ==(MTypeRestrictionsEntry p1, MTypeRestrictionsEntry p2)
        {
            if (p1.IsEmpty && p2.IsEmpty)
            {
                return true;
            }
            if (p1.DataType != p2.DataType)
            {
                return false;
            }
            if (p1.ValueRestrictions.Length != p2.ValueRestrictions.Length)
            {
                return false;
            }
            for (int i = 0; i < p1.ValueRestrictions.Length; i++)
            {
                if (p1.ValueRestrictions[i] != p2.ValueRestrictions[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(MTypeRestrictionsEntry p1, MTypeRestrictionsEntry p2)
        {
            return !(p1 == p2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MTypeRestrictionsEntry)
            {
                MTypeRestrictionsEntry value = (MTypeRestrictionsEntry)obj;
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
