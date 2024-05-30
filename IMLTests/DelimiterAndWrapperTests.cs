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
    }
}
