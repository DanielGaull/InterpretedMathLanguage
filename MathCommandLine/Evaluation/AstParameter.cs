using IML.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Evaluation
{
    public class AstParameter
    {
        public AstType Type { get; private set; }
        public string Name { get; private set; }

        public AstParameter(string name, AstType type)
        {
            Name = name;
            Type = type;
        }
    }
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
                return new Argument(RestrictionArgumentType.Number, 0, null, v);
            }
        }
    }
}
