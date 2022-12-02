using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Variables
{
    public class MReferencedValue
    {
        private MValue value;

        public bool CanGetValue { get; protected set; }
        public bool CanSetValue { get; protected set; }
        public bool CanDeleteValue { get; protected set; }

        public MReferencedValue(MValue value, bool canGet, bool canSet, bool canDelete)
        {
            this.value = value;
            this.CanGetValue = canGet;
            this.CanSetValue = canSet;
            this.CanDeleteValue = canDelete;
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
