using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public abstract class MDataTypeEntry
    {
        public static readonly MDataTypeEntry Any = new MConcreteDataTypeEntry(MDataType.Any);
        public static readonly MDataTypeEntry Number = new MConcreteDataTypeEntry(MDataType.Number);
        public static readonly MDataTypeEntry Boolean = new MConcreteDataTypeEntry(MDataType.Boolean);
        public static readonly MDataTypeEntry String = new MConcreteDataTypeEntry(MDataType.String);
        public static readonly MDataTypeEntry Type = new MConcreteDataTypeEntry(MDataType.Type);
        public static readonly MDataTypeEntry Error = new MConcreteDataTypeEntry(MDataType.Error);
        public static readonly MDataTypeEntry Void = new MConcreteDataTypeEntry(MDataType.Null);
        public static readonly MDataTypeEntry Null = new MConcreteDataTypeEntry(MDataType.Void);
        public static MDataTypeEntry List(MType of)
        {
            return new MConcreteDataTypeEntry(MDataType.List, of);
        }
        public static MDataTypeEntry Reference(MType of)
        {
            return new MConcreteDataTypeEntry(MDataType.Reference, of);
        }
        public static MDataTypeEntry Fuction(MType returnType, params MType[] paramTypes)
        {
            return new MFunctionDataTypeEntry(returnType, new List<MType>(paramTypes), new List<string>());
        }
        public static MDataTypeEntry Fuction(MType returnType, List<MType> paramTypes)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, new List<string>());
        }
        public static MDataTypeEntry Fuction(MType returnType, List<MType> paramTypes, List<string> genericNames)
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
    public class MGenericDataTypeEntry
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
