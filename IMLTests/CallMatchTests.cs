using IML.Parsing.AST.ValueAsts;
using IML.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using IML.CoreDataTypes;
using IML.Parsing;
using IML.Parsing.AST;
using IML.Parsing.Util;

namespace IMLTests
{
    [TestClass]
    public class CallMatchTests
    {
        private static Parser parser;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            VariableAstTypeMap baseTypeMap = InterpreterHelper.CreateBaseTypeMap();

            Parser parser = new Parser(baseTypeMap);

            CallMatchTests.parser = parser;
        }

        [TestMethod]
        public void TestCallMatch_Invalid()
        {
            CallMatch callMatch = CallMatcher.MatchCall("((x)=>{_add(x,7)})");
            Assert.IsFalse(callMatch.IsMatch);
        }

        [TestMethod]
        public void TestCallMatch_Simple()
        {
            CallMatch callMatch = CallMatcher.MatchCall("x()");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("x", callMatch.Caller);
            Assert.AreEqual("", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_SimpleArgs()
        {
            CallMatch callMatch = CallMatcher.MatchCall("x(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("x", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_ComplexCaller()
        {
            CallMatch callMatch = CallMatcher.MatchCall("((x,y)=>{x})(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("((x,y)=>{x})", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_ComplexCallerWithGenericsInCaller()
        {
            CallMatch callMatch = CallMatcher.MatchCall("(call<number>(5))(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("(call<number>(5))", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_WithGenerics()
        {
            CallMatch callMatch = CallMatcher.MatchCall("x<number>(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("x", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("number", callMatch.Generics);
        }

        [TestMethod]
        public void TestCallMatch_WithComplexAndGenerics()
        {
            CallMatch callMatch = CallMatcher.MatchCall("(call<number>(5))<number,string>(5,2)");
            Assert.IsTrue(callMatch.IsMatch);
            Assert.AreEqual("(call<number>(5))", callMatch.Caller);
            Assert.AreEqual("5,2", callMatch.Args);
            Assert.AreEqual("number,string", callMatch.Generics);
        }
    }
}
