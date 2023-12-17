using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public abstract class MDataTypeEntry
    {
        public static readonly MConcreteDataTypeEntry Any = new MConcreteDataTypeEntry(MDataType.Any);
        public static readonly MConcreteDataTypeEntry Number = new MConcreteDataTypeEntry(MDataType.Number);
        public static readonly MConcreteDataTypeEntry Boolean = new MConcreteDataTypeEntry(MDataType.Boolean);
        public static readonly MConcreteDataTypeEntry String = new MConcreteDataTypeEntry(MDataType.String);
        public static readonly MConcreteDataTypeEntry Type = new MConcreteDataTypeEntry(MDataType.Type);
        public static readonly MConcreteDataTypeEntry Error = new MConcreteDataTypeEntry(MDataType.Error);
        public static readonly MConcreteDataTypeEntry Void = new MConcreteDataTypeEntry(MDataType.Null);
        public static readonly MConcreteDataTypeEntry Null = new MConcreteDataTypeEntry(MDataType.Void);
        public static MConcreteDataTypeEntry List(MType of)
        {
            return new MConcreteDataTypeEntry(MDataType.List, of);
        }
        public static MConcreteDataTypeEntry Reference(MType of)
        {
            return new MConcreteDataTypeEntry(MDataType.Reference, of);
        }
        public static MConcreteDataTypeEntry Function(MType returnType, params MType[] paramTypes)
        {
            return new MFunctionDataTypeEntry(returnType, new List<MType>(paramTypes), new List<string>());
        }
        public static MConcreteDataTypeEntry Function(MType returnType, List<MType> paramTypes)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, new List<string>());
        }
        public static MConcreteDataTypeEntry Function(MType returnType, List<MType> paramTypes, List<string> genericNames)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, genericNames);
        }
    }
    public class MConcreteDataTypeEntry : MDataTypeEntry
    {
        public MDataType DataType { get; private set; }
        public List<MType> Generics { get; private set; }

        public MConcreteDataTypeEntry(MDataType dt)
        {
            DataType = dt;
            Generics = new List<MType>();
        }
        public MConcreteDataTypeEntry(MDataType dt, List<MType> generics)
        {
            DataType = dt;
            Generics = generics;
        }
        public MConcreteDataTypeEntry(MDataType dt, params MType[] generics)
        {
            DataType = dt;
            Generics = new List<MType>(generics);
        }
    }
    public class MFunctionDataTypeEntry : MConcreteDataTypeEntry
    {
        public MType ReturnType { get; private set; }
        public List<MType> ParameterTypes { get; private set; }
        public List<string> GenericNames { get; private set; }

        public MFunctionDataTypeEntry(MType returnType, List<MType> paramTypes, List<string> genericNames)
            : base(MDataType.Function)
        {
            ReturnType = returnType;
            ParameterTypes = paramTypes;
            GenericNames = genericNames;
        }
    }
    public class MGenericDataTypeEntry : MDataTypeEntry
    {
        // A generic type that isn't resolved yet
        // Simply has a name
        // This type gets resolved when used later
        public string Name { get; set; }
        public MGenericDataTypeEntry(string name)
        {
            Name = name;
        }
    }
}
