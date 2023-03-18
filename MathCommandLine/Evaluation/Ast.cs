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
        // For number literals
        public double NumberArg { get; private set; }
        // Used for invalid ASTs (and internally for string ones, but we also have another getter for it
        public string Expression { get; private set; }
        // Used for lambdas
        public Ast Body { get; set; }
        // Used for lambdas
        public bool CreatesEnv { get; set; }
        // For lambdas
        public AstParameter[] Parameters { get; private set; }
        // Array of ASTs, used for: Arguments -> Functions, Elements -> Lists
        public Ast[] AstCollectionArg { get; private set; }
        // Used for Variable & TypeLiteral types
        public string Name { get; private set; }
        // Used for Calls
        public Ast CalledAst { get; private set; }
        // Used for strings (just a wrapper around Expression)
        public string StringArg
        {
            get
            {
                return Expression;
            }
        }

        /* Used:
         * NumberLiteral: NumberArg
         * ListLiteral: AstCollectionArg
         * LambdaLiteral: Parameters, Body
         * StringLiteral: Expression (StringArg)
         * Variable: Name
         * Call: CalledAst, AstCollectionArg
         * Invalid: Expression
         */

        public Ast(AstTypes type, double numberArg, Ast[] astCollectionArg, string expression, AstParameter[] parameters,
            string name, Ast calledAst, Ast body, bool createsEnv)
        {
            Type = type;
            NumberArg = numberArg;
            AstCollectionArg = astCollectionArg;
            Expression = expression;
            Parameters = parameters;
            Name = name;
            CalledAst = calledAst;
            Body = body;
            CreatesEnv = createsEnv;
        }

        #region Constructor Functions

        public static Ast NumberLiteral(double value)
        {
            return new Ast(AstTypes.NumberLiteral, value, null, null, null, null, null, null, false);
        }
        public static Ast ListLiteral(Ast[] elements)
        {
            return new Ast(AstTypes.ListLiteral, 0, elements, null, null, null, null, null, false);
        }
        public static Ast LambdaLiteral(AstParameter[] parameters, Ast body, bool createsEnv)
        {
            return new Ast(AstTypes.LambdaLiteral, 0, null, null, parameters, null, null, body, createsEnv);
        }
        public static Ast StringLiteral(string text)
        {
            return new Ast(AstTypes.StringLiteral, 0, null, text, null, null, null, null, false);
        }
        public static Ast Call(Ast calledAst, params Ast[] args)
        {
            return new Ast(AstTypes.Call, 0, args, null, null, null, calledAst, null, false);
        }
        public static Ast Variable(string name)
        {
            return new Ast(AstTypes.Variable, 0, null, null, null, name, null, null, false);
        }
        public static Ast Invalid(string expr)
        {
            return new Ast(AstTypes.Invalid, 0, null, expr, null, null, null, null, false);
        }

        #endregion

        public string ToExpressionString()
        {
            switch (Type) 
            {
                case AstTypes.NumberLiteral:
                    return NumberArg.ToString();
                case AstTypes.ListLiteral:
                    return "{" + string.Join(',', AstCollectionArg.Select(x => x.ToExpressionString()).ToArray()) + "}";
                case AstTypes.LambdaLiteral:
                    return "(" + string.Join(',', Parameters.Select(x => x.ToString()).ToArray()) + ")" + 
                        (CreatesEnv ? "=>" : "~>") + "{" + 
                        Body.ToExpressionString() + "}";
                case AstTypes.Variable:
                    return Name;
                case AstTypes.Call:
                    return CalledAst.ToExpressionString() + "(" + 
                        string.Join(',', AstCollectionArg.Select(x => x.ToExpressionString()).ToArray())
                        + ")";
                case AstTypes.StringLiteral:
                    return "\"" + StringArg + "\"";
                case AstTypes.Invalid:
                    return Expression;
            }
            return "";
        }
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

        public override string ToString()
        {
            // Ex)
            // my_var_name:[>(0)]number|list
            return Name + ":" + string.Join('|', TypeEntries.Select(x => x.ToString()).ToArray());
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

        public override string ToString()
        {
            return "[" + string.Join(',', ValueRestrictions) + "]" + DataTypeName;
        }
    }

    public enum AstTypes
    {
        // A literal number, such as 5, -10.2, or 0.6
        NumberLiteral,
        // A literal list, in the format of { [element0], [element1], [element2], ... }
        ListLiteral,
        // A literal lamda, in the format of ([params])=>{[body]}
        LambdaLiteral,
        // A literal string, in the format of "[text]"
        StringLiteral,
        // A literal variable, which must be made up of valid characters (A-Z, a-z, 0-9, _)
        // Can only start with a letter
        Variable,
        // An instance in which an invokation is being performed on some other object
        // i.e. _add(1, 2)
        // Format is: [callee]([arg0],[arg1],...)
        Call,
        // Anything that doesn't fit into the above definitions
        // Used in syntax handling, but if it appears when parsing some final expression,
        // then there's an error
        Invalid,
    }
}
