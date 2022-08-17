﻿using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class Ast
    {
        public AstTypes Type { get; private set; }
        // Used for Literal types
        // For number literals
        public double NumberArg { get; private set; }
        public Ast Expression { get; private set; }
        // For lambdas
        public AstParameter[] Parameters { get; private set; }

        // Array of ASTs, used for: Arguments -> Functions, Elements -> Lists
        public Ast[] AstCollectionArg { get; private set; }
        // Used for Function, Variable, TypeLiteral types
        public string Name { get; private set; }

        public Ast(AstTypes type, double numberArg, Ast[] astCollectionArg, Ast expression, AstParameter[] parameters, string name)
        {
            Type = type;
            NumberArg = numberArg;
            AstCollectionArg = astCollectionArg;
            Expression = expression;
            Parameters = parameters;
            Name = name;
        }

        public static Ast NumberLiteral(double value)
        {
            return new Ast(AstTypes.NumberLiteral, value, null, null, null, null);
        }
        public static Ast ListLiteral(Ast[] elements)
        {
            return new Ast(AstTypes.ListLiteral, 0, elements, null, null, null);
        }
        public static Ast LambdaLiteral(AstParameter[] parameters, Ast expression)
        {
            return new Ast(AstTypes.LambdaLiteral, 0, null, expression, parameters, null);
        }
        public static Ast TypeLiteral(string typeName)
        {
            return new Ast(AstTypes.TypeLiteral, 0, null, null, null, typeName);
        }
        public static Ast Function(string name, params Ast[] args)
        {
            return new Ast(AstTypes.Parameter, 0, args, null, null, name);
        }
        public static Ast Variable(string name)
        {
            return new Ast(AstTypes.NumberLiteral, 0, null, null, null, name);
        }
    }

    public class AstParameter
    {
        public MDataType[] AcceptedTypes { get; private set; }
        public ParamRequirement[] ParamRequirements { get; private set; }

        public AstParameter(MDataType[] acceptedTypes, ParamRequirement[] paramRequirements)
        {
            AcceptedTypes = acceptedTypes;
            ParamRequirements = paramRequirements;
        }
    }

    public enum AstTypes
    {
        NumberLiteral,
        ListLiteral,
        LambdaLiteral,
        TypeLiteral,
        Function,
        Variable,
        Parameter,
    }
}
