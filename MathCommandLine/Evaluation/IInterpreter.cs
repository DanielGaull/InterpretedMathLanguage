using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
using MathCommandLine.Functions;
using MathCommandLine.Structure;

namespace MathCommandLine.Evaluation
{
    public interface IInterpreter
    {
        public MValue Evaluate(string expression, MEnvironment env);
        public MDataType GetDataType(string typeName);
        public MValue PerformCall(MClosure closure, MArguments args, MEnvironment currentEnv);
    }
}
