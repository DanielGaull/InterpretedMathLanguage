using IML.CoreDataTypes;
using IML.Evaluation;
using IML.Exceptions;
using IML.Util;
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
            VariableAstTypeMap baseTypeMap = new VariableAstTypeMap();
            baseTypeMap.Add("true", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("false", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("void", new AstType(MDataType.VOID_TYPE_NAME));
            baseTypeMap.Add("null", new AstType(MDataType.NULL_TYPE_NAME));

            parser = new Parser(baseTypeMap);
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
        private void AssertParsedType(string input, string expected)
        {
            string output;
            try
            {
                AstType type = parser.ParseType(input);
                output = parser.UnparseType(type);
            }
            catch (Exception ex)
            {
                output = "Exception: " + ex.Message;
            }
            Assert.AreEqual(expected, output);
        }
        private void AssertParsedType(string input)
        {
            AssertParsedType(input, input);
        }
        private void AssertParsedTypeException(string input)
        {
            try
            {
                AstType type = parser.ParseType(input);
                Assert.Fail();
            }
            catch (InvalidParseException ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestSimpleNumberParam()
        {
            AssertParsedParameter("value:number", "value:number<>");
        }
        [TestMethod]
        public void TestSimpleUnion()
        {
            AssertParsedParameter("value:number|string", "value:number<>|string<>");
        }
        [TestMethod]
        public void TestSimpleSingleGeneric()
        {
            AssertParsedParameter("value:list<number>", "value:list<number<>>");
        }
        [TestMethod]
        public void TestSimpleDoubleGeneric()
        {
            AssertParsedParameter("value:tuple<number<>,number<>>");
        }
        [TestMethod]
        public void TestUnionDoubleGeneric()
        {
            AssertParsedParameter("value:tuple<number<>,number<>>|list<string<>>");
        }
        [TestMethod]
        public void TestNestedGeneric()
        {
            AssertParsedParameter("value:list<list<boolean>>", "value:list<list<boolean<>>>");
        }
        [TestMethod]
        public void TestUnionNestedGeneric()
        {
            AssertParsedParameter("value:list<list<boolean>>|list<list<string>>", 
                "value:list<list<boolean<>>>|list<list<string<>>>");
        }
        [TestMethod]
        public void TestTriplyNestedGeneric()
        {
            AssertParsedParameter("value:list<list<list<boolean<>>>>");
        }
        [TestMethod]
        public void TestQuadruplyNestedGeneric()
        {
            AssertParsedParameter("value:list<list<list<list<boolean<>>>>>");
        }

        [TestMethod]
        public void TestSimpleLambda()
        {
            AssertParsedType("()=>void", "()=>void<>");
        }
        [TestMethod]
        public void TestLambdaOneArg()
        {
            AssertParsedType("(number)=>void", "(number<>)=>void<>");
        }
        [TestMethod]
        public void TestLambdaTwoArg()
        {
            AssertParsedType("(number,string)=>void", "(number<>,string<>)=>void<>");
        }
        [TestMethod]
        public void TestLambdaUnionArgs()
        {
            AssertParsedType("(number|list,string)=>void", "(number<>|list<>,string<>)=>void<>");
        }
        [TestMethod]
        public void TestLambdaUnionArgsGenerics()
        {
            AssertParsedType("(number|list<string>,string)=>void", 
                "(number<>|list<string<>>,string<>)=>void<>");
        }
        [TestMethod]
        public void TestLambdaReturnType()
        {
            AssertParsedType("()=>number<>");
        }
        [TestMethod]
        public void TestLambdaReturnTypeUnion()
        {
            AssertParsedType("()=>number<>|list<>");
        }
        [TestMethod]
        public void TestLambdaReturnTypeGeneric()
        {
            AssertParsedType("()=>number<>|list<string<>>");
        }
        [TestMethod]
        public void TestLambdaReturnTypeGenericAndParameters()
        {
            AssertParsedType("(number<>|list<string<>>,string<>)=>number<>|list<strin<>>");
        }
        [TestMethod]
        public void TestLambdaReturnTypeGenericAndParametersNoEnv()
        {
            AssertParsedType("(number|list<string>,string)~>number|list<string>",
                "(number<>|list<string<>>,string<>)!~>number<>|list<string<>>");
        }
        [TestMethod]
        public void TestLambdaReturnTypeGenericAndParametersForceEnv()
        {
            AssertParsedType("(number<>|list<string<>>,string<>)!=>number<>|list<string<>>");
        }

        [TestMethod]
        public void TestParenWrappedType()
        {
            AssertParsedType("(number|string)", "number<>|string<>");
        }
        [TestMethod]
        public void TestParenWrappedTypeEntry()
        {
            AssertParsedType("(number)|string", "number<>|string<>");
        }

        [TestMethod]
        public void TestImbalancedBracketsException()
        {
            AssertParsedTypeException("(number)|(string");
        }
        [TestMethod]
        public void TestImbalancedBracketsException2()
        {
            AssertParsedTypeException("(number=>void");
        }
        [TestMethod]
        public void TestImbalancedBracketsException3()
        {
            AssertParsedTypeException("list<list<list<number<>>>");
        }

        // Varargs tests
        [TestMethod]
        public void TestSimpleVarargs()
        {
            AssertParsedType("(list<number<>>...)=>void<>");
        }
        [TestMethod]
        public void TestMultipleVarargs()
        {
            AssertParsedType("(number<>,string<>,list<number<>>...)=>void<>");
        }
        [TestMethod]
        public void TestVarargsTooEarlyFails()
        {
            AssertParsedTypeException("(number<>,string<>...,list<number>)=>void<>");
        }
        [TestMethod]
        public void TestVarargsNotAListFail()
        {
            AssertParsedTypeException("(number<>,string<>...)=>void<>");
        }
        [TestMethod]
        public void TestMultipleVarargsMaxWhitespaceWrapped()
        {
            string test = " ( ( < T , R > ( number < > , string < > , list < number < > > ... ) => void < > ) ) ";
            string noWhitespace = test.Replace(" ", "");
            string noParens = noWhitespace.SubstringBetween(2, noWhitespace.Length - 2);
            AssertParsedType(test, noParens);
        }

        [TestMethod]
        public void TestGenericNames()
        {
            AssertParsedType("<T>(T<>)=>T<>");
        }
        [TestMethod]
        public void TestGenericNamesMultiple()
        {
            AssertParsedType("<T,R,S>(T<>,R<>)=>S<>");
        }
        [TestMethod]
        public void TestGenericNamesInvalidFail1()
        {
            AssertParsedTypeException("<123>(number<>)=>void<>");
        }
        [TestMethod]
        public void TestGenericNamesInvalidFail2()
        {
            AssertParsedTypeException("<,>(number<>)=>void<>");
        }
        [TestMethod]
        public void TestGenericNamesInvalidFail3()
        {
            AssertParsedTypeException("<<>>(number<>)=>void<>");
        }
    }
}
