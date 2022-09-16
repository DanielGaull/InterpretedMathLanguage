using MathCommandLine.CoreDataTypes;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Evaluation
{
    /// <summary>
    /// Parses strings to ASTs. Capable of parsing many different types of strings, such as expressions and declarations.
    /// </summary>
    public class Parser
    {
        // Regexes for common necessities
        private const string NUMBER_REGEX_PATTERN = @"[+-]?[0-9]+(\.[0-9]*)?";
        private const string SYMBOL_PATTERN = @"[a-zA-Z_][a-zA-Z0-9_]*";

        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex("^" + NUMBER_REGEX_PATTERN + "$");
        // Group for the list elements
        private static readonly Regex LIST_REGEX = new Regex(@"^\{(.*)\}$");
        private static readonly Regex LAMBDA_REGEX = new Regex(@"^\((.*)\)=>\{(.*)\}$");
        // Group for the type name
        private static readonly Regex TYPE_REGEX = new Regex(@"^#([a-zA-Z_][a-zA-Z0-9_]*)$");

        // Call parsing values
        private const char CALL_END_WRAPPER = ')';
        private const char CALL_START_WRAPPER = '(';

        // Param parsing values
        private const char GENERIC_END_WRAPPER = ')';
        private const char GENERIC_START_WRAPPER = '(';

        // Parameter parsing regexes
        private const char PARAM_DELIMITER = ',';
        private const char PARAM_TYPES_DELIMITER = '|';
        private const char PARAM_REQS_DELIMITER = ',';
        // Group for param name and group for type(s)
        private static readonly Regex PARAM_NAME_TYPE_REGEX = new Regex(@"(.*):(.*)");
        // Group for restrictions, and for type name
        private static readonly Regex PARAM_TYPE_RESTS_REGEX = new Regex(@"(?:\[(.*)\])?([a-zA-Z_][a-zA-Z0-9_]*)");

        // Value restriction regexes
        private static readonly Regex VALUE_REST_INTEGER = new Regex(@"%");
        private static readonly Regex VALUE_REST_POSITIVE = new Regex(@"\+");
        private static readonly Regex VALUE_REST_NEGATIVE = new Regex(@"-");
        private static readonly Regex VALUE_REST_LT = new Regex(@$"<\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex VALUE_REST_GT = new Regex(@$">\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex VALUE_REST_LTE = new Regex(@$"<=\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex VALUE_REST_GTE = new Regex(@$">=\(({NUMBER_REGEX_PATTERN})\)");

        // Other useful regexes
        private const char LIST_DELIMITER = ',';
        private const char ARG_DELIMITER = ',';
        private static readonly Regex SYMBOL_NAME_REGEX = new Regex(@$"^{SYMBOL_PATTERN}$");
        // Group to parse out the characters in the string
        private static readonly Regex STRING_LITERAL_REGEX = new Regex("\"([^\"]*)\"");

        public Parser()
        {
        }

        // TODO: Function that removes whitespace from expressions

        /// <summary>
        /// Converts all string literals appearing in an expression to _str string declarations
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>The modified expression</returns>
        public string ConvertStringsToLists(string expression)
        {
            return STRING_LITERAL_REGEX.Replace(expression, delegate(Match match)
            {
                MList list = Utilities.StringToMList(match.Groups[1].Value);
                return "_str(" + list.ToString() + ")";
            });
        }

        /// <summary>
        /// Parses a finalized expression into an AST
        /// Recall: Finalized expressions consist only of functions, literals, and variables
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Ast ParseExpression(string expression)
        {
            // TODO: Add support for big_decimal and big_int (require D or L at the end of the number literal, i.e. 750L or 0.642D)

            // Initially, attempt to extract an expression from parentheses
            while (IsParamWrapped(expression))
            {
                // Pull out the expression without the first and last characters
                expression = expression.Substring(1, expression.Length - 2);
            }

            // 'expression' is either a call, variable, or literal
            // May be something that is wrapped entirely in parenthesis
            CallMatch attempedCallMatch = MatchCall(expression);

            if (attempedCallMatch.IsMatch)
            {
                // Performing some sort of call
                string callerString = attempedCallMatch.Caller;
                string argsString = attempedCallMatch.Args;

                Ast caller = ParseExpression(callerString);

                string[] argStrings = SplitByDelimiter(argsString, ARG_DELIMITER);
                // Parse all the arguments
                Ast[] args = argStrings.Select(ParseExpression).ToArray();

                return Ast.Call(caller, args);
            }
            else if (NUMBER_REGEX.IsMatch(expression))
            {
                // Parse the double and throw it into the AST
                double number = double.Parse(expression);
                return Ast.NumberLiteral(number);
            }
            else if (LIST_REGEX.IsMatch(expression))
            {
                // Extract the elements of the list
                string elements = LIST_REGEX.Match(expression).Groups[1].Value;
                // Separate by the list delimiter
                string[] elementStrings = SplitByDelimiter(elements, LIST_DELIMITER);
                List<Ast> elementAsts = new List<Ast>();
                foreach (string str in elementStrings)
                {
                    elementAsts.Add(ParseExpression(str));
                }
                return Ast.ListLiteral(elementAsts.ToArray());
            }
            else if (LAMBDA_REGEX.IsMatch(expression))
            {
                // Two parts: The expression, and the parameters
                var groups = LAMBDA_REGEX.Match(expression).Groups;
                string paramsString = groups[1].Value;
                string exprString = groups[2].Value;

                // Parse Parameters
                AstParameter[] parsedParams = SplitByDelimiter(paramsString, PARAM_DELIMITER).Select((paramString) =>
                {
                    return ParseParameter(paramString);
                }).ToArray();

                return Ast.LambdaLiteral(parsedParams, exprString);
            }
            else if (TYPE_REGEX.IsMatch(expression))
            {
                string dtString = TYPE_REGEX.Match(expression).Groups[1].Value;
                return Ast.TypeLiteral(dtString);
            }
            else if (SYMBOL_NAME_REGEX.IsMatch(expression))
            {
                // We've got a variable
                return Ast.Variable(expression);
            }
            throw new InvalidParseException(expression);
        }

        /// <summary>
        /// Parses a single parameter into an AstParameter object
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public AstParameter ParseParameter(string parameter)
        {
            // Check if no type provided. If parameter simply matches valid variable name, then it is of the any type
            if (SYMBOL_NAME_REGEX.IsMatch(parameter))
            {
                // Name is the parameter string, and type is of the any type
                return new AstParameter(parameter, MDataType.Any.Name);
            }
            var groups = PARAM_NAME_TYPE_REGEX.Match(parameter).Groups;
            string nameString = groups[1].Value;
            // Note: There can be multiple types, and any type can have restrictions
            string typesString = groups[2].Value;
            if (!SYMBOL_NAME_REGEX.IsMatch(nameString))
            {
                // Not a valid parameter name
                throw new InvalidParseException("\"" + nameString + "\" is not a valid parameter name.", parameter);
            }
            // Start getting out the data types
            string[] eachTypes = SplitByDelimiter(typesString, PARAM_TYPES_DELIMITER);
            AstParameterTypeEntry[] typeEntries = eachTypes.Select((typeString) =>
            {
                // Need to extract the restrictions from this string
                var typeGroups = PARAM_TYPE_RESTS_REGEX.Match(typeString).Groups;
                // Don't always have reqs array, but it will always appear as the first group, and be empty if it doesn't exist
                string reqsArray = typeGroups[1].Value;
                string typeName = typeGroups[2].Value;

                ValueRestriction[] reqs = new ValueRestriction[0];
                // Need to evaluate all of the restrictions
                if (reqsArray.Length > 0)
                {
                    string[] reqDefsStrings = SplitByDelimiter(reqsArray, PARAM_REQS_DELIMITER);
                    reqs = reqDefsStrings.Select((reqStr) =>
                    {
                        if (VALUE_REST_INTEGER.IsMatch(reqStr))
                        {
                            return ValueRestriction.Integer();
                        }
                        else if (VALUE_REST_POSITIVE.IsMatch(reqStr))
                        {
                            return ValueRestriction.Positive();
                        }
                        else if (VALUE_REST_NEGATIVE.IsMatch(reqStr))
                        {
                            return ValueRestriction.Negative();
                        }
                        else if (VALUE_REST_LT.IsMatch(reqStr))
                        {
                            var thisReqGroup = VALUE_REST_LT.Match(reqStr).Groups;
                            string argAsString = thisReqGroup[1].Value;
                            if (double.TryParse(argAsString, out double arg))
                            {
                                return ValueRestriction.LessThan(arg);
                            }
                            else
                            {
                                // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                            }
                        }
                        else if (VALUE_REST_LTE.IsMatch(reqStr))
                        {
                            var thisReqGroup = VALUE_REST_LTE.Match(reqStr).Groups;
                            string argAsString = thisReqGroup[1].Value;
                            if (double.TryParse(argAsString, out double arg))
                            {
                                return ValueRestriction.LessThanOrEqualTo(arg);
                            }
                            else
                            {
                                // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                            }
                        }
                        else if (VALUE_REST_GT.IsMatch(reqStr))
                        {
                            var thisReqGroup = VALUE_REST_GT.Match(reqStr).Groups;
                            string argAsString = thisReqGroup[1].Value;
                            if (double.TryParse(argAsString, out double arg))
                            {
                                return ValueRestriction.GreaterThan(arg);
                            }
                            else
                            {
                                // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                            }
                        }
                        else if (VALUE_REST_GTE.IsMatch(reqStr))
                        {
                            var thisReqGroup = VALUE_REST_GTE.Match(reqStr).Groups;
                            string argAsString = thisReqGroup[1].Value;
                            if (double.TryParse(argAsString, out double arg))
                            {
                                return ValueRestriction.GreaterThanOrEqualTo(arg);
                            }
                            else
                            {
                                // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                            }
                        }

                        // String is not a valid restriction
                        throw new InvalidParseException("\"" + reqStr + "\" is an invalid value restriction.", parameter);
                    }).ToArray();
                }

                return new AstParameterTypeEntry(typeName, reqs);
            }).ToArray();
            return new AstParameter(nameString, typeEntries);
        }

        /// <summary>
        /// Returns true if the expression is wrapped in a pair of matching parentheses
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool IsParamWrapped(string expression)
        {
            if (expression[0] != GENERIC_START_WRAPPER || expression[expression.Length - 1] != GENERIC_END_WRAPPER)
            {
                return false;
            }

            int wrapperCounter = 0;
            int startWrapperIndex = -1;
            for (int i = expression.Length - 2; i >= 0; i--)
            {
                if (expression[i] == CALL_END_WRAPPER)
                {
                    wrapperCounter++;
                }
                else if (expression[i] == CALL_START_WRAPPER)
                {
                    if (wrapperCounter == 0)
                    {
                        // We've found the corresponding location!
                        startWrapperIndex = i;
                        break;
                    }
                    else
                    {
                        wrapperCounter--;
                    }
                }
            }
            if (startWrapperIndex == 0)
            {
                // Starting parenthesis was the first character, so this is an expression wrapped in parentheses
                return true;
            }
            return false;
        }

        /// <summary>
        /// Splits "expr" into many strings delimited by "delimiter", but not if the delimiter is wrapped in parentheses or braces
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        private static string[] SplitByDelimiter(string expr, char delimiter)
        {
            int parenCounter = 0;
            int braceCounter = 0;
            List<string> substrings = new List<string>();
            // TODO: Finish
            StringBuilder currentString = new StringBuilder();
            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];
                if (c == delimiter)
                {
                    // If not in parentheses or braces, then we add this as a split string
                    if (parenCounter == 0 && braceCounter == 0)
                    {
                        substrings.Add(currentString.ToString());
                        currentString.Clear();
                    }
                    else
                    {
                        // The delimiter is part of a currently-being-built string
                        currentString.Append(c);
                    }
                }
                else
                {
                    // Add the character to our current string, update the counts as necessary
                    currentString.Append(c);
                    if (c == '(')
                    {
                        parenCounter++;
                    }
                    else if (c == ')')
                    {
                        parenCounter--;
                    }
                    else if (c == '{')
                    {
                        braceCounter++;
                    }
                    else if (c == '}')
                    {
                        braceCounter--;
                    }
                }
            }
            // Remember to add the string that we are in the middle of building
            substrings.Add(currentString.ToString());
            return substrings.ToArray();
        }
        
        /// <summary>
        /// Attempts to match this expression to a "call", returning if the match succeeded, and the
        /// caller/arguments
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private CallMatch MatchCall(string expression)
        {
            // What makes a call a call?
            // Well, it's simply some "thing" followed by parentheses, which may have arguments in them
            // It's important to note that the "thing" must have balanced brackets: that is, balanced curly braces
            // and balanced parentheses. That way, '((x)' doesn't count as a call, because it appears to really be
            // the start of a lambda ex. '((x)=>{_add(x,7)})
            // So, lets start from the back. The last character should be a parenthesis. Then, we simply find its
            // matching balanced one. As long as the match is not the first character in the string, then
            // there is something before the pair, and we've found what should be interpreted as a call!
            if (expression[expression.Length - 1] != CALL_END_WRAPPER)
            {
                // Last character is not a closing parenthesis
                return CallMatch.Failure;
            }
            // Now time to backtrack. We'll go character by character. Depending on parentheses we find, we'll keep a count
            // Need to find the open wrapper when the counter is zero though
            // Start at second-to-last char b/c we know the last one is a ')'
            int wrapperCounter = 0;
            int startWrapperIndex = -1;
            for (int i = expression.Length - 2; i >= 0; i--)
            {
                if (expression[i] == CALL_END_WRAPPER)
                {
                    wrapperCounter++;
                }
                else if (expression[i] == CALL_START_WRAPPER)
                {
                    if (wrapperCounter == 0)
                    {
                        // We've found the corresponding location!
                        startWrapperIndex = i;
                        break;
                    }
                    else
                    {
                        wrapperCounter--;
                    }
                }
            }
            if (startWrapperIndex < 0 || startWrapperIndex == 0)
            {
                // Didn't find the matching start in the whole string, or the starting parenthesis was the first character,
                // meaning this is just an expression wrapped in parentheses
                return CallMatch.Failure;
            }

            // Everything before the start index is what was called (the callee/caller, terminology is not consistent here)
            string calledPart = expression.Substring(0, startWrapperIndex);

            string argsString = expression.Substring(startWrapperIndex + 1, expression.Length - (startWrapperIndex + 1) - 1);

            return new CallMatch(true, calledPart, argsString);
        }
        private struct CallMatch
        {
            public bool IsMatch;
            public string Caller;
            public string Args;

            public CallMatch(bool isMatch, string caller, string args)
            {
                IsMatch = isMatch;
                Caller = caller;
                Args = args;
            }

            public static readonly CallMatch Failure = new CallMatch(false, null, null);
        }
    }
}
