using IML.CoreDataTypes;
using IML.Environments;
using IML.Functions;
using IML.Structure;

namespace IML.Evaluation
{
    public interface IInterpreter
    {
        public MValue Evaluate(string expression, MEnvironment env);
        public MDataType GetDataType(string typeName);
        public MValue PerformCall(MClosure closure, MArguments args, MEnvironment currentEnv);
        public void Exit();
    }
}
