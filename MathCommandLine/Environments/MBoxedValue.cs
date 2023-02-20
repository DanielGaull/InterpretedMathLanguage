using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Environments
{
    // Used so that we can have references to the values and not access to the actual values directly
    public class MBoxedValue
    {
        private MValue value;
        public bool CanGet { get; private set; }
        public bool CanSet { get; private set; }

        public MBoxedValue(MValue value, bool canGet, bool canSet)
        {
            this.value = value;
            CanGet = canGet;
            CanSet = canSet;
        }

        public MValue GetValue()
        {
            if (CanGet)
            {
                return value;
            }
            return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST);
        }

        public MValue SetValue(MValue value)
        {
            if (CanSet)
            {
                this.value = value;
                return MValue.Void();
            }
            return MValue.Error(Util.ErrorCodes.CANNOT_ASSIGN);
        }

        public override string ToString()
        {
            if (CanGet)
            {
                return value.ToString();
            }
            return "(hidden)";
        }
    }
}
