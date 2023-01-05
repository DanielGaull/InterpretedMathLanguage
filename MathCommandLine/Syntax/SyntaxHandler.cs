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
            Dictionary<string, SyntaxArgument> varDict = new Dictionary<string, SyntaxArgument>();
            List<SyntaxDefSymbol> varSymbols = def.GetParameterSymbols();
            for (int i = 0; i < varSymbols.Count; i++)
            {
                SyntaxParameter symbol = varSymbols[i].ParameterArg;
                //MValue val;
                string literalValue = result.Groups[i + 1].Value;
                //if (symbol.IsStringSymbol)
                //{
                //    // The literal value is wrapped into a string
                //    val = MValue.String(literalValue);
                //}
                //else if (symbol.IsWrappingLambda)
                //{
                //    // Need to throw the literal value into a lambda
                //    val = MValue.Lambda(new CoreDataTypes.MLambda(MParameters.Empty,
                //        new MExpression(literalValue)));
                //}
                //else
                //{
                //    val = interpreter.Evaluate(new MExpression(literalValue), MArguments.Empty);
                //}
                varDict.Add(symbol.Name, new SyntaxArgument(symbol, literalValue));
            }
            return new SyntaxMatchResult(true, varDict);
        }

        public string Convert(SyntaxDef def, string source)
        {
            SyntaxMatchResult match = Match(def, source);
            if (!match.IsMatch)
            {
                // TODO: Add an exception here because syntax is attempting to be handled when it can't be
                return null;
            }
            // This is nice because we already have all of the variable string representations
            // However, need to handle string/lambda cases
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < def.GetResultSymbolCount(); i++)
            {
                SyntaxResultSymbol sym = def.GetResultSymbol(i);
                if (sym.Type == SyntaxResultSymbolTypes.Argument)
                {
                    // Need to grab the string rep of this and possibly extend upon it
                    string name = sym.ArgName;
                    SyntaxArgument arg = match.GetValue(name);
                    string literal = arg.LiteralValue;
                    if (arg.DefiningParameter.IsStringSymbol)
                    {
                        literal = "\"" + literal + "\"";
                    }
                    else if (arg.DefiningParameter.IsWrappingLambda)
                    {
                        literal = "()=>{" + literal + "}";
                    }
                    result.Append(literal);
                }
                else if (sym.Type == SyntaxResultSymbolTypes.ExpressionPiece)
                {
                    result.Append(sym.ExpressionArg);
                }
            }
            return result.ToString();
        }
    }
}
