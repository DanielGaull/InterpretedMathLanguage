using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class MDataType
    {
        // This is the abstract definition of a data type,
        // such as a number or list
        // It defines the type arguments that are allowed, for example

        private const int ANY_TYPE_ID = -1;
        private int internalId;
        private static int internalIdTracker = 0;

        public string Name { get; private set; }
        public List<MTypeArgumentDefinition> ValidTypeArguments { get; private set; }


        private MDataType(int id, string name, List<MTypeArgumentDefinition> validTypeArguments)
        {
            internalId = id;
            Name = name;
            ValidTypeArguments = validTypeArguments;
        }
        public MDataType(string name, List<MTypeArgumentDefinition> validTypeArguments)
            : this(internalIdTracker++, name, validTypeArguments)
        {
        }

        public static readonly MDataType Any = 
            new MDataType(ANY_TYPE_ID, "any", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Number =
            new MDataType("number", new List<MTypeArgumentDefinition>());
        public static readonly MDataType List =
            new MDataType("list", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Function =
            new MDataType("function", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Boolean =
            new MDataType("boolean", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Type =
            new MDataType("type", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Reference =
            new MDataType("ref", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Error =
            new MDataType("error", new List<MTypeArgumentDefinition>());
        public static readonly MDataType String =
            new MDataType("string", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Null =
            new MDataType("null", new List<MTypeArgumentDefinition>());
        public static readonly MDataType Void =
            new MDataType("void", new List<MTypeArgumentDefinition>());

        public bool MatchesType(MDataType other)
        {
            return other.internalId == internalId || 
                internalId == ANY_TYPE_ID || 
                other.internalId == ANY_TYPE_ID;
        }
        public bool MatchesTypeExactly(MDataType other)
        {
            return other.internalId == internalId;
        }
    }
}
