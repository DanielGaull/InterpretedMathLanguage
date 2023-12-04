using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Functions
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

        public static MArguments Empty = new MArguments(new MArgument[0]);

        public MArgument this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Set(index, value);
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
            return args.Where((arg) => arg.Name == name).First();
        }

        public void Set(int index, MArgument value)
        {
            args[index] = value;
        }

        public bool HasArg(string name)
        {
            return args.Where((arg) => arg.Name == name).Count() > 0;
        }
        public static MArguments Concat(MArguments first, MArguments second)
        {
            MArguments newArgs = new MArguments(first.args);
            newArgs.args.AddRange(new List<MArgument>(second.args));
            return newArgs;
        }
    }
}
