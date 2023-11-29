using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    // This represents a parameter or variable type, where we define what the allowed values are
    // This includes unions and restrictions
    public class MType
    {
        private List<MDataTypeRestrictionEntry> entries;

        public List<MDataTypeRestrictionEntry> Entries
        {
            get
            {
                return new List<MDataTypeRestrictionEntry>(entries);
            }
        }

        public MType(List<MDataTypeRestrictionEntry> entries)
        {
            this.entries = new List<MDataTypeRestrictionEntry>(entries);
        }
        public MType(params MDataTypeRestrictionEntry[] entries)
        {
            this.entries = new List<MDataTypeRestrictionEntry>(entries);
        }
        public MType(MDataTypeRestrictionEntry entry)
        {
            entries = new List<MDataTypeRestrictionEntry>();
            entries.Add(entry);
        }
    }
}
