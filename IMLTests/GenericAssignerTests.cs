using IML.CoreDataTypes;
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
                new List<AstType>() { genericT }, LambdaEnvironmentType.AllowAny,
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
                new List<AstType>() { genericT }, LambdaEnvironmentType.AllowAny,
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

        [TestMethod]
        public void Assign_WithTwoArg_Success()
        {
            AstType number = new AstType(new AstTypeEntry("number"));
            AstType str = new AstType(new AstTypeEntry("string"));

            AstType genericT = new AstType(new AstTypeEntry("T"));
            LambdaAstTypeEntry callee = new LambdaAstTypeEntry(number,
                new List<AstType>() { genericT, genericT }, LambdaEnvironmentType.AllowAny,
                false, false, new List<string>() { "T" });

            AstType realReturnType = number;
            List<AstType> result = assigner.AssignGenerics(callee, realReturnType,
                new List<AstType>() { number, str });

            Assert.AreEqual(1, result.Count);
            AstType r = result[0];
            Assert.AreEqual(2, r.Entries.Count);
            // Order is undefined, so must allow any order
            Assert.IsTrue(r.Entries.Exists(x => x.DataTypeName == "number"));
            Assert.IsTrue(r.Entries.Exists(x => x.DataTypeName == "string"));
        }

        [TestMethod]
        public void Assign_WithTwoGenerics_Success()
        {
            AstType number = new AstType(new AstTypeEntry("number"));
            AstType str = new AstType(new AstTypeEntry("string"));

            AstType genericT = new AstType(new AstTypeEntry("T"));
            AstType genericR = new AstType(new AstTypeEntry("R"));
            LambdaAstTypeEntry callee = new LambdaAstTypeEntry(number,
                new List<AstType>() { genericT, genericR }, LambdaEnvironmentType.AllowAny,
                false, false, new List<string>() { "T", "R" });

            AstType realReturnType = number;
            List<AstType> result = assigner.AssignGenerics(callee, realReturnType,
                new List<AstType>() { number, str });

            Assert.AreEqual(2, result.Count);
            AstType r1 = result[0];
            AstType r2 = result[1];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.IsTrue(r1.Entries[0].DataTypeName == "number");
            Assert.AreEqual(1, r2.Entries.Count);
            Assert.IsTrue(r2.Entries[0].DataTypeName == "string");
        }

        // TODO: Tests where we have a data type with the generic INSIDE it
        // Like: list<T>
        [TestMethod]
        public void Assign_WithList_Success()
        {
            AstType number = new AstType(new AstTypeEntry("number"));
            AstType listOfNumber = new AstType(new AstTypeEntry("list", number));

            AstType genericT = new AstType(new AstTypeEntry("T"));
            AstType listOfT = new AstType(new AstTypeEntry("list", genericT));
            LambdaAstTypeEntry callee = new LambdaAstTypeEntry(number,
                new List<AstType>() { listOfT }, LambdaEnvironmentType.AllowAny,
                false, false, new List<string>() { "T", "R" });
            List<AstType> args = new List<AstType>()
            {
                listOfNumber
            };

            AstType realReturnType = number;
            List<AstType> result = assigner.AssignGenerics(callee, realReturnType, args);

            Assert.AreEqual(1, result.Count);
            AstType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.IsTrue(r1.Entries[0].DataTypeName == "number");
        }
    }
}
