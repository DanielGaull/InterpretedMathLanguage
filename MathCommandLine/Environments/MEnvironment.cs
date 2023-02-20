﻿using MathCommandLine.Structure;
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

        public void AddValue(string name, MValue value, bool canGet, bool canSet)
        {
            values.Add(name, new MBoxedValue(value, canGet, canSet));
        }

        public void AddVariable(string name, MValue value)
        {
            AddValue(name, value, true, true);
        }

        public void AddConstant(string name, MValue value)
        {
            AddValue(name, value, true, false);
        }

        public bool Has(string name)
        {
            if (this == Empty)
            {
                return false;
            }
            if (values.ContainsKey(name))
            {
                return true;
            }
            return parent.Has(name);
        }

        public MBoxedValue GetBox(string name)
        {
            if (this == Empty)
            {
                return null;//MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST);
            }

            if (values.ContainsKey(name))
            {
                return values[name];
            }
            else
            {
                return parent.GetBox(name);
            }
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
    }
}