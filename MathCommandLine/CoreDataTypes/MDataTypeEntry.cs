using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public class MDataTypeEntry
    {
        public MDataType DataType { get; private set; }
        public List<MType> Generics { get; private set; }

        public MDataTypeEntry(MDataType dt)
        {
            DataType = dt;
            Generics = new List<MType>();
        }
        public MDataTypeEntry(MDataType dt, List<MType> generics)
        {
            DataType = dt;
            Generics = generics;
        }
        public MDataTypeEntry(MDataType dt, params MType[] generics)
        {
            DataType = dt;
            Generics = new List<MType>(generics);
        }

        public static readonly MDataTypeEntry Any = new MDataTypeEntry(MDataType.Any);
        public static readonly MDataTypeEntry Number = new MDataTypeEntry(MDataType.Number);
        public static readonly MDataTypeEntry Boolean = new MDataTypeEntry(MDataType.Boolean);
        public static readonly MDataTypeEntry String = new MDataTypeEntry(MDataType.String);
        public static readonly MDataTypeEntry Type = new MDataTypeEntry(MDataType.Type);
        public static readonly MDataTypeEntry Error = new MDataTypeEntry(MDataType.Error);
        public static readonly MDataTypeEntry Void = new MDataTypeEntry(MDataType.Null);
        public static readonly MDataTypeEntry Null = new MDataTypeEntry(MDataType.Void);
        public static MDataTypeEntry List(MType of)
        {
            return new MDataTypeEntry(MDataType.List, of);
        }
        public static MDataTypeEntry Reference(MType of)
        {
            return new MDataTypeEntry(MDataType.Reference, of);
        }


    }
}
