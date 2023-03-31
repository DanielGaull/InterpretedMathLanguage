using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public struct MField
    {
        public const int PUBLIC = 1;
        public const int PRIVATE = 0;

        public int ReadPermission { get; private set; }
        public int WritePermission { get; private set; }
        public MValue Value { get; set; }

        public MField(MValue value, int readPerm, int writePerm)
        {
            this.Value = value;
            this.ReadPermission = readPerm;
            this.WritePermission = writePerm;
        }
        
        public void SetValue(MValue value)
        {
            Value = value;
        }
    }
}
