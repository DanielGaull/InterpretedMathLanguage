using IML.CoreDataTypes;
using IML.Exceptions;
using IML.Parsing.AST;
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
            if (HasUnionedGenerics(callee.ReturnType, callee.GenericNames) ||
                callee.ParameterTypes.Any(p => HasUnionedGenerics(p, callee.GenericNames)))
            {
                throw new TypeDeterminationException("Cannot infer generics when they appear in a union. " +
                    "Please manually define generics.");
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

        private bool HasUnionedGenerics(MType type, List<string> genericNames)
        {
            bool doesThisTypeHasUnionedGenerics = type.Entries.Count > 1 &&
                type.Entries.Any(t =>
                {
                    if (t is MGenericDataTypeEntry gt)
                    {
                        return genericNames.Contains(gt.Name);
                    }
                    return false;
                });
            bool doNestedTypesHaveUnionedGenerics =
                type.Entries.Any(t => 
                {
                    if (t is MConcreteDataTypeEntry ct)
                    {
                        return ct.Generics.Any(g => HasUnionedGenerics(g, genericNames));
                    }
                    else if (t is MFunctionDataTypeEntry ft)
                    {
                        return ft.ParameterTypes.Any(p => HasUnionedGenerics(p, genericNames)) ||
                            HasUnionedGenerics(ft.ReturnType, genericNames);
                    }
                    return false;
                });
            return doesThisTypeHasUnionedGenerics || doNestedTypesHaveUnionedGenerics;
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
