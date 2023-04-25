using MathCommandLine.Environments;
using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public delegate MValue NativeExpression(MArguments args, MEnvironment env);
    public class MClosure
    {
        public MParameters Parameters { get; private set; }
        public MEnvironment Environment { get; private set; }
        public Ast AstBody { get; private set; }
        public NativeExpression NativeBody { get; private set; }
        public bool IsNativeBody { get; private set; }
        public bool CreatesEnv { get; private set; }

        public MClosure(MParameters parameters, MEnvironment env, Ast body, bool createsEnv)
        {
            Parameters = parameters;
            Environment = env;
            AstBody = body;
            IsNativeBody = false;
            CreatesEnv = createsEnv;
        }
        public MClosure(MParameters parameters, MEnvironment env, NativeExpression nativeBody)
        {
            Parameters = parameters;
            Environment = env;
            NativeBody = nativeBody;
            IsNativeBody = true;
        }

        public static MClosure Empty = new MClosure(MParameters.Empty, null, Ast.Invalid(null), false);

        public override string ToString()
        {
            if (this == Empty)
            {
                return "<empty>";
            }
            return "(" + Parameters.ToString() + ")" + (CreatesEnv ? "=>" : "~>") + "{ <function> }";
        }

        public MClosure CloneWithNewEnvironment(MEnvironment env)
        {
            if (IsNativeBody)
            {
                return new MClosure(Parameters, env, NativeBody);
            }
            else
            {
                return new MClosure(Parameters, env, AstBody, CreatesEnv);
            }
        }
    }
}
