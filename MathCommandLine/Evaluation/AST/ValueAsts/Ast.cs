using IML.CoreDataTypes;
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

        public Ast(AstTypes type)
        {
            Type = type;
        }

        #region Constructor Functions

        public static NumberAst NumberLiteral(double value)
        {
            return new NumberAst(value);
        }
        public static ListAst ListLiteral(Ast[] elements)
        {
            return new ListAst(new List<Ast>(elements));
        }
        public static LambdaAst LambdaLiteral(AstParameter[] parameters, List<Ast> body, AstType returnType, 
            bool createsEnv, bool isPure, bool isLastVarArgs, List<string> generics)
        {
            return new LambdaAst(new List<AstParameter>(parameters), body, returnType, isPure, createsEnv, 
                isLastVarArgs, generics);
        }
        public static StringAst StringLiteral(string text)
        {
            return new StringAst(text);
        }
        public static ReferenceAst ReferenceLiteral(string refName)
        {
            return new ReferenceAst(refName);
        }
        public static CallAst Call(Ast calledAst, params Ast[] args)
        {
            return new CallAst(calledAst, new List<Ast>(args));
        }
        public static VariableAst Variable(string name)
        {
            return new VariableAst(name);
        }
        public static VariableDeclarationAst VariableDeclaration(string name, Ast value, VariableType type)
        {
            return new VariableDeclarationAst(name, value, type);
        }
        public static VariableAssignmentAst VariableAssignment(IdentifierAst identifier, Ast value)
        {
            return new VariableAssignmentAst(identifier, value);
        }
        public static MemberAccessAst MemberAccess(Ast parent, string name)
        {
            return new MemberAccessAst(parent, name);
        }
        public static ReturnAst Return(Ast body)
        {
            return new ReturnAst(body);
        }
        public static InvalidAst Invalid(string expr)
        {
            return new InvalidAst(expr);
        }

        #endregion
    }
}
