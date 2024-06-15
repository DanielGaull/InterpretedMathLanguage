using IML.CoreDataTypes;
using IML.Exceptions;
using IML.Parsing;
using IML.Parsing.AST;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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
            MType genericT = new MType(new MGenericDataTypeEntry("T"));
            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Number,
                new List<MType> { genericT }, new List<string> { "T" }, false, LambdaEnvironmentType.AllowAny,
                false);

            List<MType> result = assigner.AssignGenerics(callee, new List<MType>() { MType.String });

            Assert.AreEqual(1, result.Count);
            MType r = result[0];
            Assert.AreEqual(1, r.Entries.Count);
            Assert.AreEqual("string", ((MConcreteDataTypeEntry)r.Entries[0]).DataType.Name);
        }

        [TestMethod]
        public void Assign_WithSingleArgAndReturnSame_Success()
        {
            MType genericT = new MType(new MGenericDataTypeEntry("T"));
            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(genericT,
                new List<MType> { genericT }, new List<string> { "T" }, false, LambdaEnvironmentType.AllowAny,
                false);

            List<MType> result = assigner.AssignGenerics(callee, new List<MType>() { MType.String });

            Assert.AreEqual(1, result.Count);
            MType r = result[0];
            Assert.AreEqual(1, r.Entries.Count);
            Assert.IsTrue(r.Entries.Exists(e => ((MConcreteDataTypeEntry)e).DataType.Name == "string"));
        }

        [TestMethod]
        public void Assign_WithTwoArg_Success()
        {
            MType genericT = new MType(new MGenericDataTypeEntry("T"));
            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Number,
                new List<MType> { genericT, genericT }, new List<string> { "T" }, false, 
                LambdaEnvironmentType.AllowAny, false);

            List<MType> result = assigner.AssignGenerics(callee, new List<MType>() { MType.Number, MType.String });

            Assert.AreEqual(1, result.Count);
            MType r = result[0];
            Assert.AreEqual(2, r.Entries.Count);
            Assert.IsTrue(r.Entries.Exists(e => ((MConcreteDataTypeEntry)e).DataType.Name == "number"));
            Assert.IsTrue(r.Entries.Exists(e => ((MConcreteDataTypeEntry)e).DataType.Name == "string"));
        }

        [TestMethod]
        public void Assign_WithTwoGenerics_Success()
        {
            MType genericT = new MType(new MGenericDataTypeEntry("T"));
            MType genericR = new MType(new MGenericDataTypeEntry("R"));
            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Number,
                new List<MType> { genericT, genericR }, new List<string> { "T", "R" }, false, 
                LambdaEnvironmentType.AllowAny, false);

            List<MType> result = assigner.AssignGenerics(callee, new List<MType>() { MType.Number, MType.String });

            Assert.AreEqual(2, result.Count);
            MType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.AreEqual("number", ((MConcreteDataTypeEntry)r1.Entries[0]).DataType.Name);
            MType r2 = result[1];
            Assert.AreEqual(1, r2.Entries.Count);
            Assert.AreEqual("string", ((MConcreteDataTypeEntry)r2.Entries[0]).DataType.Name);
        }

        [TestMethod]
        public void Assign_WithList_Success()
        {
            MType genericT = new MType(new MGenericDataTypeEntry("T"));
            MType listOfT = MType.List(genericT);
            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Number,
                new List<MType> { listOfT }, new List<string> { "T" }, false,
                LambdaEnvironmentType.AllowAny, false);

            List<MType> result = assigner.AssignGenerics(callee, new List<MType>() { MType.List(MType.Number) });

            Assert.AreEqual(1, result.Count);
            MType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.AreEqual("number", ((MConcreteDataTypeEntry)r1.Entries[0]).DataType.Name);
        }

        [TestMethod]
        public void Assign_WithDoublyNestedList_Success()
        {
            MType genericT = new MType(new MGenericDataTypeEntry("T"));
            MType listOfT = MType.List(genericT);
            MType listOfListOfT = MType.List(listOfT);
            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Number,
                new List<MType> { listOfListOfT }, new List<string> { "T" }, false,
                LambdaEnvironmentType.AllowAny, false);

            List<MType> result = assigner.AssignGenerics(callee, 
                new List<MType>() { MType.List(MType.List(MType.Number)) });

            Assert.AreEqual(1, result.Count);
            MType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.AreEqual("number", ((MConcreteDataTypeEntry)r1.Entries[0]).DataType.Name);
        }

        [TestMethod]
        public void Assign_WithNestedUnionedGenerics_ThrowException()
        {
            MType genericTOrString = new MType(new MGenericDataTypeEntry("T"), MDataTypeEntry.String);
            MType listOfTOrString = MType.List(genericTOrString);
            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Number,
                new List<MType> { listOfTOrString }, new List<string> { "T" }, false,
                LambdaEnvironmentType.AllowAny, false);

            try
            {
                assigner.AssignGenerics(callee, 
                    new List<MType>() { MType.List(MType.Number) });
            }
            catch (TypeDeterminationException ex)
            {   
                Assert.AreEqual("Type verification error: \"Cannot infer generics when they exclusively appear in " +
                    "type unions. Please manually define generics.\".", ex.Message);
            }
        }

        [TestMethod]
        public void Assign_WithFunctionParam_Success()
        {
            MType predicateFuncType = new MType(new MFunctionDataTypeEntry(
                MType.Boolean,
                new List<MType>() { new MType(new MGenericDataTypeEntry("T")) },
                new List<string>(),
                false, LambdaEnvironmentType.AllowAny, false));
            MType numberPredicateFuncType = new MType(new MFunctionDataTypeEntry(
                MType.Boolean,
                new List<MType>() { MType.Number },
                new List<string>(),
                false, LambdaEnvironmentType.AllowAny, false));

            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Boolean,
                new List<MType> { predicateFuncType }, new List<string> { "T" }, false,
                LambdaEnvironmentType.AllowAny, false);

            List<MType> result = assigner.AssignGenerics(callee, 
                new List<MType>() { numberPredicateFuncType });

            Assert.AreEqual(1, result.Count);
            MType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.AreEqual("number", ((MConcreteDataTypeEntry)r1.Entries[0]).DataType.Name);
        }

        [TestMethod]
        public void Assign_WithFunctionReturnVal_Success()
        {
            MType factoryFuncType = new MType(new MFunctionDataTypeEntry(
                new MType(new MGenericDataTypeEntry("T")),
                new List<MType>(),
                new List<string>(),
                false, LambdaEnvironmentType.AllowAny, false));
            MType stringFactoryFuncType = new MType(new MFunctionDataTypeEntry(
                MType.String,
                new List<MType>(),
                new List<string>(),
                false, LambdaEnvironmentType.AllowAny, false));

            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(MType.Boolean,
                new List<MType> { factoryFuncType }, new List<string> { "T" }, false,
                LambdaEnvironmentType.AllowAny, false);

            List<MType> result = assigner.AssignGenerics(callee,
                new List<MType>() { stringFactoryFuncType });

            Assert.AreEqual(1, result.Count);
            MType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.AreEqual("string", ((MConcreteDataTypeEntry)r1.Entries[0]).DataType.Name);
        }

        [TestMethod]
        public void Assign_WithSimpleValidUnion_Success()
        {
            MType returnType = MType.Union(new MType(new MGenericDataTypeEntry("T1")),
                new MType(new MGenericDataTypeEntry("T2")));
            MType p1Type = new MType(new MGenericDataTypeEntry("T1"));
            MType p2Type = new MType(new MGenericDataTypeEntry("T2"));

            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(returnType,
                new List<MType> { p1Type, p2Type }, new List<string> { "T1", "T2" }, false,
                LambdaEnvironmentType.AllowAny, false);

            MType concreteP1Type = MType.String;
            MType concreteP2Type = MType.Null;

            List<MType> result = assigner.AssignGenerics(callee,
                new List<MType>() { concreteP1Type, concreteP2Type });

            Assert.AreEqual(2, result.Count);
            MType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.AreEqual("string", ((MConcreteDataTypeEntry)r1.Entries[0]).DataType.Name);
            MType r2 = result[1];
            Assert.AreEqual(1, r2.Entries.Count);
            Assert.AreEqual("null", ((MConcreteDataTypeEntry)r2.Entries[0]).DataType.Name);
        }

        [TestMethod]
        public void Assign_WithFunctionValidUnion_Success()
        {
            MType returnType = MType.Union(new MType(new MGenericDataTypeEntry("T1")),
                new MType(new MGenericDataTypeEntry("T2")));
            MType p1Type = MType.Function(new MType(new MGenericDataTypeEntry("T1")));
            MType p2Type = MType.Function(new MType(new MGenericDataTypeEntry("T2")));

            MFunctionDataTypeEntry callee = new MFunctionDataTypeEntry(returnType,
                new List<MType> { p1Type, p2Type }, new List<string> { "T1", "T2" }, false,
                LambdaEnvironmentType.AllowAny, false);

            MType concreteP1Type = MType.Function(MType.String);
            MType concreteP2Type = MType.Function(MType.Null);

            List<MType> result = assigner.AssignGenerics(callee,
                new List<MType>() { concreteP1Type, concreteP2Type });

            Assert.AreEqual(2, result.Count);
            MType r1 = result[0];
            Assert.AreEqual(1, r1.Entries.Count);
            Assert.AreEqual("string", ((MConcreteDataTypeEntry)r1.Entries[0]).DataType.Name);
            MType r2 = result[1];
            Assert.AreEqual(1, r2.Entries.Count);
            Assert.AreEqual("null", ((MConcreteDataTypeEntry)r2.Entries[0]).DataType.Name);
        }
    }
}
