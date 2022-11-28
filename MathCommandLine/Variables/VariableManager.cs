using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Variables
{
    public class VariableManager
    {
        public List<MNamedValue> NamedValues { get; private set; }

        public VariableManager()
        {
            NamedValues = new List<MNamedValue>();
        }

        public VariableReader GetReader()
        {
            return new VariableReader(this);
        }

        public MValue GetValue(string name)
        {
            var selection = NamedValues.Where((v) => v.Name == name && v.CanGetValue);
            if (selection.Count() > 0)
            {
                return selection.First().GetValue();
            }
            // TODO: Exception for var doesn't exist
            return MValue.Empty;
        }

        public bool HasValue(string name)
        {
            return NamedValues.Where((v) => v.Name == name && v.CanGetValue).Count() > 0;
        }

        public bool CanModifyValue(string name, MValue value)
        {
            return NamedValues.Where((v) => v.Name == name && v.CanSetValue).Count() > 0;
        }

        public void SetValue(string name, MValue value)
        {
            var selection = NamedValues.Where((v) => v.Name == name && v.CanSetValue);
            if (selection.Count() > 0)
            {
                selection.First().Assign(value);
            }
            else
            {
                // TODO: Exception for var doesn't exist, or for var cannot be modified
            }
        }

        // TODO: Don't allow any duplicate names
        public void AddNamedValues(params MNamedValue[] vals)
        {
            NamedValues.AddRange(vals);
        }
        public void AddVariable(string name, MValue value)
        {
            NamedValues.Add(new MNamedValue(name, value, true, true));
        }
        public void AddConstant(string name, MValue value)
        {
            NamedValues.Add(new MNamedValue(name, value, true, false));
        }
    }
}
