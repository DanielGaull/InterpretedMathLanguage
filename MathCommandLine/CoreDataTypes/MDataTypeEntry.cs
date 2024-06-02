using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace IML.CoreDataTypes
{
    public abstract class MDataTypeEntry : IEquatable<MDataTypeEntry>
    {
        public static readonly MConcreteDataTypeEntry Any = new MConcreteDataTypeEntry(MDataType.Any);
        public static readonly MConcreteDataTypeEntry Number = new MConcreteDataTypeEntry(MDataType.Number);
        public static readonly MConcreteDataTypeEntry Boolean = new MConcreteDataTypeEntry(MDataType.Boolean);
        public static readonly MConcreteDataTypeEntry String = new MConcreteDataTypeEntry(MDataType.String);
        public static readonly MConcreteDataTypeEntry Type = new MConcreteDataTypeEntry(MDataType.Type);
        public static readonly MConcreteDataTypeEntry Error = new MConcreteDataTypeEntry(MDataType.Error);
        public static readonly MConcreteDataTypeEntry Void = new MConcreteDataTypeEntry(MDataType.Void);
        public static readonly MConcreteDataTypeEntry Null = new MConcreteDataTypeEntry(MDataType.Null);
        public static MConcreteDataTypeEntry List(MType of)
        {
            return new MConcreteDataTypeEntry(MDataType.List, of);
        }
        public static MConcreteDataTypeEntry Reference(MType of)
        {
            return new MConcreteDataTypeEntry(MDataType.Reference, of);
        }
        public static MFunctionDataTypeEntry Function(MType returnType, List<MType> paramTypes)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, new List<string>(),
                false, LambdaEnvironmentType.AllowAny, false);
        }
        public static MFunctionDataTypeEntry Function(MType returnType, List<MType> paramTypes, bool isPure,
            bool isLastVarArgs)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, new List<string>(), isPure, 
                LambdaEnvironmentType.AllowAny, isLastVarArgs);
        }
        public static MFunctionDataTypeEntry Function(MType returnType, List<MType> paramTypes, bool isPure, 
            bool isLastVarArgs, LambdaEnvironmentType envType)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, new List<string>(), isPure, envType, isLastVarArgs);
        }
        public static MFunctionDataTypeEntry Function(MType returnType, List<MType> paramTypes, List<string> genericNames,
            bool isPure, bool isLastVarArgs)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, genericNames, isPure,
                LambdaEnvironmentType.AllowAny, isLastVarArgs);
        }
        public static MFunctionDataTypeEntry Function(MType returnType, List<MType> paramTypes, List<string> genericNames, 
            bool isPure, bool isLastVarArgs, LambdaEnvironmentType envType)
        {
            return new MFunctionDataTypeEntry(returnType, paramTypes, genericNames, isPure, envType, isLastVarArgs);
        }

        public abstract MDataTypeEntry DeepClone();

        public static bool operator ==(MDataTypeEntry e1, MDataTypeEntry e2)
        {
            if (e1 is MFunctionDataTypeEntry && e2 is MFunctionDataTypeEntry)
            {
                MFunctionDataTypeEntry fe1 = e1 as MFunctionDataTypeEntry;
                MFunctionDataTypeEntry fe2 = e2 as MFunctionDataTypeEntry;
                return fe1.Equals(fe2);
            }
            else if (e1 is MConcreteDataTypeEntry && e2 is MConcreteDataTypeEntry)
            {
                MConcreteDataTypeEntry ce1 = e1 as MConcreteDataTypeEntry;
                MConcreteDataTypeEntry ce2 = e2 as MConcreteDataTypeEntry;
                return ce1.Equals(ce2);
            }
            return false;
        }
        public static bool operator !=(MDataTypeEntry e1, MDataTypeEntry e2)
        {
            return !(e1 == e2);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is MDataTypeEntry))
            {
                return false;
            }
            MDataTypeEntry o = obj as MDataTypeEntry;
            return this == o;
        }

        public bool Equals([AllowNull] MDataTypeEntry other)
        {
            return this == other;
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
        public bool Equals(MConcreteDataTypeEntry other)
        {
            // Verify that generic counts match
            // Do this first since it's the fastest
            if (Generics.Count != other.Generics.Count)
            {
                return false;
            }
            // Now verify that the base types match
            if (!DataType.MatchesTypeExactly(other.DataType))
            {
                return false;
            }
            // NOW we can actually iterate over the generics and compare them
            // This is the most expensive check
            for (int i = 0; i < Generics.Count; i++)
            {
                if (Generics[i] != other.Generics[i])
                {
                    return false;
                }
            }
            return true;
        }
        public override MDataTypeEntry DeepClone()
        {
            List<MType> generics = new List<MType>();
            foreach (MType gen in Generics)
            {
                generics.Add(gen.DeepClone());
            }
            return new MConcreteDataTypeEntry(DataType, generics);
        }

        public override string ToString()
        {
            List<string> genericNames = Generics.Select(x => x.ToString()).ToList();
            return DataType.Name + "<" + string.Join(",", genericNames) + ">";
        }
    }
    public class MFunctionDataTypeEntry : MConcreteDataTypeEntry
    {
        public MType ReturnType { get; private set; }
        public List<MType> ParameterTypes { get; private set; }
        public List<string> GenericNames { get; private set; }
        public LambdaEnvironmentType EnvironmentType { get; private set; }
        public bool IsPure { get; private set; }
        public bool IsLastVarArgs { get; private set; }

        public MFunctionDataTypeEntry(MType returnType, List<MType> paramTypes, List<string> genericNames, bool isPure,
            LambdaEnvironmentType envType, bool isLastVarArgs)
            : base(MDataType.Function)
        {
            ReturnType = returnType;
            ParameterTypes = paramTypes;
            GenericNames = genericNames;
            IsPure = isPure;
            EnvironmentType = envType;
            IsLastVarArgs = isLastVarArgs;
        }

        public override string ToString()
        {
            string genericPart = "<" + string.Join(",", GenericNames) + ">";
            string paramPart = "(" + string.Join(",", ParameterTypes.Select(x => x.ToString())) +
                (IsLastVarArgs ? "..." : "") + ")";
            string arrowPart = EnvironmentType == LambdaEnvironmentType.ForceNoEnvironment ? "~>" :
                EnvironmentType == LambdaEnvironmentType.ForceEnvironment ? "!=>" : "=>";
            return genericPart + paramPart + arrowPart + ":" + ReturnType.ToString();
        }

        public bool Equals(MFunctionDataTypeEntry other)
        {
            // Check all the small easy stuff
            if (IsPure != other.IsPure || EnvironmentType != other.EnvironmentType || IsLastVarArgs != other.IsLastVarArgs)
            {
                return false;
            }
            // The generic names don't have to match, but should have the same count of them
            if (GenericNames.Count != other.GenericNames.Count)
            {
                return false;
            }
            // Need to have equal param counts
            if (ParameterTypes.Count != other.ParameterTypes.Count)
            {
                return false;
            }
            // For the types, we have to resolve the generic names
            // We need to ensure that we use the same names for all of them
            // For example, we could have this: [T](T)=>void and [F](F)=>void which are the same function types,
            // but have different generic names
            // So, we have to generate new return types and parameter types that honor this by recognizing the function
            // generics are the same
            // We will replace these with special FunctionPositionalGenerics that encode the index in the generic list
            // for generics defined at the function level (higher-level generics will be untouched, so ex
            // if we have (T)=>void and T is defined higher up, we will leave it as T
            MType myReturnType = ReturnType.DeepClone();
            MType otherReturnType = other.ReturnType.DeepClone();
            List<MType> myParams = new List<MType>();
            List<MType> otherParams = new List<MType>();
            for (int i = 0; i < ParameterTypes.Count; i++)
            {
                myParams.Add(ParameterTypes[i].DeepClone());
                // Parameters are same length so we can use the same loop for both types
                otherParams.Add(other.ParameterTypes[i].DeepClone());
            }
            // Now convert these to use positions for generics if the name is defined...
            ConvertToGenericPositionalEncodings(myReturnType, GenericNames);
            ConvertToGenericPositionalEncodings(otherReturnType, other.GenericNames);
            for (int i = 0; i < myParams.Count; i++)
            {
                ConvertToGenericPositionalEncodings(myParams[i], GenericNames);
                ConvertToGenericPositionalEncodings(otherParams[i], other.GenericNames);
            }
            // Ok, now we can strictly compare them. If a generic was defined within the function, it is positoinally encoded
            // If a generic was defined outside of the function, the scope must match

            // Verify the return types
            if (myReturnType != otherReturnType)
            {
                return false;
            }
            // Verify the parameters
            for (int i = 0; i < myParams.Count; i++)
            {
                if (myParams[i] != otherParams[i])
                {
                    return false;
                }
            }
            return true;
        }

        // Modifies the type in-place
        private void ConvertToGenericPositionalEncodings(MType type, List<string> genericNames)
        {
            for (int i = 0; i < type.Entries.Count; i++)
            {
                if (type.Entries[i] is MGenericDataTypeEntry)
                {
                    // This is the type entry we'll need to modify
                    MGenericDataTypeEntry ge = type.Entries[i] as MGenericDataTypeEntry;
                    string name = ge.Name;
                    int index = genericNames.IndexOf(name);
                    if (index >= 0)
                    {
                        // Replace with the positionally-encoded entry
                        FunctionPositionalGeneric newGe = new FunctionPositionalGeneric(index);
                        type.Entries[i] = newGe;
                    }
                }
            }
        }

        public override MDataTypeEntry DeepClone()
        {
            List<MType> paramList = new List<MType>();
            foreach (MType p in ParameterTypes)
            {
                paramList.Add(p.DeepClone());
            }
            List<string> gens = new List<string>();
            foreach (string g in GenericNames)
            {
                gens.Add(g);
            }
            return new MFunctionDataTypeEntry(ReturnType.DeepClone(), paramList, gens, IsPure, EnvironmentType, IsLastVarArgs);
        }

        private class FunctionPositionalGeneric : MDataTypeEntry
        {
            public int Position { get; private set; }
            public FunctionPositionalGeneric(int pos)
            {
                Position = pos;
            }

            public override MDataTypeEntry DeepClone()
            {
                return new FunctionPositionalGeneric(Position);
            }
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

        public override MDataTypeEntry DeepClone()
        {
            return new MGenericDataTypeEntry(Name);
        }
    }
}
