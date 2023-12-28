using IML.Evaluation;
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
            Assert.AreEqual(true, ast.CreatesEnv);
            Assert.AreEqual(0, ast.Parameters.Length);
            Ast body = ast.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body.Type);
            Assert.AreEqual(5, body.NumberArg);
        }

        [TestMethod]
        public void TestSimpleEnvironmentlessLambda()
        {
            Ast ast = parser.Parse("()~>{5}");
            Assert.AreEqual(AstTypes.LambdaLiteral, ast.Type);
            Assert.AreEqual(false, ast.CreatesEnv);
            Assert.AreEqual(0, ast.Parameters.Length);
            Ast body = ast.Body;
            Assert.AreEqual(AstTypes.NumberLiteral, body.Type);
            Assert.AreEqual(5, body.NumberArg);
        }
    }
}
