using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Variables
{
    public class MConstant : MNamedValue
    {
        public MConstant(string name, MValue value) : base(name, value)
        {
        }
        public override bool CanAssign(MValue value)
        {
            return false;
        }

        public override bool CanGetValue()
        {
            return true;
        }
    }
}
