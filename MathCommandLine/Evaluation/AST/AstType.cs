﻿using IML.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation
{
    public class AstType
    {
        // All the entries that are union'ed together
        public List<AstTypeEntry> Entries { get; private set; }

        public AstType(List<AstTypeEntry> entries)
        {
            Entries = entries;
        }
        public AstType(AstTypeEntry entry)
            : this(new List<AstTypeEntry>() { entry })
        {
        }
        public AstType(string typeName, params AstType[] generics)
            : this(new AstTypeEntry(typeName, new List<AstType>(generics)))
        {
        }

        public static readonly AstType Any =
            new AstType(new List<AstTypeEntry>()
            {
                new AstTypeEntry(MDataType.ANY_TYPE_NAME, new List<AstType>())
            });

        public static bool operator ==(AstType a1, AstType a2)
        {
            if (a1.Entries.Count != a2.Entries.Count)
            {
                return false;
            }
            for (int i = 0; i < a1.Entries.Count; i++)
            {
                if (a1.Entries[i] is LambdaAstTypeEntry)
                {
                    if (!(a2.Entries[i] is LambdaAstTypeEntry))
                    {
                        return false;
                    }
                    LambdaAstTypeEntry e1 = (LambdaAstTypeEntry)a1.Entries[i];
                    LambdaAstTypeEntry e2 = (LambdaAstTypeEntry)a2.Entries[i];
                    return e1 == e2;
                }
                if (a1.Entries[i] != a2.Entries[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(AstType a1, AstType a2)
        {
            return !(a1 == a2);
        }
    }
    public class AstTypeEntry
    {
        public string DataTypeName { get; private set; }
        public List<AstType> Generics { get; private set; }

        public AstTypeEntry(string dataTypeName, List<AstType> generics)
        {
            DataTypeName = dataTypeName;
            Generics = generics;
        }

        public static AstTypeEntry Simple(string name)
        {
            return new AstTypeEntry(name, new List<AstType>());
        }

        public static bool operator ==(AstTypeEntry a1, AstTypeEntry a2)
        {
            if (a1.DataTypeName != a2.DataTypeName)
            {
                return false;
            }
            if (a1.Generics.Count != a2.Generics.Count)
            {
                return false;
            }
            for (int i = 0; i < a1.Generics.Count; i++)
            {
                if (a1.Generics[i] != a2.Generics[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(AstTypeEntry a1, AstTypeEntry a2)
        {
            return !(a1 == a2);
        }
    }
    public class LambdaAstTypeEntry : AstTypeEntry
    {
        // This special AST type has a return type and argument types
        // Also need to store if we're forcing a particular environment or a pure function

        public AstType ReturnType { get; private set; }
        public List<AstType> ParamTypes { get; private set; }
        public LambdaEnvironmentType EnvironmentType { get; private set; }
        public bool IsPure { get; private set; }
        public bool IsLastVarArgs { get; private set; }
        public List<string> GenericNames { get; private set; }

        public LambdaAstTypeEntry(AstType returnType, List<AstType> argTypes, LambdaEnvironmentType envType, bool isPure,
            bool isLastVarArgs, List<string> genericNames)
            : base(MDataType.FUNCTION_TYPE_NAME, new List<AstType>())
        {
            ReturnType = returnType;
            ParamTypes = argTypes;
            EnvironmentType = envType;
            IsPure = isPure;
            IsLastVarArgs = isLastVarArgs;
            GenericNames = genericNames;
        }

        public static bool operator ==(LambdaAstTypeEntry a1, LambdaAstTypeEntry a2)
        {
            if (a1.EnvironmentType != a2.EnvironmentType)
            {
                return false;
            }
            if (a1.IsPure != a2.IsPure || a1.IsLastVarArgs != a2.IsLastVarArgs)
            {
                return false;
            }
            if (a1.ReturnType != a2.ReturnType)
            {
                return false;
            }
            if (a1.GenericNames.Count != a2.GenericNames.Count)
            {
                return false;
            }
            if (a1.ParamTypes.Count != a2.ParamTypes.Count)
            {
                return false;
            }
            for (int i = 0; i < a1.GenericNames.Count; i++)
            {
                if (a1.GenericNames[i] != a2.GenericNames[i])
                {
                    return false;
                }
            }
            for (int i = 0; i < a1.ParamTypes.Count; i++)
            {
                if (a1.ParamTypes[i] != a2.ParamTypes[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(LambdaAstTypeEntry a1, LambdaAstTypeEntry a2)
        {
            return !(a1 == a2);
        }
    }
}
