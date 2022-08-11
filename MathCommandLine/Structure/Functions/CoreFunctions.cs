using MathCommandLine.Evaluation;
using MathCommandLine.Structure.FunctionTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure.Functions
{
    /**
     * Static class providing the core functions of the language
     */
    public static class CoreFunctions
    {
        public static MFunction Add()
        {
            return new MFunction("_add", MDataType.Number, (args) => 
            {
                return MValue.Number(args.Get(0).Value.NumberValue + args.Get(1).Value.NumberValue);
            },
            new MParameters(
                new MParameter(MDataType.Number, "a"),
                new MParameter(MDataType.Number, "b")
            ));
        }
    }
}
