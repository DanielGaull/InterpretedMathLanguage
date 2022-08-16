using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Functions
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
        public MArguments(List<MArgument> args)
        {
            this.args = new List<MArgument>(args);
        }

        public MArgument this[int index]
        {
            get
            {
                return Get(index);
            }
        }
        public MArgument this[string key]
        {
            get
            {
                return Get(key);
            }
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
