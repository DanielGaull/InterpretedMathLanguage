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
        private const string WRAPPER_EXCLUSION_PATTERN = @"(?![^\(\[\{]*[\)\]\}])";
        private const string LAMBDA_PATTERN = @"\((.*)\)=>\{(.*)\}";  // Group for param list and for the expression
        private const string SYMBOL_PATTERN = @"[a-zA-Z_][a-zA-Z0-9_]*";

        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex("^" + NUMBER_REGEX_PATTERN + "$");
        // Group for the list elements
        private static readonly Regex LIST_REGEX = new Regex(@"^\{(.*)\}$");
        private static readonly Regex LAMBDA_REGEX = new Regex("^" + LAMBDA_PATTERN + "$");
        // Group for the type name
        private static readonly Regex TYPE_REGEX = new Regex(@"^#([a-zA-Z_][a-zA-Z0-9_]*)$");
        // Group for the function name and for the arguments
        private static readonly Regex FUNCTION_REGEX = new Regex(@$"^({SYMBOL_PATTERN})\((.*)\)$");
        // Group for param list, expression, and args
        private static readonly Regex LAMBDA_CALL_REGEX = new Regex(@$"^{LAMBDA_PATTERN}\((.*)\)$");
        // Group for the thing being called, and a group for the arguments
        private static readonly Regex CALL_REGEX = new Regex(@"^(.*)\((.*)\)$");
        
        // Parameter parsing regexes
        private static readonly Regex PARAM_DELIMITER_REGEX = new Regex(@"," + WRAPPER_EXCLUSION_PATTERN);
        private static readonly Regex PARAM_TYPES_DELIMITER_REGEX = new Regex(@"\|");
        private static readonly Regex PARAM_REQS_DELIMITER_REGEX = new Regex(@"," + WRAPPER_EXCLUSION_PATTERN);
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
        private static readonly Regex LIST_DELIMITER_REGEX = new Regex(@"," + WRAPPER_EXCLUSION_PATTERN);
        private static readonly Regex ARG_DELIMITER_REGEX = new Regex(@"," + WRAPPER_EXCLUSION_PATTERN);
        private static readonly Regex SYMBOL_NAME_REGEX = new Regex(@$"^{SYMBOL_PATTERN}$");
        // Group to parse out the characters in the string
        private static readonly Regex STRING_LITERAL_REGEX = new Regex("\"([^\"]*)\"");

        public Parser()
        {
        }

        // TODO: Function that removes whitespace from expressions
        // TODO: Function that converts strings to lists (returns back an expression string)
        // TODO: Error checking for if Regex doesn't return enough groups

        /// <summary>
        /// Converts all string literals appearing in an expression to list literals within the string
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>The modified expression</returns>
        public string ConvertStringsToLists(string expression)
        {
            return STRING_LITERAL_REGEX.Replace(expression, delegate(Match match)
            {
                MList list = Utilities.StringToMList(match.Groups[1].Value);
                return list.ToString();
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

            // 'expression' is either a call, variable, or literal
            if (CALL_REGEX.IsMatch(expression))
            {
                // Performing some sort of call
                var groups = CALL_REGEX.Match(expression).Groups;
                string callerString = groups[1].Value;
                string argsString = groups[2].Value;

                Ast caller = ParseExpression(callerString);

                string[] argStrings = ARG_DELIMITER_REGEX.Split(argsString);
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
                string[] elementStrings = LIST_DELIMITER_REGEX.Split(elements);
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
                AstParameter[] parsedParams = PARAM_DELIMITER_REGEX.Split(paramsString).Select((paramString) =>
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
            string[] eachTypes = PARAM_TYPES_DELIMITER_REGEX.Split(typesString);
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
                    string[] reqDefsStrings = PARAM_REQS_DELIMITER_REGEX.Split(reqsArray);
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
    }
}
