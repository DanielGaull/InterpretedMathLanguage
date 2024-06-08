using IML.CoreDataTypes;
using IML.Environments;
using IML.Functions;
using System.Collections.Generic;

namespace IML.Evaluation
{
    public interface IInterpreter
    {
        public MValue Evaluate(string expression, MEnvironment env);
        public MDataType GetDataType(string typeName);
        public ValueOrReturn PerformCall(MFunction function, MArguments args, MEnvironment currentEnv, 
            List<MType> providedGenerics);
        public void Exit();
    }
}
