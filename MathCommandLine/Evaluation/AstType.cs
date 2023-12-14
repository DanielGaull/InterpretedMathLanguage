using IML.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation
{
    public class AstType
    {
        public List<AstTypeEntry> Entries { get; private set; }

        public AstType(List<AstTypeEntry> entries)
        {
            Entries = entries;
        }

        public static readonly AstType Any =
            new AstType(new List<AstTypeEntry>()
            {
                new AstTypeEntry("any", new List<AstTypeRestriction>())
            });
    }
    public class AstTypeEntry
    {
        public string DataTypeName { get; private set; }
        public List<AstTypeRestriction> Restrictions { get; private set; }

        public AstTypeEntry(string dataTypeName, List<AstTypeRestriction> restrictions)
        {
            DataTypeName = dataTypeName;
            Restrictions = restrictions;
        }

        public static AstTypeEntry Simple(string name)
        {
            return new AstTypeEntry(name, new List<AstTypeRestriction>());
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
            : base("function", new List<AstTypeRestriction>())
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

    public class AstTypeRestriction
    {
        public string Name { get; private set; }
        public List<Argument> Args { get; private set; }

        public AstTypeRestriction(string name, List<Argument> args)
        {
            Name = name;
            Args = args;
        }

        public class Argument
        {
            public RestrictionArgumentType ArgType { get; private set; }
            public double NumberValue { get; private set; }
            public string StringValue { get; private set; }
            public AstType TypeValue { get; private set; }

            private Argument(RestrictionArgumentType type, double num, string str, AstType t)
            {
                ArgType = type;
                NumberValue = num;
                StringValue = str;
                TypeValue = t;
            }

            public static Argument Number(double v)
            {
                return new Argument(RestrictionArgumentType.Number, v, null, null);
            }
            public static Argument String(string v)
            {
                return new Argument(RestrictionArgumentType.String, 0, v, null);
            }
            public static Argument Type(AstType v)
            {
                return new Argument(RestrictionArgumentType.Type, 0, null, v);
            }
        }
    }
}
