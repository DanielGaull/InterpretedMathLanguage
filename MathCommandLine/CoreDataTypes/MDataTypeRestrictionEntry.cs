using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class MDataTypeRestrictionEntry
    {
        public MDataType TypeDefinition { get; private set; }
        public List<MTypeRestriction> TypeRestrictions { get; private set; }

        private MDataTypeRestrictionEntry(MDataType def, List<MTypeRestriction> restrictions)
        {
            TypeDefinition = def;
            TypeRestrictions = restrictions;
        }

        public static MDataTypeRestrictionEntry CreateDataType(MDataType def)
        {
            return new MDataTypeRestrictionEntry(def, new List<MTypeRestriction>());
        }
        public static MDataTypeRestrictionEntry CreateDataType(MDataType def, List<MTypeRestriction> args)
        {
            return new MDataTypeRestrictionEntry(def, args);
        }

        #region Static Type Defs
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
        #endregion

        public static bool operator ==(MDataTypeRestrictionEntry d1, MDataTypeRestrictionEntry d2)
        {
            if (!d1.TypeDefinition.MatchesTypeExactly(d2.TypeDefinition))
            {
                return false;
            }
            if (d1.TypeRestrictions.Count != d2.TypeRestrictions.Count)
            {
                return false;
            }
            for (int i = 0; i < d1.TypeRestrictions.Count; i++)
            {
                if (d1.TypeRestrictions[i] != d2.TypeRestrictions[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(MDataTypeRestrictionEntry d1, MDataTypeRestrictionEntry d2)
        {
            return !(d1 == d2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MDataTypeRestrictionEntry other)
            {
                return this == other;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
