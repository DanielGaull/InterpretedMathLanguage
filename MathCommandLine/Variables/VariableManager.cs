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
        private List<MNamedValue> namedValues;

        public VariableManager()
        {
            namedValues = new List<MNamedValue>();
        }

        public MValue GetValue(string name)
        {
            var selection = namedValues.Where((v) => v.Name == name && v.CanGetValue());
            if (selection.Count() > 0)
            {
                return selection.First().GetValue();
            }
            // TODO: Exception for var doesn't exist
            return MValue.Empty;
        }

        public void SetValue(string name, MValue value)
        {
            var selection = namedValues.Where((v) => v.Name == name && v.CanAssign(value));
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
            namedValues.AddRange(vals);
        }
        public void AddConstant(string name, MValue value)
        {
            namedValues.Add(new MConstant(name, value));
        }
        public void AddVariable(string name, List<MTypeRestrictionsEntry> entries)
        {
            namedValues.Add(new MVariable(name, entries));
        }
        public void AddVariable(string name, List<MTypeRestrictionsEntry> entries, MValue value)
        {
            namedValues.Add(new MVariable(name, entries, value));
        }
    }
}
