using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class Ast
    {
        public AstTypes Type { get; private set; }
        // Used for Literal types
        // For number literals
        public double NumberArg { get; private set; }
        public string Expression { get; private set; }
        // For lambdas
        public AstParameter[] Parameters { get; private set; }
        // Array of ASTs, used for: Arguments -> Functions, Elements -> Lists
        public Ast[] AstCollectionArg { get; private set; }
        // Used for Variable & TypeLiteral types
        public string Name { get; private set; }
        // Used for Calls
        public Ast CalledAst { get; private set; }

        /* Used:
         * NumberLiteral: NumberArg
         * ListLiteral: AstCollectionArg
         * LambdaLiteral: Parameters, Expression
         * TypeLiteral: Name
         * Variable: Name
         * Call: CalledAst, AstCollectionArg
         */

        public Ast(AstTypes type, double numberArg, Ast[] astCollectionArg, string expression, AstParameter[] parameters,
            string name, Ast calledAst)
        {
            Type = type;
            NumberArg = numberArg;
            AstCollectionArg = astCollectionArg;
            Expression = expression;
            Parameters = parameters;
            Name = name;
            CalledAst = calledAst;
        }

        #region Constructor Functions

        public static Ast NumberLiteral(double value)
        {
            return new Ast(AstTypes.NumberLiteral, value, null, null, null, null, null);
        }
        public static Ast ListLiteral(Ast[] elements)
        {
            return new Ast(AstTypes.ListLiteral, 0, elements, null, null, null, null);
        }
        public static Ast LambdaLiteral(AstParameter[] parameters, string expression)
        {
            return new Ast(AstTypes.LambdaLiteral, 0, null, expression, parameters, null, null);
        }
        public static Ast Call(Ast calledAst, params Ast[] args)
        {
            return new Ast(AstTypes.Call, 0, args, null, null, null, calledAst);
        }
        public static Ast Variable(string name)
        {
            return new Ast(AstTypes.Variable, 0, null, null, null, name, null);
        }

        #endregion
    }

    public class AstParameter
    {
        public AstParameterTypeEntry[] TypeEntries { get; private set; }
        public string Name { get; private set; }

        public AstParameter(string name, params AstParameterTypeEntry[] typeEntries)
        {
            Name = name;
            TypeEntries = typeEntries;
        }
        public AstParameter(string name, params string[] dataTypeNames)
            : this(name, dataTypeNames.Select(x => new AstParameterTypeEntry(x)).ToArray())
        {

        }
    }
    public class AstParameterTypeEntry
    {
        public string DataTypeName { get; private set; }
        public ValueRestriction[] ValueRestrictions { get; private set; }

        public AstParameterTypeEntry(string dataTypeName, ValueRestriction[] valueRestrictions)
        {
            DataTypeName = dataTypeName;
            ValueRestrictions = valueRestrictions;
        }
        public AstParameterTypeEntry(string dataTypeName)
            : this(dataTypeName, new ValueRestriction[0])
        {
        }
    }

    public enum AstTypes
    {
        NumberLiteral,
        ListLiteral,
        LambdaLiteral,
        Variable,
        Call,
    }
}
