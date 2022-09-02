using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Variables
{
    public class MVariable : MNamedValue
    {
        private List<MTypeRestrictionsEntry> entries;
        private bool hasBeenAssigned = false;

        public MVariable(string name, List<MTypeRestrictionsEntry> entries)
            : base(name, MValue.Empty)
        {
            this.entries = entries;
            hasBeenAssigned = false;
        }
        public MVariable(string name, List<MTypeRestrictionsEntry> entries, MValue value)
            : base(name, MValue.Empty)
        {
            this.entries = entries;
            Assign(value);
        }

        public override bool CanAssign(MValue value)
        {
            // Get the entry that relates to this type
            MTypeRestrictionsEntry entry = entries.Where((entry) => entry.DataType == value.DataType).FirstOrDefault();
            if (entry.IsEmpty)
            {
                // No data type for this exists
                return false;
            }
            if (value.DataType == MDataType.Number)
            {
                return entry.ValueRestrictions.Any(
                    (restriction) => !restriction.SatisfiesNumRestriction(value.NumberValue)
                );
            }
            return true;
        }
        public override bool Assign(MValue value)
        {
            bool assigned = base.Assign(value);
            if (assigned)
            {
                hasBeenAssigned = true;
            }
            return assigned;
        }

        public override bool CanGetValue()
        {
            return hasBeenAssigned;
        }
    }
}
