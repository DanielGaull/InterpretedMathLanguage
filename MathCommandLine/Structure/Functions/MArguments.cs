using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Structure.FunctionTypes
{
    public struct MArguments
    {
        private List<MArgument> args;

        public int Length
        {
            get
            {
                return args.Count;
            }
        }

        public MArguments(params MArgument[] args)
        {
            this.args = new List<MArgument>(args);
        }

        public MArgument Get(int index)
        {
            return args[index];
        }
        public MArgument Get(string name)
        {
            return args.Where((arg) => arg.Name == name).FirstOrDefault();
        }
    }
}
