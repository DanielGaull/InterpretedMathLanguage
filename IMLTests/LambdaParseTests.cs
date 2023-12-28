using IML.Evaluation;
using IML.Evaluation.AST.ValueAsts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    class LambdaParseTests
    {
        private static Parser parser;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            Parser parser = new Parser();

            LambdaParseTests.parser = parser;
        }

        [TestMethod]
        public void TestSimpleLambda()
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
        public void TestSimpleEnvironmentlessLambda()
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
    }
}
