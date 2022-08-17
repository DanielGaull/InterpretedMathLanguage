using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Text.RegularExpressions;

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
