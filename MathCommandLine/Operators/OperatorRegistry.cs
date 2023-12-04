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
        private Dictionary<MOperator, List<MClosure>> operators;

        public OperatorRegistry()
        {
            operators = new Dictionary<MOperator, List<MClosure>>();
            List<MOperator> allOps = MOperator.GetOperators();
            foreach (MOperator op in allOps)
            {
                operators.Add(op, new List<MClosure>());
            }
        }

        public void Register(MOperator op, MClosure action)
        {
            if (!operators.ContainsKey(op))
            {
                throw new OperatorException($"Operator \"{op.Name}\" (\"{op.CodeString}\") is undefined.");
            }
            List<MClosure> registry = operators[op];
            // See if the registry contains a definition for a closure with the same parameters
            // If it does, throw an exception
            // This causes issues because how do we determine if two types are equal? There could be two
            // restrictions defined that have the exact same effect. Maybe add back in the future,
            // but for now, we'll allow this. However, it we will use the first closure that matches, meaning
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
