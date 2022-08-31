using MathCommandLine.CoreDataTypes;
using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MathCommandLine
{
    class Program
    {
        static GenericEvaluator evaluator;
        static void Main(string[] args)
        {
            evaluator = new GenericEvaluator();

            // Simple reading for now
            while (true)
            {
                Console.Write("Enter Expression: ");
                string input = Console.ReadLine();
                MValue result = evaluator.Evaluate(new MExpression(input), new MArguments());
                string resultString = result.ToLongString();
                Console.WriteLine(resultString);
            }
        }
    }
}
