using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Variables
{
    public class MNamedValue
    {
        // TODO: Add reference to the module this value appears in
        public string Name { get; protected set; }
        private MValue value;

        public bool CanGetValue { get; protected set; }
        public bool CanSetValue { get; protected set; }

        public MNamedValue(string name, MValue value, bool canGet, bool canSet)
        {
            Name = name;
            this.value = value;
            this.CanGetValue = canGet;
            this.CanSetValue = canSet;
        }

        public virtual bool Assign(MValue value)
        {
            if (CanSetValue)
            {
                this.value = value;
                return true;
            }
            return false;
        }
        public virtual MValue GetValue()
        {
            if (CanGetValue)
            {
                return value;
            }
            // TODO: Throw exception
            return MValue.Empty;
        }
    }
}
