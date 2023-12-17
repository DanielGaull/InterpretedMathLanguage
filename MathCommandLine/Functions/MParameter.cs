using IML.CoreDataTypes;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IML.Functions
{
    public class MParameter
    {
        public readonly MType Type;
        public readonly string Name;
        private bool isNotEmpty;

        public MParameter(MType type, string name)
        {
            Type = type;
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(MDataTypeEntry singleEntry, string name)
        {
            Type = new MType(singleEntry);
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(string name, params MDataTypeEntry[] entries)
        {
            Type = new MType(entries);
            Name = name;
            isNotEmpty = true;
        }
        public MParameter(string name, List<MDataTypeEntry> entries)
        {
            Type = new MType(entries);
            Name = name;
            isNotEmpty = true;
        }

        public bool IsEmpty
        {
            get
            {
                return !isNotEmpty;
            }
        }

        // TODO: To string methods
        public override string ToString()
        {
            return Name;// + ":" + string.Join('|', TypeEntries);
        }

        public string DataTypeString()
        {
            return "";// string.Join('|', GetDataTypes());
        }

        //public static bool operator ==(MParameter p1, MParameter p2)
        //{
        //    // TODO: Define MParameter Equals

        //    return false;
        //}
        //public static bool operator !=(MParameter p1, MParameter p2)
        //{
        //    return !(p1 == p2);
        //}
        //public override bool Equals(object obj)
        //{
        //    if (obj is MParameter)
        //    {
        //        MParameter value = (MParameter)obj;
        //        return value == this;
        //    }
        //    return false;
        //}
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
