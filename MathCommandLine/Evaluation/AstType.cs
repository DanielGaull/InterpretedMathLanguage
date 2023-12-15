using IML.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation
{
    public class AstType
    {
        // All the entries that are union'ed together
        public List<AstTypeEntry> Entries { get; private set; }

        public AstType(List<AstTypeEntry> entries)
        {
            Entries = entries;
        }

        public static readonly AstType Any =
            new AstType(new List<AstTypeEntry>()
            {
                new AstTypeEntry("any", new List<AstType>())
            });
    }
    public class AstTypeEntry
    {
        public string DataTypeName { get; private set; }
        public List<AstType> Generics { get; private set; }

        public AstTypeEntry(string dataTypeName, List<AstType> generics)
        {
            DataTypeName = dataTypeName;
            Generics = generics;
        }

        public static AstTypeEntry Simple(string name)
        {
            return new AstTypeEntry(name, new List<AstType>());
        }
    }
    public class LambdaAstTypeEntry : AstTypeEntry
    {
        // This special AST type has a return type and argument types
        // Also need to store if we're forcing a particular environment or a pure function

        public AstType ReturnType { get; private set; }
        public List<AstType> ArgTypes { get; private set; }
        public LambdaEnvironmentType EnvironmentType { get; private set; }
        public bool IsPure { get; private set; }

        public LambdaAstTypeEntry(AstType returnType, List<AstType> argTypes, LambdaEnvironmentType envType, bool isPure)
            : base("function", new List<AstType>())
        {
            ReturnType = returnType;
            ArgTypes = argTypes;
            EnvironmentType = envType;
            IsPure = isPure;
        }

        public enum LambdaEnvironmentType
        {
            ForceEnvironment, // Ex. ()!=>{}
            ForceNoEnvironment, // Ex. ()!~>{}
            AllowAny, // Ex. ()=>{}
        }
    }
}
