using IML.Environments;
using IML.Evaluation.AST.ValueAsts;
using IML.Functions;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Evaluation
{
    public class Ast
    {
        public AstTypes Type { get; private set; }
        //// For number literals
        //public double NumberArg { get; private set; }
        //// Used for invalid ASTs (and internally for string ones, but we also have another getter for it
        //public string Expression { get; private set; }
        //// Used for lambdas, and the right-hand expression for assignment/declarations
        //public Ast Body { get; set; }
        //// Used for lambdas
        //public bool CreatesEnv { get; set; }
        //// For lambdas
        //public AstParameter[] Parameters { get; private set; }
        //// Array of ASTs, used for: Arguments -> Functions, Elements -> Lists
        //public Ast[] AstCollectionArg { get; private set; }
        //// Used for Variable, ReferenceLiteral types, declaration, member access
        //public string Name { get; private set; }
        //// Used for assignment
        //public IdentifierAst Identifier { get; private set; }
        //// Used for Calls & Dot access
        //public Ast ParentAst { get; private set; }
        //// Used for Var Declarations
        //// Casted to & from enum
        //public int EnumArg { get; private set; }

        // Wrappers around existing properties, for readability
        // Used for strings (just a wrapper around Expression)
        //public string StringArg
        //{
        //    get
        //    {
        //        return Expression;
        //    }
        //}
        //public VariableType VariableType
        //{
        //    get
        //    {
        //        return (VariableType)EnumArg;
        //    }
        //}


        public Ast(AstTypes type)
        {
            Type = type;
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
         * VariableAssignment: Identifier, Body
         * MemberAccess: ParentAst, Name
         * Invalid: Expression
         */

        //public Ast(AstTypes type, double numberArg, Ast[] astCollectionArg, string expression, AstParameter[] parameters,
        //    string name, Ast parentAst, Ast body, bool createsEnv, int enumArg, 
        //    IdentifierAst identifier)
        //{
        //    Type = type;
        //    NumberArg = numberArg;
        //    AstCollectionArg = astCollectionArg;
        //    Expression = expression;
        //    Parameters = parameters;
        //    Name = name;
        //    ParentAst = parentAst;
        //    Body = body;
        //    CreatesEnv = createsEnv;
        //    EnumArg = enumArg;
        //    Identifier = identifier;
        //}

        #region Constructor Functions

        public static NumberAst NumberLiteral(double value)
        {
            return new NumberAst(value);
            //return new Ast(AstTypes.NumberLiteral, value, null, null, null, null, null, null, false, -1, null);
        }
        public static ListAst ListLiteral(Ast[] elements)
        {
            return new ListAst(new List<Ast>(elements));
            //return new Ast(AstTypes.ListLiteral, 0, elements, null, null, null, null, null, false, -1, null);
        }
        public static Ast LambdaLiteral(AstParameter[] parameters, Ast body, bool createsEnv)
        {
            //return new Ast(AstTypes.LambdaLiteral, 0, null, null, parameters, null, null, body, createsEnv, -1, null);
        }
        public static StringAst StringLiteral(string text)
        {
            return new StringAst(text);
            //return new Ast(AstTypes.StringLiteral, 0, null, text, null, null, null, null, false, -1, null);
        }
        public static ReferenceAst ReferenceLiteral(string refName)
        {
            return new ReferenceAst(refName);
            //return new Ast(AstTypes.ReferenceLiteral, 0, null, null, null, refName, null, null, false, -1, null);
        }
        public static CallAst Call(Ast calledAst, params Ast[] args)
        {
            return new CallAst(calledAst, new List<Ast>(args));
            //return new Ast(AstTypes.Call, 0, args, null, null, null, calledAst, null, false, -1, null);
        }
        public static VariableAst Variable(string name)
        {
            return new VariableAst(name);
            //return new Ast(AstTypes.Variable, 0, null, null, null, name, null, null, false, -1, null);
        }
        public static VariableDeclarationAst VariableDeclaration(string name, Ast value, VariableType type)
        {
            return new VariableDeclarationAst(name, value, type);
            //return new Ast(AstTypes.VariableDeclaration, 0, null, null, null, name, null, value, false, (int) type, null);
        }
        public static VariableAssignmentAst VariableAssignment(IdentifierAst identifier, Ast value)
        {
            return new VariableAssignmentAst(identifier, value);
            //return new Ast(AstTypes.VariableAssignment, 0, null, null, null, null, null, value, false, -1, identifier);
        }
        public static MemberAccessAst MemberAccess(Ast parent, string name)
        {
            return new MemberAccessAst(parent, name);
            //return new Ast(AstTypes.MemberAccess, 0, null, null, null, name, parent, null, false, -1, null);
        }
        public static InvalidAst Invalid(string expr)
        {
            return new InvalidAst(expr);
            //return new Ast(AstTypes.Invalid, 0, null, expr, null, null, null, null, false, -1, null);
        }

        #endregion
    }
}
