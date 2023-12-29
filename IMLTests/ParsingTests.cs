using IML.CoreDataTypes;
using IML.Evaluation;
using IML.Evaluation.AST.ValueAsts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    public class ParsingTests
    {
        private static Parser parser;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            Parser parser = new Parser();

            ParsingTests.parser = parser;
        }

        [TestMethod]
        public void TestReturn()
        {
            Ast ast = parser.Parse("return 5", new VariableAstTypeMap());
            Assert.AreEqual(AstTypes.Return, ast.Type);
            ReturnAst rast = (ReturnAst)ast;
            Assert.AreEqual(AstTypes.NumberLiteral, rast.Body.Type);
            Assert.AreEqual(5, ((NumberAst)rast.Body).Value);
        }
    }
}
