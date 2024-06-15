using IML.CoreDataTypes;
using IML.Environments;
using IML.Evaluation;
using IML.Functions;
using IML.Parsing;
using IML.Parsing.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    public class TypeMatchingTests
    {
        private static Interpreter interpreter;
        private static MEnvironment baseEnv;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            VariableAstTypeMap baseTypeMap = InterpreterHelper.CreateBaseTypeMap();

            Parser parser = new Parser(baseTypeMap);

            Interpreter evaluator = new Interpreter();

            DataTypeDict dtDict = InterpreterHelper.CreateBaseDtDict();
            evaluator.Initialize(dtDict, parser, () => { });

            interpreter = evaluator;
            
            baseEnv = InterpreterHelper.CreateBaseEnv();
        }

        private void AssertTypes(MType type, MValue value)
        {
            Assert.IsTrue(type.ValueMatches(value));
        }
        private void AssertTypesFail(MType type, MValue value)
        {
            Assert.IsFalse(type.ValueMatches(value));
        }

        [TestMethod]
        public void TestSimpleMatch()
        {
            MType type = new MType(MDataTypeEntry.Boolean);
            MValue value = MValue.Bool(true);
            AssertTypes(type, value);
        }

        [TestMethod]
        public void TestSimpleMatchAny()
        {
            MType type = new MType(MDataTypeEntry.Any);
            MValue value = MValue.Bool(true);
            AssertTypes(type, value);
        }

        [TestMethod]
        public void TestSimpleMatchFail()
        {
            MType type = new MType(MDataTypeEntry.String);
            MValue value = MValue.Bool(true);
            AssertTypesFail(type, value);
        }

        [TestMethod]
        public void TestUnionMatch()
        {
            MType type = new MType(MDataTypeEntry.String, MDataTypeEntry.Boolean);
            MValue value = MValue.Bool(true);
            AssertTypes(type, value);
        }

        [TestMethod]
        public void TestUnionMatchFail()
        {
            MType type = new MType(MDataTypeEntry.String, MDataTypeEntry.Number);
            MValue value = MValue.Bool(true);
            AssertTypesFail(type, value);
        }

        [TestMethod]
        public void TestComplexLambda()
        {
            MType type = MType.Function(MType.Any, MType.Number, MType.Number);
            MValue value = MValue.Function(new MFunction(
                new MFunctionDataTypeEntry(MType.Boolean, new List<MType>() { MType.Any, MType.Any },
                    new List<string>(), false, LambdaEnvironmentType.ForceEnvironment, false),
                new List<string>() { "p1", "p2" }, baseEnv, new List<IML.Parsing.AST.ValueAsts.Ast>()));
            AssertTypes(type, value);
        }
    }
}
