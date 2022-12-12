using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Variables
{
    public class VariableManager
    {
        Dictionary<string, int> nameMap;
        Dictionary<int, MReferencedValue> addressMap;
        int addrCounter = 0;
        // TODO: Intelligently handle when addresses are freed (can switch to an array/list w/ indexing)
        // Otherwise addrCounter can overflow

        public VariableManager()
        {
            nameMap = new Dictionary<string, int>();
            addressMap = new Dictionary<int, MReferencedValue>();
            addrCounter = 0;
        }

        public VariableReader GetReader()
        {
            return new VariableReader(this);
        }

        public MValue GetValue(string name)
        {
            if (nameMap.ContainsKey(name))
            {
                int addr = nameMap[name];
                MReferencedValue reffed = addressMap[addr];
                if (reffed.CanGetValue)
                {
                    return reffed.GetValue();
                }
                // TODO: Error for var cannot be read
            }
            // TODO: Error for var does not exist
            return MValue.Empty;
        }
        public MValue GetValue(int addr)
        {
            return addressMap[addr].GetValue();
        }

        public int AddressForName(string name)
        {
            return nameMap[name];
        }

        public bool HasValue(string name)
        {
            return nameMap.ContainsKey(name);
        }
        public bool HasValue(int addr)
        {
            return addressMap.ContainsKey(addr);
        }

        public bool CanModifyValue(int addr, MValue value)
        {
            return addressMap.ContainsKey(addr) && addressMap[addr].CanSetValue;
        }
        public void SetValue(int addr, MValue value)
        {
            if (addressMap.ContainsKey(addr))
            {
                if (addressMap[addr].CanSetValue)
                {
                    addressMap[addr].Assign(value);
                }
                // TODO: Error since var can't be set?
            }
            // TODO: Error since var doesn't exist? This func returns nothing so maybe not
        }

        // TODO: Don't allow any duplicate names
        public void AddVariable(string name, MValue value, bool canDelete)
        {
            AddNamedValue(name, new MReferencedValue(value, true, true, canDelete));
        }
        public void AddConstant(string name, MValue value, bool canDelete)
        {
            AddNamedValue(name, new MReferencedValue(value, true, false, canDelete));
        }
        public void AddNamedValue(string name, MReferencedValue refValue)
        {
            int addr = addrCounter++;
            nameMap.Add(name, addr);
            addressMap.Add(addr, refValue);
        }

        public bool CanDelete(int addr)
        {
            return addressMap[addr].CanDeleteValue;
        }
        public void Delete(int addr)
        {
            addressMap.Remove(addr);
            // Remove all names pointing to this address
            List<string> namesToDelete = new List<string>();
            foreach (var kv in nameMap)
            {
                if (kv.Value == addr)
                {
                    namesToDelete.Add(kv.Key);
                }
            }
            foreach (string name in namesToDelete)
            {
                nameMap.Remove(name);
            }
        }

        // Creates a clone of this to represent a new "scope frame"
        public VariableManager Clone()
        {
            Dictionary<string, int> nnameMap = new Dictionary<string, int>();
            Dictionary<int, MReferencedValue> naddressMap = new Dictionary<int, MReferencedValue>();
            foreach (var kv in nameMap)
            {
                nnameMap.Add(kv.Key, kv.Value);
            }
            foreach (var kv in addressMap)
            {
                naddressMap.Add(kv.Key, kv.Value);
            }
            VariableManager nvm = new VariableManager();
            nvm.nameMap = nnameMap;
            nvm.addressMap = naddressMap;
            nvm.addrCounter = addrCounter;
            return nvm;
        }
    }
}
