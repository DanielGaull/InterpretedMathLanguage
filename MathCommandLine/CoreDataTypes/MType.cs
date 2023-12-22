using IML.Environments;
using IML.Evaluation;
using IML.Structure;
using IML.Util;
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
                if (entries[i] == value.DataType)
                {
                    return true;
                }
            }
            return false;
        }

        public static MType Union(MType t1, MType t2)
        {
            return t1.Union(t2);
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
            // Now create a SET of the types that appear in each subtype
            // Compare each to see if they're equal
            Set<MDataTypeEntry> entries = new Set<MDataTypeEntry>();
            for (int i = 0; i < Entries.Count; i++)
            {
                entries.Add(Entries[i]);
            }
            for (int i = 0; i < other.Entries.Count; i++)
            {
                entries.Add(other.Entries[i]);
            }
            // Now we have a set of entries
            return new MType(entries.ToList());
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

        public MType DeepClone()
        {
            List<MDataTypeEntry> entries = new List<MDataTypeEntry>();
            foreach (MDataTypeEntry e in Entries)
            {
                entries.Add(e.DeepClone());
            }
            return new MType(entries);
        }

        public static readonly MType Any = new MType(MDataTypeEntry.Any);
        public static readonly MType Number = new MType(MDataTypeEntry.Number);
        public static readonly MType Boolean = new MType(MDataTypeEntry.Boolean);
        public static readonly MType Type = new MType(MDataTypeEntry.Type);
        public static readonly MType String = new MType(MDataTypeEntry.String);
        public static readonly MType Error = new MType(MDataTypeEntry.Error);
        public static readonly MType Null = new MType(MDataTypeEntry.Null);
        public static readonly MType Void = new MType(MDataTypeEntry.Void);
        public static MType List(MType of)
        {
            return new MType(MDataTypeEntry.List(of));
        }
        public static MType Reference(MType of)
        {
            return new MType(MDataTypeEntry.Reference(of));
        }
        public static MType Function(MType returnType, List<MType> paramTypes, List<string> genericNames,
            bool isPure, bool isLastVarArgs, LambdaEnvironmentType envType)
        {
            return new MType(MDataTypeEntry.Function(returnType, paramTypes, genericNames, isPure, isLastVarArgs, envType));
        }
        public static MType Function(MType returnType, List<MType> paramTypes, List<string> genericNames)
        {
            return new MType(MDataTypeEntry.Function(returnType, paramTypes, genericNames, false, false, 
                LambdaEnvironmentType.AllowAny));
        }
        public static MType Function(MType returnType, params MType[] paramTypes)
        {
            return new MType(MDataTypeEntry.Function(returnType, new List<MType>(paramTypes)));
        }
        public static MType Function(MType returnType, List<MType> paramTypes)
        {
            return new MType(MDataTypeEntry.Function(returnType, paramTypes));
        }
        public static MType Function(MType returnType, List<MType> paramTypes, bool isPure, bool isLastVarArgs)
        {
            return new MType(MDataTypeEntry.Function(returnType, paramTypes, isPure, isLastVarArgs, 
                LambdaEnvironmentType.AllowAny));
        }

        // Strict equals, have to completely match
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
