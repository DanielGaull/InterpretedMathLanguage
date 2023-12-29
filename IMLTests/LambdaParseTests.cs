using IML.Evaluation;
using IML.Evaluation.AST.ValueAsts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    public class LambdaParseTests
    {
        private static Parser parser;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            Parser parser = new Parser();

            LambdaParseTests.parser = parser;
        }

        [TestMethod]
        public void TestSimple()
        {
            Ast ast = parser.Parse("()=>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            Assert.AreEqual(true, last.CreatesEnv);
            Assert.AreEqual(0, last.Parameters.Count);
            List<Ast> body = last.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body[0].Type);
            Assert.AreEqual(5, ((NumberAst)body[0]).Value);
        }

        [TestMethod]
        public void TestSimpleEnvironmentless()
        {
            Ast ast = parser.Parse("()~>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            Assert.AreEqual(false, last.CreatesEnv);
            Assert.AreEqual(0, last.Parameters.Count);
            List<Ast> body = last.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body[0].Type);
            Assert.AreEqual(5, ((NumberAst)body[0]).Value);
        }

        [TestMethod]
        public void TestSingleArg()
        {
            Ast ast = parser.Parse("(x)=>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            Assert.AreEqual(true, last.CreatesEnv);
            Assert.AreEqual(1, last.Parameters.Count);
            AstParameter p = last.Parameters[0];
            Assert.AreEqual("x", p.Name);
            Assert.AreEqual(AstType.Any, p.Type);
            List<Ast> body = last.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body[0].Type);
            Assert.AreEqual(5, ((NumberAst)body[0]).Value);
        }

        [TestMethod]
        public void TestSingleArgWithType()
        {
            Ast ast = parser.Parse("(x:number)=>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            AstParameter p = last.Parameters[0];
            Assert.AreEqual("x", p.Name);
            AstType type = p.Type;
            Assert.AreEqual(1, type.Entries.Count);
            AstTypeEntry entry = type.Entries[0];
            Assert.AreEqual("number", entry.DataTypeName);
            Assert.AreEqual(0, entry.Generics.Count);
        }

        [TestMethod]
        public void TestSingleArgWithTypeGeneric()
        {
            Ast ast = parser.Parse("(x:list[number])=>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            AstParameter p = last.Parameters[0];
            Assert.AreEqual("x", p.Name);
            AstType type = p.Type;
            Assert.AreEqual(1, type.Entries.Count);
            AstTypeEntry entry = type.Entries[0];
            Assert.AreEqual("list", entry.DataTypeName);
            Assert.AreEqual(1, entry.Generics.Count);
            AstType generic = entry.Generics[0];
            Assert.AreEqual(1, generic.Entries.Count);
            AstTypeEntry genericEntry = generic.Entries[0];
            Assert.AreEqual("number", genericEntry.DataTypeName);
            Assert.AreEqual(0, genericEntry.Generics.Count);
        }

        [TestMethod]
        public void TestDoubleArgWithTypeGeneric()
        {
            Ast ast = parser.Parse("(x:list[number],y:list[string])=>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            Assert.AreEqual(true, last.CreatesEnv);
            Assert.AreEqual(2, last.Parameters.Count);

            AstParameter p = last.Parameters[0];
            Assert.AreEqual("x", p.Name);
            AstType type = p.Type;
            Assert.AreEqual(1, type.Entries.Count);
            AstTypeEntry entry = type.Entries[0];
            Assert.AreEqual("list", entry.DataTypeName);
            Assert.AreEqual(1, entry.Generics.Count);
            AstType generic = entry.Generics[0];
            Assert.AreEqual(1, generic.Entries.Count);
            AstTypeEntry genericEntry = generic.Entries[0];
            Assert.AreEqual("number", genericEntry.DataTypeName);
            Assert.AreEqual(0, genericEntry.Generics.Count);

            AstParameter p2 = last.Parameters[1];
            Assert.AreEqual("y", p2.Name);
            AstType type2 = p2.Type;
            Assert.AreEqual(1, type2.Entries.Count);
            AstTypeEntry entry2 = type2.Entries[0];
            Assert.AreEqual("list", entry2.DataTypeName);
            Assert.AreEqual(1, entry2.Generics.Count);
            AstType generic2 = entry2.Generics[0];
            Assert.AreEqual(1, generic2.Entries.Count);
            AstTypeEntry genericEntry2 = generic2.Entries[0];
            Assert.AreEqual("string", genericEntry2.DataTypeName);
            Assert.AreEqual(0, genericEntry2.Generics.Count);

            List<Ast> body = last.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body[0].Type);
            Assert.AreEqual(5, ((NumberAst)body[0]).Value);
        }

        [TestMethod]
        public void TestFunctionGenericDoubleArgGenerics()
        {
            Ast ast = parser.Parse("[T](x:list[T],y:list[string])=>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            Assert.AreEqual(true, last.CreatesEnv);
            Assert.AreEqual(2, last.Parameters.Count);
            Assert.AreEqual(1, last.GenericNames.Count);
            Assert.AreEqual("T", last.GenericNames[0]);

            AstParameter p = last.Parameters[0];
            Assert.AreEqual("x", p.Name);
            AstType type = p.Type;
            Assert.AreEqual(1, type.Entries.Count);
            AstTypeEntry entry = type.Entries[0];
            Assert.AreEqual("list", entry.DataTypeName);
            Assert.AreEqual(1, entry.Generics.Count);
            AstType generic = entry.Generics[0];
            Assert.AreEqual(1, generic.Entries.Count);
            AstTypeEntry genericEntry = generic.Entries[0];
            Assert.AreEqual("T", genericEntry.DataTypeName);
            Assert.AreEqual(0, genericEntry.Generics.Count);

            AstParameter p2 = last.Parameters[1];
            Assert.AreEqual("y", p2.Name);
            AstType type2 = p2.Type;
            Assert.AreEqual(1, type2.Entries.Count);
            AstTypeEntry entry2 = type2.Entries[0];
            Assert.AreEqual("list", entry2.DataTypeName);
            Assert.AreEqual(1, entry2.Generics.Count);
            AstType generic2 = entry2.Generics[0];
            Assert.AreEqual(1, generic2.Entries.Count);
            AstTypeEntry genericEntry2 = generic2.Entries[0];
            Assert.AreEqual("string", genericEntry2.DataTypeName);
            Assert.AreEqual(0, genericEntry2.Generics.Count);

            List<Ast> body = last.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body[0].Type);
            Assert.AreEqual(5, ((NumberAst)body[0]).Value);
        }

        [TestMethod]
        public void TestFunctionDoubleGenericDoubleArgGenerics()
        {
            Ast ast = parser.Parse("[T,S](x:list[T],y:list[S])=>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            Assert.AreEqual(true, last.CreatesEnv);
            Assert.AreEqual(2, last.Parameters.Count);
            Assert.AreEqual(2, last.GenericNames.Count);
            Assert.AreEqual("T", last.GenericNames[0]);
            Assert.AreEqual("S", last.GenericNames[1]);

            AstParameter p = last.Parameters[0];
            Assert.AreEqual("x", p.Name);
            AstType type = p.Type;
            Assert.AreEqual(1, type.Entries.Count);
            AstTypeEntry entry = type.Entries[0];
            Assert.AreEqual("list", entry.DataTypeName);
            Assert.AreEqual(1, entry.Generics.Count);
            AstType generic = entry.Generics[0];
            Assert.AreEqual(1, generic.Entries.Count);
            AstTypeEntry genericEntry = generic.Entries[0];
            Assert.AreEqual("T", genericEntry.DataTypeName);
            Assert.AreEqual(0, genericEntry.Generics.Count);

            AstParameter p2 = last.Parameters[1];
            Assert.AreEqual("y", p2.Name);
            AstType type2 = p2.Type;
            Assert.AreEqual(1, type2.Entries.Count);
            AstTypeEntry entry2 = type2.Entries[0];
            Assert.AreEqual("list", entry2.DataTypeName);
            Assert.AreEqual(1, entry2.Generics.Count);
            AstType generic2 = entry2.Generics[0];
            Assert.AreEqual(1, generic2.Entries.Count);
            AstTypeEntry genericEntry2 = generic2.Entries[0];
            Assert.AreEqual("S", genericEntry2.DataTypeName);
            Assert.AreEqual(0, genericEntry2.Generics.Count);

            List<Ast> body = last.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body[0].Type);
            Assert.AreEqual(5, ((NumberAst)body[0]).Value);
        }

        [TestMethod]
        public void TestFunctionDoubleGenericDoubleArgGenericsMaxWhitespace()
        {
            Ast ast = parser.Parse("[ T , S ] (x : list [ T ] , y : list [ S ] ) => { 5 }");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            LambdaAst last = (LambdaAst)ast;
            Assert.AreEqual(true, last.CreatesEnv);
            Assert.AreEqual(2, last.Parameters.Count);
            Assert.AreEqual(2, last.GenericNames.Count);
            Assert.AreEqual("T", last.GenericNames[0]);
            Assert.AreEqual("S", last.GenericNames[1]);

            AstParameter p = last.Parameters[0];
            Assert.AreEqual("x", p.Name);
            AstType type = p.Type;
            Assert.AreEqual(1, type.Entries.Count);
            AstTypeEntry entry = type.Entries[0];
            Assert.AreEqual("list", entry.DataTypeName);
            Assert.AreEqual(1, entry.Generics.Count);
            AstType generic = entry.Generics[0];
            Assert.AreEqual(1, generic.Entries.Count);
            AstTypeEntry genericEntry = generic.Entries[0];
            Assert.AreEqual("T", genericEntry.DataTypeName);
            Assert.AreEqual(0, genericEntry.Generics.Count);

            AstParameter p2 = last.Parameters[1];
            Assert.AreEqual("y", p2.Name);
            AstType type2 = p2.Type;
            Assert.AreEqual(1, type2.Entries.Count);
            AstTypeEntry entry2 = type2.Entries[0];
            Assert.AreEqual("list", entry2.DataTypeName);
            Assert.AreEqual(1, entry2.Generics.Count);
            AstType generic2 = entry2.Generics[0];
            Assert.AreEqual(1, generic2.Entries.Count);
            AstTypeEntry genericEntry2 = generic2.Entries[0];
            Assert.AreEqual("S", genericEntry2.DataTypeName);
            Assert.AreEqual(0, genericEntry2.Generics.Count);

            List<Ast> body = last.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body[0].Type);
            Assert.AreEqual(5, ((NumberAst)body[0]).Value);
        }
    }
}
