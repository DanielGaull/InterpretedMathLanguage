using MathCommandLine.CoreDataTypes;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathCommandLine.Functions
{
    public struct MParameter
    {
        public List<MDataTypeRestrictionEntry> TypeEntries;
        public string Name;
        private bool isNotEmpty;

        public MParameter(MDataTypeRestrictionEntry dataType, string name)
        {
            TypeEntries = new List<MDataTypeRestrictionEntry> { dataType };
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(string name, params MDataTypeRestrictionEntry[] dataTypes)
        {
            TypeEntries = new List<MDataTypeRestrictionEntry>(dataTypes);
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(string name, List<MDataTypeRestrictionEntry> entries)
        {
            TypeEntries = entries;
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

        public override string ToString()
        {
            return Name + ":" + string.Join('|', TypeEntries);
        }

        public List<MDataTypeRestrictionEntry> GetDataTypes()
        {
            return TypeEntries;
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
}
