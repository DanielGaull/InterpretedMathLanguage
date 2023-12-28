using IML.Evaluation;
using IML.Evaluation.AST.ValueAsts;
using IML.Exceptions;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IML.Syntax
{
    class SyntaxParser : Parser
    {
        private List<SyntaxDef> definitions;

        public SyntaxParser(List<SyntaxDef> definitions)
        {
            this.definitions = definitions;
        }

        public override Ast Parse(string expression)
        {
            Ast result = base.Parse(expression);
            // If result is not valid, then we want to do our syntax evaluation stuff on it
            if (result.Type == AstTypes.Invalid)
            {
                return ParseSyntax(expression);
            }
            return result;
        }

        private Ast ParseSyntax(string expression)
        {
            // Check if any syntax matches this expression
            SyntaxDef matchingDef = null;
            for (int i = 0; i < definitions.Count; i++)
            {
                SyntaxMatchResult match = Match(definitions[i], expression);
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
                string res = Convert(matchingDef, expression);
                return Parse(res);
            }
            else
            {
                // No match, so we have an error
                throw new InvalidParseException(expression);
            }
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
                        literal = "()~>{" + literal + "}";
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

        public SyntaxMatchResult Match(SyntaxDef def, string source)
        {
            Regex regex = def.GenerateMatchingRegex();
            var result = regex.Match(source);
            if (!result.Success)
            {
                return new SyntaxMatchResult(false);
            }
            // Matched, but did not match the entire thing
            if (result.Value != source)
            {
                return new SyntaxMatchResult(false);
            }
            // Make sure that parentheses/braces are handled properly when we split these
            // If there is an opening brace or paren in one part of the split, it has to have a corresponding
            // closing one, and vice versa
            Dictionary<string, SyntaxArgument> varDict = new Dictionary<string, SyntaxArgument>();
            List<SyntaxDefSymbol> varSymbols = def.GetParameterSymbols();
            for (int i = 0; i < varSymbols.Count; i++)
            {
                SyntaxParameter symbol = varSymbols[i].ParameterArg;
                string literalValue = result.Groups[i + 1].Value;
                if (literalValue.CountChar('(') != literalValue.CountChar(')') ||
                    literalValue.CountChar('{') != literalValue.CountChar('}'))
                {
                    // Paren/brace imbalance, so we actually don't have a match
                    return new SyntaxMatchResult(false);
                }
                varDict.Add(symbol.Name, new SyntaxArgument(symbol, literalValue));
            }
            return new SyntaxMatchResult(true, varDict);
        }
    }
}
