using System.Text;

namespace MathCommandLine.Structure
{
    public struct MDataType
    {
        // The internal ID for "any" type
        private const int ANY_TYPE_ID = -1;
        private static int internalIdTracker = 1;
        // An internal ID for the data type. This is unique for each data type, and therefore determines whether or not two data types are equal
        // Start it at 1 since the empty type will use the default value (0) for internal ID
        private int internalId;
        // The name of this data type. This is the name that is used in-code to refer to this type
        public string Name;
        // True for primitive types (number, list, lambda, big_int, big_decimal, type)
        public bool IsPrimitive;

        public MDataType(string name, bool isPrimitive)
        {
            internalId = internalIdTracker++;
            Name = name;
            IsPrimitive = isPrimitive;
        }
        private MDataType(int internalId, string name)
        {
            this.internalId = internalId;
            Name = name;
            IsPrimitive = true;
        }

        public static MDataType Empty = new MDataType();

        public bool IsAnyType
        {
            get
            {
                return internalId == ANY_TYPE_ID;
            }
        }

        // Core data types
        public static MDataType Any = new MDataType(ANY_TYPE_ID, "any");
        public static MDataType Number = new MDataType("number", true);
        public static MDataType List = new MDataType("list", true);
        public static MDataType Closure = new MDataType("function", true);
        public static MDataType Type = new MDataType("type", true);
        public static MDataType BigInt = new MDataType("big_int", true);
        public static MDataType BigDecimal = new MDataType("big_decimal", true);
        public static MDataType Error = new MDataType("error", false);
        public static MDataType Reference = new MDataType("reference", true);
        public static MDataType String = new MDataType("string", false);
        public static MDataType Void = new MDataType("void", true);
        public static MDataType Boolean = new MDataType("bool", true);
        public static MDataType Null = new MDataType("null", true);

        public static bool operator ==(MDataType dt1, MDataType dt2)
        {
            // The "any" type is equivelant to every other type
            // Type equality is not transitive
            return (dt1.internalId == dt2.internalId) || (dt1.internalId == ANY_TYPE_ID) || (dt2.internalId == ANY_TYPE_ID);
        }
        public static bool operator !=(MDataType dt1, MDataType dt2)
        {
            return (dt1.internalId != dt2.internalId) && (dt1.internalId != ANY_TYPE_ID) && (dt2.internalId != ANY_TYPE_ID);
        }
        public override bool Equals(object obj)
        {
            if (obj is MDataType)
            {
                MDataType dt = (MDataType)obj;
                return dt == this;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsEmpty()
        {
            return this == Empty;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("#");
            builder.Append(Name);
            return builder.ToString();
        }
    }
}
