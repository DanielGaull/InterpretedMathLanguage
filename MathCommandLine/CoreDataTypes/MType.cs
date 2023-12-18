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

        // Performs a union of the two types and returns the simplest-form reduced type
        // Ex. any UNION number => any
        // Ex. string UNION number => string|number
        // Ex. list[string] UNION list[number] => list[string|number]
        // Ex. tuple[number,number] UNION tuple[string,string] => tuple[number,number]|tuple[string,string]
        // Ex. string|number UNION number => string|number
        // Notable properties:
        // (Order doesn't matter for the type '|' operator)
        // A UNION B => A|B
        // A|B UNION A => A|B
        // A UNION any => any
        // If there is a single generic and both base types are the same, unions the generics
        // If there are multiple generics, then unions the types separately
        // ex A[T] UNION A[R] = A[T|R], but B[T,R] UNION B[C,D] = B[T,R]|B[C,D]
        // This is because A[T|R] = A[T]|A[R]
        public MType Union(MType other)
        {
            // First check if either has the "any" type
            // If so, then just return "any"
            if (IsAnyType() || other.IsAnyType())
            {
                return Any;
            }
            // Now create a SET of the types that appear in each
        }
        public bool IsAnyType()
        {
            foreach (MDataTypeEntry entry in entries)
            {
                if (entry is MConcreteDataTypeEntry)
                {
                    MConcreteDataTypeEntry e = (MConcreteDataTypeEntry)entry;
                    if (e.DataType.MatchesTypeExactly(MDataType.Any))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static readonly MType Any = new MType(MDataTypeEntry.Any);

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
