using IML.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    public class ParameterAndTypeParsingTests
    {
        private static Parser parser;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            parser = new Parser();
        }

        private void AssertParsedParameter(string input, string expected)
        {
            string output;
            try
            {
                AstParameter param = parser.ParseParameter(input);
                output = parser.UnparseParameter(param);
            }
            catch (Exception ex)
            {
                output = "Exception: " + ex.Message;
            }
            Assert.AreEqual(expected, output);
        }
        private void AssertParsedParameter(string input)
        {
            AssertParsedParameter(input, input);
        }

        [TestMethod]
        public void TestSimpleNumberParam()
        {
            AssertParsedParameter("value:number", "value:number[]");
        }
        [TestMethod]
        public void TestSimpleUnion()
        {
            AssertParsedParameter("value:number|string", "value:number[]|string[]");
        }
        [TestMethod]
        public void TestSimpleSingleRestriction()
        {
            AssertParsedParameter("value:number[%]", "value:number[%()]");
        }
        [TestMethod]
        public void TestSimpleSingleRestrictionArg()
        {
            AssertParsedParameter("value:number[<(5)]");
        }
        [TestMethod]
        public void TestSimpleDoubleRestrictionArg()
        {
            AssertParsedParameter("value:number[<(5),>(0)]");
        }
        [TestMethod]
        public void TestUnionDoubleRestrictionArg()
        {
            AssertParsedParameter("value:number[<(5),>(0)]|string[l(5)]");
        }
        [TestMethod]
        public void TestSimpleStringArgRestriction()
        {
            AssertParsedParameter("value:number[v(\"hello\")]");
        }
        [TestMethod]
        public void TestUnionStringArgRestriction()
        {
            AssertParsedParameter("value:number[v(\"hello\")]|string[like(\"([a-b])*\")]");
        }
        [TestMethod]
        public void TestSimpleTypeArgRestriction()
        {
            AssertParsedParameter("value:list[t(number[])]");
        }
        [TestMethod]
        public void TestUnionTypeArgRestriction()
        {
            AssertParsedParameter("value:list[t(number[]|string[])]");
        }
        [TestMethod]
        public void TestUnionTypeArgWithRestriction()
        {
            AssertParsedParameter("value:list[t(number[>(0)]|string[])]");
        }
        [TestMethod]
        public void TestNestedUnionTypeArgWithRestriction()
        {
            AssertParsedParameter("value:list[t(number[>(0)]|list[t(boolean[])])]");
        }

        [TestMethod]
        public void TestSimpleLambda()
        {
            AssertParsedParameter("()=>void", "()=>void[]");
        }
        [TestMethod]
        public void TestLambdaOneArg()
        {
            AssertParsedParameter("(number)=>void", "(number[])=>void[]");
        }
        [TestMethod]
        public void TestLambdaTwoArg()
        {
            AssertParsedParameter("(number,string)=>void", "(number[],string[])=>void[]");
        }
        [TestMethod]
        public void TestLambdaUnionArgs()
        {
            AssertParsedParameter("(number|list,string)=>void", "(number[]|list[],string[])=>void[]");
        }
        [TestMethod]
        public void TestLambdaUnionArgsRestrictions()
        {
            AssertParsedParameter("(number[<(20)]|list[t(string)],string)=>void", 
                "(number[<(20)]|list[t(string)],string[])=>void[]");
        }
        [TestMethod]
        public void TestLambdaReturnType()
        {
            AssertParsedParameter("()=>number[]");
        }
        [TestMethod]
        public void TestLambdaReturnTypeUnion()
        {
            AssertParsedParameter("()=>number[]|list[]");
        }
        [TestMethod]
        public void TestLambdaReturnTypeRestriction()
        {
            AssertParsedParameter("()=>number[>(20)]|list[t(string)]");
        }
        [TestMethod]
        public void TestLambdaReturnTypeRestrictionAndParameters()
        {
            AssertParsedParameter("(number[<(20)]|list[t(string)],string[])=>number[>(20)]|list[t(string)]");
        }
        [TestMethod]
        public void TestLambdaReturnTypeRestrictionAndParametersNoEnv()
        {
            AssertParsedParameter("(number[<(20)]|list[t(string)],string[])~>number[>(20)]|list[t(string)]",
                "(number[<(20)]|list[t(string)],string[])!~>number[>(20)]|list[t(string)]");
        }
        [TestMethod]
        public void TestLambdaReturnTypeRestrictionAndParametersForceEnv()
        {
            AssertParsedParameter("(number[<(20)]|list[t(string)],string[])!=>number[>(20)]|list[t(string)]");
        }
    }
}
