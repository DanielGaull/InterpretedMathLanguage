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
            VariableAstTypeMap baseTypeMap = new VariableAstTypeMap();
            baseTypeMap.Add("true", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("false", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("void", new AstType(MDataType.VOID_TYPE_NAME));
            baseTypeMap.Add("null", new AstType(MDataType.NULL_TYPE_NAME));

            Parser parser = new Parser(baseTypeMap);

            Interpreter evaluator = new Interpreter();

            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List,
                MDataType.Function, MDataType.Type, MDataType.Error, MDataType.Reference,
                MDataType.String, MDataType.Void, MDataType.Boolean, MDataType.Null,
                MDataType.Any);
            evaluator.Initialize(dtDict, parser, () => { });

            interpreter = evaluator;

            List<MNativeFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions();
            baseEnv = new MEnvironment(MEnvironment.Empty);
            baseEnv.AddConstant("null", MValue.Null());
            baseEnv.AddConstant("void", MValue.Void());
            baseEnv.AddConstant("true", MValue.Bool(true));
            baseEnv.AddConstant("false", MValue.Bool(false));
            for (int i = 0; i < coreFuncs.Count; i++)
            {
                MValue function = MValue.Function(coreFuncs[i].ToFunction());
                baseEnv.AddConstant(coreFuncs[i].Name, function, coreFuncs[i].Description);
            }
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
    }
}
