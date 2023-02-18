using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Environments
{
    public class MEnvironment
    {
        // TODO: Write this class
        public static MEnvironment Empty = new MEnvironment(null);

        Dictionary<string, MBoxedValue> values = new Dictionary<string, MBoxedValue>();
        MEnvironment parent;

        public MEnvironment(MEnvironment parent)
        {
            this.parent = parent;
        }

        public void AddValue(string name, MValue value, bool canGet, bool canSet, bool canDelete)
        {
            values.Add(name, new MBoxedValue(value, canGet, canSet, canDelete));
        }

        public void AddVariable(string name, MValue value)
        {
            AddValue(name, value, true, true, true);
        }

        public void AddConstant(string name, MValue value)
        {
            AddValue(name, value, true, false, false);
        }

        public MValue Get(string name)
        {
            if (this == Empty)
            {
                return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST);
            }

            if (values.ContainsKey(name))
            {
                return values[name].GetValue();
            }
            else
            {
                return parent.Get(name);
            }
        }

        public MValue Set(string name, MValue value)
        {
            if (this == Empty)
            {
                return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST);
            }

            if (values.ContainsKey(name))
            {
                return values[name].SetValue(value);
            }
            else
            {
                return parent.Set(name, value);
            }
        }

        // Used so that we can have references to the values and not access to the actual values directly
        private class MBoxedValue
        {
            private MValue value;
            public bool CanGet { get; private set; }
            public bool CanSet { get; private set; }
            public bool CanDelete { get; private set; }

            public MBoxedValue(MValue value, bool canGet, bool canSet, bool canDelete)
            {
                this.value = value;
                CanGet = canGet;
                CanSet = canSet;
                CanDelete = canDelete;
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
        }
    }
}
