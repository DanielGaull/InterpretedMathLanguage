using IML.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Evaluation
{
    public class AstParameter
    {
        public AstParameterType Type { get; private set; }
        public string Name { get; private set; }

        public AstParameter(string name, AstParameterType type)
        {
            Name = name;
            Type = type;
        }
    }
    public class AstParameterType
    {
        public List<AstParameterTypeEntry> Entries { get; private set; }

        public AstParameterType(List<AstParameterTypeEntry> entries)
        {
            Entries = entries;
        }

        public static readonly AstParameterType Any = 
            new AstParameterType(new List<AstParameterTypeEntry>()
            {
                new AstParameterTypeEntry("any", new List<AstParameterTypeRestriction>())
            });
    }
    public class AstParameterTypeEntry
    {
        public string DataTypeName { get; private set; }
        public List<AstParameterTypeRestriction> Restrictions { get; private set; }

        public AstParameterTypeEntry(string dataTypeName, List<AstParameterTypeRestriction> restrictions)
        {
            DataTypeName = dataTypeName;
            Restrictions = restrictions;
        }

        public static AstParameterTypeEntry Simple(string name)
        {
            return new AstParameterTypeEntry(name, new List<AstParameterTypeRestriction>());
        }
    }
    public class AstParameterTypeRestriction
    {
        public string Name { get; private set; }
        public List<Argument> Args { get; private set; }

        public AstParameterTypeRestriction(string name, List<Argument> args)
        {
            Name = name;
            Args = args;
        }

        public class Argument
        {
            public RestrictionArgumentType ArgType { get; private set; }
            public double NumberValue { get; private set; }
            public string StringValue { get; private set; }
            public AstParameterType TypeValue { get; private set; }

            private Argument(RestrictionArgumentType type, double num, string str, AstParameterType t)
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
            public static Argument Type(AstParameterType v)
            {
                return new Argument(RestrictionArgumentType.Number, 0, null, v);
            }
        }
    }
}
