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
            if (HasUnionedGenerics(returnType, callee.GenericNames) ||
                parameters.Any(p => HasUnionedGenerics(p, callee.GenericNames)))
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
                foreach (AstType param in parameters)
                {
                    if (IsThisGeneric(param, name))
                    {
                        type = type.Union(param);
                    }
                }
                if (IsThisGeneric(returnType, name))
                {
                    type = type.Union(returnType);
                }

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
            return type.Entries.Count > 1 && 
                type.Entries.Any(t => genericNames.Contains(t.DataTypeName));
        }

        private bool IsThisGeneric(AstType type, string name)
        {
            return type.Entries.Count == 1 && type.Entries.Any(t => t.DataTypeName == name);
        }
    }
}
