using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Functions
{
    public class FunctionDict
    {
        private Dictionary<string, MFunction> internalDict;

        public FunctionDict(params MFunction[] functions)
        {
            internalDict = new Dictionary<string, MFunction>();
            AddFunctions(functions);
        }
        public FunctionDict(List<MFunction> functions)
        {
            internalDict = new Dictionary<string, MFunction>();
            AddFunctions(functions);
        }

        public void AddFunctions(List<MFunction> functions)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                internalDict.Add(functions[i].Name, functions[i]);
            }
        }
        public void AddFunctions(params MFunction[] functions)
        {
            for (int i = 0; i < functions.Length; i++)
            {
                internalDict.Add(functions[i].Name, functions[i]);
            }
        }

        public MFunction GetFunction(string name)
        {
            if (internalDict.ContainsKey(name))
            {
                return internalDict[name];
            }
            // TODO: Return function doesn't exist errors
            return null;
        }
    }
}
