﻿using IML.CoreDataTypes;
using IML.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Evaluation.AST.ValueAsts
{
    public class LambdaAst : Ast
    {
        public List<AstParameter> Parameters { get; private set; }
        public List<Ast> Body { get; private set; }
        public AstType ReturnType { get; private set; }
        public bool IsPure { get; private set; }
        public bool CreatesEnv { get; private set; }
        
        public LambdaAst(List<AstParameter> parameters, List<Ast> body, AstType returnType, bool pure, bool createsEnv)
            : base(AstTypes.LambdaLiteral)
        {
            Parameters = parameters;
            Body = body;
            ReturnType = returnType;
            IsPure = pure;
            CreatesEnv = createsEnv;
        }
        public LambdaAst(List<AstParameter> parameters, Ast body, AstType returnType, bool pure, bool createsEnv)
            : this(parameters, new List<Ast>() { body }, returnType, pure, createsEnv)
        {
        }
    }
}