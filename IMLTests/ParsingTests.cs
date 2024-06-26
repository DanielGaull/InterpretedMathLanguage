﻿using IML.CoreDataTypes;
using IML.Evaluation;
using IML.Parsing;
using IML.Parsing.AST;
using IML.Parsing.AST.ValueAsts;
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
            VariableAstTypeMap baseTypeMap = InterpreterHelper.CreateBaseTypeMap();

            Parser parser = new Parser(baseTypeMap);

            ParsingTests.parser = parser;
        }

        [TestMethod]
        public void TestReturn()
        {
            Ast ast = parser.Parse("return 5");
            Assert.AreEqual(AstTypes.Return, ast.Type);
            ReturnAst rast = (ReturnAst)ast;
            Assert.AreEqual(false, rast.ReturnsVoid);
            Assert.AreEqual(AstTypes.NumberLiteral, rast.Body.Type);
            Assert.AreEqual(5, ((NumberAst)rast.Body).Value);

            ast = parser.Parse("return");
            Assert.AreEqual(AstTypes.Return, ast.Type);
            rast = (ReturnAst)ast;
            Assert.AreEqual(true, rast.ReturnsVoid);
        }
    }
}
