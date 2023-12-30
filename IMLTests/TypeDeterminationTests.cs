using IML.CoreDataTypes;
using IML.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    public class TypeDeterminationTests
    {
        private static Parser parser;
        private static TypeDeterminer typeDeterminer;
        private static VariableAstTypeMap baseTypeMap;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            baseTypeMap = new VariableAstTypeMap();
            baseTypeMap.Add("true", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("false", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("void", new AstType(MDataType.VOID_TYPE_NAME));
            baseTypeMap.Add("null", new AstType(MDataType.NULL_TYPE_NAME));

            parser = new Parser(baseTypeMap);
            typeDeterminer = new TypeDeterminer();
        }

        public void AssertTypes(string input, AstType type, VariableAstTypeMap typeMap)
        {
            Ast parsed = parser.Parse(input);
            AstType parsedType = typeDeterminer.DetermineDataType(parsed, typeMap);
            Assert.IsTrue(type == parsedType);
        }

        [TestMethod]
        public void SimpleLiterals()
        {
            AssertTypes("5", new AstType(MDataType.NUMBER_TYPE_NAME), new VariableAstTypeMap());
            AssertTypes("\"a\"", new AstType(MDataType.STRING_TYPE_NAME), new VariableAstTypeMap());
        }
        [TestMethod]
        public void ListGenerics()
        {
            AssertTypes("{1,2,3}", new AstType(MDataType.LIST_TYPE_NAME, new AstType(MDataType.NUMBER_TYPE_NAME)),
                new VariableAstTypeMap());
            AssertTypes("{{1,2,3},{\"h\"}}", new AstType(
                    new AstTypeEntry(MDataType.LIST_TYPE_NAME, 
                        new AstType(new List<AstTypeEntry>()
                            {
                                new AstTypeEntry(MDataType.LIST_TYPE_NAME, new AstType(MDataType.NUMBER_TYPE_NAME)),
                                new AstTypeEntry(MDataType.LIST_TYPE_NAME, new AstType(MDataType.STRING_TYPE_NAME))
                            }
                        )
                    )
                ),
                new VariableAstTypeMap());
        }
        [TestMethod]
        public void RefGenerics()
        {
            VariableAstTypeMap typeMap = baseTypeMap.Clone();
            typeMap.Add("test_list", new AstType(MDataType.LIST_TYPE_NAME, new AstType(MDataType.NUMBER_TYPE_NAME)));

            AssertTypes("&true", new AstType(MDataType.REF_TYPE_NAME, new AstType(MDataType.BOOLEAN_TYPE_NAME)), typeMap);
            AssertTypes("&test_list", new AstType(MDataType.REF_TYPE_NAME,
                new AstType(MDataType.LIST_TYPE_NAME, new AstType(MDataType.NUMBER_TYPE_NAME))), typeMap);
        }
        [TestMethod]
        public void SimpleFunction()
        {
            AssertTypes("()=>{true}()", new AstType(MDataType.BOOLEAN_TYPE_NAME), baseTypeMap);
        }
        [TestMethod]
        public void MultiLineFunction()
        {
            AssertTypes("()=>{var x = 5; return true;}()", new AstType(MDataType.BOOLEAN_TYPE_NAME), baseTypeMap);
        }
        [TestMethod]
        public void MultiLineFunctionWithVariable()
        {
            AssertTypes("()=>{var x = 5; return x;}()", new AstType(MDataType.NUMBER_TYPE_NAME), baseTypeMap);
        }
    }
}
