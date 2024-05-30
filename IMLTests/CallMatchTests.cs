using IML.Evaluation.AST.ValueAsts;
using IML.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using IML.CoreDataTypes;

namespace IMLTests
{
    [TestClass]
    public class CallMatchTests
    {
        private static Parser parser;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            VariableAstTypeMap baseTypeMap = new VariableAstTypeMap();
            baseTypeMap.Add("true", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("false", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("void", new AstType(MDataType.VOID_TYPE_NAME));
            baseTypeMap.Add("null", new AstType(MDataType.NULL_TYPE_NAME));

            Parser parser = new Parser(baseTypeMap);

            CallMatchTests.parser = parser;
        }

        [TestMethod]
        public void TestCallMatch_Invalid()
        {
            Parser.CallMatch callMatch = parser.MatchCall("((x)=>{_add(x,7)})");
            Assert.IsFalse(callMatch.IsMatch);
        }

        [TestMethod]
        public void TestCallMatch_Simple()
        {
            Parser.CallMatch callMatch = parser.MatchCall("x()");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("x", callMatch.Caller);
            Assert.AreEqual("", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_SimpleArgs()
        {
            Parser.CallMatch callMatch = parser.MatchCall("x(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("x", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_ComplexCaller()
        {
            Parser.CallMatch callMatch = parser.MatchCall("((x,y)=>{x})(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("((x,y)=>{x})", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_ComplexCallerWithGenericsInCaller()
        {
            Parser.CallMatch callMatch = parser.MatchCall("(call<number>(5))(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("(call<number>(5))", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_WithGenerics()
        {
            Parser.CallMatch callMatch = parser.MatchCall("x<number>(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("x", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("number", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_WithComplexAndGenerics()
        {
            Parser.CallMatch callMatch = parser.MatchCall("(call<number>(5))<number,string>(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("(call<number>(5))", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("number,string", callMatch.Generics);
        }
    }
}
