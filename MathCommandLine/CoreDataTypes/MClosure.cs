using MathCommandLine.Environments;
using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class MClosure
    {
        public MParameters Parameters { get; private set; }
        public MEnvironment Environment { get; private set; }
        public Ast AstBody { get; private set; }
        public NativeExpression NativeBody { get; private set; }
        public bool IsNativeBody { get; private set; }

        public MClosure(MParameters parameters, MEnvironment env, Ast body)
        {
            Parameters = parameters;
            Environment = env;
            AstBody = body;
            IsNativeBody = false;
        }
        public MClosure(MParameters parameters, MEnvironment env, NativeExpression nativeBody)
        {
            Parameters = parameters;
            Environment = env;
            NativeBody = nativeBody;
            IsNativeBody = true;
        }
    }
}
