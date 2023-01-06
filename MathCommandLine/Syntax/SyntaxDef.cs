using MathCommandLine.Evaluation;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Syntax
{
    // Represents an instance of a single syntax definition
    // Contains an ordered list of syntax symbols (should alternate between strings/params)
    // Then contains the expression those symbols should evaluate to
    // Needs to do a few things:
    // Matching: Decide whether or not a given expression matches this syntax
    // Converting: Given an expression, convert it into a proper result expression
    public class SyntaxDef
    {
        private const string SYMBOL_PATTERN = @"[a-zA-Z_][a-zA-Z0-9_]*";

        List<SyntaxDefSymbol> defSymbols = new List<SyntaxDefSymbol>();
        List<SyntaxResultSymbol> resultSymbols = new List<SyntaxResultSymbol>();

        public SyntaxDef(List<SyntaxDefSymbol> defSymbols, List<SyntaxResultSymbol> resultSymbols)
        {
            this.defSymbols = defSymbols;
            this.resultSymbols = resultSymbols;
        }

        // Generates a regex for testing if a string matches, and also pulling the arguments out of the source
        public Regex GenerateMatchingRegex()
        {
            StringBuilder regexBuilder = new StringBuilder();
            for (int i = 0; i < defSymbols.Count; i++)
            {
                if (defSymbols[i].Type == SyntaxDefSymbolTypes.LiteralString)
                {
                    // Just want to search for this string, but of course have to escape it
                    string s = defSymbols[i].StringArg;
                    s = Regex.Escape(s);
                    regexBuilder.Append(s);
                }
                else if (defSymbols[i].Type == SyntaxDefSymbolTypes.SyntaxParam)
                {
                    // We have a variable, so need to recognize that we'll have to select for anything in here
                    // And wrap it into a group
                    // Pretty simple because we don't know what this variable expression could look like
                    // We will handle selecting this later
                    // However, if looking for a symbol, we WILL use that regex to force a symbol selection
                    if (defSymbols[i].ParameterArg.IsStringSymbol)
                    {
                        regexBuilder.Append("(").Append(SYMBOL_PATTERN).Append(")");
                    }
                    else
                    {
                        regexBuilder.Append(@"(.*)");
                    }
                }
                else if (defSymbols[i].Type == SyntaxDefSymbolTypes.Whitespace)
                {
                    // Allow variable whitespace, but require it
                    regexBuilder.Append(@"\s+");
                }
            }
            return new Regex(regexBuilder.ToString());
        }

        public List<SyntaxDefSymbol> GetParameterSymbols()
        {
            return defSymbols.Where((symbol) => symbol.Type == SyntaxDefSymbolTypes.SyntaxParam).ToList();
        }

        public int GetResultSymbolCount()
        {
            return resultSymbols.Count;
        }
        public SyntaxResultSymbol GetResultSymbol(int index)
        {
            return resultSymbols[index];
        }
    }
}
