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
            parser = new Parser();
            typeDeterminer = new TypeDeterminer();

            baseTypeMap = new VariableAstTypeMap();
            baseTypeMap.Add("true", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("false", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("void", new AstType(MDataType.VOID_TYPE_NAME));
            baseTypeMap.Add("null", new AstType(MDataType.NULL_TYPE_NAME));
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
                new List<AstTypeEntry>() 
                { 
                    new AstTypeEntry(MDataType.LIST_TYPE_NAME, 
                        new List<AstType>()
                        {
                            new AstType(MDataType.LIST_TYPE_NAME, new AstType(MDataType.NUMBER_TYPE_NAME))
                        }),
                    new AstTypeEntry(MDataType.LIST_TYPE_NAME,
                        new List<AstType>()
                        {
                            new AstType(MDataType.LIST_TYPE_NAME, new AstType(MDataType.STRING_TYPE_NAME))
                        }),
                }),
                new VariableAstTypeMap());
        }
        [TestMethod]
        public void RefGenerics()
        {
            AssertTypes("&true", new AstType(MDataType.REF_TYPE_NAME, new AstType(MDataType.BOOLEAN_TYPE_NAME)), baseTypeMap);
            AssertTypes("&&void", new AstType(MDataType.REF_TYPE_NAME,
                new AstType(MDataType.REF_TYPE_NAME, new AstType(MDataType.VOID_TYPE_NAME))), baseTypeMap);
            // TODO: Use multi-line parsing to do a test where we create a list and then a ref to it
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
