﻿using IML.Environments;
using IML.Evaluation;
using IML.Functions;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public delegate MValue NativeExpression(MArguments args, MEnvironment env, IInterpreter evaluator);
    public class MClosure
    {
        public bool IsEmpty { get; private set; }
        public MEnvironment Environment { get; private set; }
        public List<Ast> AstBody { get; private set; }
        public NativeExpression NativeBody { get; private set; }
        public bool IsNativeBody { get; private set; }
        public bool CreatesEnv 
        { 
            get
            {
                return TypeEntry.EnvironmentType != LambdaEnvironmentType.ForceNoEnvironment;
            }
        }
        public MFunctionDataTypeEntry TypeEntry { get; private set; }
        List<string> paramNames;
        public MParameters Parameters { get; private set; }

        public MType ReturnType
        {
            get
            {
                return TypeEntry.ReturnType;
            }
        }
        public List<MType> ParameterTypes
        {
            get
            {
                return TypeEntry.ParameterTypes;
            }
        }
        public List<string> DefinedGenerics
        {
            get
            {
                return TypeEntry.GenericNames;
            }
        }
        public bool IsPure
        {
            get
            {
                return TypeEntry.IsPure;
            }
        }
        public bool IsLastVarArgs
        {
            get
            {
                return TypeEntry.IsLastVarArgs;
            }
        }

        private MClosure()
        {
            IsEmpty = true;
        }

        public MClosure(MFunctionDataTypeEntry type, List<string> paramNames, MEnvironment env, List<Ast> body)
        {
            TypeEntry = type;
            this.paramNames = paramNames;
            Environment = env;
            AstBody = body;
            IsNativeBody = false;
            if (!(type is null))
            {
                Parameters = ConstructParameters(type.ParameterTypes, paramNames);
            }
            else
            {
                Parameters = null;
            }
        }
        public MClosure(MFunctionDataTypeEntry type, List<string> paramNames, MEnvironment env, NativeExpression nativeBody)
        {
            TypeEntry = type;
            this.paramNames = paramNames;
            Environment = env;
            NativeBody = nativeBody;
            IsNativeBody = true;
            if (!(type is null))
            {
                Parameters = ConstructParameters(type.ParameterTypes, paramNames);
            }
            else
            {
                Parameters = null;
            }
        }

        private static MParameters ConstructParameters(List<MType> types, List<string> names)
        {
            if (types.Count != names.Count)
            {
                throw new InvalidOperationException("Types must have same length as names");
            }
            List<MParameter> ps = new List<MParameter>();
            for (int i = 0; i < types.Count; i++)
            {
                MParameter p = new MParameter(types[i], names[i]);
                ps.Add(p);
            }
            return new MParameters(ps);
        }

        public static MClosure Empty = new MClosure();

        public MClosure CloneWithNewEnvironment(MEnvironment env)
        {
            if (IsNativeBody)
            {
                return new MClosure(TypeEntry, paramNames, env, NativeBody);
            }
            else
            {
                return new MClosure(TypeEntry, paramNames, env, AstBody);
            }
        }
    }
}
