using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
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
        public List<MTypeRestrictionDefinition> ValidTypeArguments { get; private set; }


        private MDataType(int id, string name, List<MTypeRestrictionDefinition> validTypeArguments)
        {
            internalId = id;
            Name = name;
            ValidTypeArguments = validTypeArguments;
        }
        public MDataType(string name, List<MTypeRestrictionDefinition> validTypeArguments)
            : this(internalIdTracker++, name, validTypeArguments)
        {
        }

        public const string ANY_TYPE_NAME = "any";
        public const string NUMBER_TYPE_NAME = "number";
        public const string LIST_TYPE_NAME = "list";
        public const string FUNCTION_TYPE_NAME = "function";
        public const string BOOLEAN_TYPE_NAME = "boolean";
        public const string TYPE_TYPE_NAME = "type";
        public const string REF_TYPE_NAME = "ref";
        public const string ERROR_TYPE_NAME = "error";
        public const string STRING_TYPE_NAME = "string";
        public const string VOID_TYPE_NAME = "void";
        public const string NULL_TYPE_NAME = "null";

        public static readonly MDataType Any = 
            new MDataType(ANY_TYPE_ID, ANY_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Number =
            new MDataType(NUMBER_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType List =
            new MDataType(LIST_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Function =
            new MDataType(FUNCTION_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Boolean =
            new MDataType(BOOLEAN_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Type =
            new MDataType(TYPE_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Reference =
            new MDataType(REF_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Error =
            new MDataType(ERROR_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType String =
            new MDataType(STRING_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Null =
            new MDataType(NULL_TYPE_NAME, new List<MTypeRestrictionDefinition>());
        public static readonly MDataType Void =
            new MDataType(VOID_TYPE_NAME, new List<MTypeRestrictionDefinition>());

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
