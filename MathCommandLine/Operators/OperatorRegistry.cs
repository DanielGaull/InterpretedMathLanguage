using MathCommandLine.CoreDataTypes;
using MathCommandLine.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Operators
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
            if (registry.Any(c => c.Parameters == action.Parameters))
            {
                throw new OperatorException($"Action with parameters \"{action.Parameters}\" already exists on operator " +
                    $"\"{op.Name}\" (\"{op.CodeString}\").");
            }
            // Register this action if we haven't failed yet
            registry.Add(action);
        }

        
    }
}
