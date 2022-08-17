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
        // TODO: Move parsing an expression to an AST to its own class
        // That way, the parser can handle parsing actual expressions as well as function/datatype/etc. definitions

        private const string NUMBER_REGEX_STR = @"[+-]?[0-9]+(\.[0-9]*)?";
        private const string LIST_REGEX_STR = @"\{[^}]*\}";
        private const string LAMBDA_REGEX_STR = @"\([^)]*\)=>\{[^}]*\}";
        private const string TYPE_REGEX_STR = @"#[a-zA-Z_][a-zA-Z0-9_]*";
        private const string FUNCTION_REGEX_STR = @"[a-zA-Z_][a-zA-Z0-9_]*\(.*\)";

        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex(NUMBER_REGEX_STR);
        private static readonly Regex LIST_REGEX = new Regex(LIST_REGEX_STR);
        private static readonly Regex LAMBDA_REGEX = new Regex(LAMBDA_REGEX_STR);
        private static readonly Regex TYPE_REGEX = new Regex(TYPE_REGEX_STR);
        private static readonly Regex FUNCTION_REGEX = new Regex(FUNCTION_REGEX_STR);
        // Regexes for matching language symbols, where they are the ONLY thing in the string (i.e. find if the whole string is a list) 
        private static readonly Regex NUMBER_ONLY_REGEX = new Regex("^" + NUMBER_REGEX_STR + "$");
        private static readonly Regex LIST_ONLY_REGEX = new Regex("^" + LIST_REGEX_STR + "$");
        private static readonly Regex LAMBDA_ONLY_REGEX = new Regex("^" + LAMBDA_REGEX_STR + "$");
        private static readonly Regex TYPE_ONLY_REGEX = new Regex("^" + TYPE_REGEX_STR + "$");
        private static readonly Regex FUNCTION_ONLY_REGEX = new Regex("^" + FUNCTION_REGEX_STR + "$");

        // TODO: Move extractor regexes to just use capture groups in the regular regexes
        // Other useful regexes
        // List helpers
        private static readonly Regex LIST_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex LIST_EXTRACTOR_REGEX = new Regex(@"\{([^}]*)\}");
        // Lambda helpers
        private static readonly Regex PARAM_DELIMITER_REGEX = new Regex(@",");
        private static readonly Regex PARAM_TYPES_DELIMITER_REGEX = new Regex(@"\|");
        private static readonly Regex PARAM_NAME_TYPE_SPLITTER_REGEX = new Regex(@":");
        private static readonly Regex LAMBDA_EXTRACTOR_REGEX = new Regex(@"\(([^)]*)\)=>\{([^}]*)\}");
        // Type helpers
        private static readonly Regex TYPE_EXTRACTOR_REGEX = new Regex(@"#([a-zA-Z_][a-zA-Z0-9_]*)");
        // Other helpers
        private static readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");
        public Ast ParseExpression(string expression)
        {
            // TODO: Add support for big_decimal and big_int (require D or L at the end of the number literal, i.e. 750L or 0.642D)
            // 'expression' is either a function or literal
            if (FUNCTION_ONLY_REGEX.IsMatch(expression))
            {

            }
            else if (NUMBER_ONLY_REGEX.IsMatch(expression))
            {
                // Parse the double and throw it into the AST
                double number = double.Parse(expression);
                return Ast.NumberLiteral(number);
            }
            else if (LIST_ONLY_REGEX.IsMatch(expression))
            {
                // Extract the elements of the list
                string elements = LIST_EXTRACTOR_REGEX.Match(expression).Groups[1].Value;
                // Separate by the list delimiter
                string[] elementStrings = LIST_DELIMITER_REGEX.Split(elements);
                List<Ast> elementAsts = new List<Ast>();
                foreach (string str in elementStrings)
                {
                    elementAsts.Add(ParseExpression(str));
                }
                return Ast.ListLiteral(elementAsts.ToArray());
            }
            else if (LAMBDA_ONLY_REGEX.IsMatch(expression))
            {
                // Two parts: The expression, and the parameters
                var groups = LAMBDA_EXTRACTOR_REGEX.Match(expression).Groups;
                string paramsString = groups[1].Value;
                string exprString = groups[2].Value;

                // TODO: Parse Parameters

                // Getting this to an AST is simple
                Ast expressionAst = ParseExpression(exprString);
            }
            else if (TYPE_ONLY_REGEX.IsMatch(expression))
            {
                string dtString = TYPE_EXTRACTOR_REGEX.Match(expression).Groups[1].Value;
                return Ast.TypeLiteral(dtString);
            }
            throw new InvalidParseException(expression);
        }
    }
}
