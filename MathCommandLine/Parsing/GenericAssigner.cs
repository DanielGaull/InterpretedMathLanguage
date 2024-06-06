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
        public List<AstType> AssignGenerics(LambdaAstTypeEntry callee, AstType returnType, 
            List<AstType> parameters)
        {
            if (callee.GenericNames.Count == 0)
            {
                return new List<AstType>();
            }

            // If either the return type or parameters have generics with multiple entries,
            // then we simply cannot determine the type
            if (HasUnionedGenerics(callee.ReturnType, callee.GenericNames) ||
                callee.ParamTypes.Any(p => HasUnionedGenerics(p, callee.GenericNames)))
            {
                throw new TypeDeterminationException("Cannot infer generics when they appear in a union. " +
                    "Please manually define generics.");
            }

            // Now, for each generic name, find the associated parameter or return type
            // If there are multiple, we just union them together
            List<AstType> result = new List<AstType>();
            for (int i = 0; i < callee.GenericNames.Count; i++)
            {
                AstType type = AstType.UNION_BASE;
                string name = callee.GenericNames[i];
                for (int j = 0; j < parameters.Count; j++)
                {
                    type = type.Union(MatchGenerics(callee.ParamTypes[j], parameters[j], name));
                }
                type = type.Union(MatchGenerics(callee.ReturnType, returnType, name));

                // Generic is not assigned; we throw an error
                // For example, a user could declare a variable of this type, so we need an actual
                // value for it
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

        private bool HasUnionedGenerics(AstType type, List<string> genericNames)
        {
            bool doesThisTypeHasUnionedGenerics = type.Entries.Count > 1 &&
                type.Entries.Any(t => genericNames.Contains(t.DataTypeName));
            bool doNestedTypesHaveUnionedGenerics =
                type.Entries.Any(t => t.Generics.Any(g => HasUnionedGenerics(g, genericNames)));
            return doesThisTypeHasUnionedGenerics || doNestedTypesHaveUnionedGenerics;
        }

        private bool IsThisGeneric(AstType type, string name)
        {
            return type.Entries.Count == 1 && type.Entries.Any(t => t.DataTypeName == name);
        }

        private AstType MatchGenerics(AstType parameterizedType, AstType trueType, string name)
        {
            AstType returnType = AstType.UNION_BASE;
            if (IsThisGeneric(parameterizedType, name))
            {
                returnType = returnType.Union(trueType);
            }
            // Now need to traverse the generics (subtypes) of the parameterized types
            for (int i = 0; i < parameterizedType.Entries.Count; i++)
            {
                for (int j = 0; j < parameterizedType.Entries[i].Generics.Count; j++)
                {
                    returnType = returnType.Union(MatchGenerics(parameterizedType.Entries[i].Generics[j],
                        trueType.Entries[i].Generics[j], name));
                }
            }

            return returnType;
        }
    }
}
