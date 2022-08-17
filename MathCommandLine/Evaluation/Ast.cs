using MathCommandLine.Functions;
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
        // For type literals
        public MDataType DataType { get; private set; }

        // For parameters
        // TODO: Remove parameter AST type. As a rule, all ASTs should be able to evaluate to an MValue
        public MDataType[] AcceptedTypes { get; private set; }
        public ParamRequirement[] ParamRequirements { get; private set; }

        // Array of ASTs, used for: Arguments -> Functions, Parameters -> Lambdas, Elements -> Lists
        public Ast[] AstCollectionArg { get; private set; }
        // Used for Function, Variable, Parameter types
        public string Name { get; private set; }

        public Ast(AstTypes type, double numberArg, Ast[] astCollectionArg, Ast expression, MDataType dataType, MDataType[] acceptedTypes, 
            ParamRequirement[] paramRequirements, string name)
        {
            Type = type;
            NumberArg = numberArg;
            AstCollectionArg = astCollectionArg;
            Expression = expression;
            DataType = dataType;
            AcceptedTypes = acceptedTypes;
            ParamRequirements = paramRequirements;
            Name = name;
        }

        public static Ast NumberLiteral(double value)
        {
            return new Ast(AstTypes.NumberLiteral, value, null, null, MDataType.Empty, null, null, null);
        }
        public static Ast ListLiteral(Ast[] elements)
        {
            return new Ast(AstTypes.ListLiteral, 0, elements, null, MDataType.Empty, null, null, null);
        }
        public static Ast LambdaLiteral(Ast[] parameters, Ast expression)
        {
            return new Ast(AstTypes.LambdaLiteral, 0, parameters, expression, MDataType.Empty, null, null, null);
        }
        public static Ast TypeLiteral(MDataType type)
        {
            return new Ast(AstTypes.TypeLiteral, 0, null, null, type, null, null, null);
        }
        public static Ast Parameter(MDataType[] acceptedTypes, ParamRequirement[] requirements, string name)
        {
            return new Ast(AstTypes.Parameter, 0, null, null, MDataType.Empty, acceptedTypes, requirements, name);
        }
        public static Ast Function(string name, params Ast[] args)
        {
            return new Ast(AstTypes.Parameter, 0, args, null, MDataType.Empty, null, null, name);
        }
        public static Ast Variable(string name)
        {
            return new Ast(AstTypes.NumberLiteral, 0, null, null, MDataType.Empty, null, null, name);
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
