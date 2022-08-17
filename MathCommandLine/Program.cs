using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;

namespace MathCommandLine
{
    class Program
    {
        static GenericEvaluator evaluator;
        static void Main(string[] args)
        {
            evaluator = new GenericEvaluator();
        }
    }
}
