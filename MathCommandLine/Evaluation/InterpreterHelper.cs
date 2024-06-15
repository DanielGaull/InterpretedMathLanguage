using IML.CoreDataTypes;
using IML.Environments;
using IML.Functions;
using IML.Parsing;
using IML.Parsing.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Evaluation
{
    public class InterpreterHelper
    {
        public static MEnvironment CreateBaseEnv()
        {
            List<MNativeFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions();
            MEnvironment baseEnv = new MEnvironment(MEnvironment.Empty);
            baseEnv.AddConstant("null", MValue.Null());
            baseEnv.AddConstant("void", MValue.Void());
            baseEnv.AddConstant("true", MValue.Bool(true));
            baseEnv.AddConstant("false", MValue.Bool(false));
            for (int i = 0; i < coreFuncs.Count; i++)
            {
                MValue function = MValue.Function(coreFuncs[i].ToFunction());
                baseEnv.AddConstant(coreFuncs[i].Name, function, coreFuncs[i].Description);
            }
            return baseEnv;
        }
        public static VariableAstTypeMap CreateBaseTypeMap()
        {
            VariableAstTypeMap baseTypeMap = new VariableAstTypeMap();
            baseTypeMap.Add("true", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("false", new AstType(MDataType.BOOLEAN_TYPE_NAME));
            baseTypeMap.Add("void", new AstType(MDataType.VOID_TYPE_NAME));
            baseTypeMap.Add("null", new AstType(MDataType.NULL_TYPE_NAME));
            List<MNativeFunction> coreFuncs = CoreFunctions.GenerateCoreFunctions();
            for (int i = 0; i < coreFuncs.Count; i++)
            {
                MType type = new MType(coreFuncs[i].ToFunction().TypeEntry);
                baseTypeMap.Add(coreFuncs[i].Name, MTypeToAstType(type));
            }
            return baseTypeMap;
        }
        public static DataTypeDict CreateBaseDtDict()
        {
            return new DataTypeDict(MDataType.Number, MDataType.List,
                MDataType.Function, MDataType.Type, MDataType.Error, MDataType.Reference,
                MDataType.String, MDataType.Void, MDataType.Boolean, MDataType.Null,
                MDataType.Any);
        }

        private static AstType MTypeToAstType(MType type)
        {
            List<AstTypeEntry> entries = new List<AstTypeEntry>();
            foreach (MDataTypeEntry entry in type.Entries)
            {
                if (entry is MGenericDataTypeEntry gt)
                {
                    entries.Add(new AstTypeEntry(gt.Name));
                }
                else if (entry is MFunctionDataTypeEntry ft)
                {
                    entries.Add(new LambdaAstTypeEntry(
                        MTypeToAstType(ft.ReturnType),
                        ft.ParameterTypes.Select(p => MTypeToAstType(p)).ToList(),
                        ft.EnvironmentType,
                        ft.IsPure,
                        ft.IsLastVarArgs,
                        ft.GenericNames
                    ));
                }
                else if (entry is MConcreteDataTypeEntry ct)
                {
                    entries.Add(new AstTypeEntry(ct.DataType.Name,
                        ct.Generics.Select(g => MTypeToAstType(g)).ToList()));
                }
            }
            return new AstType(entries);
        }
    }
}
