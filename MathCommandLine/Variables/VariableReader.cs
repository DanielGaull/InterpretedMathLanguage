using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Variables
{
    // A readonly variable manager
    public class VariableReader
    {
        private VariableManager srcManager;

        public VariableReader(VariableManager manager)
        {
            srcManager = manager;
        }

        public MValue GetValue(string name)
        {
            return srcManager.GetValue(name);
        }
    }
}
