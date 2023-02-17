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
        Parser parser;

        public SyntaxHandler(Parser parser)
        {
            this.parser = parser;
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
                string literalValue = result.Groups[i + 1].Value;
                varDict.Add(symbol.Name, new SyntaxArgument(symbol, literalValue));
            }
            return new SyntaxMatchResult(true, varDict);
        }

        // Converts one string to another using a single syntax definition
        // Performs only a single conversion
        // ** This is used as one step of a full conversion
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

        // Recursively performs conversions to completely convert a source string
        // to pure core language using the provided definitions
        // ** This is a full conversion
        public string FullConvert(List<SyntaxDef> definitions, string source)
        {
            // Steps:
            /*
             * Check if the source matches any of the definitions
             *  If yes:
             *   Convert using that definition
             *   Recursively call FullConvert on our new string
             *   
             *  If no, need to check the pieces of it (described later...)
             *   Use some sort of parser to extract the subexpressions in the string (which is in a legal format)
             *    and recursively FullConvert those
             *   However, if there are no subexpressions, then the string is final
             * 
             */
            SyntaxDef matchingDef = null;
            for (int i = 0; i < definitions.Count; i++)
            {
                SyntaxMatchResult match = Match(definitions[i], source);
                if (match.IsMatch)
                {
                    matchingDef = definitions[i];
                    break;
                }
            }
            if (matchingDef != null)
            {
                // We've got a match
                // Need to Convert this expression, then recurse over that result
                string result = Convert(matchingDef, source);
                return FullConvert(definitions, result);
            }
            else
            {
                // No match, need to parse out the subexpressions
                Ast result = parser.ParseExpression(source);

            }
            return null;
        }

        public SyntaxDef ParseSyntaxDefinitionStatement(string source, string result)
        {
            
            return null;
        }
        private List<SyntaxDefSymbol> ParseSourceString(string source)
        {
            // Source is made up of string literals, and instances of {{ [parameter] }}
            // The parameter is going to be the standard 
            return null;
        }
    }
}
