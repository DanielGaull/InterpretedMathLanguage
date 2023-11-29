using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class MDataTypeRestrictionEntry
    {
        public MDataType TypeDefinition { get; private set; }
        public List<MTypeArgument> TypeArguments { get; private set; }

        private MDataTypeRestrictionEntry(MDataType def, List<MTypeArgument> args)
        {
            TypeDefinition = def;
            TypeArguments = args;
        }

        public static MDataTypeRestrictionEntry CreateDataType(MDataType def)
        {
            return new MDataTypeRestrictionEntry(def, new List<MTypeArgument>());
        }
        public static MDataTypeRestrictionEntry CreateDataType(MDataType def, List<MTypeArgument> args)
        {
            return new MDataTypeRestrictionEntry(def, args);
        }

        public static readonly MDataTypeRestrictionEntry Any = CreateDataType(MDataType.Any);
        public static readonly MDataTypeRestrictionEntry Number = CreateDataType(MDataType.Number);
        public static readonly MDataTypeRestrictionEntry List = CreateDataType(MDataType.List);
        public static readonly MDataTypeRestrictionEntry Function = CreateDataType(MDataType.Function);
        public static readonly MDataTypeRestrictionEntry Boolean = CreateDataType(MDataType.Boolean);
        public static readonly MDataTypeRestrictionEntry Reference = CreateDataType(MDataType.Reference);
        public static readonly MDataTypeRestrictionEntry Type = CreateDataType(MDataType.Type);
        public static readonly MDataTypeRestrictionEntry String = CreateDataType(MDataType.String);
        public static readonly MDataTypeRestrictionEntry Error = CreateDataType(MDataType.Error);
        public static readonly MDataTypeRestrictionEntry Void = CreateDataType(MDataType.Void);
        public static readonly MDataTypeRestrictionEntry Null = CreateDataType(MDataType.Null);
    }
}
