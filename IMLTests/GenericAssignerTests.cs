using IML.Parsing;
using IML.Parsing.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMLTests
{
    [TestClass]
    public class GenericAssignerTests
    {
        static GenericAssigner assigner;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            assigner = new GenericAssigner();
        }

        [TestMethod]
        public void Assign_WithSimpleSingle_Success()
        {
            AstType number = new AstType(new AstTypeEntry("number"));
            AstType str = new AstType(new AstTypeEntry("string"));
            AstType genericT = new AstType(new AstTypeEntry("T"));
            LambdaAstTypeEntry callee = new LambdaAstTypeEntry(number,
                new List<AstType>() { genericT }, IML.CoreDataTypes.LambdaEnvironmentType.AllowAny,
                false, false, new List<string>() { "T" });

            AstType realReturnType = number;
            AstType realArgumentType = str;
            List<AstType> result = assigner.AssignGenerics(callee, realReturnType,
                new List<AstType>() { realArgumentType });

            Assert.AreEqual(1, result.Count);
            AstType r = result[0];
            Assert.AreEqual(1, r.Entries.Count);
            Assert.AreEqual("string", r.Entries[0].DataTypeName);
        }

        [TestMethod]
        public void Assign_WithSingleArgAndReturnSame_Success()
        {
            AstType number = new AstType(new AstTypeEntry("number"));
            AstType str = new AstType(new AstTypeEntry("string"));
            AstType genericT = new AstType(new AstTypeEntry("T"));
            LambdaAstTypeEntry callee = new LambdaAstTypeEntry(genericT,
                new List<AstType>() { genericT }, IML.CoreDataTypes.LambdaEnvironmentType.AllowAny,
                false, false, new List<string>() { "T" });

            AstType realReturnType = number;
            AstType realArgumentType = str;
            List<AstType> result = assigner.AssignGenerics(callee, realReturnType,
                new List<AstType>() { realArgumentType });

            Assert.AreEqual(1, result.Count);
            AstType r = result[0];
            Assert.AreEqual(2, r.Entries.Count);
            // Order is undefined, so must allow any order
            Assert.IsTrue(r.Entries.Exists(x => x.DataTypeName == "number"));
            Assert.IsTrue(r.Entries.Exists(x => x.DataTypeName == "string"));
        }

    }
}
