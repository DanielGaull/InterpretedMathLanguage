using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class DataTypeDict
    {
        private Dictionary<string, MDataType> internalDict;

        public DataTypeDict(params MDataType[] types)
        {
            internalDict = new Dictionary<string, MDataType>();
            AddTypes(types);
        }
        public DataTypeDict(List<MDataType> types)
        {
            internalDict = new Dictionary<string, MDataType>();
            AddTypes(types);
        }

        public MDataType CreateAndRegisterType(string name)
        {
            MDataType t = new MDataType(name, false);
            internalDict.Add(name, t);
            return t;
        }

        public void AddTypes(List<MDataType> types)
        {
            for (int i = 0; i < types.Count; i++)
            {
                internalDict.Add(types[i].Name, types[i]);
            }
        }
        public void AddTypes(params MDataType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                internalDict.Add(types[i].Name, types[i]);
            }
        }

        public MDataType GetType(string name)
        {
            if (internalDict.ContainsKey(name))
            {
                return internalDict[name];
            }
            // DT doesn't exist
            return MDataType.Empty;
        }

        public bool Contains(string name) 
        {
            return internalDict.ContainsKey(name);
        }
    }
}
