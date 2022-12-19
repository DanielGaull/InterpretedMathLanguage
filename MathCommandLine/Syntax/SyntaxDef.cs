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
                    // We also want to convert all whitespaces into a single whitespace \w indicator
                    string s = symbols[i].StringArg;
                    s = Regex.Replace(s, @"\w", "\\w");
                    // TODO: Verify that this works properly with whitespace
                    s = Regex.Escape(s);
                    regexBuilder.Append(s);
                }
                else
                {
                    // We have a variable, so need to recognize that we'll have to select for anything in here
                    // And wrap it into a group
                    // Pretty simple because we don't know what this variable expression could look like
                    // We will handle selecting this later
                    regexBuilder.Append(@"(.*)");
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
