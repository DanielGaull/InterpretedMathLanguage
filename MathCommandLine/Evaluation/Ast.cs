using MathCommandLine.Environments;
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
        // Used for lambdas, and the right-hand expression for assignment/declarations
        public Ast Body { get; set; }
        // Used for lambdas
        public bool CreatesEnv { get; set; }
        // For lambdas
        public AstParameter[] Parameters { get; private set; }
        // Array of ASTs, used for: Arguments -> Functions, Elements -> Lists
        public Ast[] AstCollectionArg { get; private set; }
        // Used for Variable, ReferenceLiteral types, declaration/assignments, member access
        public string Name { get; private set; }
        // Used for Calls & Dot access
        public Ast ParentAst { get; private set; }
        // Used for Var Declarations
        // Casted to & from enum
        public int EnumArg { get; private set; }

        // Wrappers around existing properties, for readability
        // Used for strings (just a wrapper around Expression)
        public string StringArg
        {
            get
            {
                return Expression;
            }
        }
        public VariableType VariableType
        {
            get
            {
                return (VariableType)EnumArg;
            }
        }
        

        /* Used:
         * NumberLiteral: NumberArg
         * ListLiteral: AstCollectionArg
         * LambdaLiteral: Parameters, Body
         * StringLiteral: Expression (StringArg)
         * ReferenceLiteral: Name
         * Variable: Name
         * Call: ParentAst, AstCollectionArg
         * VariableDeclaration: Name, Body, EnumArg (VariableType)
         * VariableAssignment: Name, Body
         * MemberAccess: ParentAst, Name
         * Invalid: Expression
         */

        public Ast(AstTypes type, double numberArg, Ast[] astCollectionArg, string expression, AstParameter[] parameters,
            string name, Ast parentAst, Ast body, bool createsEnv, int enumArg)
        {
            Type = type;
            NumberArg = numberArg;
            AstCollectionArg = astCollectionArg;
            Expression = expression;
            Parameters = parameters;
            Name = name;
            ParentAst = parentAst;
            Body = body;
            CreatesEnv = createsEnv;
            EnumArg = enumArg;
        }

        #region Constructor Functions

        public static Ast NumberLiteral(double value)
        {
            return new Ast(AstTypes.NumberLiteral, value, null, null, null, null, null, null, false, -1);
        }
        public static Ast ListLiteral(Ast[] elements)
        {
            return new Ast(AstTypes.ListLiteral, 0, elements, null, null, null, null, null, false, -1);
        }
        public static Ast LambdaLiteral(AstParameter[] parameters, Ast body, bool createsEnv)
        {
            return new Ast(AstTypes.LambdaLiteral, 0, null, null, parameters, null, null, body, createsEnv, -1);
        }
        public static Ast StringLiteral(string text)
        {
            return new Ast(AstTypes.StringLiteral, 0, null, text, null, null, null, null, false, -1);
        }
        public static Ast ReferenceLiteral(string refName)
        {
            return new Ast(AstTypes.ReferenceLiteral, 0, null, null, null, refName, null, null, false, -1);
        }
        public static Ast Call(Ast calledAst, params Ast[] args)
        {
            return new Ast(AstTypes.Call, 0, args, null, null, null, calledAst, null, false, -1);
        }
        public static Ast Variable(string name)
        {
            return new Ast(AstTypes.Variable, 0, null, null, null, name, null, null, false, -1);
        }
        public static Ast VariableDeclaration(string name, Ast value, VariableType type)
        {
            return new Ast(AstTypes.VariableDeclaration, 0, null, null, null, name, null, value, false, (int) type);
        }
        public static Ast VariableAssignment(string name, Ast value)
        {
            return new Ast(AstTypes.VariableAssignment, 0, null, null, null, name, null, value, false, -1);
        }
        public static Ast MemberAccess(Ast parent, string name)
        {
            return new Ast(AstTypes.MemberAccess, 0, null, null, null, name, parent, null, false, -1);
        }
        public static Ast Invalid(string expr)
        {
            return new Ast(AstTypes.Invalid, 0, null, expr, null, null, null, null, false, -1);
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
        // A literal number, such as 5, -10.2, or 0.6
        NumberLiteral,
        // A literal list, in the format of { [element0], [element1], [element2], ... }
        ListLiteral,
        // A literal lamda, in the format of ([params])=>{[body]}
        LambdaLiteral,
        // A literal string, in the format of "[text]"
        StringLiteral,
        // A literal reference, in the format of "&[varname]"
        ReferenceLiteral,
        // A literal variable, which must be made up of valid characters (A-Z, a-z, 0-9, _)
        // Can only start with a letter
        Variable,
        // Used when accessing a member of an object
        // Ex. "str".chars
        MemberAccess,
        // An instance in which an invokation is being performed on some other object
        // i.e. _add(1, 2)
        // Format is: [callee]([arg0],[arg1],...)
        Call,
        // A variable declaration
        // TODO: Add in type declarations to variables
        // Example: var x: number = 7
        VariableDeclaration,
        // A variable assignment
        // May or may not include a binary operator, too
        // Example: x = 7; x += 5; b &&= a || c;
        VariableAssignment,
        // Anything that doesn't fit into the above definitions
        // Used in syntax handling, but if it appears when parsing some final expression,
        // then there's an error
        Invalid,
    }
}
