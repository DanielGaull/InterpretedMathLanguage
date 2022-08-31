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

            Parser parser = new Parser();

            string expr = "{5,3,{1,2}}";
            Console.WriteLine(evaluator.Evaluate(new MExpression(expr), new MArguments()).ToLongString());
        }
    }
}
