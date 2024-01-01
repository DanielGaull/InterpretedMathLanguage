using IML.CoreDataTypes;
using IML.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Environments
{
    public class MEnvironment
    {
        public static MEnvironment Empty = new MEnvironment(null);

        Dictionary<string, MBoxedValue> values = new Dictionary<string, MBoxedValue>();
        // Hidden values in an environment are only available to the interpreter
        Dictionary<string, MBoxedValue> hiddenValues = new Dictionary<string, MBoxedValue>();
        MEnvironment parent;

        private Dictionary<string, MType> genericMap;

        public MEnvironment(MEnvironment parent)
        {
            this.parent = parent;
            genericMap = new Dictionary<string, MType>();
        }

        public void AddHiddenValue(string name, MValue value, bool canGet, bool canSet)
        {
            hiddenValues.Add(name, new MBoxedValue(value, canGet, canSet, ""));
        }
        public void AddHiddenValue(string name, MValue value)
        {
            AddHiddenValue(name, value, true, true);
        }

        public void AddValue(string name, MValue value, bool canGet, bool canSet, string desc)
        {
            values.Add(name, new MBoxedValue(value, canGet, canSet, desc));
        }

        public void AddVariable(string name, MValue value)
        {
            AddVariable(name, value, null);
        }
        public void AddVariable(string name, MValue value, string desc)
        {
            AddValue(name, value, true, true, desc);
        }

        public void AddConstant(string name, MValue value)
        {
            AddConstant(name, value, null);
        }
        public void AddConstant(string name, MValue value, string desc)
        {
            AddValue(name, value, true, false, desc);
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
        public bool HasGeneric(string name)
        {
            if (this == Empty)
            {
                return false;
            }
            if (genericMap.ContainsKey(name))
            {
                return true;
            }
            return parent.HasGeneric(name);
        }

        public MBoxedValue GetBox(string name)
        {
            if (this == Empty)
            {
                return null;
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
                return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST, $"Variable \"{name}\" does not exist.", MList.Empty);
            }

            if (values.ContainsKey(name))
            {
                try
                {
                    return values[name].GetValue();
                }
                catch (BoxedValueException)
                {
                    return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST, 
                        $"Variable \"{name}\" does not exist.", MList.Empty);
                }
            }
            else
            {
                return parent.Get(name);
            }
        }
        public MType GetGeneric(string name)
        {
            if (this == Empty)
            {
                throw new FatalRuntimeException($"Generic \"{name}\" is not defined in this context.");
            }

            if (genericMap.ContainsKey(name))
            {
                return genericMap[name];
            }
            else
            {
                return parent.GetGeneric(name);
            }
        }

        public MValue Set(string name, MValue value)
        {
            if (this == Empty)
            {
                return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST, $"Variable \"{name}\" does not exist.", MList.Empty);
            }

            if (values.ContainsKey(name))
            {
                try
                {
                    return values[name].SetValue(value);
                }
                catch (BoxedValueException)
                {
                    return MValue.Error(Util.ErrorCodes.CANNOT_ASSIGN,
                        $"Variable \"{name}\" cannot be assigned to.", MList.Empty);
                }
            }
            else
            {
                return parent.Set(name, value);
            }
        }
        public void AddGeneric(string name, MType type)
        {
            if (genericMap.ContainsKey(name))
            {
                genericMap[name] = type;
            }
            else
            {
                genericMap.Add(name, type);
            }
        }

        public MValue GetHidden(string name)
        {
            if (this == Empty)
            {
                return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST, $"Variable \"{name}\" does not exist.", MList.Empty);
            }

            if (hiddenValues.ContainsKey(name))
            {
                try
                {
                    return hiddenValues[name].GetValue();
                }
                catch (BoxedValueException)
                {
                    return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST,
                        $"Variable \"{name}\" does not exist.", MList.Empty);
                }
            }
            else
            {
                return parent.GetHidden(name);
            }
        }

        public MValue SetHidden(string name, MValue value)
        {
            if (this == Empty)
            {
                return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST, $"Variable \"{name}\" does not exist.", MList.Empty);
            }

            if (hiddenValues.ContainsKey(name))
            {
                try
                {
                    return hiddenValues[name].SetValue(value);
                }
                catch (BoxedValueException)
                {
                    return MValue.Error(Util.ErrorCodes.CANNOT_ASSIGN, 
                        $"Variable \"{name}\" cannot be assigned to.", MList.Empty);
                }
            }
            else
            {
                return parent.SetHidden(name, value);
            }
        }
    }
}
