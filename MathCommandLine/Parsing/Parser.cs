using IML.CoreDataTypes;
using IML.Environments;
using IML.Exceptions;
using IML.Functions;
using IML.Parsing.AST;
using IML.Parsing.AST.ValueAsts;
using IML.Parsing.Util;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IML.Parsing
{
    /// <summary>
    /// Parses strings to ASTs. Capable of parsing many different types of strings, such as expressions and declarations.
    /// </summary>
    public class Parser
    {
        // Returns true if we should continue, false if we should break
        private delegate bool CharacterProcessor(int i, WrapperLevels level);

        #region Definitions

        // Regexes for common necessities
        private const string NUMBER_REGEX_PATTERN = @"[+-]?(([0-9]+(\.[0-9]*)?)|(\.[0-9]+))";
        private const string SYMBOL_PATTERN = @"[a-zA-Z_][a-zA-Z0-9_]*";
        private static readonly char MEMBER_ACCESS_TOKEN = '.';

        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex("^" + NUMBER_REGEX_PATTERN + "$");
        private static readonly Regex REFERENCE_REGEX = new Regex(@"^\&(" + SYMBOL_PATTERN + ")$");
        private static readonly Regex LIST_REGEX = new Regex(@"^\{(.*)\}$");
        private static readonly Regex SIMPLE_LAMBDA_REGEX = new Regex(@"^\[(.*)\]$");
        // Group for params. Non capturing to make the ':' optional, capturing for the type
        // Capturing for the arrow type and the return type
        private static readonly Regex LAMBDA_REGEX = new Regex(@"^(?:\<(.*)\>)?\s*\((.*)\)(?:\s*\:(.+))?\s*([=~])>\s*\{(.*)\}$");
        private static readonly Regex STRING_REGEX = new Regex("^\"([^\"]*)\"$");
        private static readonly Regex MEMBER_ACCESS_REGEX = new Regex(@$"^(.*)\{MEMBER_ACCESS_TOKEN}(" + SYMBOL_PATTERN + ")$");

        // Call parsing values
        public const char CALL_END_WRAPPER = ')';
        public const char CALL_START_WRAPPER = '(';
        public const char ARG_DELIMITER = ',';

        public const string VAR_ARGS_SYMBOL = "...";

        // Param parsing values
        public const char GENERAL_END_WRAPPER = ')';
        public const char GENERAL_START_WRAPPER = '(';
        public static readonly string GENERAL_WRAPPERS = $"{GENERAL_START_WRAPPER}{GENERAL_END_WRAPPER}";

        // Simple lambda parsing values
        private const char SIMPLE_LAMBDA_START_WRAPPER = '[';
        private const char SIMPLE_LAMBDA_END_WRAPPER = ']';
        private static readonly string SIMPLE_LAMBDA_WRAPPERS = $"{SIMPLE_LAMBDA_START_WRAPPER}{SIMPLE_LAMBDA_END_WRAPPER}";

        // List parsing values
        private const char LIST_START_WRAPPER = '{';
        private const char LIST_END_WRAPPER = '}';
        private const char LIST_DELIMITER = ',';
        private static readonly string LIST_WRAPPERS = $"{LIST_START_WRAPPER}{LIST_END_WRAPPER}";

        // String parsing values
        public const char STRING_START_WRAPPER = '"';
        public const char STRING_END_WRAPPER = '"';
        public const char STRING_ESCAPE_STARTER = '\\';

        // Parameter parsing regexes
        public const char PARAM_DELIMITER = ',';
        public const string PARAM_NAME_TYPE_SEPARATOR = ":";
        public const char TYPE_UNION_DELIMITER = '|';
        public const char TYPE_GENERICS_DELIMITER = ',';
        public const char TYPE_GENERIC_START_WRAPPER = '<';
        public const char TYPE_GENERIC_END_WRAPPER = '>';
        public static readonly string TYPE_GENERICS_WRAPPERS = $"{TYPE_GENERIC_START_WRAPPER}{TYPE_GENERIC_END_WRAPPER}";

        public const char CODE_LINE_DELIMITER = ';';

        public static readonly string[] ALL_WRAPPERS = new string[]
        {
            GENERAL_WRAPPERS, SIMPLE_LAMBDA_WRAPPERS, LIST_WRAPPERS, TYPE_GENERICS_WRAPPERS
        };

        public const char LAMBDA_TYPE_NO_ENVIRONMENT_LINE = '~';
        public const char LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE = '=';
        public const char LAMBDA_TYPE_ARROW_TIP = '>';
        public const char LAMBDA_TYPE_REQ_ENV_CHARACTER = '!'; // Character for requiring lambda to create env
        public const string LAMBDA_TYPE_PARAM_WRAPPERS = "()";
        public const char LAMBDA_TYPE_PARAM_DELMITER = ',';
        public const string LAMBDA_TYPE_VARARGS_SYMBOL = "...";
        public static readonly Regex LAMBDA_TYPE_REGEX =
            new Regex($@"({TYPE_GENERICS_WRAPPERS[0]}(.*){TYPE_GENERICS_WRAPPERS[1]})?\s*" +
                $@"{LAMBDA_TYPE_PARAM_WRAPPERS[0]}(.*){LAMBDA_TYPE_PARAM_WRAPPERS[1]}\s*" +
                $@"[{LAMBDA_TYPE_REQ_ENV_CHARACTER}]?[{LAMBDA_TYPE_NO_ENVIRONMENT_LINE}|{LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE}]" +
                $@"{LAMBDA_TYPE_ARROW_TIP}\s*(.*)");

        // Variable declaration and assigment syntax
        private const char ASSIGNMENT_TOKEN = '=';
        private const string DECLARATION_VAR_KEYWORD = "var";
        private const string DECLARATION_CONST_KEYWORD = "const";
        private static readonly Regex DECLARATION_REGEX =
            new Regex($@"^({DECLARATION_VAR_KEYWORD}|{DECLARATION_CONST_KEYWORD})\s+({SYMBOL_PATTERN})" +
                $@"(?:\s*\:(.*))?\s*{ASSIGNMENT_TOKEN}\s*(.*)$");

        private const string RETURN_KEYWORD = "return";
        private static readonly Regex RETURN_REGEX =
            new Regex(@$"^{RETURN_KEYWORD}(?:\s+(.*))?$");

        private const string DEREFERENCE_TOKEN = "*";

        // Other useful regexes
        public static readonly Regex SYMBOL_NAME_REGEX = new Regex(@$"^{SYMBOL_PATTERN}$");
        private static readonly List<string> RESERVED_KEYWORDS = new List<string>()
        {
            DECLARATION_VAR_KEYWORD,
            DECLARATION_CONST_KEYWORD,
            RETURN_KEYWORD,
        };

        #endregion

        private TypeDeterminer typeDeterminer;
        private VariableAstTypeMap defaultVariables;
        private ParameterParser parameterParser;

        public Parser(VariableAstTypeMap defaultVariables)
        {
            typeDeterminer = new TypeDeterminer();
            this.defaultVariables = defaultVariables;
            parameterParser = new ParameterParser();
        }

        public virtual Ast Parse(string expression)
        {
            return Parse(expression, defaultVariables);
        }

        /// <summary>
        /// Parses a finalized expression into an AST
        /// Recall: Finalized expressions consist only of functions, literals, and variables
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Ast Parse(string expression, VariableAstTypeMap typeMap)
        {
            try
            {
                // Initially, attempt to extract an expression from parentheses
                while (IsParenWrapped(expression))
                {
                    // Pull out the expression without the first and last characters
                    expression = expression.Substring(1, expression.Length - 2);
                    return Parse(expression, typeMap);
                }

                // 'expression' is either a call, variable, or literal
                // May be something that is wrapped entirely in parenthesis
                CallMatch attempedCallMatch = CallMatcher.MatchCall(expression);
                int attemptedAssignmentMatchIndex = TryMatchAssignment(expression);

                if (attempedCallMatch.IsMatch)
                {
                    // Performing some sort of call
                    string callerString = attempedCallMatch.Caller;
                    string argsString = attempedCallMatch.Args;
                    string genericsString = attempedCallMatch.Generics;

                    Ast caller = Parse(callerString, typeMap);
                    if (caller.Type == AstTypes.Invalid)
                    {
                        // Invalid caller means we need to return an invalid overall, instead of a valid call
                        return Ast.Invalid(expression);
                    }

                    string[] argStrings = argsString.Length > 0 ? SplitByDelimiter(argsString, ARG_DELIMITER) : new string[0];
                    // Parse all the arguments
                    var args = argStrings.Select(x => Parse(x, typeMap)).ToList();

                    string[] genericStrings = genericsString.Length > 0 ? SplitByDelimiter(genericsString, TYPE_GENERICS_DELIMITER) : new string[0];
                    var generics = genericStrings.Select(x => parameterParser.ParseType(x)).ToList();

                    return Ast.Call(caller, args, generics);
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
                        elementAsts.Add(Parse(str.Trim(), typeMap));
                    }
                    return Ast.ListLiteral(elementAsts.ToArray());
                }
                else if (LAMBDA_REGEX.IsMatch(expression))
                {
                    // 3-4 parts: parameters, return type (optional), type of lambda (environment/no environment), expression/body
                    var groups = LAMBDA_REGEX.Match(expression).Groups;
                    string genericsString = groups[1].Value;
                    string paramsString = groups[2].Value;
                    string returnTypeString = groups[3].Value;
                    string arrowBit = groups[4].Value;
                    string exprString = groups[5].Value;

                    bool provideReturnType = returnTypeString.Length > 0;

                    // Parse Parameters
                    List<AstParameter> parsedParamsList = new List<AstParameter>();
                    AstParameter[] parsedParams;
                    bool hasVarArgs = false;
                    if (paramsString.Length > 0)
                    {
                        string[] splitParamsStrings = SplitByDelimiter(paramsString, PARAM_DELIMITER);
                        for (int i = 0; i < splitParamsStrings.Length; i++)
                        {
                            if (i + 1 == splitParamsStrings.Length)
                            {
                                // This is the last parameter; check for varargs
                                if (splitParamsStrings[i].EndsWith(VAR_ARGS_SYMBOL))
                                {
                                    hasVarArgs = true;
                                    splitParamsStrings[i] = splitParamsStrings[i].SubstringBetween(0,
                                        splitParamsStrings[i].Length - VAR_ARGS_SYMBOL.Length);
                                }
                            }
                            parsedParamsList.Add(parameterParser.ParseParameter(splitParamsStrings[i]));
                        }
                        parsedParams = parsedParamsList.ToArray();
                    }
                    else
                    {
                        parsedParams = new AstParameter[0];
                    }

                    bool createsEnv = arrowBit == LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE.ToString();

                    VariableAstTypeMap mapToUseForBody = typeMap;
                    if (createsEnv)
                    {
                        mapToUseForBody = typeMap.Clone();
                        // Need to add all the params to it
                        for (int i = 0; i < parsedParams.Length; i++)
                        {
                            mapToUseForBody.Add(parsedParams[i].Name, parsedParams[i].Type);
                        }
                    }

                    // In the event that we don't create an env, the typeMap will be updated by parsing the body
                    // We clone the map to use for body return type since we will always have our own type map updated by parsing
                    List<Ast> body = ParseBody(exprString, mapToUseForBody);
                    AstType bodyReturnType = typeDeterminer.DetermineDataType(body, mapToUseForBody.Clone());
                    AstType returnType;
                    if (provideReturnType)
                    {
                        returnType = parameterParser.ParseType(returnTypeString);
                        if (bodyReturnType != returnType)
                        {
                            throw new InvalidParseException("Function return type does not match returned value(s)", expression);
                        }
                    }
                    else
                    {
                        returnType = bodyReturnType;
                    }

                    List<string> generics = ParseGenericNames(genericsString);

                    return Ast.LambdaLiteral(parsedParams, body, returnType, createsEnv, false, hasVarArgs, generics);
                }
                else if (MatchesSimpleLambda(expression))
                {
                    // Simple lambda; no params, does not create body
                    string contents = SIMPLE_LAMBDA_REGEX.Match(expression).Groups[1].Value;
                    Ast body = Parse(contents, typeMap); // Don't clone the type map since we don't create env
                    return Ast.LambdaLiteral(new AstParameter[0], new List<Ast>() { body }, AstType.Any, false, false, false,
                        new List<string>());
                }
                else if (MEMBER_ACCESS_REGEX.IsMatch(expression))
                {
                    var groups = MEMBER_ACCESS_REGEX.Match(expression).Groups;
                    string parentStr = groups[1].Value;
                    string name = groups[2].Value;
                    Ast parent = Parse(parentStr, typeMap);
                    return Ast.MemberAccess(parent, name);
                }
                else if (RETURN_REGEX.IsMatch(expression))
                {
                    var groups = RETURN_REGEX.Match(expression).Groups;
                    string body = groups[1].Value.Trim();
                    bool returnsVoid = false;
                    Ast bodyAst = null;
                    if (body.Length <= 0)
                    {
                        returnsVoid = true;
                    }
                    else
                    {
                        bodyAst = Parse(body, typeMap);
                    }
                    return Ast.Return(bodyAst, returnsVoid);
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
                    VariableType varType = 0;
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
                    string assignedExpr = groups[4].Value;
                    Ast assigned = Parse(assignedExpr, typeMap);

                    // See if we've been given a type; if so, verify that it matches; if not, infer the type
                    string variableType = groups[3].Value;
                    AstType varValType;
                    AstType bodyType = typeDeterminer.DetermineDataType(assigned, typeMap);
                    if (variableType.Length > 0)
                    {
                        varValType = parameterParser.ParseType(variableType);
                        if (varValType != bodyType)
                        {
                            throw new InvalidParseException($"Variable {varName} is not assigned the type it is declared.",
                                expression);
                        }
                    }
                    else
                    {
                        varValType = bodyType;
                    }

                    return Ast.VariableDeclaration(varName, assigned, varType, varValType);
                }
                else if (attemptedAssignmentMatchIndex >= 0)
                {
                    string identifierExpr = expression.Substring(0, attemptedAssignmentMatchIndex).Trim();
                    string assignedExpr = expression.Substring(attemptedAssignmentMatchIndex + 1,
                        expression.Length - attemptedAssignmentMatchIndex - 1).Trim();
                    // Parse the assigned expression
                    Ast assigned = Parse(assignedExpr, typeMap);
                    // Parse the identifier
                    IdentifierAst identifier = ParseIdentifier(identifierExpr, typeMap);
                    // TODO: Add operator-assignments, ex. +=, &&=, etc.
                    // Should be valid for ANY binary operator; even doing something like !== or === should work,
                    // though they'd be pretty rare to want to do
                    return Ast.VariableAssignment(identifier, assigned);
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

        private List<Ast> ParseBody(string bodyExpr, VariableAstTypeMap typeMap)
        {
            string[] lines = SplitByDelimiter(bodyExpr, CODE_LINE_DELIMITER);

            if (lines.Length == 1)
            {
                string line = lines[0].Trim();
                if (line.Length > 0)
                {
                    // Single line, no semicolon after
                    // This is valid syntax
                    // Special case so the code below doesn't give invalid for missing a semicolon
                    return new List<Ast>()
                    {
                        Parse(line, typeMap)
                    };
                }
                else
                {
                    // Lambda has an empty body
                    throw new InvalidOperationException("Lambda has an empty body; Currently UNIMPLEMENTED");
                }
            }

            // The last line should be empty, denoting that a semicolon was used after the last actual line of code
            // If not, then we didn't have a final semicolon
            if (lines[lines.Length - 1].Trim().Length > 0)
            {
                // Last "line" is not empty, meaning we did not have a semicolon after the final function line
                throw new InvalidParseException($"Statement not terminated (missing '{CODE_LINE_DELIMITER}')", bodyExpr);
            }

            List<Ast> bodyLines = new List<Ast>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                // Don't add the last (empty) line
                if (i + 1 < lines.Length)
                {
                    bodyLines.Add(Parse(line, typeMap));
                }
            }
            return bodyLines;
        }

        public static List<string> ParseGenericNames(string expression)
        {
            string[] genericNamesArray = SplitByDelimiter(expression, TYPE_GENERICS_DELIMITER,
                new string[] { TYPE_GENERICS_WRAPPERS });
            List<string> genericNames = new List<string>(genericNamesArray);
            // Make sure all the names are valid, also trim out whitespace
            for (int i = 0; i < genericNames.Count; i++)
            {
                genericNames[i] = genericNames[i].Trim();
                if (!SYMBOL_NAME_REGEX.IsMatch(genericNames[i]))
                {
                    throw new InvalidParseException($"{genericNames[i]} is not a valid symbol name", genericNames[i]);
                }
            }
            return genericNames;
        }

        public IdentifierAst ParseIdentifier(string expression, VariableAstTypeMap typeMap)
        {
            if (IsParenWrapped(expression))
            {
                // Pull out the expression without the first and last characters
                expression = expression.Substring(1, expression.Length - 2);
                return ParseIdentifier(expression, typeMap);
            }

            // Check if this is just a symbol string
            if (SYMBOL_NAME_REGEX.IsMatch(expression))
            {
                if (RESERVED_KEYWORDS.Contains(expression))
                {
                    // Attempting to use a reserved keyword as a variable
                    throw new InvalidParseException(expression);
                }
                return IdentifierAst.RawVariable(expression);
            }

            // Try to do a dereference, if the string starts with it
            if (expression.StartsWith(DEREFERENCE_TOKEN))
            {
                // Everything else gets turned into the AST
                Ast r = Parse(expression.Substring(DEREFERENCE_TOKEN.Length), typeMap);
                return IdentifierAst.Dereference(r);
            }

            // Try to do member access
            if (MEMBER_ACCESS_REGEX.IsMatch(expression))
            {
                var groups = MEMBER_ACCESS_REGEX.Match(expression).Groups;
                string parentExpr = groups[1].Value;
                string varName = groups[2].Value;
                Ast parent = Parse(parentExpr, typeMap);
                return IdentifierAst.MemberAccess(parent, varName);
            }

            throw new InvalidParseException(expression);
        }

        #region Utilities

        // Passes over the expression, performing "operation" for each 
        // Returns the final wrapper levels
        private static WrapperLevels PassOverExpression(string expression, CharacterProcessor operation,
            params string[] wrappers)
        {
            WrapperLevels levels = new WrapperLevels();
            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (c == STRING_START_WRAPPER)
                {
                    operation.Invoke(i, levels);
                    levels.SetInString(true);
                    // Need to continue until we reach the end of the string
                    bool stringTerminated = false;
                    while (!stringTerminated)
                    {
                        char prev = c;
                        i++;
                        if (i >= expression.Length)
                        {
                            break;
                        }
                        operation.Invoke(i, levels);
                        c = expression[i];
                        if (c == STRING_END_WRAPPER && prev != STRING_ESCAPE_STARTER)
                        {
                            stringTerminated = true;
                        }
                    }
                    if (stringTerminated)
                    {
                        levels.SetInString(false);
                    }
                    // Go to next iteration since c = '"' (the closing quote)
                    continue;
                }
                foreach (string wrapper in wrappers)
                {
                    if (wrapper[0] == c)
                    {
                        levels.ChangeLevel(wrapper, 1);
                    }
                    else if (wrapper[1] == c && !IsLambdaArrow(expression, i))
                    {
                        levels.ChangeLevel(wrapper, -1);
                        // Fast return if levels are unbalanced
                        if (levels.GetLevel(wrapper) < 0)
                        {
                            return levels;
                        }
                    }
                }
                operation.Invoke(i, levels);
            }

            return levels;
        }

        // Same as PassOverExpression, but WON'T call operation on wrapper characters
        private static WrapperLevels PassOverExpressionIgnoreWrappers(string expression,
            CharacterProcessor operation, params string[] wrappers)
        {
            string jointWrappers = string.Join("", wrappers);
            return PassOverExpression(expression, (i, l) =>
            {
                char c = expression[i];
                if (!jointWrappers.Contains(c))
                {
                    return operation(i, l);
                }
                return true;

            }, wrappers);
        }

        public static bool IsWrappedBy(string expression, char start, char end, params string[] wrappers)
        {
            if (expression.Length <= 2 || expression[0] != start || expression[expression.Length - 1] != end)
            {
                return false;
            }
            WrapperLevels levels = PassOverExpression(expression, (a, b) => true, wrappers);
            WrapperLevels innerLevels = PassOverExpression(expression.Substring(1, expression.Length - 2),
                (a, b) => true, wrappers);
            return levels.AtLevelZero() && innerLevels.AtLevelZero();
        }

        // Private helper method for the IsWrappedBy and other such methods
        // Check if the two characters provide a valid lambda arrow, which would mean we ignore
        // the <> bracket count
        private static bool IsLambdaArrow(string exp, int currentIndex)
        {
            if (currentIndex <= 0)
            {
                return false;
            }
            char c1 = exp[currentIndex - 1];
            char c2 = exp[currentIndex];
            return (c1 == LAMBDA_TYPE_NO_ENVIRONMENT_LINE || c1 == LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE) &&
                c2 == LAMBDA_TYPE_ARROW_TIP;
        }

        private static bool IsWrappedBy(string expression, char start, char end)
        {
            return IsWrappedBy(expression, start, end, ALL_WRAPPERS);
        }

        public static bool IsParenWrapped(string expression)
        {
            return IsWrappedBy(expression, GENERAL_START_WRAPPER, GENERAL_END_WRAPPER);
        }

        public static int GetBracketEndIndex(string str, int idx, char start, char end)
        {
            int level = 0;
            for (int i = idx; i < str.Length; i++)
            {
                if (str[i] == start)
                {
                    level++;
                }
                if (str[i] == end)
                {
                    level--;
                    if (level == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private static bool MatchesList(string expression)
        {
            return IsWrappedBy(expression, LIST_START_WRAPPER, LIST_END_WRAPPER);
        }

        private static bool MatchesSimpleLambda(string expression)
        {
            return IsWrappedBy(expression, SIMPLE_LAMBDA_START_WRAPPER, SIMPLE_LAMBDA_END_WRAPPER);
        }

        public static string[] SplitByDelimiter(string expr, char delimiter)
        {
            return SplitByDelimiter(expr, delimiter, ALL_WRAPPERS);
        }

        public static string[] SplitByDelimiter(string expr, char delimiter, string[] wrappers)
        {
            if (expr.Length == 0)
            {
                return new string[0];
            }
            List<string> result = new List<string>();
            StringBuilder current = new StringBuilder();
            PassOverExpression(expr, (index, levels) =>
            {
                char debug_character = expr[index];
                if (expr[index] == delimiter && levels.AtLevelZero())
                {
                    result.Add(current.ToString());
                    current = new StringBuilder("");
                }
                else
                {
                    current.Append(expr[index]);
                }
                return true;

            }, wrappers);
            result.Add(current.ToString());
            return result.ToArray();
        }

        // Returns index of the '='
        public static int TryMatchAssignment(string expression)
        {
            Container<int> equalsIndex = new Container<int>(-1);
            PassOverExpressionIgnoreWrappers(expression, (index, levels) =>
            {
                if (expression[index] == ASSIGNMENT_TOKEN && levels.AtLevelZero())
                {
                    equalsIndex.Set(index);
                    return false;
                }
                return true;
            }, ALL_WRAPPERS);
            return equalsIndex.Get();
        }

        #endregion
    }
}
