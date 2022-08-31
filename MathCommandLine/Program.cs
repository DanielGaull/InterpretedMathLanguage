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
            //evaluator = new GenericEvaluator();

            //Regex PARAM_TYPE_REQS_REGEX = new Regex(@"(?:\[(.*)\])?([a-zA-Z_][a-zA-Z0-9_]*)");
            //var groups = PARAM_TYPE_REQS_REGEX.Match("number").Groups;
            //Console.WriteLine("'" + groups[0].Value + "'; '" + groups[1].Value + "'; '" + groups[2].Value + "'");

            Regex t = new Regex(@"^[+-]?[0-9]+(\.[0-9]*)?$");

            List<MFunction> coreFuncs = new List<MFunction>();
            FunctionDict funcDict = new FunctionDict(coreFuncs);
            DataTypeDict dtDict = new DataTypeDict(MDataType.Number, MDataType.List, MDataType.Lambda,
                MDataType.Type, MDataType.Error);
            Parser parser = new Parser(funcDict, dtDict);

            var res = parser.ParseExpression("(x:[]number)=>{5}");
            Console.WriteLine("Done");
        }
    }
}
