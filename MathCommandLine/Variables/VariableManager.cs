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

        public MValue GetValue(string name)
        {
            var selection = NamedValues.Where((v) => v.Name == name && v.CanGetValue());
            if (selection.Count() > 0)
            {
                return selection.First().GetValue();
            }
            // TODO: Exception for var doesn't exist
            return MValue.Empty;
        }

        public void SetValue(string name, MValue value)
        {
            var selection = NamedValues.Where((v) => v.Name == name && v.CanAssign(value));
            if (selection.Count() > 0)
            {
                selection.First().Assign(value);
            }
            else
            {
                // TODO: Exception for var doesn't exist
            }
        }

        // TODO: Don't allow any duplicate names
        public void AddNamedValues(params MNamedValue[] vals)
        {
            NamedValues.AddRange(vals);
        }
        public void AddConstant(string name, MValue value)
        {
            NamedValues.Add(new MConstant(name, value));
        }
        public void AddVariable(string name, List<MTypeRestrictionsEntry> entries)
        {
            NamedValues.Add(new MVariable(name, entries));
        }
        public void AddVariable(string name, List<MTypeRestrictionsEntry> entries, MValue value)
        {
            NamedValues.Add(new MVariable(name, entries, value));
        }
    }
}
