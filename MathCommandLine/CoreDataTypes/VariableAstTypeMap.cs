using IML.Parsing.AST;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public class VariableAstTypeMap
    {
        private Dictionary<string, AstType> dict;
        private Dictionary<string, Dictionary<string, AstType>> dtMemberTypes;

        public VariableAstTypeMap()
        {
            dict = new Dictionary<string, AstType>();
            dtMemberTypes = new Dictionary<string, Dictionary<string, AstType>>();
        }

        public void Add(string name, AstType type)
        {
            if (dict.ContainsKey(name))
            {
                dict[name] = type;
            }
            else
            {
                dict.Add(name, type);
            }
        }
        public AstTypeOrEmpty Get(string name)
        {
            if (dict.ContainsKey(name))
            {
                return AstTypeOrEmpty.AstType(dict[name]);
            }
            return AstTypeOrEmpty.Empty;
        }

        public void AddMemberType(string dataTypeName, string memberName, AstType memberType)
        {
            if (dtMemberTypes.ContainsKey(dataTypeName))
            {
                AddMemberTypeToDict(dtMemberTypes[dataTypeName], memberName, memberType);
            }
            else
            {
                Dictionary<string, AstType> newDict = new Dictionary<string, AstType>();
                dtMemberTypes.Add(dataTypeName, newDict);
                AddMemberTypeToDict(newDict, memberName, memberType);
            }
        }
        private void AddMemberTypeToDict(Dictionary<string, AstType> dict, string memberName, 
            AstType memberType)
        {
            if (dict.ContainsKey(memberName))
            {
                dict[memberName] = memberType;
            }
            else
            {
                dict.Add(memberName, memberType);
            }
        }
        public AstTypeOrEmpty GetMemberType(string dataTypeName, string memberName)
        {
            if (dtMemberTypes.ContainsKey(dataTypeName))
            {
                if (dtMemberTypes[dataTypeName].ContainsKey(memberName))
                {
                    return AstTypeOrEmpty.AstType(dtMemberTypes[dataTypeName][memberName]);
                }
            }
            return AstTypeOrEmpty.Empty;
        }

        public VariableAstTypeMap Clone()
        {
            Dictionary<string, AstType> dict2 = new Dictionary<string, AstType>();
            foreach (KeyValuePair<string, AstType> kv in dict)
            {
                dict2.Add(kv.Key, kv.Value);
            }
            return new VariableAstTypeMap() { dict = dict2 };
        }
    }

    public class AstTypeOrEmpty
    {
        private bool empty;
        private AstType type;

        public bool IsEmpty
        {
            get
            {
                return empty;
            }
        }
        public AstType Type
        {
            get
            {
                return type;
            }
        }

        public AstTypeOrEmpty(bool empty, AstType type)
        {
            this.empty = empty;
            this.type = type;
        }

        public static AstTypeOrEmpty Empty = new AstTypeOrEmpty(true, null);
        public static AstTypeOrEmpty AstType(AstType t)
        {
            return new AstTypeOrEmpty(false, t);
        }
    }
}
