﻿using IML.Environments;
using IML.Evaluation;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.CoreDataTypes
{
    // This represents a parameter or variable type, where we define what the allowed values are
    // This includes unions and restrictions
    public class MType
    {
        private List<MDataTypeEntry> entries;

        // UNION_BASE is a special MType that is not actually valid. It is used when iteratively unioning types.
        // The UNION_BASE type will assign itself to the type to union to. This is to simplify code so you don't have to
        // assign a variable to null and check if its null at first; you just perform unions with the UNION_BASE
        // UNION_BASE SHOULD NEVER BE USED AS AN ACTUAL TYPE, ONLY PRECEEDING UNION OPERATIONS
        private bool isUnionBase = false;
        public static readonly MType UNION_BASE = new MType(true);

        public bool IsUnionBase
        {
            get
            {
                return isUnionBase;
            }
        }

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
        private MType(bool isUnionBase)
        {
            entries = new List<MDataTypeEntry>();
            this.isUnionBase = isUnionBase;
        }

        public bool ValueMatches(MValue value)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] == value.DataType || entries[i] == MDataTypeEntry.Any)
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
        // Ex. ()=>string UNION ()=>number => ()=>string|number
        // Ex. (number)=>string UNION ()=>boolean => (number)=>string|()=>boolean
        // Notable properties:
        // (Order doesn't matter for the type '|' operator)
        // A UNION B => A|B
        // A|B UNION A => A|B
        // A UNION any => any
        // If there is a single generic and both base types are the same, unions the generics
        // If there are multiple generics, then unions the types separately
        // ex A[T] UNION A[R] = A[T|R], but B[T,R] UNION B[C,D] = B[T,R]|B[C,D]
        // This is because A[T|R] = A[T]|A[R]
        // If there is a function and the parameters are the same, then union the return types
        // (A...)=>T|(A...)=>R = (A...)=>T|R
        public MType Union(MType other)
        {
            // If either are the union base, return the other
            if (isUnionBase) return other;
            if (other.isUnionBase) return this;

            // First check if either has the "any" type
            // If so, then just return "any"
            if (IsAnyType() || other.IsAnyType())
            {
                return Any();
            }

            // Now create a SET of the types that appear in each subtype
            // Compare each to see if they're equal
            List<MDataTypeEntry> rawCombinedEntries = Entries.Concat(other.Entries).ToList();
            Set<MDataTypeEntry> entries = new Set<MDataTypeEntry>();
            for (int i = 0; i < rawCombinedEntries.Count; i++)
            {
                if (rawCombinedEntries[i] is MFunctionDataTypeEntry ft)
                {
                    bool unioningFunctionTypes = false;
                    MFunctionDataTypeEntry entryToRemove = null;
                    MFunctionDataTypeEntry entryToAdd = null;
                    foreach (var existingEntry in entries)
                    {
                        if (existingEntry is MFunctionDataTypeEntry existingFt)
                        {
                            // Check if equal (ignoring return type
                            if (existingFt.Equals(ft, true))
                            {
                                MType unionedReturnType = existingFt.ReturnType.Union(ft.ReturnType);
                                entryToAdd = new MFunctionDataTypeEntry(
                                    unionedReturnType, ft.ParameterTypes, existingFt.GenericNames,
                                    ft.IsPure, existingFt.EnvironmentType, ft.IsLastVarArgs);
                                entryToRemove = existingFt;
                                unioningFunctionTypes = true;
                                break;
                            }
                        }
                    }
                    if (unioningFunctionTypes)
                    {
                        entries.Remove(entryToRemove);
                        entries.Add(entryToAdd);
                    }
                    else
                    {
                        entries.Add(ft);
                    }
                }
                else if (rawCombinedEntries[i] is MConcreteDataTypeEntry ct)
                {
                    bool unioningGenerics = false;
                    MConcreteDataTypeEntry entryToRemove = null;
                    MConcreteDataTypeEntry entryToAdd = null;
                    foreach (MDataTypeEntry e in entries)
                    {
                        if (e is MConcreteDataTypeEntry existingCt)
                        {
                            if (ct.DataType == existingCt.DataType &&
                                ct.DataType.NumberOfGenerics == 1)
                            {
                                MType unionedGeneric = existingCt.Generics[0].Union(ct.Generics[0]);
                                entryToAdd = new MConcreteDataTypeEntry(ct.DataType, unionedGeneric);
                                entryToRemove = existingCt;
                                unioningGenerics = true;
                            }
                        }
                    }
                    if (unioningGenerics)
                    {
                        entries.Remove(entryToRemove);
                        entries.Add(entryToAdd);
                    }
                    else
                    {
                        entries.Add(ct);
                    }
                }
                else
                {
                    entries.Add(rawCombinedEntries[i]);
                }
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

        public static MType Any() => new MType(MDataTypeEntry.Any);
        public static MType Number() => new MType(MDataTypeEntry.Number);
        public static MType Boolean() => new MType(MDataTypeEntry.Boolean);
        public static MType Type() => new MType(MDataTypeEntry.Type);
        public static MType String() => new MType(MDataTypeEntry.String);
        public static MType Error() => new MType(MDataTypeEntry.Error);
        public static MType Null() => new MType(MDataTypeEntry.Null);
        public static MType Void() => new MType(MDataTypeEntry.Void);
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
                LambdaEnvironmentType.ForceEnvironment));
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
                LambdaEnvironmentType.ForceEnvironment));
        }
        public static MType Generic(string name)
        {
            return new MType(new MGenericDataTypeEntry(name));
        }

        // Strict equals, have to completely match
        public static bool operator ==(MType t1, MType t2)
        {
            if (t1.entries.Count != t2.entries.Count)
            {
                return false;
            }
            // Order of entries does not matter
            // Just put everything into a list, and remove as needed
            List<MDataTypeEntry> entriesToCompare = new List<MDataTypeEntry>(t1.Entries);
            for (int i = 0; i < t2.Entries.Count; i++)
            {
                if (entriesToCompare.Contains(t2.Entries[i]))
                {
                    entriesToCompare.Remove(t2.Entries[i]);
                }
                else
                {
                    // Found something in t2 that isn't in t1
                    return false;
                }
            }
            if (entriesToCompare.Count > 0)
            {
                // Found something(s) in t1 that aren't in t2
                return false;
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

        public override string ToString()
        {
            List<string> typeStrings = Entries.Select(x => x.ToString()).ToList();
            return string.Join("|", typeStrings);
        }
    }
}
