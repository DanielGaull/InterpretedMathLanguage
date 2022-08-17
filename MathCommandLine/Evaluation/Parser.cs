using MathCommandLine.Exceptions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Evaluation
{
    /// <summary>
    /// Parses strings to ASTs. Capable of parsing many different types of strings, such as expressions and declarations.
    /// </summary>
    public class Parser
    {
        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex(@"^[+-]?[0-9]+(\.[0-9]*)?$"); // No groups
        private static readonly Regex LIST_REGEX = new Regex(@"^\{([^}]*)\}$"); // Group for the list elements
        private static readonly Regex LAMBDA_REGEX = new Regex(@"^\(([^)]*)\)=>\{([^}]*)\}$"); // Group for param list and for the expression
        private static readonly Regex TYPE_REGEX = new Regex(@"^#([a-zA-Z_][a-zA-Z0-9_]*)$"); // Group for the type name
        private static readonly Regex FUNCTION_REGEX = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*\(.*\)$"); // TODO: Group for the function name and for the arguments

        // Other useful regexes
        private static readonly Regex LIST_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex PARAM_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex PARAM_TYPES_DELIMITER_REGEX = new Regex(@"\|");
        private static readonly Regex PARAM_NAME_TYPE_SPLITTER_REGEX = new Regex(@":");
        private static readonly Regex ARG_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");

        // TODO: Function that removes whitespace from expressions
        // TODO: Function that converts strings to lists (returns back an expression string)

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
            // TODO
            return null;
        }
    }
}
