using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public class ValueOrReturn
    {
        public MValue Value { get; private set; }
        public bool IsReturn { get; private set; }

        public ValueOrReturn(bool isReturn, MValue value)
        {
            IsReturn = isReturn;
            Value = value;
        }
        public ValueOrReturn(MValue value)
            : this(false, value)
        {
        }
    }
}
