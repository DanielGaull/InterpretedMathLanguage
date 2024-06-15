using IML.CoreDataTypes;
using IML.Exceptions;
using IML.Parsing.AST;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Parsing
{
    // This class is in charge of assigning generics when a function is called
    public class GenericAssigner
    {
        public GenericAssigner() 
        {
        }

        // Will assign generics in order, returning a list in the order desired
        public List<MType> AssignGenerics(MFunctionDataTypeEntry callee, List<MType> parameters)
        {
            if (callee.GenericNames.Count == 0)
            {
                return new List<MType>();
            }

            // If either the return type or parameters have generics with multiple entries,
            // then we simply cannot determine the type
            // This is true only if a generic exclusively appears in unions
            // If it appears alone at any point, then we can assign it
            // So, need to check each generic to see if it is non-unioned anywhere
            Set<string> isolatedGenerics = new Set<string>();
            isolatedGenerics.AddRange(GetIsolatedGenerics(callee.ReturnType, callee.GenericNames));
            for (int i = 0; i < callee.ParameterTypes.Count; i++)
            {
                isolatedGenerics.AddRange(GetIsolatedGenerics(callee.ParameterTypes[i], callee.GenericNames));
            }
            if (isolatedGenerics.Count != callee.GenericNames.Count)
            {
                throw new TypeDeterminationException("Cannot infer generics when they exclusively appear in " +
                    "type unions. Please manually define generics.");
            }

            // Now, for each generic name, find the associated parameter
            // If there are multiple, we just union them together
            List<MType> result = new List<MType>();
            for (int i = 0; i < callee.GenericNames.Count; i++)
            {
                MType type = MType.UNION_BASE;
                string name = callee.GenericNames[i];
                for (int j = 0; j < parameters.Count; j++)
                {
                    type = type.Union(MatchGenerics(callee.ParameterTypes[j], parameters[j], name));
                }

                // Generic is not assigned; we throw an error
                // For example (on why this is a problem), a user could declare a variable of this type,
                // so we need an actual value for it
                if (type.IsUnionBase)
                {
                    throw new TypeDeterminationException("Could infer generic for \"" + name + "\". " + 
                        "The generic does not appear in the parameters or return type. Please " + 
                        "manually define generics.");
                }

                result.Add(type);
            }

            return result;
        }

        private List<string> GetIsolatedGenerics(MType type, List<string> genericNames)
        {
            List<string> result = new List<string>();
            // ONLY consider single-entry case
            // For example, we could have list<T>|list<R>
            // We don't want to consider T and R to be isolated; only if
            // there is no type-union at all
            if (type.Entries.Count == 1)
            {
                // Check if this entry is one of the generics
                if (type.Entries[0] is MGenericDataTypeEntry gt)
                {
                    if (genericNames.Contains(gt.Name))
                    {
                        result.Add(gt.Name);
                    }
                }
                else if (type.Entries[0] is MFunctionDataTypeEntry ft)
                {
                    result.AddRange(GetIsolatedGenerics(ft.ReturnType, genericNames));
                    for (int j = 0; j < ft.ParameterTypes.Count; j++)
                    {
                        result.AddRange(GetIsolatedGenerics(ft.ParameterTypes[j], genericNames));
                    }
                }
                else if (type.Entries[0] is MConcreteDataTypeEntry ct)
                {
                    for (int j = 0; j < ct.Generics.Count; j++)
                    {
                        result.AddRange(GetIsolatedGenerics(ct.Generics[j], genericNames));
                    }
                }
            }
            return result;
        }

        private bool IsThisGeneric(MType type, string name)
        {
            return type.Entries.Count == 1 && 
                type.Entries.Any(t =>
                {
                    if (t is MGenericDataTypeEntry gt)
                    {
                        return gt.Name == name;
                    }
                    return false;
                });
        }

        private MType MatchGenerics(MType parameterizedType, MType trueType, string name)
        {
            MType resultType = MType.UNION_BASE;
            if (IsThisGeneric(parameterizedType, name))
            {
                resultType = resultType.Union(trueType);
            }
            // Now need to traverse the generics (subtypes) of the parameterized types
            for (int i = 0; i < parameterizedType.Entries.Count; i++)
            {
                MDataTypeEntry typeEntry = parameterizedType.Entries[i];
                MDataTypeEntry trueTypeEntry = trueType.Entries[i];
                // NOTE: the case above covers if this is a generic type
                // Right now, generics can't have generics of their own
                // (so can't have T<R> or something)
                if (typeEntry is MFunctionDataTypeEntry ft)
                {
                    if (trueTypeEntry is MFunctionDataTypeEntry trueFt)
                    {
                        // Need to handle the return type AND parameter types
                        resultType = resultType.Union(MatchGenerics(ft.ReturnType, trueFt.ReturnType, name));
                        for (int k = 0; k < ft.ParameterTypes.Count; k++)
                        {
                            resultType = resultType.Union(MatchGenerics(ft.ParameterTypes[k],
                                trueFt.ParameterTypes[k], name));
                        }
                    }
                    else
                    {
                        throw new TypeDeterminationException($"Assigning generics: " +
                            "Found a parameterized function type with no true function type.");
                    }
                }
                else if (typeEntry is MConcreteDataTypeEntry ct)
                {
                    if (trueTypeEntry is MConcreteDataTypeEntry trueCt)
                    {
                        for (int j = 0; j < ct.Generics.Count; j++)
                        {
                            resultType = resultType.Union(MatchGenerics(ct.Generics[j], trueCt.Generics[j], name));
                        }
                    }
                    else
                    {
                        throw new TypeDeterminationException($"Assigning generics: " + 
                            "Found a parameterized concrete type with no true concrete type.");
                    }
                }
            }

            return resultType;
        }
    }
}
