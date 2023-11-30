using MathCommandLine.Environments;
using MathCommandLine.Evaluation;
using MathCommandLine.Structure;
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

        public bool ValueMatches(MValue value, IInterpreter interpreter, MEnvironment env)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].TypeDefinition.MatchesType(value.DataType))
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
    }
}
