using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Variables
{
    public abstract class MNamedValue
    {
        // TODO: Add reference to the module this value appears in
        public string Name { get; protected set; }
        private MValue value;
        public MNamedValue(string name, MValue value)
        {
            Name = name;
            this.value = value;
        }

        public abstract bool CanAssign(MValue value);
        public virtual bool Assign(MValue value)
        {
            if (CanAssign(value))
            {
                this.value = value;
                return true;
            }
            return false;
        }
        public abstract bool CanGetValue();
        public virtual MValue GetValue()
        {
            if (CanGetValue())
            {
                return value;
            }
            // TODO: Throw exception
            return MValue.Empty;
        }
    }
}
