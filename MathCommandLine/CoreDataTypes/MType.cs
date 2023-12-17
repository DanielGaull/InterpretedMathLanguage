using IML.Environments;
using IML.Evaluation;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    // This represents a parameter or variable type, where we define what the allowed values are
    // This includes unions and restrictions
    public class MType
    {
        private List<MDataTypeEntry> entries;

        public List<MDataTypeEntry> Entries
        {
            get
            {
                return new List<MDataTypeEntry>(entries);
            }
        }

        public MType(List<MDataTypeEntry> entries)
        {
            this.entries = new List<MDataTypeEntry>(entries);
        }
        public MType(params MDataTypeEntry[] entries)
        {
            this.entries = new List<MDataTypeEntry>(entries);
        }
        public MType(MDataTypeEntry entry)
        {
            entries = new List<MDataTypeEntry>();
            entries.Add(entry);
        }

        public bool ValueMatches(MValue value, IInterpreter interpreter, MEnvironment env)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].DataType.MatchesType(value.DataType))
                {
                    bool failsRestrictions = false;
                    foreach (MTypeRestriction rest in entries[i].TypeRestrictions)
                    {
                        if (!rest.Definition.IsValid(value, rest, interpreter, env))
                        {
                            failsRestrictions = true;
                            break;
                        }
                    }
                    if (!failsRestrictions)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool operator ==(MType t1, MType t2)
        {
            if (t1.entries.Count != t2.entries.Count)
            {
                return false;
            }
            for (int i = 0; i < t1.entries.Count; i++)
            {
                if (t1.entries[i] != t2.entries[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(MType t1, MType t2)
        {
            return !(t1 == t2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MType other)
            {
                return this == other;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
