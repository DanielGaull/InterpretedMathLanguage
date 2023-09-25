using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
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
        private const string NUMBER_REGEX_PATTERN = @"[+-]?(([0-9]+(\.[0-9]*)?)|(\.[0-9]+))";
        private const string SYMBOL_PATTERN = @"[a-zA-Z_][a-zA-Z0-9_]*";

        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex("^" + NUMBER_REGEX_PATTERN + "$");
        private static readonly Regex REFERENCE_REGEX = new Regex(@"^\&(" + SYMBOL_PATTERN + ")$");
        // Group for the list elements
        private static readonly Regex LIST_REGEX = new Regex(@"^\{(.*)\}$");
        private static readonly Regex LAMBDA_REGEX = new Regex(@"^\((.*?)\)([=~])>\{(.*)\}$");
        private static readonly Regex STRING_REGEX = new Regex("^\"([^\"]*)\"$");

        // Call parsing values
        private const char CALL_END_WRAPPER = ')';
        private const char CALL_START_WRAPPER = '('; 
        private const char ARG_DELIMITER = ',';

        // Param parsing values
        private const char GENERIC_END_WRAPPER = ')';
        private const char GENERIC_START_WRAPPER = '(';

        // List parsing values
        private const char LIST_START_WRAPPER = '{';
        private const char LIST_END_WRAPPER = '}';
        private const char LIST_DELIMITER = ',';

        // String parsing values
        private const char STRING_START_WRAPPER = '"';
        private const char STRING_END_WRAPPER = '"';

        // Parameter parsing regexes
        private const char PARAM_DELIMITER = ',';
        private const char PARAM_TYPES_DELIMITER = '|';
        private const char PARAM_REQS_DELIMITER = ',';
        // Group for param name and group for type(s)
        private static readonly Regex PARAM_NAME_TYPE_REGEX = new Regex(@"(.*):(.*)");
        // Group for restrictions, and for type name
        private static readonly Regex PARAM_TYPE_RESTS_REGEX = new Regex(@"(?:\[(.*)\])?([a-zA-Z_][a-zA-Z0-9_]*)");

        // Variable declaration and assigment syntax
        private const string ASSIGNMENT_TOKEN = "=";
        private const string DECLARATION_VAR_KEYWORD = "var";
        private const string DECLARATION_CONST_KEYWORD = "const";
        private static readonly Regex DECLARATION_REGEX = 
            new Regex($@"^({DECLARATION_VAR_KEYWORD}|{DECLARATION_CONST_KEYWORD})\s+({SYMBOL_PATTERN})\s*" + 
                $@"{ASSIGNMENT_TOKEN}\s*(.*)$");
        private static readonly Regex ASSIGMENT_REGEX = new Regex(
            $@"^({SYMBOL_PATTERN})\s*{ASSIGNMENT_TOKEN}\s*(.*)$");

        // Value restriction regexes
        private static readonly Regex VALUE_REST_INTEGER = new Regex(@"%");
        private static readonly Regex VALUE_REST_POSITIVE = new Regex(@"\+");
        private static readonly Regex VALUE_REST_NEGATIVE = new Regex(@"-");
        private static readonly Regex VALUE_REST_LT = new Regex(@$"<\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex VALUE_REST_GT = new Regex(@$">\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex VALUE_REST_LTE = new Regex(@$"<=\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex VALUE_REST_GTE = new Regex(@$">=\(({NUMBER_REGEX_PATTERN})\)");
        private static readonly Regex VALUE_REST_TYPES = new Regex(@"$t\((.*)\)^");
        private static readonly Regex VALUE_REST_LEN_EQ = new Regex(@"$l=\(([+]?[0-9]*)\)^");

        // Other useful regexes
        private static readonly Regex SYMBOL_NAME_REGEX = new Regex(@$"^{SYMBOL_PATTERN}$");
        private static readonly List<string> RESERVED_KEYWORDS = new List<string>()
        {
            DECLARATION_VAR_KEYWORD,
            DECLARATION_CONST_KEYWORD,
        };

        public Parser()
        {
        }

        /// <summary>
        /// Parses a finalized expression into an AST
        /// Recall: Finalized expressions consist only of functions, literals, and variables
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Ast Parse(string expression)
        {
            try
            {
                // TODO: Add support for big_decimal and big_int (require D or L at the end of the number literal, i.e. 750L or 0.642D)

                // Initially, attempt to extract an expression from parentheses
                while (IsParamWrapped(expression))
                {
                    // Pull out the expression without the first and last characters
                    expression = expression.Substring(1, expression.Length - 2);
                    return Parse(expression);
                }

                // 'expression' is either a call, variable, or literal
                // May be something that is wrapped entirely in parenthesis
                CallMatch attempedCallMatch = MatchCall(expression);

                if (attempedCallMatch.IsMatch)
                {
                    // Performing some sort of call
                    string callerString = attempedCallMatch.Caller;
                    string argsString = attempedCallMatch.Args;

                    Ast caller = Parse(callerString);
                    if (caller.Type == AstTypes.Invalid)
                    {
                        // Invalid caller means we need to return an invalid overall, instead of a valid call
                        return Ast.Invalid(expression);
                    }

                    string[] argStrings = argsString.Length > 0 ? SplitByDelimiter(argsString, ARG_DELIMITER) : new string[0];
                    // Parse all the arguments
                    Ast[] args = argStrings.Select(Parse).ToArray();

                    return Ast.Call(caller, args);
                }
                else if (NUMBER_REGEX.IsMatch(expression))
                {
                    // Parse the double and throw it into the AST
                    double number = double.Parse(expression);
                    return Ast.NumberLiteral(number);
                }
                else if (STRING_REGEX.IsMatch(expression))
                {
                    string str = STRING_REGEX.Match(expression).Groups[1].Value;
                    return Ast.StringLiteral(str);
                }
                else if (REFERENCE_REGEX.IsMatch(expression))
                {
                    string varName = REFERENCE_REGEX.Match(expression).Groups[1].Value;
                    return Ast.ReferenceLiteral(varName);
                }
                else if (MatchesList(expression)) //LIST_REGEX.IsMatch(expression))
                {
                    // Extract the elements of the list
                    string elements = LIST_REGEX.Match(expression).Groups[1].Value;
                    // Separate by the list delimiter
                    string[] elementStrings = elements.Length > 0 ? SplitByDelimiter(elements, LIST_DELIMITER) : new string[0];
                    List<Ast> elementAsts = new List<Ast>();
                    foreach (string str in elementStrings)
                    {
                        elementAsts.Add(Parse(str));
                    }
                    return Ast.ListLiteral(elementAsts.ToArray());
                }
                else if (LAMBDA_REGEX.IsMatch(expression))
                {
                    // Three parts: The expression, the parameters, and the type of lambda (environment/no environment)
                    var groups = LAMBDA_REGEX.Match(expression).Groups;
                    string paramsString = groups[1].Value;
                    string arrowBit = groups[2].Value;
                    string exprString = groups[3].Value;

                    // Parse Parameters
                    AstParameter[] parsedParams = paramsString.Length > 0 ?
                        (SplitByDelimiter(paramsString, PARAM_DELIMITER).Select((paramString) =>
                        {
                            return ParseParameter(paramString);
                        }).ToArray()) : new AstParameter[0];

                    Ast body = Parse(exprString);

                    return Ast.LambdaLiteral(parsedParams, body, arrowBit == "=");
                }
                else if (ASSIGMENT_REGEX.IsMatch(expression))
                {
                    var groups = ASSIGMENT_REGEX.Match(expression).Groups;
                    string varName = groups[1].Value;
                    string assignedExpr = groups[2].Value;
                    // Make sure the variable name isn't reserved
                    if (RESERVED_KEYWORDS.Contains(varName))
                    {
                        // Attempting to use a reserved keyword as a variable
                        throw new InvalidParseException(expression);
                    }
                    // Parse the assigned expression
                    Ast assigned = Parse(assignedExpr);
                    // TODO: Add operator-assignments, ex. +=, &&=, etc.
                    // Should be valid for ANY operator; even doing something like !== or === should work,
                    // though they'd be pretty rare to want to do
                    return Ast.VariableAssignment(varName, assigned);
                }
                else if (DECLARATION_REGEX.IsMatch(expression))
                {
                    var groups = DECLARATION_REGEX.Match(expression).Groups;
                    string varName = groups[2].Value;
                    // Make sure the variable name isn't reserved
                    if (RESERVED_KEYWORDS.Contains(varName))
                    {
                        // Attempting to use a reserved keyword as a variable
                        throw new InvalidParseException(expression);
                    }
                    string declarationType = groups[1].Value;
                    VariableType varType = (VariableType)0;
                    if (declarationType == DECLARATION_VAR_KEYWORD)
                    {
                        varType = VariableType.Variable;
                    }
                    else if (declarationType == DECLARATION_CONST_KEYWORD)
                    {
                        varType = VariableType.Constant;
                    }
                    else
                    {
                        // Should never get here, we'd have not matched the regex
                        throw new InvalidParseException(expression);
                    }
                    string assignedExpr = groups[3].Value;
                    Ast assigned = Parse(assignedExpr);

                    return Ast.VariableDeclaration(varName, assigned, varType);
                }
                else if (SYMBOL_NAME_REGEX.IsMatch(expression))
                {
                    // We've got a variable, or an attempt at one
                    if (RESERVED_KEYWORDS.Contains(expression))
                    {
                        // Attempting to use a reserved keyword as a variable
                        throw new InvalidParseException(expression);
                    }
                    // Valid variable
                    return Ast.Variable(expression);
                }
                else
                {
                    // We don't recognize this, so call it "invalid" and chuck it in here
                    // Someone else will either handle it or throw an error
                    return Ast.Invalid(expression);
                }
            }
            catch (InvalidParseException)
            {
                return Ast.Invalid(expression);
            }
        }

        public string Unparse(Ast ast)
        {
            StringBuilder builder = new StringBuilder();
            switch (ast.Type)
            {
                case AstTypes.NumberLiteral:
                    return ast.NumberArg.ToString();
                case AstTypes.ListLiteral:
                    builder.Append(LIST_START_WRAPPER);
                    for (int i = 0; i < ast.AstCollectionArg.Length; i++)
                    {
                        builder.Append(Unparse(ast.AstCollectionArg[i]));
                        if (i + 1 < ast.AstCollectionArg.Length)
                        {
                            builder.Append(LIST_DELIMITER);
                        }
                    }
                    builder.Append(LIST_END_WRAPPER);
                    return builder.ToString();
                case AstTypes.StringLiteral:
                    builder.Append(STRING_START_WRAPPER);
                    builder.Append(ast.Expression);
                    builder.Append(STRING_END_WRAPPER);
                    return builder.ToString();
                case AstTypes.Variable:
                    return ast.Name;
                case AstTypes.Call:
                    builder.Append(Unparse(ast.CalledAst));
                    builder.Append(CALL_START_WRAPPER);
                    for (int i = 0; i < ast.AstCollectionArg.Length; i++)
                    {
                        builder.Append(Unparse(ast.AstCollectionArg[i]));
                        if (i + 1 < ast.AstCollectionArg.Length)
                        {
                            builder.Append(ARG_DELIMITER);
                        }
                    }
                    builder.Append(CALL_END_WRAPPER);
                    return builder.ToString();
                case AstTypes.Invalid:
                    return ast.Expression;
                case AstTypes.LambdaLiteral:
                    return "(" + string.Join(',', ast.Parameters.Select(x => UnparseParameter(x)).ToArray()) + ")" +
                        (ast.CreatesEnv ? "=>" : "~>") + "{" +
                        Unparse(ast.Body) + "}";
            }
            return "";
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
                        else if (VALUE_REST_LEN_EQ.IsMatch(reqStr))
                        {
                            var thisReqGroup = VALUE_REST_LEN_EQ.Match(reqStr).Groups;
                            string argAsString = thisReqGroup[1].Value;
                            if (int.TryParse(argAsString, out int arg))
                            {
                                return ValueRestriction.LengthEqual(arg);
                            }
                            else
                            {
                                // TODO: Invalid input somehow, even though Regex should prevent this
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

        public string UnparseParameter(AstParameter parameter)
        {
            return parameter.Name + ":" + string.Join('|', 
                parameter.TypeEntries.Select(x => 
                    "[" + string.Join(',', x.ValueRestrictions) + "]" + x.DataTypeName).ToArray());
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
                    if (c == GENERIC_START_WRAPPER)
                    {
                        parenCounter++;
                    }
                    else if (c == GENERIC_END_WRAPPER)
                    {
                        parenCounter--;
                    }
                    else if (c == LIST_START_WRAPPER)
                    {
                        braceCounter++;
                    }
                    else if (c == LIST_END_WRAPPER)
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
        /// Returns true if the expression is a list, or false if not
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool MatchesList(string expression)
        {
            if (expression.Length < 2)
            {
                return false;
            }
            if (expression[0] != LIST_START_WRAPPER)
            {
                return false;
            }
            if (expression[expression.Length - 1] != LIST_END_WRAPPER)
            {
                return false;
            }
            int listLevels = 0;
            for (int i = 1; i < expression.Length - 1; i++)
            {
                char c = expression[i];
                if (c == GENERIC_START_WRAPPER)
                {
                    // We want to jump to the end of the list parenthesis
                    while (c != GENERIC_END_WRAPPER)
                    {
                        c = expression[i];
                        i++;
                        if (i >= expression.Length - 1)
                        {
                            // If no end to the paren, we have bigger problems
                            return false;
                        }
                    }
                }
                else if (c == LIST_START_WRAPPER)
                {
                    listLevels++;
                }
                else if (c == LIST_END_WRAPPER)
                {
                    // If we're closing the current list, then that's a problem
                    if (listLevels == 0)
                    {
                        return false;
                    }
                    listLevels--;
                }
            }
            // If we haven't failed yet, then just return
            return true;
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
