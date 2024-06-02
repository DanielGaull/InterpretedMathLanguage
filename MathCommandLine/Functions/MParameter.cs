using IML.CoreDataTypes;
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
            return Type.ToString();// string.Join('|', GetDataTypes());
        }
    }
}
