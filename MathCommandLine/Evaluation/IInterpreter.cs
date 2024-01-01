using IML.CoreDataTypes;
using IML.Environments;
using IML.Functions;

namespace IML.Evaluation
{
    public interface IInterpreter
    {
        public MValue Evaluate(string expression, MEnvironment env);
        public MDataType GetDataType(string typeName);
        public ValueOrReturn PerformCall(MFunction function, MArguments args, MEnvironment currentEnv);
        public void Exit();
    }
}
