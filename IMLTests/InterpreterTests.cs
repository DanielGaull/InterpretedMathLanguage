using IML.CoreDataTypes;
using IML.Environments;
using IML.Evaluation;
using IML.Functions;
using IML.Parsing;
using IML.Parsing.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace IMLTests
{
    [TestClass]
    public class InterpreterTests
    {
        private static Interpreter interpreter;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            VariableAstTypeMap baseTypeMap = InterpreterHelper.CreateBaseTypeMap();

            Parser parser = new Parser(baseTypeMap);

            Interpreter evaluator = new Interpreter();

            DataTypeDict dtDict = InterpreterHelper.CreateBaseDtDict();
            evaluator.Initialize(dtDict, parser, () => {});

            interpreter = evaluator;
        }
        
        private void AssertInterpreterValues(string input, string expected)
        {
            string output = "";
            try
            {
                MEnvironment env = InterpreterHelper.CreateBaseEnv();
                MValue result = interpreter.Evaluate(input, env);
                output = result.ToLongString();
            }
            catch (Exception ex)
            {
                output = "Exception: " + ex.Message;
            }
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestSimpleCheckFunction()
        {
            AssertInterpreterValues("_c({{()=>{true},()=>{1}},{()=>{true},()=>{2}}})", "(number) 1");
        }
        [TestMethod]
        public void TestCheckWithNull()
        {
            AssertInterpreterValues("_c({{()=>{null},()=>{1}},{()=>{true},()=>{2}}})", "(number) 2");
        }
        [TestMethod]
        public void TestSimpleReference()
        {
            AssertInterpreterValues("&true", "(ref) <ref -> (boolean) true>");
        }
        [TestMethod]
        public void TestSimpleList()
        {
            AssertInterpreterValues("{1, 2, 3}", "(list) { 1, 2, 3 }");
        }
        [TestMethod]
        public void TestVarNotExisting()
        {
            AssertInterpreterValues("&varnotexist",
                    "(error) Error: #11 (VAR_DOES_NOT_EXIST) 'Variable \"varnotexist\" does not exist.' Data: {  }");
        }
        [TestMethod]
        public void TestSimpleAdd()
        {
            AssertInterpreterValues("_add(1,2)", "(number) 3");
        }
        [TestMethod]
        public void TestComplexNestedLambda()
        {
            AssertInterpreterValues("(()=>{(()=>{1})()})()", "(number) 1");
        }
        [TestMethod]
        public void TestSimpleNestedLambda()
        {
            AssertInterpreterValues("(()=>{2})()", "(number) 2");
        }
        [TestMethod]
        public void TestAnotherComplexNestedLambda()
        {
            AssertInterpreterValues("(()=>{()=>{3}})()()", "(number) 3");
        }
        [TestMethod]
        public void TestMap()
        {
            AssertInterpreterValues("{1,2,3}.map((x)=>{_add(x,1)})", "(list) { 2, 3, 4 }");
        }
        [TestMethod]
        public void TestReduce()
        {
            AssertInterpreterValues("{1,2,3,4,5}.reduce((prev,current)=>{_add(prev,current)},0)", "(number) 15");
        }
        [TestMethod]
        public void TestCRangeMap()
        {
            AssertInterpreterValues("(_crange(5)).map((x)=>{_add(x,5)})", "(list) { 5, 6, 7, 8, 9 }");
        }
        [TestMethod]
        public void TestSimpleOr()
        {
            AssertInterpreterValues("_or_e(()=>{1},()=>{4})", "(number) 1");
        }
        [TestMethod]
        public void TestOrWithNull()
        {
            AssertInterpreterValues("_or_e(()=>{null},()=>{4})", "(number) 4");
        }
        [TestMethod]
        public void TestAndWithNull()
        {
            AssertInterpreterValues("_and_e(()=>{null},()=>{4})", "(null) null");
        }
        [TestMethod]
        public void TestSimpleAnd()
        {
            AssertInterpreterValues("_and_e(()=>{1},()=>{4})", "(number) 4");
        }
        [TestMethod]
        public void TestVarAssignments()
        {
            AssertInterpreterValues("_do({()=>{var x = 5; var y = &x; _set(y,3); return x;}})", "(number) 3");
        }
        [TestMethod]
        public void TestMapToFunction()
        {
            AssertInterpreterValues("{1,2}.map((x)=>{_exit})", "(list) { ()=>{<function>}, ()=>{<function>} }");
        }
        [TestMethod]
        public void TestBlockingEnvironmentlessLambdasFromHavingParameters()
        {
            AssertInterpreterValues("(x)~>{}",
                    "(error) Error: #15 (ILLEGAL_LAMBDA) 'Lambdas that don't create environments (~>)" +
                    " cannot have parameters' Data: {  }");
        }
        [TestMethod]
        public void TestVarDeclarationReturnValue()
        {
            AssertInterpreterValues("var x=7", "(void) void");
        }
        [TestMethod]
        public void TestVarAssignmentReturnValue()
        {
            AssertInterpreterValues("_do({()=>{var y=3; y=4;}})", "(void) void");
        }
        [TestMethod]
        public void TestBlockAssignmentToConstant()
        {
            AssertInterpreterValues("_do({()=>{const z=3; z=4;}})",
                    "(error) Error: #12 (CANNOT_ASSIGN) 'Cannot assign value to constant \"z\"' Data: {  }");
        }
        [TestMethod]
        public void TestShortenedParamlessLambda()
        {
            AssertInterpreterValues("[5]()", "(number) 5");
        }
        [TestMethod]
        public void TestComplextShortenedLambda()
        {
            AssertInterpreterValues("_do({()=>{var x=false; return x;}})", "(boolean) false");
        }
        [TestMethod]
        public void TestStringMemberAccess()
        {
            AssertInterpreterValues("\"hi\".chars", "(list) { 104, 105 }");
        }
        [TestMethod]
        public void TestIndexC()
        {
            AssertInterpreterValues("_do({()=>{var list={1,2,3}; return list.indexc(2,(a,b)=>{_eq(a,b)});}})",
                    "(number) 1");
        }
        [TestMethod]
        public void TestIndexSimple()
        {
            AssertInterpreterValues("_do({()=>{var list={1,2,3}; return list.index(2);}})", "(number) 1");
        }
        [TestMethod]
        public void TestListAdd()
        {
            AssertInterpreterValues("_do({()=>{var list={1,2}; list.add(3); return list;}})", 
                "(list) { 1, 2, 3 }");
            AssertInterpreterValues("_do({()=>{var list={1,2}; list.add(3); return list.length();}})", 
                "(number) 3");
        }
        [TestMethod]
        public void TestListRemoveAt()
        {
            AssertInterpreterValues("_do({()=>{var list={1,2,3}; list.removeAt(0); return list;}})", 
                "(list) { 2, 3 }");
        }
        [TestMethod]
        public void TestListRemoveFail()
        {
            AssertInterpreterValues("_do({()=>{var list={1,2,3}; return list.remove(0);}})", "(boolean) false");
        }
        [TestMethod]
        public void TestListRemoveSucceed()
        {
            AssertInterpreterValues("_do({()=>{var list={1,2,3}; list.remove(2);}})", "(boolean) true");
            AssertInterpreterValues("_do({()=>{var list={1,2,3}; list.remove(2); return list;}})", 
                "(list) { 1, 3 }");
        }
        [TestMethod]
        public void TestListRemoveC()
        {
            AssertInterpreterValues("_do({()=>{var list={1,2,3}; list.removec(2,(a,b)=>{_eq(a,b)}); return list;}})",
                    "(list) { 1, 3 }");
        }
        [TestMethod]
        public void TestListAddAll()
        {
            AssertInterpreterValues("_do({()=>{var list1={1,2,3};var list2={4,5,6};list1.addAll(list2); return list1;}})",
                    "(list) { 1, 2, 3, 4, 5, 6 }");
        }
        [TestMethod]
        public void TestMapWithAccess()
        {
            AssertInterpreterValues("{1,2,3}.map((x)=>{_add(x,1)}).get(0)", "(number) 2");
        }
        //[TestMethod]
        //public void TestDereference()
        //{
        //    AssertInterpreterValues("_do({()~>{var x = 5},()~>{var r = &x},()~>{*r = 2},()~>{x}})", "(number) 2");
        //}
    }
}
