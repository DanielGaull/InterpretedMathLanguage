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

        public Dictionary<string, MType> FieldTypes;
        public string Name { get; private set; }
        public int NumberOfGenerics { get; private set; }

        private MDataType(int id, string name, int numberOfGenerics)
        {
            internalId = id;
            Name = name;
            NumberOfGenerics = numberOfGenerics;
            FieldTypes = new Dictionary<string, MType>();
        }
        public MDataType(string name, int numberOfGenerics)
            : this(internalIdTracker++, name, numberOfGenerics)
        {
        }

        private void SetFieldTypes(Dictionary<string, MType> fieldTypes)
        {
            FieldTypes = fieldTypes;
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

        private static Dictionary<string, MType> ListMemberTypes() => new Dictionary<string, MType>()
        {
            { "get", MType.Function(MType.Generic("T"), MType.Number()) },
            { "index", MType.Function(MType.Number(), MType.Generic("T")) },
            { "indexc", MType.Function(MType.Number(), new List<MType>()
                        {
                            MType.Generic("T"),
                            MType.Function(MType.Any(), MType.Generic("T"), MType.Generic("T"))
                        })
            },
            { "length", MType.Function(MType.Number()) },
            { "map", MType.Function(MType.List(new MType(new MGenericDataTypeEntry("R"))),
                            new List<MType>()
                            {
                                MType.Function(new MType(new MGenericDataTypeEntry("R")), MType.Generic("T"))
                            },
                            new List<string>()
                            {
                                "R"
                            })
            },
            { "reduce", MType.Function(new MType(new MGenericDataTypeEntry("R")),
                            new List<MType>()
                            {
                                MType.Function(new MType(new MGenericDataTypeEntry("R")),
                                    new MType(new MGenericDataTypeEntry("R")), MType.Generic("T")),
                                new MType(new MGenericDataTypeEntry("R"))
                            },
                            new List<string>() // Generics
                            {
                                "R"
                            })
            },
            { "add", MType.Function(MType.Void(), MType.Generic("T")) },
            { "insert", MType.Function(MType.Void(), MType.Generic("T"), MType.Number()) },
            { "removeAt", MType.Function(MType.Void(), MType.Number()) },
            { "remove", MType.Function(MType.Boolean(), MType.Generic("T")) },
            { "removec", MType.Function(MType.Boolean(),
                            new List<MType>()
                            {
                                MType.Generic("T"),
                                MType.Function(MType.Any(), MType.Generic("T"), MType.Generic("T"))
                            })
            },
            { "addAll", MType.Function(MType.Void(), MType.List(MType.Generic("T"))) },
        };
        private static Dictionary<string, MType> StringMemberTypes() => new Dictionary<string, MType>()
        {
            { "chars", MType.List(MType.Number()) }
        };
        private static Dictionary<string, MType> ErrorMemberTypes() => new Dictionary<string, MType>()
        {
            { "code", MType.Number() },
            { "message", MType.String() },
            { "data", MType.List(MType.Any()) },
        };



        public static readonly MDataType Any =
            new MDataType(ANY_TYPE_ID, ANY_TYPE_NAME, 0);
        public static readonly MDataType Number =
            new MDataType(NUMBER_TYPE_NAME, 0);
        public static readonly MDataType List =
            new MDataType(LIST_TYPE_NAME, 1);
        public static readonly MDataType Function =
            new MDataType(FUNCTION_TYPE_NAME, 0);
        public static readonly MDataType Boolean =
            new MDataType(BOOLEAN_TYPE_NAME, 0);
        public static readonly MDataType Type =
            new MDataType(TYPE_TYPE_NAME, 0);
        public static readonly MDataType Reference =
            new MDataType(REF_TYPE_NAME, 1);
        public static readonly MDataType Error =
            new MDataType(ERROR_TYPE_NAME, 0);
        public static readonly MDataType String =
            new MDataType(STRING_TYPE_NAME, 0);
        public static readonly MDataType Null =
            new MDataType(NULL_TYPE_NAME, 0);
        public static readonly MDataType Void =
            new MDataType(VOID_TYPE_NAME, 0);

        static MDataType()
        {
            List.SetFieldTypes(ListMemberTypes());
            String.SetFieldTypes(StringMemberTypes());
            Error.SetFieldTypes(ErrorMemberTypes());
        }
    }
}
