using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Syntax
{
    public class SyntaxHandler
    {
        IInterpreter interpreter;

        public SyntaxHandler(IInterpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public SyntaxMatchResult Match(SyntaxDef def, string source)
        {
            Regex regex = def.GenerateMatchingRegex();
            var result = regex.Match(source);
            if (!result.Success)
            {
                return new SyntaxMatchResult(false);
            }
            Dictionary<string, MValue> varDict = new Dictionary<string, MValue>();
            List<SyntaxSymbol> varSymbols = def.GetParameterSymbols();
            for (int i = 0; i < varSymbols.Count; i++)
            {
                SyntaxParameter symbol = varSymbols[i].ParameterArg;
                MValue val;
                string literalValue = result.Groups[i + 1].Value;
                if (symbol.IsStringSymbol)
                {
                    // The literal value is wrapped into a string
                    val = MValue.String(literalValue);
                }
                else if (symbol.IsWrappingLambda)
                {
                    // Need to throw the literal value into a lambda
                    val = MValue.Lambda(new CoreDataTypes.MLambda(MParameters.Empty,
                        new MExpression(literalValue)));
                }
                else
                {
                    val = interpreter.Evaluate(new MExpression(literalValue), MArguments.Empty);
                }
                varDict.Add(symbol.Name, val);
            }
            return new SyntaxMatchResult(true, varDict);
        }
    }
}
