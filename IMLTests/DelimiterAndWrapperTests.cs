using IML.CoreDataTypes;
using IML.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    public class DelimiterAndWrapperTests
    {
        [TestMethod]
        public void TestWrappedByParen_SingleValue_ReturnTrue()
        {
            bool result = Parser.IsWrappedBy("(0)", '(', ')', "()", "[]", "<>", "{}");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestWrappedByParen_TooShort_ReturnFalse()
        {
            bool result = Parser.IsWrappedBy("()", '(', ')', "()", "[]", "<>", "{}");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestWrappedByParen_MissingClose_ReturnFalse()
        {
            bool result = Parser.IsWrappedBy("(9", '(', ')', "()", "[]", "<>", "{}");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestWrappedByParen_ImbalanceParen_ReturnFalse()
        {
            bool result = Parser.IsWrappedBy("((9)", '(', ')', "()", "[]", "<>", "{}");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestWrappedByParen_ImbalanceOther_ReturnFalse()
        {
            bool result = Parser.IsWrappedBy("([9)", '(', ')', "()", "[]", "<>", "{}");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestWrappedByParen_Complex_ReturnTrue()
        {
            bool result = Parser.IsWrappedBy("(()=>{obj.call<number>({1,2,3})})", '(', ')', "()", "[]", "<>", "{}");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestWrappedByParen_ComplexString_ReturnTrue()
        {
            bool result = Parser.IsWrappedBy("(()=>{obj.eval<number>(\"tuple]\\\")\")})", '(', ')', "()", "[]", "<>", "{}");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestTryMatchAssignment_WithValid_ReturnProperIndex()
        {
            int result = Parser.TryMatchAssignment("x=2");
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void TestTryMatchAssignment_WithNoEquals_ReturnNegative()
        {
            int result = Parser.TryMatchAssignment("x");
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void TestTryMatchAssignment_WithLevels_ReturnNegative()
        {
            int result = Parser.TryMatchAssignment("((x=4))");
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void TestTryMatchAssignment_WithLevelsInIdentifier_ReturnProperIndex()
        {
            int result = Parser.TryMatchAssignment("(*x)=5");
            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void TestTryMatchAssignment_WithListLevels_ReturnNegative()
        {
            int result = Parser.TryMatchAssignment("{x=5}");
            Assert.AreEqual(-1, result);
        }

        [TestMethod] 
        public void TestSplitByDelimiter_WithArgsInGoodFormat_Success()
        {
            string[] result = Parser.SplitByDelimiter("x:list[number],y:list[string]",
                ',');
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("x:list[number]", result[0]);
            Assert.AreEqual("y:list[string]", result[1]);
        }

        [TestMethod]
        public void TestSplitByDelimiter_WithArgsAndSpaces_Success()
        {
            string[] result = Parser.SplitByDelimiter(" x : list [ T ] , y : list [ S ] ",
                ',');
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(" x : list [ T ] ", result[0]);
            Assert.AreEqual(" y : list [ S ] ", result[1]);
        }

        [TestMethod]
        public void TestSplitByDelimiter_WithEmpty_ReturnNothing()
        {
            string[] result = Parser.SplitByDelimiter("", ',');
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void TestSplitByDelimiter_WithComplex_Success()
        {
            string[] result = Parser.SplitByDelimiter("()=>{{5,2}},\"hello,\",7", 
                ',');
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("()=>{{5,2}}", result[0]);
            Assert.AreEqual("\"hello,\"", result[1]);
            Assert.AreEqual("7", result[2]);
        }
    }
}
