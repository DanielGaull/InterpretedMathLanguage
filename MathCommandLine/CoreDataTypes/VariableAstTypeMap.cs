using IML.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public class VariableAstTypeMap
    {
        private Dictionary<string, AstType> dict;

        public VariableAstTypeMap()
        {
            dict = new Dictionary<string, AstType>();
        }

        public void Add(string name, AstType type)
        {
            dict.Add(name, type);
        }
        public AstTypeOrEmpty Get(string name)
        {
            if (dict.ContainsKey(name))
            {
                return AstTypeOrEmpty.AstType(dict[name]);
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
