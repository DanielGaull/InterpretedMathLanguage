using MathCommandLine.Structure;
using MathCommandLine.Structure.FunctionTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public struct MLambda
    {
        public static MLambda Empty = new MLambda();

        public MValue Execute(MArguments args)
        {
            return MValue.Error(ErrorCodes.FATAL_UNKNOWN, "Not Implemented", MList.Empty);
        }
    }
}
