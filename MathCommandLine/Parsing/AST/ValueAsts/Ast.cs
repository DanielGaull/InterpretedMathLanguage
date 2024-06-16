using IML.CoreDataTypes;
using IML.Environments;
using IML.Functions;
using IML.Parsing.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
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
        public static CallAst Call(Ast calledAst, List<Ast> args, List<AstType> providedGenerics)
        {
            return new CallAst(calledAst, args, providedGenerics);
        }
        public static VariableAst Variable(string name)
        {
            return new VariableAst(name);
        }
        public static VariableDeclarationAst VariableDeclaration(string name, Ast value, VariableType type, AstType varValType)
        {
            return new VariableDeclarationAst(name, value, type, varValType);
        }
        public static VariableAssignmentAst VariableAssignment(IdentifierAst identifier, Ast value)
        {
            return new VariableAssignmentAst(identifier, value);
        }
        public static MemberAccessAst MemberAccess(Ast parent, string name)
        {
            return new MemberAccessAst(parent, name);
        }
        public static ReturnAst Return(Ast body, bool returnVoid)
        {
            return new ReturnAst(body, returnVoid);
        }

        #endregion
    }
}
