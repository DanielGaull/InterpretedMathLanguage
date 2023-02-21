using MathCommandLine.Evaluation;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Syntax
{
    public class SyntaxHandler
    {
        Parser parser;
        List<char> illegalChars;

        public SyntaxHandler(Parser parser, List<char> illegalChars)
        {
            this.illegalChars = illegalChars;
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
            Ast result = parser.ParseExpression(source);
            // Right now, lambdas do not parse their bodies until later
            // Only types that require any handling are calls and lists since those
            // are the only ASTs with sub-ASTs
            switch (result.Type)
            {
                case AstTypes.ListLiteral:
                    // Need to syntax-handle each element
                    string[] results = result.AstCollectionArg.Select(elem =>
                    {
                        if (elem.Type == AstTypes.Invalid)
                        {
                            return FullConvert(definitions, elem.Expression);
                        }
                        else
                        {
                            return elem.ToExpressionString();
                        }
                    }).ToArray();
                    return parser.ListToString(results);
                case AstTypes.Call:
                    // Need to syntax-handle the callee and each element
                    string callee = "";
                    if (result.CalledAst.Type == AstTypes.Invalid)
                    {
                        callee = FullConvert(definitions, result.CalledAst.Expression);
                    }
                    else
                    {
                        callee = result.CalledAst.ToExpressionString();
                    }
                    string[] args = result.AstCollectionArg.Select(elem =>
                    {
                        if (elem.Type == AstTypes.Invalid)
                        {
                            return FullConvert(definitions, elem.Expression);
                        }
                        else
                        {
                            return elem.ToExpressionString();
                        }
                    }).ToArray();
                    return parser.CallToString(callee, args);
                case AstTypes.LambdaLiteral:
                    // Need to syntax-handle the body
                    string body = null;
                    if (result.Body.Type == AstTypes.Invalid)
                    {
                        body = FullConvert(definitions, result.Body.Expression);
                    }
                    else
                    {
                        body = result.Body.ToString();
                    }
                    return parser.LambdaToString(result, body);
                case AstTypes.Invalid:
                    // Check if any syntax matches this expression
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
                        string res = Convert(matchingDef, source);
                        return FullConvert(definitions, res);
                    }
                    else
                    {
                        // No match, so we have an error
                        throw new InvalidParseException(source);
                    }
                default:
                    return source;
            }
        }

        public SyntaxDef ParseSyntaxDefinitionLine(string line)
        {
            if (line.StartsWith("syntax"))
            {
                List<string> tokens = Regex.Matches(line, @"[\""].+?[\""]|[^ ]+")
                                    .Cast<Match>()
                                    .Select(m => m.Value)
                                    .ToList();
                string src = tokens[1];
                var srcMatch = Regex.Match(src, "\"(.*)\"");
                if (!srcMatch.Success)
                {
                    throw new IllegalSyntaxException(line);
                }
                SyntaxDef result = ParseSyntaxDefinitionStatement(srcMatch.Groups[1].Value, tokens[2]);
                if (result == null)
                {
                    // Means something went wrong
                    throw new IllegalSyntaxException(line);
                }
                return result;
            }
            throw new IllegalSyntaxException(line);
        }
        public SyntaxDef ParseSyntaxDefinitionStatement(string source, string result)
        {
            List<SyntaxDefSymbol> defSymbols = ParseSourceString(source);
            return null;
        }
        // Returns null if something goes wrong
        private List<SyntaxDefSymbol> ParseSourceString(string source)
        {
            // Source is made up of string literals, and instances of {{ [parameter] }}
            StringBuilder currentBuilder = new StringBuilder();
            List<SyntaxDefSymbol> symbolDefs = new List<SyntaxDefSymbol>();
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != '{')
                {
                    if (illegalChars.Contains(source[i]))
                    { 
                        return null;
                    }
                    else if (Regex.IsMatch(source[i].ToString(), "\\s"))
                    {
                        // Found whitespace. Save off current character, find end of whitespace, and then add a whitespace
                        if (currentBuilder.Length > 0)
                        {
                            symbolDefs.Add(new SyntaxDefSymbol(currentBuilder.ToString()));
                        }
                        while (Regex.IsMatch(source[i].ToString(), "\\s"))
                        {
                            i++;
                        }
                        symbolDefs.Add(new SyntaxDefSymbol());
                    }
                    else
                    {
                        currentBuilder.Append(source[i]);
                    }
                }
                else
                {
                    // Look at param definition, save off current string literal
                    if (currentBuilder.Length > 0)
                    {
                        symbolDefs.Add(new SyntaxDefSymbol(currentBuilder.ToString()));
                    }
                    currentBuilder = new StringBuilder();

                    // Get info about param here
                    StringBuilder paramNameBuilder = new StringBuilder();
                    for (i++; source[i] != '}'; i++)
                    {
                        if (i >= source.Length)
                        {
                            // No valid closing brace, and we're over the limit of the string
                            // Error so return null
                            return null;
                        }
                        paramNameBuilder.Append(source[i]);
                    }

                    // Make sure that this is a valid param name
                    string paramName = paramNameBuilder.ToString();
                    bool symbolStr = false;
                    bool literalCode = false;
                    if (paramName.StartsWith('$'))
                    {
                        symbolStr = true;
                        paramName = paramName.Substring(1);
                    }
                    else if (paramName.StartsWith('^'))
                    {
                        literalCode = true;
                        paramName = paramName.Substring(1);
                    }

                    if (!Regex.IsMatch(paramName, "^[A-Za-z_][A-Za-z0-9_]*$"))
                    {
                        return null;
                    }

                    // Create and add our new parameter
                    symbolDefs.Add(new SyntaxDefSymbol(new SyntaxParameter(paramName, symbolStr, literalCode)));
                }
            }
            // Add the last string segment
            if (currentBuilder.Length > 0)
            {
                symbolDefs.Add(new SyntaxDefSymbol(currentBuilder.ToString()));
            }
            return symbolDefs;
        }
    }
}
