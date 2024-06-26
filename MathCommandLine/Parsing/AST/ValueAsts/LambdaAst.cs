﻿using IML.CoreDataTypes;
using IML.Functions;
using IML.Parsing.AST;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
{
    public class LambdaAst : Ast
    {
        public List<AstParameter> Parameters { get; private set; }
        public List<Ast> Body { get; private set; }
        public AstType ReturnType { get; private set; }
        public bool IsPure { get; private set; }
        public bool CreatesEnv { get; private set; }
        public bool IsLastVarArgs { get; private set; }
        public List<string> GenericNames { get; private set; }

        public LambdaAst(List<AstParameter> parameters, List<Ast> body, AstType returnType, bool pure, bool createsEnv,
            bool isLastVarArgs, List<string> generics)
            : base(AstTypes.LambdaLiteral)
        {
            Parameters = parameters;
            Body = body;
            ReturnType = returnType;
            IsPure = pure;
            CreatesEnv = createsEnv;
            IsLastVarArgs = isLastVarArgs;
            GenericNames = generics;
        }
        public LambdaAst(List<AstParameter> parameters, Ast body, AstType returnType, bool pure, bool createsEnv,
            bool isLastVarArgs, List<string> generics)
            : this(parameters, new List<Ast>() { body }, returnType, pure, createsEnv, isLastVarArgs, generics)
        {
        }
    }
}
