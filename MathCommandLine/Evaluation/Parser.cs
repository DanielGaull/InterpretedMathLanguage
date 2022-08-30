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
        private const string NUMBER_REGEX_PATTERN = @"^[+-]?[0-9]+(\.[0-9]*)?$";

        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex(NUMBER_REGEX_PATTERN); // No groups
        private static readonly Regex LIST_REGEX = new Regex(@"^\{([^}]*)\}$"); // Group for the list elements
        private static readonly Regex LAMBDA_REGEX = new Regex(@"^\(([^)]*)\)=>\{([^}]*)\}$"); // Group for param list and for the expression
        private static readonly Regex TYPE_REGEX = new Regex(@"^#([a-zA-Z_][a-zA-Z0-9_]*)$"); // Group for the type name
        private static readonly Regex FUNCTION_REGEX = new Regex(@"^([a-zA-Z_][a-zA-Z0-9_]*)\((.*)\)$"); // Group for the function name and for the arguments
        private static readonly Regex SYMBOL_NAME_REGEX = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
        
        // Parameter parsing regexes
        private static readonly Regex PARAM_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex PARAM_TYPES_DELIMITER_REGEX = new Regex(@"\|");
        private static readonly Regex PARAM_REQS_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex PARAM_NAME_TYPE_REGEX = new Regex(@"(.*):(.*)"); // Group for param name and group for type(s)
        private static readonly Regex PARAM_TYPE_REQS_REGEX = new Regex(@"(?:\[(.*)\])?([a-zA-Z_][a-zA-Z0-9_]*)"); // Group for requirements, and for type name

        // Parameter Requirement regexes
        private static readonly Regex PARAM_REQ_INTEGER = new Regex(@"%");
        private static readonly Regex PARAM_REQ_POSITIVE = new Regex(@"\+");
        private static readonly Regex PARAM_REQ_NEGATIVE = new Regex(@"-");
        private static readonly Regex PARAM_REQ_LT = new Regex(@$"<\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex PARAM_REQ_GT = new Regex(@$">\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex PARAM_REQ_LTE = new Regex(@$"<=\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex PARAM_REQ_GTE = new Regex(@$">=\(({NUMBER_REGEX_PATTERN})\)");

        // Other useful regexes
        private static readonly Regex LIST_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex ARG_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");

        private FunctionDict funcDict;
        private DataTypeDict dtDict;

        public Parser(FunctionDict funcDict, DataTypeDict dtDict)
        {
            this.funcDict = funcDict;
            this.dtDict = dtDict;
        }

        // TODO: Function that removes whitespace from expressions
        // TODO: Function that converts strings to lists (returns back an expression string)
        // TODO: Error checking for if Regex doesn't return enough groups

        /// <summary>
        /// Parses a finalized expression into an AST
        /// Recall: Finalized expressions consist only of functions, literals, and variables
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Ast ParseExpression(string expression)
        {
            // TODO: Add support for big_decimal and big_int (require D or L at the end of the number literal, i.e. 750L or 0.642D)

            // 'expression' is either a function or literal
            if (FUNCTION_REGEX.IsMatch(expression))
            {
                // TODO: Select function name & args list
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

                // TODO: Parse Parameters

                // Getting this to an AST is simple
                Ast expressionAst = ParseExpression(exprString);
            }
            else if (TYPE_REGEX.IsMatch(expression))
            {
                string dtString = TYPE_REGEX.Match(expression).Groups[1].Value;
                return Ast.TypeLiteral(dtString);
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
                return new AstParameter(parameter, MDataType.Any);
            }
            var groups = PARAM_NAME_TYPE_REGEX.Match(parameter).Groups;
            string nameString = groups[1].Value;
            // Note: There can be multiple types, and any type can have requirements
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
                // Need to extract the requirements from this string
                var typeGroups = PARAM_TYPE_REQS_REGEX.Match(typeString).Groups;
                // Don't always have reqs array, but it will always appear as the first group, and be empty if it doesn't exist
                string reqsArray = typeGroups[1].Value;
                string typeName = typeGroups[2].Value;
                // Get the type from the dictionary
                MDataType type = dtDict.GetType(typeName);
                if (type.IsEmpty())
                {
                    // Invalid DT
                    throw new InvalidParseException("\"" + typeName + "\" is not a valid data type.", parameter);
                }

                // Need to evaluate all of the requirements
                // TODO: Evaluate all requirements
                // Use the requirement regexes
                string[] reqDefsStrings = PARAM_REQS_DELIMITER_REGEX.Split(reqsArray);
                ParamRequirement[] reqs = reqDefsStrings.Select((reqStr) =>
                {
                    if (PARAM_REQ_INTEGER.IsMatch(reqStr))
                    {
                        return ParamRequirement.Integer();
                    }
                    else if (PARAM_REQ_POSITIVE.IsMatch(reqStr))
                    {
                        return ParamRequirement.Positive();
                    }
                    else if (PARAM_REQ_NEGATIVE.IsMatch(reqStr))
                    {
                        return ParamRequirement.Negative();
                    }
                    else if (PARAM_REQ_LT.IsMatch(reqStr))
                    {
                        var thisReqGroup = PARAM_REQ_LT.Match(reqStr).Groups;
                        string argAsString = thisReqGroup[1].Value;
                        if (double.TryParse(argAsString, out double arg))
                        {
                            return ParamRequirement.LessThan(arg);
                        }
                        else
                        {
                            // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                        }
                    }
                    else if (PARAM_REQ_LTE.IsMatch(reqStr))
                    {
                        var thisReqGroup = PARAM_REQ_LTE.Match(reqStr).Groups;
                        string argAsString = thisReqGroup[1].Value;
                        if (double.TryParse(argAsString, out double arg))
                        {
                            return ParamRequirement.LessThanOrEqualTo(arg);
                        }
                        else
                        {
                            // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                        }
                    }
                    else if (PARAM_REQ_GT.IsMatch(reqStr))
                    {
                        var thisReqGroup = PARAM_REQ_GT.Match(reqStr).Groups;
                        string argAsString = thisReqGroup[1].Value;
                        if (double.TryParse(argAsString, out double arg))
                        {
                            return ParamRequirement.GreaterThan(arg);
                        }
                        else
                        {
                            // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                        }
                    }
                    else if (PARAM_REQ_GTE.IsMatch(reqStr))
                    {
                        var thisReqGroup = PARAM_REQ_GTE.Match(reqStr).Groups;
                        string argAsString = thisReqGroup[1].Value;
                        if (double.TryParse(argAsString, out double arg))
                        {
                            return ParamRequirement.GreaterThanOrEqualTo(arg);
                        }
                        else
                        {
                            // TODO: Invalid input somehow, though Regex should ensure no invalid inputs are provided
                        }
                    }

                    // String is not a valid restriction
                    throw new InvalidParseException("\"" + reqStr + "\" is an invalid value restriction.", parameter);
                }).ToArray();

                return new AstParameterTypeEntry(type, reqs);
            }).ToArray();
            return new AstParameter(nameString, typeEntries);
        }
    }
}
