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

        List<SyntaxSymbol> symbols = new List<SyntaxSymbol>();
        public string ResultExpression { get; private set; }

        public SyntaxDef(List<SyntaxSymbol> symbols, string result)
        {
            this.symbols = symbols;
            ResultExpression = result;
        }

        // Generates a regex for testing if a string matches, and also pulling the arguments out of the source
        public Regex GenerateMatchingRegex()
        {
            StringBuilder regexBuilder = new StringBuilder();
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i].Type == SyntaxSymbolTypes.LiteralString)
                {
                    // Just want to search for this string, but of course have to escape it
                    string s = symbols[i].StringArg;
                    s = Regex.Escape(s);
                    regexBuilder.Append(s);
                }
                else if (symbols[i].Type == SyntaxSymbolTypes.SyntaxParam)
                {
                    // We have a variable, so need to recognize that we'll have to select for anything in here
                    // And wrap it into a group
                    // Pretty simple because we don't know what this variable expression could look like
                    // We will handle selecting this later
                    // However, if looking for a symbol, we WILL use that regex to force a symbol selection
                    if (symbols[i].ParameterArg.IsStringSymbol)
                    {
                        regexBuilder.Append(SYMBOL_PATTERN);
                    }
                    else
                    {
                        regexBuilder.Append(@"(.*)");
                    }
                }
                else if (symbols[i].Type == SyntaxSymbolTypes.Whitespace)
                {
                    // Allow variable whitespace, but require it
                    regexBuilder.Append(@"\w+");
                }
            }
            return new Regex(regexBuilder.ToString());
        }

        public List<SyntaxSymbol> GetParameterSymbols()
        {
            return symbols.Where((symbol) => symbol.Type == SyntaxSymbolTypes.SyntaxParam).ToList();
        }
    }
}
