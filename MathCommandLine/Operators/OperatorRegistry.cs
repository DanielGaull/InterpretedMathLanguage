using IML.CoreDataTypes;
using IML.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Operators
{
    public class OperatorRegistry
    {
        private Dictionary<MOperator, List<MFunction>> operators;

        public OperatorRegistry()
        {
            operators = new Dictionary<MOperator, List<MFunction>>();
            List<MOperator> allOps = MOperator.GetOperators();
            foreach (MOperator op in allOps)
            {
                operators.Add(op, new List<MFunction>());
            }
        }

        public void Register(MOperator op, MFunction action)
        {
            if (!operators.ContainsKey(op))
            {
                throw new OperatorException($"Operator \"{op.Name}\" (\"{op.CodeString}\") is undefined.");
            }
            List<MFunction> registry = operators[op];
            // See if the registry contains a definition for a function with the same parameters
            // If it does, throw an exception
            // This causes issues because how do we determine if two types are equal? There could be two
            // restrictions defined that have the exact same effect. Maybe add back in the future,
            // but for now, we'll allow this. However, it we will use the first function that matches, meaning
            // that duplicates won't ever be run.
            //if (registry.Any(c => c.Parameters == action.Parameters))
            //{
            //    throw new OperatorException($"Action with parameters \"{action.Parameters}\" already exists on operator " +
            //        $"\"{op.Name}\" (\"{op.CodeString}\").");
            //}
            // Register this action if we haven't failed yet
            registry.Add(action);
        }

        
    }
}
