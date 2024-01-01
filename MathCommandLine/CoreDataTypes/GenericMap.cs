using IML.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public class GenericMap
    {
        private Dictionary<string, MType> map;

        public GenericMap()
        {
            map = new Dictionary<string, MType>();
        }

        public void Add(string name, MType type)
        {
            // We always overwrite if needed, since generics of the same name just get rewritten
            if (map.ContainsKey(name))
            {
                map[name] = type;
            }
            else
            {
                map.Add(name, type);
            }
        }
        public bool Has(string name)
        {
            return map.ContainsKey(name);
        }
        public MType Get(string name)
        {
            if (!Has(name))
            {
                throw new FatalRuntimeException($"Generic \"{name}\" is not defined in this context");
            }
            return map[name];
        }

        public GenericMap Clone()
        {
            GenericMap newMap = new GenericMap();
            foreach (KeyValuePair<string, MType> kv in map)
            {
                newMap.Add(kv.Key, kv.Value);
            }
            return newMap;
        }
    }
}
